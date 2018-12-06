using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

public class Main : MonoBehaviour {

    [Header("Networking")]
    [SerializeField] private string _serverUrl = "https://serverHost.com";
    [SerializeField] private RouteConfiguration _serverRouteGetSpinnerValues = new RouteConfiguration {
        Path = "/values",
        Verb = UnityWebRequest.kHttpVerbGET
    };
    [SerializeField] private RouteConfiguration _serverRouteGetSpinnerSpin = new RouteConfiguration {
        Path = "/spin",
        Verb = UnityWebRequest.kHttpVerbPOST
    };

    [Header("Networking: VirtualServer")]
    [Tooltip("When enabled, the UI will consume the current spinner values and the last result of the history. Both of those can be edited via the Runtime section.")]
    [SerializeField] private bool _virtualServerEnabled = false;
    [SerializeField] private float _virtualServerDelayInSeconds = 2f;
    [SerializeField] private string _virtualServerError;
    [SerializeField] private SpinnerData _virtualServerResult = new SpinnerData {
        SpinnerValue = 100,
        SpinnerValues = new int[] { 100, 500, 1000, 10000, 50000, 100000 }
    };

    private IPostman _postman;

    [Header("UI")]
    [SerializeField] private CanvasHideableUI _loadingUI;
    [SerializeField] private GameplayUI _gameplayUI;
    [SerializeField] private PopupTextUI _errorPopupUI;

    [Header("Runtime")]
    [SerializeField] private int[] _spinnerValues;
    [SerializeField] private List<string> _spinnerResultHistory;

    private void Awake() {
        Debug.Assert(_loadingUI != null, "_loadingUI should not be null.");
        Debug.Assert(_gameplayUI != null, "_gameplayUI should not be null.");
        Debug.Assert(_errorPopupUI != null, "_errorPopupUI should not be null.");

        _postman = new UnityPostman(this, _serverUrl, new Logger(Debug.unityLogger.logHandler), new JsonSerializer());

        _loadingUI.Initialize();

        _gameplayUI.Initialize(_errorPopupUI);
        _gameplayUI.Spinner.OnSpinAction += Spin;

        _errorPopupUI.Initialize();

        _loadingUI.Show();

        if (!_virtualServerEnabled) {
            _postman.Send<SpinnerData>(_serverRouteGetSpinnerValues, OnSpinValuesReceived);
        }
        else {
            CallbackAfterSeconds(_virtualServerDelayInSeconds, () => OnSpinValuesReceived(_virtualServerError, _virtualServerResult));
        }

        _spinnerResultHistory = new List<string>(100);
    }

    private void Spin() {
        _gameplayUI.Spinner.StartSpinAnimation();

        if (!_virtualServerEnabled) {
            _postman.Send<SpinnerData>(_serverRouteGetSpinnerSpin, OnSpinResultReceived);
        }
        else {
            CallbackAfterSeconds(_virtualServerDelayInSeconds, () => OnSpinResultReceived(_virtualServerError, _virtualServerResult));
        }
    }

    private void OnSpinValuesReceived(string err, SpinnerData data) {
        if (string.IsNullOrEmpty(err)) {
            _spinnerValues = data.SpinnerValues;

            _loadingUI.Hide();

            _gameplayUI.Spinner.Reset(_spinnerValues);
            _gameplayUI.Show();
        }
        else {
            _errorPopupUI.SetTextAndShow(err);
        }
    }

    private void OnSpinResultReceived(string err, SpinnerData data) {
        if (_spinnerResultHistory.Count == _spinnerResultHistory.Capacity) {
            _spinnerResultHistory.RemoveAt(0);
        }

        if (string.IsNullOrEmpty(err)) {
            _spinnerResultHistory.Add(data.SpinnerValue.ToString());

            _gameplayUI.Spinner.StopSpinAnimationAtValue(data.SpinnerValue);
        } else {
            _spinnerResultHistory.Add(err);

            _gameplayUI.Spinner.StopSpinAnimationWithError(err);
        }
    }

    private void CallbackAfterSeconds(float seconds, Action onDone) {
        StartCoroutine(CallbackAfterSecondsRoutine(seconds, onDone));
    }

    private IEnumerator CallbackAfterSecondsRoutine(float seconds, Action onDone) {
        yield return new WaitForSecondsRealtime(seconds);

        onDone();
    }
}

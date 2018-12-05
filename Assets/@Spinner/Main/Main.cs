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
    [SerializeField] private SpinnerData _virtualServerResult = new SpinnerData {
        SpinnerValue = 100,
        SpinnerValues = new int[] { 100, 500, 1000, 10000, 50000, 100000 }
    };

    private IPostman _postman;

    [Header("UI")]
    [SerializeField] private CanvasHideableUI _loadingUI;
    [SerializeField] private GameplayUI _gameplayUI;

    [Header("Runtime")]
    [SerializeField] private int[] _spinnerValues;
    [SerializeField] private List<string> _spinnerResultHistory;

    private void Awake() {
        Debug.Assert(_loadingUI != null, "_loadingUI should not be null.");
        Debug.Assert(_gameplayUI != null, "_gameplayUI should not be null.");

        _postman = new UnityPostman(this, new Uri(_serverUrl), new Logger(Debug.unityLogger.logHandler), new JsonSerializer());

        _loadingUI.Initialize();

        _gameplayUI.OnSpinAction += Spin;
        _gameplayUI.Initialize();

        _loadingUI.Show();

        if (!_virtualServerEnabled) {
            _postman.Send<SpinnerData>(_serverRouteGetSpinnerValues, OnSpinValuesReceived);
        }
        else {
            CallbackAfterSeconds(_virtualServerDelayInSeconds, () => OnSpinValuesReceived(null, _virtualServerResult));
        }

        _spinnerResultHistory = new List<string>(100);
    }

    private void Spin() {
        _gameplayUI.StartSpinAnimation();

        if (!_virtualServerEnabled) {
            _postman.Send<SpinnerData>(_serverRouteGetSpinnerSpin, OnSpinResultReceived);
        }
        else {
            CallbackAfterSeconds(_virtualServerDelayInSeconds, () => OnSpinResultReceived(null, _virtualServerResult));
        }
    }

    private void OnSpinValuesReceived(string err, SpinnerData data) {
        _spinnerValues = data.SpinnerValues;

        _gameplayUI.Reset(_spinnerValues);
        _gameplayUI.Show();
        _loadingUI.Hide();
    }

    private void OnSpinResultReceived(string err, SpinnerData data) {
        if (_spinnerResultHistory.Count == _spinnerResultHistory.Capacity) {
            _spinnerResultHistory.RemoveAt(0);
        }

        if (string.IsNullOrEmpty(err)) {
            _spinnerResultHistory.Add(data.SpinnerValue.ToString());

            _gameplayUI.StopSpinAnimationAtValue(data.SpinnerValue);
        } else {
            _spinnerResultHistory.Add(err);

            _gameplayUI.StopSpinAnimationWithError(err);
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

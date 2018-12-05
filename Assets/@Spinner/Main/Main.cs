using System;
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

    private IPostman _postman;

    [Header("UI")]
    [SerializeField] private CanvasHideableUI _loadingUI;
    [SerializeField] private GameplayUI _gameplayUI;

    [Header("Runtime")]
    [SerializeField] private int[] _spinnerValues;
    [SerializeField] private List<string> _spinnerResultHistory = new List<string>();

    private void Awake() {
        Debug.Assert(_loadingUI != null, "_loadingUI should not be null.");
        Debug.Assert(_gameplayUI != null, "_gameplayUI should not be null.");

        _postman = new UnityPostman(this, new Uri(_serverUrl), new Logger(Debug.unityLogger.logHandler), new JsonSerializer());

        _loadingUI.Initialize();

        _gameplayUI.OnSpinAction += Spin;
        _gameplayUI.Initialize();

        _loadingUI.Show();
        _postman.Send(_serverRouteGetSpinnerValues, (string err, SpinnerData data) => {
            _spinnerValues = data.SpinnerValues;

            _gameplayUI.Show();
            _loadingUI.Hide();
        });
    }

    private void Spin() {
        _gameplayUI.StartSpinAnimation();

        _postman.Send(_serverRouteGetSpinnerSpin, (string err, SpinnerData data) => {
            if (!string.IsNullOrEmpty(err)) {
                _gameplayUI.StopSpinAnimationWithError(err);

                _spinnerResultHistory.Add(err);
            }
            else {
                _gameplayUI.StopSpinAnimationWithResult(data.SpinnerValue);

                _spinnerResultHistory.Add(data.SpinnerValue.ToString());
            }
        });
    }
}

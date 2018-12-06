using UnityEngine;

public class SpinnerDriverUI : MonoBehaviour {

    [Header("VFX")]
    [SerializeField] private GameObject _VFX;

    [Header("Animation")]
    [SerializeField] private Vector3 _localRotationAnglesPing = new Vector3(0, 0, 5);
    [SerializeField] private Vector3 _localRotationAnglesPong = new Vector3(0, 0, -5);
    [SerializeField, Range(0f, 5f)] private float _pingPongAnimationMinIntervalInSeconds = .1f;
    [SerializeField, Range(0f, 5f)] private float _pingPongAnimationMaxIntervalInSeconds = 1f;

    [Header("Runtime")]
    [SerializeField] private Vector3 _initialRotationAngles;
    [SerializeField] private float _lastPingPongOccuredAtSeconds;
    [SerializeField] private float _currentPingPongIntervalInSeconds;
    [SerializeField] private bool _isAtPingState;

    private void OnValidate() {
        _pingPongAnimationMinIntervalInSeconds = Mathf.Min(
            _pingPongAnimationMinIntervalInSeconds,
            _pingPongAnimationMaxIntervalInSeconds
        );
    }

    public void Initialize() {

        Debug.AssertFormat(
            _pingPongAnimationMinIntervalInSeconds <= _pingPongAnimationMaxIntervalInSeconds,
            "_pingPongAnimationMinIntervalInSeconds({0}) should be less than or equal to _pingPongAnimationMaxIntervalInSeconds({1}).",
            _pingPongAnimationMinIntervalInSeconds, _pingPongAnimationMaxIntervalInSeconds
        );

        _initialRotationAngles = transform.rotation.eulerAngles;

        StopAnimation();
    }

    public void StartAnimation(int speedPercentage = 1) {
        _VFX.SetActive(true);

        _lastPingPongOccuredAtSeconds = 0;
        _isAtPingState = false;

        SetSpeedPercentage(speedPercentage);
    }

    public void ProgressAnimation(float timeInSeconds, float? speedPercentage = null) {

        if (speedPercentage.HasValue) {
            SetSpeedPercentage(speedPercentage.Value);
        }

        if (timeInSeconds - _lastPingPongOccuredAtSeconds > _currentPingPongIntervalInSeconds) {
            _lastPingPongOccuredAtSeconds = timeInSeconds;

            if (_isAtPingState) {
                transform.localEulerAngles = _localRotationAnglesPong;
            } else {
                transform.localEulerAngles = _localRotationAnglesPing;
            }

            _isAtPingState = !_isAtPingState;
        }
    }

    /// <summary>
    /// Sets the Ping Pong animation interval(speed) based on min & max interval settings.
    /// Zero: sets the animation to max interval (slowest).
    /// One:  sets the animation to min interval (fastest).
    /// </summary>
    /// <param name="percentage">[0,1]</param>
    public void SetSpeedPercentage(float percentage) {
        _currentPingPongIntervalInSeconds = Mathf.Lerp(
            _pingPongAnimationMinIntervalInSeconds,
            _pingPongAnimationMaxIntervalInSeconds,
            1 - Mathf.Clamp(percentage, 0, 1)
        );
    }

    public void StopAnimation() {
        enabled = false;

        _VFX.SetActive(false);

        transform.localEulerAngles = _initialRotationAngles;
    }
}

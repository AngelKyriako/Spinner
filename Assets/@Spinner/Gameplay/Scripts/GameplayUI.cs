using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : CanvasHideableUI {

    private enum State {
        Waiting,
        SpinningStarted,
        SpinningAtMaxSpeed,
        SpinningEnding,
        SpinningAtMinSpeedToCenterResult,
        SpinningEnded
    }

    private const float MIN_SPEED = .1f;
    private const float MAX_SPEED = 1f;

    private const float MIN_ACCELERATION = 1f;
    private const float MAX_ACCELERATION = 10f;

    public event Action OnSpinAction;

    [Header("UI")]
    [SerializeField] private Button _spinButton;
    // TODO: Prefab per spinner row to be pooled into a list dynamically
    // TODO: Settings for spin animation
    // TODO: Prefab for left & right arrows
    // TODO: Prefab for left & right VFX
    // TODO: Text for result message
    // TODO: Text for error message

    [Header("Settings")]
    [SerializeField, Range(MIN_SPEED, MAX_SPEED)] private float _accelerationStartSpeed = MIN_SPEED;
    [SerializeField, Range(MIN_SPEED, MAX_SPEED)] private float _accelerationEndSpeed = MAX_SPEED;
    [SerializeField, Range(MIN_ACCELERATION, MAX_ACCELERATION)] private float _accelerationRate = MIN_ACCELERATION;
    [SerializeField, Range(MIN_SPEED, MAX_SPEED)] private float _decelerationEndSpeed = MIN_SPEED;
    [SerializeField, Range(MIN_ACCELERATION, MAX_ACCELERATION)] private float _decelerationRate = MIN_ACCELERATION;

    [Header("Runtime")]
    [SerializeField] private State _state;
    [SerializeField] private float _currentSpeed;
    [SerializeField] private int[] _itemValues;

    public override void Initialize() {
        Debug.Assert(_spinButton != null, "_spinButton should not be null.");
        _spinButton.onClick.AddListener(TrySpin);

        Debug.AssertFormat(
            _accelerationStartSpeed <= _accelerationEndSpeed,
            "_accelerationStartSpeed({0}) should be less than or equal to _accelerationEndSpeed({1}).",
            _accelerationStartSpeed, _accelerationEndSpeed
        );
        Debug.AssertFormat(
            _decelerationEndSpeed <= _accelerationEndSpeed,
            "_decelerationEndSpeed({0}) should be less than or equal to _accelerationEndSpeed({1}).",
            _decelerationEndSpeed, _accelerationEndSpeed
        );

        enabled = false;

        base.Initialize();
    }

    public void Reset(int[] itemValues) {
        _itemValues = itemValues;

        // TODO: Setup item UI based on _itemValues
    }

    public override void Show() {
        _state = State.Waiting;
        _spinButton.gameObject.SetActive(true);

        Debug.Assert(
            _itemValues != null || _itemValues.Length == 0,
            "_itemValues should not be null or empty. Use the .Reset(int[]) call before Showing the UI."
        );
        // TODO: Asset item UIs are populated as well.

        base.Show();
    }


    private void TrySpin() {
        if (_state != State.Waiting) {
            return;
        }
        
        OnSpinAction?.Invoke();
    }
    
    public void StartSpinAnimation() {
        Debug.AssertFormat(
            _state == State.Waiting,
            "_state should be at {0} instead of {1} to start spinning animation.",
            State.Waiting, _state
        );

        _spinButton.gameObject.SetActive(false);

        _currentSpeed = _accelerationStartSpeed;
        _state = State.SpinningStarted;

        enabled = true;
    }

    public void StopSpinAnimationAtValue(int value) {
        StartCoroutine(StopSpinAnimationRoutine(value.ToString()));
    }

    public void StopSpinAnimationWithError(string error) {
        StartCoroutine(StopSpinAnimationRoutine(error));
    }

    private IEnumerator StopSpinAnimationRoutine(string result) {
        Debug.AssertFormat(
            _state == State.SpinningStarted || _state == State.SpinningAtMaxSpeed,
            "_state should be either at {0} or {1} instead of {2} to stop spinning animation.",
            State.SpinningStarted, State.SpinningAtMaxSpeed, _state
        );

        yield return new WaitUntil(() => _state == State.SpinningAtMaxSpeed);

        _state = State.SpinningEnding;

        yield return new WaitUntil(() => _state == State.SpinningEnded);

        // TODO: Display result with tween animation => onEnd set to Waiting state.
        _state = State.Waiting; // force to Waiting for now.
        _spinButton.gameObject.SetActive(true);
    }

    private void OnValidate() {
        _accelerationStartSpeed = Mathf.Min(_accelerationStartSpeed, _accelerationEndSpeed);
        _decelerationEndSpeed = Mathf.Min(_decelerationEndSpeed, _accelerationEndSpeed);
    }

    private void Update() {
        float delta = Time.deltaTime;

        // TODO: Apply spin items movement animation based on _currentSpeed
        // IFDO: A dispatcher table by State would be nice since I am overengineering anyway.

        switch (_state) {
            case State.SpinningStarted:
                _currentSpeed += delta * _accelerationRate;

                if (_currentSpeed >= _accelerationEndSpeed) {
                    _currentSpeed = _accelerationEndSpeed;

                    _state = State.SpinningAtMaxSpeed;
                }
                break;
            case State.SpinningEnding:
                _currentSpeed -= delta * _decelerationRate;

                if (_currentSpeed <= _decelerationEndSpeed) {
                    _currentSpeed = _decelerationEndSpeed;

                    _state = State.SpinningAtMinSpeedToCenterResult;
                }
                break;
            case State.SpinningAtMaxSpeed:
                // Let it spin.
                break;
            case State.SpinningAtMinSpeedToCenterResult:
                // TODO: Distance check between: center transform & result item transform.
                //if () {
                    _currentSpeed = 0;

                    _state = State.SpinningEnded;
                    enabled = false;
                //}
                break;
            default:
                throw new InvalidProgramException(string.Format(
                    "MonoBehavior::Update should not be called at state: {0}. Ensure that the component is disabled at that state.",
                    _state
                ));
        }
    }
}

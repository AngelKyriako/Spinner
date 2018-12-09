using System;
using System.Collections;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

/// <summary>
/// IFDO: We could update visuals and VFX, based on _currentSpeed and min/max acceleration/deceleration speeds.
/// IFDO: A State/Strategy pattern would cleanup the code a lot.
/// IFDO: Abstracting out VFX and animation sequence related code to other components should help to.
/// </summary>
public class SpinnerUI : CanvasGroupHideableUI, ISpinnerUI {

    private enum State {
        Waiting,
        SpinningStarted,
        SpinningAtMaxSpeed,
        SpinningEnding,
        SpinningAtMinSpeedToCenterResult,
        SpinningEnded
    }

    private enum SpinAnimation {
        Discrete,
        continuous
    }

    private const float MIN_SPEED = .1f;
    private const float MAX_SPEED = 10f;

    private const float MIN_ACCELERATION = .1f;
    private const float MAX_ACCELERATION = 10f;

    public event Action OnSpinAction;

    [SerializeField] private TweenAnimationGroup _spinStartedAnimation;
    [SerializeField] private TweenAnimationGroup _spinEndedAnimation;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI[] _itemTexts;
    [SerializeField] private CanvasGroupHideableUI _isFrozenIndicator;
    [SerializeField] private SpinnerDriverUI[] _drivers;
    [SerializeField] private CanvasGroupHideableUI _actionButtonContainer;
    [SerializeField] private Button _actionButton;
    [SerializeField] private PopupTextUI _actionResultPopupUI;
    [SerializeField] private PopupTextUI _errorPopupUI;

    [Header("VFX")]
    [SerializeField] private int _goodResultVFXPointsThreshold = 10000;
    [SerializeField] private ParticleSystem _goodResultVFX;
    [SerializeField] private Material _itemTextDefaultFontMaterial;
    [SerializeField] private Material _itemTextMaxSpinSpeedFontMaterial;

    [Header("Spin Settings")]
    [SerializeField, Range(MIN_SPEED, MAX_SPEED)] private float _accelerationStartSpeed = MIN_SPEED;
    [SerializeField, Range(MIN_SPEED, MAX_SPEED)] private float _accelerationEndSpeed = MAX_SPEED;
    [SerializeField, Range(MIN_ACCELERATION, MAX_ACCELERATION)] private float _accelerationRate = MAX_ACCELERATION;
    [SerializeField, Range(MIN_SPEED, MAX_SPEED)] private float _decelerationEndSpeed = MIN_SPEED;
    [SerializeField, Range(MIN_ACCELERATION, MAX_ACCELERATION)] private float _decelerationRate = MAX_ACCELERATION;
    [SerializeField] private SpinAnimation _spinAnimation;
    [Header("Spin Settings: Discrete")]
    [SerializeField, Range(.1f, 10)] private float _itemDiscreteStepDistanceDelta = .5f; // Affects the frequency of the discrete scroll effect.
    [Header("Spin Settings: continuous")]
    [SerializeField, Range(.1f, 500)] private float _itemContinuousStepDistanceSpeed = 100f; // Affects the delta movement of the continuous scroll effect.
    [SerializeField] private Vector3 _itemContinuousTopLocalOverflowPosition;
    [SerializeField] private Vector3 _itemContinuousBottomLocalResetPosition;
    [SerializeField] private Vector3 _itemContinuousCenterLocalPosition;
    [SerializeField] private float _itemContinuousStopDistanceThreshold = 50f;

    [Header("Runtime")]
    [SerializeField] private int[] _itemValues;
    private int? _itemResult;

    [SerializeField] private State _state;
    [SerializeField] private float _currentSpeed;
    [SerializeField] private float _lastItemStepDistance;
    [SerializeField] private float _currentDistance;
    [SerializeField] private int _itemValuesIndexOfTopUIItem;

    private void OnValidate() {
        _accelerationStartSpeed = Mathf.Min(_accelerationStartSpeed, _accelerationEndSpeed);
        _decelerationEndSpeed = Mathf.Min(_decelerationEndSpeed, _accelerationEndSpeed);
    }

    public void Initialize(PopupTextUI errorPopup) {
        _errorPopupUI = errorPopup;

        Initialize();
    }

    public override void Initialize() {
        Debug.Assert(_errorPopupUI != null, "_errorPopupUI should not be null. Make sure you are using the Initialize(PopupTextUI) overload.");
        Debug.Assert(_isFrozenIndicator != null, "_isFrozenIndicator should not be null.");
        Debug.Assert(_drivers != null, "_drivers should not be null.");
        Debug.Assert(_actionButtonContainer != null, "_actionButtonContainer should not be null.");
        Debug.Assert(_actionResultPopupUI != null, "_actionResultPopupUI should not be null.");

        Debug.Assert(_actionButton != null, "_spinButton should not be null.");
        _actionButton.onClick.AddListener(TrySpin);

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

        _isFrozenIndicator.Initialize();
        foreach (SpinnerDriverUI driver in _drivers) {
            driver.Initialize();
        }

        ResetItemTextsFontMaterial(_itemTextDefaultFontMaterial);

        _actionButtonContainer.Initialize();
        _actionResultPopupUI.Initialize();

        enabled = false;

        base.Initialize();
    }

    public void Reset(int[] itemValues) {
        _itemValues = itemValues;

        _itemValuesIndexOfTopUIItem = 0;
        ResetItemTexts();
    }

    public override void Show(Action onDone = null) {
        _state = State.Waiting;

        _isFrozenIndicator.Show();
        _actionButtonContainer.Show();

        Debug.Assert(
            _itemValues != null || _itemValues.Length == 0,
            "_itemValues should not be null or empty. Use the .Reset(int[]) call before Showing the UI."
        );

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

        _spinStartedAnimation.Animate(this, _canvasGroup);

        _isFrozenIndicator.Hide();
        _actionButtonContainer.Hide();

        foreach (SpinnerDriverUI spinDriver in _drivers) {
            spinDriver.StartAnimation(0);
        }

        _currentSpeed = _accelerationStartSpeed;

        _currentDistance = 0;
        _lastItemStepDistance = 0;

        _itemResult = null;

        _state = State.SpinningStarted;

        enabled = true;
    }

    public void StopSpinAnimationAtValue(int value) {
        _itemResult = value;

        StartCoroutine(StopSpinAnimationRoutine(
            _actionResultPopupUI,
            value.ToString(),
            value >= _goodResultVFXPointsThreshold ? _goodResultVFX : null
        ));
    }

    public void StopSpinAnimationWithError(string error) {
        StartCoroutine(StopSpinAnimationRoutine(_errorPopupUI, error));
    }

    private IEnumerator StopSpinAnimationRoutine(PopupTextUI popupUI, string textToDisplay, ParticleSystem vfxToDisplay = null) {
        Debug.AssertFormat(
            _state == State.SpinningStarted || _state == State.SpinningAtMaxSpeed,
            "_state should be either at {0} or {1} instead of {2} to stop spinning animation.",
            State.SpinningStarted, State.SpinningAtMaxSpeed, _state
        );

        yield return new WaitUntil(() => _state == State.SpinningAtMaxSpeed);

        _state = State.SpinningEnding;
        foreach (SpinnerDriverUI spinDriver in _drivers) {
            spinDriver.SetSpeedPercentage(.5f);
        }

        yield return new WaitUntil(() => _state == State.SpinningEnded);
        foreach (SpinnerDriverUI spinDriver in _drivers) {
            spinDriver.StopAnimation();
        }

        _spinEndedAnimation.Animate(this, _canvasGroup, () => {
            //_isFrozenIndicator.Show();
        });
        _isFrozenIndicator.Show();

        if (vfxToDisplay != null) {
            vfxToDisplay.Play();
        }
        popupUI.SetTextAndShow(textToDisplay, () => {

            popupUI.Hide(() => {

                _state = State.Waiting;
                _actionButtonContainer.Show(() => {
                    //_state = State.Waiting;
                });

            });
        });
    }

    private void Update() {
        float delta = Time.deltaTime;

        ScrollDownItems(delta);

        foreach (SpinnerDriverUI spinDriver in _drivers) {
            spinDriver.TryProgressAnimation(Time.time);
        }

        switch (_state) {
            case State.SpinningStarted:
                _currentSpeed += delta * _accelerationRate;

                if (_currentSpeed >= _accelerationEndSpeed) {
                    _currentSpeed = _accelerationEndSpeed;

                    _state = State.SpinningAtMaxSpeed;

                    foreach (SpinnerDriverUI spinDriver in _drivers) {
                        spinDriver.SetSpeedPercentage(1);
                    }
                    ResetItemTextsFontMaterial(_itemTextMaxSpinSpeedFontMaterial);
                }
                break;
            case State.SpinningEnding:
                _currentSpeed -= delta * _decelerationRate;

                if (_currentSpeed <= _decelerationEndSpeed) {
                    _currentSpeed = _decelerationEndSpeed;

                    _state = State.SpinningAtMinSpeedToCenterResult;

                    ResetItemTextsFontMaterial(_itemTextDefaultFontMaterial);
                    foreach (SpinnerDriverUI spinDriver in _drivers) {
                        spinDriver.SetSpeedPercentage(.01f);
                    }
                }
                break;
            case State.SpinningAtMaxSpeed:
                // let it spin.
                break;
            case State.SpinningAtMinSpeedToCenterResult:
                if (IsResultAtCenterOrNotExists()) {
                    _currentSpeed = 0;

                    _state = State.SpinningEnded;
                    enabled = false;
                }
                break;
            default:
                throw new InvalidProgramException(string.Format(
                    "MonoBehavior::Update should not be called at state: {0}. Ensure that the component is disabled at that state.",
                    _state
                ));
        }
    }

    private void ScrollDownItems(float delta) {
        _currentDistance += delta * _currentSpeed;

        float deltaDistance = _currentDistance - _lastItemStepDistance;

        switch (_spinAnimation) {
            case SpinAnimation.continuous:
                Vector3 pos;
                Vector3 deltaPos = Vector3.up * deltaDistance * _itemContinuousStepDistanceSpeed;
                foreach (TMP_Text itemText in _itemTexts) {
                    itemText.transform.localPosition += deltaPos;

                    if (itemText.transform.localPosition.y > _itemContinuousTopLocalOverflowPosition.y) {
                        ScrollDownTopItemIndex();

                        Vector3 overflowOffset = Vector3.up * (itemText.transform.localPosition.y - _itemContinuousTopLocalOverflowPosition.y);

                        itemText.transform.localPosition = _itemContinuousBottomLocalResetPosition + overflowOffset;
                        itemText.text = GetItemValueAtIndexCircular(_itemValuesIndexOfTopUIItem + _itemTexts.Length - 1).ToString();
                    }
                }

                _lastItemStepDistance = _currentDistance;
                break;
            case SpinAnimation.Discrete:
            default:
                if (deltaDistance >= _itemDiscreteStepDistanceDelta) {
                    ScrollDownTopItemIndex();

                    _lastItemStepDistance = _currentDistance;
                }

                ResetItemTexts();
                break;
        }
    }

    private void ResetItemTexts() {
        for (int i = 0; i < _itemTexts.Length; ++i) {
            _itemTexts[i].text = GetItemValueAtIndexCircular(_itemValuesIndexOfTopUIItem + i).ToString();
        }
    }

    private void ResetItemTextsFontMaterial(Material fontMaterial) {
        for (int i = 0; i < _itemTexts.Length; ++i) {
            _itemTexts[i].fontMaterial = fontMaterial;
        }
    }

    private void ScrollDownTopItemIndex() {
        if (_itemValuesIndexOfTopUIItem == _itemValues.Length - 1) {
            _itemValuesIndexOfTopUIItem = 0;
        } else {
            ++_itemValuesIndexOfTopUIItem;
        }
    }

    private int GetItemValueAtIndexCircular(int index) {
        if (index >= _itemValues.Length) {
            index -= _itemValues.Length;
        }
        else if (index < 0) {
            index += _itemValues.Length;
        }

        return _itemValues[index];
    }

    private bool IsResultAtCenterOrNotExists() {
        if (!_itemResult.HasValue || _itemTexts.Length == 0) {
            return true;
        }

        int midIndex = _itemTexts.Length / 2;

        bool isAtCenterDiscrete = GetItemValueAtIndexCircular(_itemValuesIndexOfTopUIItem + midIndex) == _itemResult.Value;

        switch (_spinAnimation) {
            case SpinAnimation.Discrete:
                return isAtCenterDiscrete;
            case SpinAnimation.continuous:
            default:
                if (isAtCenterDiscrete) {
                    float distanceFromCenter = Vector2.Distance(_itemContinuousCenterLocalPosition, _itemTexts[midIndex].transform.localPosition);

                    Debug.Log("distanceFromCenter: " + distanceFromCenter);

                    return distanceFromCenter < _itemContinuousStopDistanceThreshold;
                }
                return false;
        }
    }

    [ContextMenu("Reset Continous Settings By UI")]
    private void ResetContinousSettingsByUI() {
        Vector3 itemTextScrollTopLocalPosition = _itemTexts.First().transform.localPosition;
        Vector3 itemTextScrollBottomLocalPosition = _itemTexts.Last().transform.localPosition;

        float itemTextHeight = (itemTextScrollTopLocalPosition.y - itemTextScrollBottomLocalPosition.y) / (_itemTexts.Length - 1);
        Vector3 itemTextHeightOffset = Vector3.up * itemTextHeight;

        _itemContinuousTopLocalOverflowPosition = itemTextScrollTopLocalPosition + itemTextHeightOffset;
        _itemContinuousBottomLocalResetPosition = itemTextScrollBottomLocalPosition;
        _itemContinuousCenterLocalPosition = Vector3.zero;
        _itemContinuousStopDistanceThreshold = itemTextHeight;
    }

}

public static class SpinnerUIExtensions {

    public static void ResetAndShow(this SpinnerUI spinner, int[] values, Action onDone = null) {
        spinner.Reset(values);
        spinner.Show(onDone);
    }
}

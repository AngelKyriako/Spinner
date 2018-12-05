using System;

using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : CanvasHideableUI {

    public event Action OnSpinAction;

    [SerializeField] private Button _spinButton;

    // TODO: Prefab per spinner row to be pooled into a list dynamically
    // TODO: Settings for spin animation
    // TODO: Prefab for left & right arrows
    // TODO: Prefab for left & right VFX
    // TODO: Text for result message
    // TODO: Text for error message

    public override void Initialize() {
        Debug.Assert(_spinButton != null, "_spinButton should not be null.");
        _spinButton.onClick.AddListener(TrySpin);

        enabled = false;

        base.Initialize();
    }

    public override void Show() {

        // TODO: Setup based on _spinner.Values

        base.Show();
    }

    public bool IsSpinning {
        get { return enabled; }
    }

    private void TrySpin() {
        if (IsSpinning) {
            return;
        }

        OnSpinAction?.Invoke();

        StartSpinAnimation();
    }

    public void StartSpinAnimation() {
        enabled = true;
        // TODO: initialize values like speed, acceleration to be used in Update.etc
    }

    private void Update() {
        // TODO: Apply spin animation
        // TODO: disable script when done
    }

    public void StopSpinAnimationWithResult(int value) {
        
    }

    public void StopSpinAnimationWithError(string error) {

    }
}

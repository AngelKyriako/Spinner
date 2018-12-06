using System;

using UnityEngine;

public class GameplayUI : CanvasHideableUI {

    [SerializeField] private SpinnerUI _spinner;

    public ISpinnerUI Spinner { get { return _spinner; } }

    public void Initialize(PopupTextUI errorPopup) {
        Debug.Assert(_spinner != null, "_spinner should not be null.");

        _spinner.Initialize(errorPopup);

        base.Initialize();
    }

    public override void Show(Action onDone = null) {
        base.Show();

        _spinner.Show(onDone);
    }

    public override void Hide(Action onDone = null) {
        _spinner.Hide(() => {
            base.Hide();

            onDone?.Invoke();
        });
    }
}

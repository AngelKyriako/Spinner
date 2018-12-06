using System;

using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupHideableUI : MonoBehaviour, IHideableUI {

    [SerializeField] private CanvasGroup _canvasGroup;

    public virtual void Initialize() {
        if (_canvasGroup == null) {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        Debug.Assert(_canvasGroup != null, "_canvasGroup should not be null, either add a component or attach an existing one via the inspector.");

        HideInternal();
    }

    public virtual bool IsShowing { get { return _canvasGroup.alpha != 0; } }

    public virtual void Show(Action onDone = null) {
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;

        onDone?.Invoke();
    }

    public virtual void Hide(Action onDone = null) {
        HideInternal();

        onDone?.Invoke();
    }

    private void HideInternal() {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
    }
}

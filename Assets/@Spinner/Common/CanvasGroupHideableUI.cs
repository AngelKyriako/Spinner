using System;

using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupHideableUI : MonoBehaviour, IHideableUI {

    [SerializeField] protected CanvasGroup _canvasGroup;

    [SerializeField] private TweenAnimationGroup _showAnimation;
    [SerializeField] private TweenAnimationGroup _hideAnimation;

    public virtual void Initialize() {
        if (_canvasGroup == null) {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        Debug.Assert(_canvasGroup != null, "_canvasGroup should not be null, either add a component or attach an existing one via the inspector.");

        HideInternal();
    }

    public virtual bool IsShowing { get { return _canvasGroup.alpha != 0; } }

    public virtual void Show(Action onDone = null) {
        Action onDoneInternal = () => {
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;

            onDone?.Invoke();
        };

        if (_showAnimation != null) {
            _showAnimation.Animate(this, _canvasGroup, onDoneInternal);
        }
        else {
            onDoneInternal();
        }
    }

    public virtual void Hide(Action onDone = null) {
        Action onDoneInternal = () => {
            HideInternal();
            onDone?.Invoke();
        };

        if (_hideAnimation != null) {
            _hideAnimation.Animate(this, _canvasGroup, onDoneInternal);
        } else {
            onDoneInternal();
        }
    }

    private void HideInternal() {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
    }
}

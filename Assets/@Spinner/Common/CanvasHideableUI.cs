using System;

using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasHideableUI : MonoBehaviour, IHideableUI {

    [SerializeField] private Canvas _canvas;

    public virtual void Initialize() {
        if (_canvas == null) {
            _canvas = GetComponent<Canvas>();
        }
        Debug.Assert(_canvas != null, "_canvas should not be null, either add a component or attach an existing one via the inspector.");

        _canvas.enabled = false;
    }

    public virtual bool IsShowing { get { return _canvas.enabled; } }

    public virtual void Show(Action onDone = null) {
        _canvas.enabled = true;

        onDone?.Invoke();
    }

    public virtual void Hide(Action onDone = null) {
        _canvas.enabled = false;

        onDone?.Invoke();
    }
}

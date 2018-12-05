using UnityEngine;

public class CanvasHideableUI : MonoBehaviour {

    [SerializeField] private Canvas _canvas;

    public virtual void Initialize() {
        if (_canvas == null) {
            _canvas = GetComponent<Canvas>();
        }
        Debug.Assert(_canvas != null, "_canvas should not be null, either add a component or attach an existing one via the inspector.");

        _canvas.enabled = false;
    }

    public virtual bool IsShowing { get { return _canvas.enabled; } }

    public virtual void Show() {
        _canvas.enabled = true;
    }

    public virtual void Hide() {
        _canvas.enabled = false;
    }
}

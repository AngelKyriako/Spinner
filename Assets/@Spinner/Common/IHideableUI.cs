using System;

public interface IHideableUI {
    bool IsShowing { get; }

    void Hide(Action onDone = null);
    void Show(Action onDone = null);
}
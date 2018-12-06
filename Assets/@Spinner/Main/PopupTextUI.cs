using System;

using UnityEngine;

using TMPro;

public class PopupTextUI : CanvasGroupHideableUI {

    [SerializeField] private TMP_Text _text;

    public override void Initialize() {
        Debug.Assert(_text != null, "_text should not be null.");

        base.Initialize();
    }

    public void SetText(string text) {
        _text.text = text;
    }
}

public static class PopupTextUIExtensions {

    public static void SetTextAndShow(this PopupTextUI popup, string text, Action onDone = null) {
        popup.SetText(text);
        popup.Show(onDone);
    }
}

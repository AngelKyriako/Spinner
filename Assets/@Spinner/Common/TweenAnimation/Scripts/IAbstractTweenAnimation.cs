using System;

using UnityEngine;

public interface ITweenAnimation {
    void Animate(MonoBehaviour routineInvoker, Transform transform, Action onDone = null);
    void Animate(MonoBehaviour routineInvoker, CanvasGroup canvasGroup, Action onDone = null);
}

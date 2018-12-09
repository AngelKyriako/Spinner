using System;
using UnityEngine;

public abstract class AbstractTweenAnimation : ScriptableObject, ITweenAnimation {

    public abstract void Animate(MonoBehaviour routineInvoker, Transform transform, Action onDone = null);
    public abstract void Animate(MonoBehaviour routineInvoker, CanvasGroup canvasGroup, Action onDone = null);
}

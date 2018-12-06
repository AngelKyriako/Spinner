﻿using System;
using System.Collections;

using UnityEngine;

using DG.Tweening;

public abstract class AbstractTweenAnimation : ScriptableObject {

    [SerializeField] protected float _durationInSeconds;
    [SerializeField] private Ease _ease = Ease.Unset;

    protected virtual Tweener FireAnimation(CanvasGroup canvasGroup) {
        throw new InvalidOperationException(string.Format("CanvasGroup Animation not supported by {0}.", name));
    }

    protected virtual Tweener FireAnimation(Transform transform) {
        throw new InvalidOperationException(string.Format("Transform Animation not supported by {0}.", name));
    }

    public void Animate(MonoBehaviour behaviour, Action onDone) {
        behaviour.StartCoroutine(AnimationRoutine(FireAnimation(behaviour.transform), onDone));
    }

    public void Animate(MonoBehaviour behaviour, CanvasGroup canvasGroup, Action onDone) {
        behaviour.StartCoroutine(AnimationRoutine(FireAnimation(canvasGroup), onDone));
    }

    private IEnumerator AnimationRoutine(Tweener tweener, Action onDone) {
        tweener.SetEase(_ease);

        yield return new WaitUntil(() => !tweener.IsPlaying());

        onDone?.Invoke();
    }

}

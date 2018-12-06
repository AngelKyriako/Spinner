using System;
using System.Collections;

using UnityEngine;

[CreateAssetMenu(menuName = "Spinner/Tween Animation/Idle")]
public class IdleTweenAnimation : AbstractTweenAnimation {

    public override void Animate(MonoBehaviour routineInvocator, Transform transform, Action onDone = null) {
        routineInvocator.StartCoroutine(CallbackAfterSeconds(_durationInSeconds, onDone));
    }

    public override void Animate(MonoBehaviour routineInvocator, CanvasGroup canvasGroup, Action onDone = null) {
        routineInvocator.StartCoroutine(CallbackAfterSeconds(_durationInSeconds, onDone));
    }

    private IEnumerator CallbackAfterSeconds(float seconds, Action onDone) {
        yield return new WaitForSeconds(seconds);

        onDone?.Invoke();
    }
}

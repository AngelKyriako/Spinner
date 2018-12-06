using System;

using UnityEngine;

[CreateAssetMenu(menuName = "Spinner/Tween Animation/Group")]
public class TweenAnimationGroup : ScriptableObject, ITweenAnimation {

    public enum Type {
        Parallel,
        Series
    }

    [SerializeField] private Type _type;
    [SerializeField] private AbstractTweenAnimation[] _tweenAnimations;

    public void Animate(MonoBehaviour routineInvoker, CanvasGroup canvasGroup, Action onDone = null) {
        Animate(routineInvoker, null, canvasGroup, onDone);
    }

    public void Animate(MonoBehaviour routineInvoker, Transform transform, Action onDone = null) {
        Animate(routineInvoker, transform, null, onDone);
    }

    private void Animate(MonoBehaviour routineInvoker, Transform transform, CanvasGroup canvasGroup, Action onDone) {
        switch(_type) {
            case Type.Parallel:
                AnimateParallel(routineInvoker, transform, canvasGroup, onDone);
                break;
            case Type.Series:
                AnimateSeries(0, routineInvoker, transform, canvasGroup, onDone);
                break;
            default:
                throw new InvalidOperationException(string.Format("Unhandled TweenAnimationGroup type: {0}", _type));
        }
    }
    
    private void AnimateParallel(MonoBehaviour routineInvoker, Transform transform, CanvasGroup canvasGroup, Action onDone) {
        if (_tweenAnimations.Length == 0) {
            onDone?.Invoke();
            return;
        }

        int animationsFinished = 0;
        Action onDoneInternal = () => {
            if (++animationsFinished == _tweenAnimations.Length) {
                onDone?.Invoke();
            }
        };

        foreach (AbstractTweenAnimation animation in _tweenAnimations) {
            RunAnimation(animation, routineInvoker, transform, canvasGroup, onDoneInternal);
        }
    }

    private void AnimateSeries(int currentAnimationIndex, MonoBehaviour routineInvoker, Transform transform, CanvasGroup canvasGroup, Action onDone) {
        if (currentAnimationIndex == _tweenAnimations.Length) {
            onDone?.Invoke();
            return;
        }

        RunAnimation(
            _tweenAnimations[currentAnimationIndex],
            routineInvoker,
            transform,
            canvasGroup,
            () => AnimateSeries(currentAnimationIndex + 1, routineInvoker, transform, canvasGroup, onDone)
        );
    }

    private void RunAnimation(AbstractTweenAnimation animation, MonoBehaviour routineInvoker, Transform transform, CanvasGroup canvasGroup, Action onDone) {
        if (canvasGroup != null) {
            animation.Animate(routineInvoker, canvasGroup, onDone);
        } else {
            animation.Animate(routineInvoker, transform, onDone);
        }
    }
}

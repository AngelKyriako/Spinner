using DG.Tweening;

using UnityEngine;

[CreateAssetMenu(menuName = "Spinner/Tween Animation/Fade Between Endpoints")]
public class FadeTweenBetweenEndpointsAnimation : AbstractTweenAnimation {

    [SerializeField] private float _fromAlpha;
    [SerializeField] private float _untilAlpha;

    protected override Tweener FireAnimation(CanvasGroup canvasGroup) {
        canvasGroup.alpha = _fromAlpha;

        return canvasGroup.DOFade(_untilAlpha, _durationInSeconds);
    }

}

using DG.Tweening;

using UnityEngine;

[CreateAssetMenu(menuName = "Spinner/Tween Animation/Move Between Endpoints")]
public class MoveBetweenEndpointsTweenAnimation : AbstractTweenAnimation {

    [SerializeField] private Vector3 _fromLocalPosition;
    [SerializeField] private Vector3 _untilLocaPosition;

    protected override Tweener FireAnimation(Transform transform) {
        transform.position = _fromLocalPosition;

        return transform.DOLocalMove(_untilLocaPosition, _durationInSeconds);
    }

    protected override Tweener FireAnimation(CanvasGroup canvasGroup) {
        return FireAnimation(canvasGroup.transform);
    }

}

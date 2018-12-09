using DG.Tweening;

using UnityEngine;

[CreateAssetMenu(menuName = "Spinner/Tween Animation/Scale Between Endpoints")]
public class ScaleBetweenEndpointsTweenAnimation : BaseTweenAnimation {

    [SerializeField] private Vector3 _fromLocalScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 _untilLocalScale = new Vector3(1f, 1f, 1f);

    protected override Tweener FireAnimation(Transform transform) {
        transform.localScale = _fromLocalScale;

        return transform.DOScale(_untilLocalScale, _durationInSeconds);
    }

    protected override Tweener FireAnimation(CanvasGroup canvasGroup) {
        return FireAnimation(canvasGroup.transform);
    }
}

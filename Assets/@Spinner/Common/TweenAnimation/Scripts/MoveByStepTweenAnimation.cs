using UnityEngine;

using DG.Tweening;

[CreateAssetMenu(menuName = "Spinner/Tween Animation/Move By Step")]
public class MoveByStepTweenAnimation : AbstractTweenAnimation {

    [SerializeField] private Vector3 _step;

    protected override Tweener FireAnimation(Transform transform) {
        return transform.DOBlendableMoveBy(_step, _durationInSeconds);
    }
}

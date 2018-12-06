using UnityEngine;

using DG.Tweening;

[CreateAssetMenu(menuName = "Spinner/Tween Animation/Scale By Step")]
public class ScaleByStepTweenAnimation : AbstractTweenAnimation {

    [SerializeField] private Vector3 _step;

    protected override Tweener FireAnimation(Transform transform) {
        return transform.DOBlendableScaleBy(_step, _durationInSeconds);
    }
}

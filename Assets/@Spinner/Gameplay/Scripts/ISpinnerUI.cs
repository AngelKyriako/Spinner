using System;

public interface ISpinnerUI {
    event Action OnSpinAction;

    void Reset(int[] itemValues);
    void StartSpinAnimation();
    void StopSpinAnimationAtValue(int value);
    void StopSpinAnimationWithError(string error);
}
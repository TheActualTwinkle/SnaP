using UnityEngine;

public class CombinationCard : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    
    private static readonly int Enable = Animator.StringToHash("Enable");
    private static readonly int Disable = Animator.StringToHash("Disable");

    public void EnableAnimation()
    {
        _animator.ResetTrigger(Disable);
        _animator.SetTrigger(Enable);
    }

    public void DisableAnimation()
    {
        _animator.ResetTrigger(Enable);
        _animator.SetTrigger(Disable);
    }
}

using UnityEngine;

public static class AnimatorExtensions
{
    public static void ResetAllTriggers(this Animator animator)
    {
        foreach (AnimatorControllerParameter trigger in animator.parameters)
        {
            if (trigger.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(trigger.name);
            }
        }
    }
}
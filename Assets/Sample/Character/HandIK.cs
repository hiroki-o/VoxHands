using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HandIK : MonoBehaviour
{
    private Animator anim;

    [Range(0f, 1.0f)]
    public float leftArmWeight;

    [Range(0f, 1.0f)]
    public float rightArmWeight;

    public Transform leftArmTarget;
    public Transform rightArmTarget;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK (int layerIndex)
    {
        if (leftArmTarget != null)
        {
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftArmTarget.position);                       
            anim.SetIKRotation(AvatarIKGoal.LeftHand, leftArmTarget.rotation);                       
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftArmWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftArmWeight);
        }
        
        if (rightArmTarget != null)
        {
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightArmTarget.position);                       
            anim.SetIKRotation(AvatarIKGoal.RightHand, rightArmTarget.rotation);                       
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightArmWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightArmWeight);
        }
    }
}

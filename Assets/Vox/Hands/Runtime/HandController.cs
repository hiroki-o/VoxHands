using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Vox.Hands
{
    /*
     * Asset stores key control config.
     */
    [ExecuteInEditMode]
    [RequireComponent(typeof(Animator))]
    public class HandController : MonoBehaviour
    {
        [SerializeField] private HandType m_handType = HandType.LeftHand;
        [SerializeField] private HandPoseData m_handPoseData;

        private HandRuntimeControl m_runtimeControl;
        private Avatar m_avatar;

        private void Awake()
        {
            InitializeRuntimeControl();
        }

        public void InitializeRuntimeControl()
        {
            var animator = GetComponent<Animator>();
            m_avatar = animator.avatar;

            if (m_avatar == null)
            {
                Debug.LogError("HandController:Animator component doesn't have a valid avatar configured.");
            }
            
            m_runtimeControl = new HandRuntimeControl(gameObject, m_avatar, m_handType);
        }

        private void LateUpdate()
        {
            m_runtimeControl?.UpdateHandPose(ref m_handPoseData);
        }
    }
}
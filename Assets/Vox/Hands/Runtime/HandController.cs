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
        [SerializeField] private HandPosePresetsAsset m_preset;
        [SerializeField] private HandType m_handType = HandType.LeftHand;
        [SerializeField] private HandPoseData m_handPoseData;

        private HandRuntimeControl m_runtimeControl;
        private Avatar m_avatar;

        private HandPoseData m_basePose;

        public HandType Hand
        {
            get { return m_handType; }
            set
            {
                m_handType = value;
                InitializeRuntimeControl();
            }
        }

        public HandPosePresetsAsset Preset
        {
            get
            {
                return m_preset;
            }
            set
            {
                m_preset = value;
            }
        }

        private void Awake()
        {
            InitializeRuntimeControl();
            #if UNITY_EDITOR
            
            #endif
        }

        /// <summary>
        /// Set current hand pose as base pose for transformation. 
        /// </summary>
        public void SetCurrentAsBasePose()
        {
            m_basePose = m_handPoseData;
        }        
        
        /// <summary>
        /// Set base hand pose from currently assigned preset 
        /// </summary>
        public void SetBasePoseFromCurrentPreset(string poseName)
        {
            m_basePose = m_preset[poseName];
        }        

        /// <summary>
        /// Set base hand pose from currently assigned preset 
        /// </summary>
        public void SetBasePoseFromCurrentPreset(int index)
        {
            m_basePose = m_preset[index];
        }        
        
        /// <summary>
        /// Set base hand pose.
        /// </summary>
        public void SetBasePose(ref HandPoseData pose)
        {
            m_basePose = pose;
        }        
        
        
        /// <summary>
        /// Set hand pose.  
        /// </summary>
        /// <param name="poseName">name of pose in RuntimeHandPosePresetAsset.</param>
        /// <param name="t">lerp value of hand pose. 0.0f=base pose, 1.0f=poseName pose</param>
        public void SetHandPose(string poseName, float t = 1.0f)
        {
            var pose = m_preset[poseName];
            SetHandPose(ref pose, t);
        }

        /// <summary>
        /// Set hand pose.  
        /// </summary>
        /// <param name="poseIndex">index of pose in RuntimeHandPosePresetAsset.</param>
        /// <param name="t">lerp value of hand pose. 0.0f=base pose, 1.0f=poseName pose</param>
        public void SetHandPose(int poseIndex, float t = 1.0f)
        {
            var pose = m_preset[poseIndex];
            SetHandPose(ref pose, t);
        }
        
        /// <summary>
        /// Set hand pose.  
        /// </summary>
        /// <param name="data">hand pose</param>
        /// <param name="t">lerp value of hand pose. 0.0f=base pose, 1.0f=data</param>
        public void SetHandPose(ref HandPoseData data, float t = 1.0f)
        {
            if (m_runtimeControl == null)
            {
                InitializeRuntimeControl();
            }
            LerpHandPose(ref m_handPoseData, ref m_basePose, ref data, t);
        }

        private static void LerpHandPose(ref HandPoseData data, ref HandPoseData src, ref HandPoseData dst, float t)
        {
            if (t == 0f)
            {
                data = src;
            } else if (t == 1.0f)
            {
                data = dst;
            }
            else
            {
                for (var i = 0; i < HandPoseData.HumanFingerCount; ++i)
                {
                    data[i] = new FingerPoseData
                    {
                        muscle1 = Mathf.Lerp(src[i].muscle1, dst[i].muscle1, t),
                        muscle2 = Mathf.Lerp(src[i].muscle2, dst[i].muscle2, t),
                        muscle3 = Mathf.Lerp(src[i].muscle3, dst[i].muscle3, t),
                        spread = Mathf.Lerp(src[i].spread, dst[i].spread, t)
                    };
                }
            }
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
            if (m_runtimeControl != null)
            {
                m_runtimeControl.UpdateHandPose(ref m_handPoseData);                
            }
        }
    }
}
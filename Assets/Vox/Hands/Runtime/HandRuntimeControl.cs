using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Vox.Hands
{
    public enum HandType
    {
        LeftHand,
        RightHand
    }

    [Serializable]
    public struct FingerPoseData
    {
        [Range(-1f, 1f)]
        public float spread;
        [Range(-1f, 1f)]
        public float muscle1;
        [Range(-1f, 1f)]
        public float muscle2;
        [Range(-1f, 1f)]
        public float muscle3;
        
        public void WeightedAddPose(float weight, ref FingerPoseData data)
        {
            spread += weight * data.spread;
            muscle1 += weight * data.muscle1;
            muscle2 += weight * data.muscle2;
            muscle3 += weight * data.muscle3;
        }
    }

    [Serializable]
    public struct HandPoseData
    {
        public FingerPoseData thumb;
        public FingerPoseData index;
        public FingerPoseData middle;
        public FingerPoseData ring;
        public FingerPoseData little;

        public const int HumanFingerCount = 5;
        
        public FingerPoseData this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= HumanFingerCount)
                {
                    throw new IndexOutOfRangeException();
                }

                switch (idx)
                {
                    case 0:
                        return thumb;
                    case 1:
                        return index;
                    case 2:
                        return middle;
                    case 3:
                        return ring;
                    default:
                        return little;
                }
            }

            set
            {
                if (idx < 0 || idx >= HumanFingerCount)
                {
                    throw new IndexOutOfRangeException();
                }
                switch (idx)
                {
                    case 0:
                        thumb = value;
                        break;
                    case 1:
                        index = value;
                        break;
                    case 2:
                        middle = value;
                        break;
                    case 3:
                        ring = value;
                        break;
                    default:
                        little = value;
                        break;
                }
            }
        }

        public void WeightedAddPose(float weight, ref HandPoseData data)
        {
            thumb.WeightedAddPose(weight, ref data.thumb);
            index.WeightedAddPose(weight, ref data.index);
            middle.WeightedAddPose(weight, ref data.middle);
            ring.WeightedAddPose(weight, ref data.ring);
            little.WeightedAddPose(weight, ref data.little);
        }
    }

    public class HandRuntimeControl
    {
        private readonly int[] m_handBoneIndexMap;
        private readonly HumanPoseHandler m_poseHandler;
        private readonly GameObject m_rootObject;
        private HumanPose m_humanPose;

        public HandRuntimeControl(GameObject rootObject, Avatar avatar, HandType handType)
        {
            m_rootObject = rootObject;
            m_poseHandler = new HumanPoseHandler(avatar, rootObject.transform);
            m_handBoneIndexMap = new int[20];

            var indexMuscleFingerBegin = handType == HandType.LeftHand ? 55 : 75;
            
            m_poseHandler.GetHumanPose(ref m_humanPose);

            for (var i = 0; i < m_handBoneIndexMap.Length; ++i)
            {
                m_handBoneIndexMap[i] = indexMuscleFingerBegin + i;
            }
        }

        public void UpdateHandPose(ref HandPoseData handPose)
        {
            m_poseHandler.GetHumanPose(ref m_humanPose);
            
            m_humanPose.bodyPosition = Vector3.zero;
            m_humanPose.bodyRotation = Quaternion.identity;
            
            var i = 0;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.thumb.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.thumb.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.thumb.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.thumb.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.index.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.index.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.index.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.index.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.middle.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.middle.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.middle.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.middle.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.ring.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.ring.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.ring.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.ring.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.little.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.little.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.little.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = handPose.little.muscle3;
            
            m_poseHandler.SetHumanPose(ref m_humanPose);
        }
    }
}

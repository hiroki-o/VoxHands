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
        private HumanPose m_humanPose;
        private GameObject m_rootObject;

        private const int kIndexMuscleFingerBegin = 55;

        public HandRuntimeControl(GameObject rootObject, Avatar avatar)
        {
            m_rootObject = rootObject;
            m_poseHandler = new HumanPoseHandler(avatar, rootObject.transform);
            m_handBoneIndexMap = new int[20 * 2]; // left & right

            for (var i = 0; i < m_handBoneIndexMap.Length; ++i)
            {
                m_handBoneIndexMap[i] = kIndexMuscleFingerBegin + i;
            }
        }

        public void UpdateHandPose(ref HandPoseData leftHandPose, ref HandPoseData rightHandPose)
        {            
            m_poseHandler.GetHumanPose(ref m_humanPose);
            
            m_humanPose.bodyPosition = m_rootObject.transform.position;
            m_humanPose.bodyRotation = m_rootObject.transform.rotation;
            
            var i = 0;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.thumb.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.thumb.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.thumb.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.thumb.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.index.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.index.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.index.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.index.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.middle.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.middle.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.middle.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.middle.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.ring.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.ring.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.ring.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.ring.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.little.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.little.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.little.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = leftHandPose.little.muscle3;
            
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.thumb.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.thumb.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.thumb.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.thumb.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.index.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.index.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.index.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.index.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.middle.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.middle.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.middle.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.middle.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.ring.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.ring.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.ring.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.ring.muscle3;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.little.muscle1;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.little.spread;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.little.muscle2;
            m_humanPose.muscles[m_handBoneIndexMap[i++]] = rightHandPose.little.muscle3;
            
            m_poseHandler.SetHumanPose(ref m_humanPose);
        }
    }
}

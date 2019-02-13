using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Vox.Hands
{
    [Serializable]
    public class HandControllerPlayableBehaviour : PlayableBehaviour
    {
        public HandPosePresetsAsset presets;
        public HandPoseData handPose;
    }
}
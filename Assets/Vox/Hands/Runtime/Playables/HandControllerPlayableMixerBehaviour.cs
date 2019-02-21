using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Vox.Hands
{
    public class HandControllerPlayableMixerBehaviour : PlayableBehaviour
    {
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var trackBinding = playerData as HandController;

            if (!trackBinding)
            {
                return;
            }

            var inputCount = playable.GetInputCount();

            var leftHandPose = new HandPoseData();
            var rightHandPose = new HandPoseData();
            
            for (var i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                var inputPlayable =
                    (ScriptPlayable<HandControllerPlayableBehaviour>) playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                // Use the above variables to process each frame of this playable.

                leftHandPose.WeightedAddPose(inputWeight, ref input.leftHandPose);
                rightHandPose.WeightedAddPose(inputWeight, ref input.rightHandPose);
            }

            trackBinding.SetHandPose(ref leftHandPose, HandType.LeftHand);
            trackBinding.SetHandPose(ref rightHandPose, HandType.RightHand);
        }
    }
}
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Vox.Hands
{
    [TrackColor(1f, 0.4f, 0f)]
    [TrackClipType(typeof(HandControllerPlayableClip))]
    [TrackBindingType(typeof(HandController))]
    public class HandControllerPlayableTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<HandControllerPlayableMixerBehaviour>.Create(graph, inputCount);
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Vox.Hands
{
    [Serializable]
    public class HandControllerPlayableClip : PlayableAsset, ITimelineClipAsset
    {
        public HandControllerPlayableBehaviour template = new HandControllerPlayableBehaviour();

        public ClipCaps clipCaps => ClipCaps.All;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<HandControllerPlayableBehaviour>.Create(graph, template);
        }
    }
}
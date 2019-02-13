using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Xml.Schema;

namespace Vox.Hands
{
    [CustomEditor(typeof(HandPosePresetsAsset))]
    public class HandPosePresetsAssetEditor : Editor
    {
        [SerializeField] private string m_presetFilter;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var asset = target as HandPosePresetsAsset;
            
            HandPosePreset removingPreset = null;

            m_presetFilter = EditorGUILayout.TextField("Filter", m_presetFilter);
            GUILayout.Space(12f);
            
            GUILayout.Label("Presets", "BoldLabel");
            
            var presets = string.IsNullOrEmpty(m_presetFilter)
                ? asset.SavedPresets
                : asset.SavedPresets.Where(p => p.Name.ToLower().Contains(m_presetFilter.ToLower()));
            
            foreach (var preset in presets)
            {
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    GUI.changed = false;
                    if (preset.HandPoseImage != null)
                    {
                        GUILayout.Label(new GUIContent(preset.HandPoseImage));
                    }
                                        
                    preset.Name = EditorGUILayout.DelayedTextField("Name", preset.Name);
                    GUILayout.Space(8f);
                    var pose = preset.HandPoseData;
                    
                    pose.thumb.spread = EditorGUILayout.Slider("Thumb Spread", pose.thumb.spread, -1f, 1f);
                    pose.thumb.muscle1 = EditorGUILayout.Slider("Thumb Muscle 1", pose.thumb.muscle1, -1f, 1f);
                    pose.thumb.muscle2 = EditorGUILayout.Slider("Thumb Muscle 2", pose.thumb.muscle2, -1f, 1f);
                    pose.thumb.muscle3 = EditorGUILayout.Slider("Thumb Muscle 3", pose.thumb.muscle3, -1f, 1f);
                    pose.index.spread = EditorGUILayout.Slider("Index Spread", pose.index.spread, -1f, 1f);
                    pose.index.muscle1 = EditorGUILayout.Slider("Index Muscle 1", pose.index.muscle1, -1f, 1f);
                    pose.index.muscle2 = EditorGUILayout.Slider("Index Muscle 2", pose.index.muscle2, -1f, 1f);
                    pose.index.muscle3 = EditorGUILayout.Slider("Index Muscle 3", pose.index.muscle3, -1f, 1f);
                    pose.middle.spread = EditorGUILayout.Slider("Middle Spread", pose.middle.spread, -1f, 1f);
                    pose.middle.muscle1 = EditorGUILayout.Slider("Middle Muscle 1", pose.middle.muscle1, -1f, 1f);
                    pose.middle.muscle2 = EditorGUILayout.Slider("Middle Muscle 2", pose.middle.muscle2, -1f, 1f);
                    pose.middle.muscle3 = EditorGUILayout.Slider("Middle Muscle 3", pose.middle.muscle3, -1f, 1f);
                    pose.ring.spread = EditorGUILayout.Slider("Ring Spread", pose.ring.spread, -1f, 1f);
                    pose.ring.muscle1 = EditorGUILayout.Slider("Ring Muscle 1", pose.ring.muscle1, -1f, 1f);
                    pose.ring.muscle2 = EditorGUILayout.Slider("Ring Muscle 2", pose.ring.muscle2, -1f, 1f);
                    pose.ring.muscle3 = EditorGUILayout.Slider("Ring Muscle 3", pose.ring.muscle3, -1f, 1f);
                    pose.little.spread = EditorGUILayout.Slider("Little Spread", pose.little.spread, -1f, 1f);
                    pose.little.muscle1 = EditorGUILayout.Slider("Little Muscle 1", pose.little.muscle1, -1f, 1f);
                    pose.little.muscle2 = EditorGUILayout.Slider("Little Muscle 2", pose.little.muscle2, -1f, 1f);
                    pose.little.muscle3 = EditorGUILayout.Slider("Little Muscle 3", pose.little.muscle3, -1f, 1f);

                    if (GUI.changed)
                    {
                        preset.HandPoseData = pose;
                        EditorUtility.SetDirty(target);
                    }
                        
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Remove", GUILayout.Width(80f)))
                        {
                            removingPreset = preset;
                        }
                    }

                    GUILayout.Space(12f);
                }
            }
            
            if (removingPreset != null)
            {
                asset.RemovePreset(removingPreset);
            }
        }
    }
}
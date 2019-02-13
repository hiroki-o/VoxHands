using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vox.Hands
{    
    /*
     * Hand Pose Presets.
     */
    [CreateAssetMenu(fileName = "New HandPosePreset", menuName = "Hand Pose Preset", order = 651)]    
    public class HandPosePresetsAsset : ScriptableObject
    {
        [SerializeField] private List<HandPosePreset> presets = null;

        // Properties (Get)
        public IEnumerable<HandPosePreset> SavedPresets => presets;

        public HandPoseData this[int index] => presets[index].HandPoseData;
        public HandPoseData this[string name] => presets[presets.FindIndex(p => p.Name == name)].HandPoseData;

#if UNITY_EDITOR
        private static HandPosePresetsAsset s_presetsAsset;
        
        private static string BasePath {
            get {
                var obj = CreateInstance<HandPosePresetsAsset> ();
                var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject (obj));
                DestroyImmediate (obj);

                var fileInfo = new FileInfo(scriptPath);
                var baseDir = fileInfo.Directory.Parent.ToString ().Replace( '\\', '/');
                var index = baseDir.LastIndexOf ("Assets/");
                baseDir = baseDir.Substring (index);
                baseDir = baseDir + "/Presets";

                return baseDir;
            }
        }

        private const string kDEFAULT_PRESET_ASSET_FILENAME = "HandPosePresets.asset";

        public static HandPosePresetsAsset GetDefaultGetPresetsAsset() {
            if(s_presetsAsset == null) {
                if(!Load()) {
                    // Create vanilla db
                    s_presetsAsset = CreateInstance<HandPosePresetsAsset>();
                    s_presetsAsset.presets = new List<HandPosePreset>();

                    var basePath = BasePath;
                    
                    if (!Directory.Exists(basePath)) {
                        Directory.CreateDirectory(basePath);
                    }

                    AssetDatabase.CreateAsset(s_presetsAsset, basePath + "/" + kDEFAULT_PRESET_ASSET_FILENAME);
                }
            }

            return s_presetsAsset;
        }

        private void OnEnable()
        {
            if (presets == null)
            {
                presets = new List<HandPosePreset>();
            }
        }

        private static bool Load() {

            var loaded = false;

            try {
                var assetPath = BasePath + "/" + kDEFAULT_PRESET_ASSET_FILENAME;
				
                if(File.Exists(assetPath)) 
                {
                    s_presetsAsset = AssetDatabase.LoadAssetAtPath<HandPosePresetsAsset>(assetPath);
                    loaded = true;
                }
            } catch(Exception e) {
                Debug.LogException(e);
            }

            return loaded;
        }

        public void AddNewPreset(HandPosePreset preset)
        {
            presets.Add(preset);
            
            if (preset.HandPoseImage != null)
            {
                AssetDatabase.AddObjectToAsset(preset.HandPoseImage, this);
            }
            EditorUtility.SetDirty(this);
        }

        public void RemovePreset(HandPosePreset preset)
        {
            AssetDatabase.RemoveObjectFromAsset(preset.HandPoseImage);
            presets.Remove(preset);
            EditorUtility.SetDirty(this);
        }
#endif //UNITY_EDITOR
    }
}
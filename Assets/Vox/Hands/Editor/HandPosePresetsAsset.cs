using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditor.Presets;

namespace Vox.Hands
{    
    /*
     * Hand Pose Presets.
     */
    public class HandPosePresetsAsset : ScriptableObject
    {
        [SerializeField] private List<HandPosePreset> presets = null;

        // Properties (Get)
        public List<HandPosePreset> SavedPresets => presets;
        
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

        private const string s_presetAssetFilename = "HandPosePresets.asset";

        public static HandPosePresetsAsset GetPresetsAsset() {
            if(s_presetsAsset == null) {
                if(!Load()) {
                    // Create vanilla db
                    s_presetsAsset = CreateInstance<HandPosePresetsAsset>();
                    s_presetsAsset.presets = new List<HandPosePreset>();

                    var basePath = BasePath;
                    
                    if (!Directory.Exists(basePath)) {
                        Directory.CreateDirectory(basePath);
                    }

                    AssetDatabase.CreateAsset(s_presetsAsset, basePath + "/" + s_presetAssetFilename);
                }
            }

            return s_presetsAsset;
        }
        
        private static bool Load() {

            var loaded = false;

            try {
                var assetPath = BasePath + "/" + s_presetAssetFilename;
				
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
    }
}
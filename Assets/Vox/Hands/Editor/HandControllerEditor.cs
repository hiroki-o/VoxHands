using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

namespace Vox.Hands
{
    [CustomEditor(typeof(HandController))]
    public class HandControllerEditor : Editor
    {
        private enum TabMode
        {
            FingerControls,
            AnimationExport
        }

        private enum PresetSaveState
        {
            None,
            ToolIsOpen,
            Capturing
        }

        private enum HandEditMode
        {
            Both,
            Left,
            Right
        }

        private enum AnimationExportMode
        {
            Simple,
            Advanced
        }

        private const int kPresetIconSize = 128;

        private HandController m_currentTarget;
        private SerializedProperty m_prop_preset;
        private HandPoseDataEditorUtility m_handPoseUtility;

        [SerializeField] private TabMode m_tabMode = TabMode.FingerControls;
        [SerializeField] private HandEditMode m_handEditMode = HandEditMode.Both;
        [SerializeField] private AnimationExportMode m_animationExportMode = AnimationExportMode.Simple;
        [SerializeField] private HandEditMode m_handAnimationExport = HandEditMode.Both;

        // Animation Export
        [SerializeField] private float m_exportAnimationSec = 1.0f;
        [SerializeField] private AnimationCurve m_curve = new AnimationCurve(new Keyframe(0, 1.0f), new Keyframe(1, 1));

        private PresetSaveState m_presetSaveState;

        //private Rect m_captureFrameRect = new Rect(0f,0f,128f,128f);
        private Vector2 m_selectPoint;
        private Vector2 m_selectMouseOffset;
        private string m_newPresetName;
        private Texture2D m_presetCaptureIcon;
        private Tool m_previousTool;

        private bool[] m_foldouts;
        private bool m_presetFoldout;
        private string m_presetFilter;
        private Vector2 m_presetScroll;

        private Texture2D[] m_handIcons;
        private Texture2D m_focusIcon;

        private float AllSpread
        {
            get
            {
                switch (m_handEditMode)
                {
                    case HandEditMode.Both:
                        return m_handPoseUtility.AllSpread;
                    case HandEditMode.Left:
                        return m_handPoseUtility.AllLeftSpread;
                    default:
                        return m_handPoseUtility.AllRightSpread;
                }
            }

            set
            {
                switch (m_handEditMode)
                {
                    case HandEditMode.Both:
                        m_handPoseUtility.AllSpread = value;
                        break;
                    case HandEditMode.Left:
                        m_handPoseUtility.AllLeftSpread = value;
                        break;
                    default:
                        m_handPoseUtility.AllRightSpread = value;
                        break;
                }
            }
        }

        private float AllFingersMuscle
        {
            get
            {
                switch (m_handEditMode)
                {
                    case HandEditMode.Both:
                        return m_handPoseUtility.AllFingersMuscle;
                    case HandEditMode.Left:
                        return m_handPoseUtility.AllLeftFingersMuscle;
                    default:
                        return m_handPoseUtility.AllRightFingersMuscle;
                }
            }

            set
            {
                switch (m_handEditMode)
                {
                    case HandEditMode.Both:
                        m_handPoseUtility.AllFingersMuscle = value;
                        break;
                    case HandEditMode.Left:
                        m_handPoseUtility.AllLeftFingersMuscle = value;
                        break;
                    default:
                        m_handPoseUtility.AllRightFingersMuscle = value;
                        break;
                }
            }
        }

        private void ResetEditor()
        {
            m_currentTarget = target as HandController;
            m_prop_preset = serializedObject.FindProperty("m_preset");
            m_handPoseUtility =
                new HandPoseDataEditorUtility(serializedObject, "m_leftHandPoseData.", "m_rightHandPoseData.");
            m_presetSaveState = PresetSaveState.None;
            m_foldouts = new bool[HandPoseData.HumanFingerCount];

            m_handIcons = new[]
            {
                IconResources.LoadTextureIconsDir(IconResources.kICON_HAND_BOTH),
                IconResources.LoadTextureIconsDir(IconResources.kICON_HAND_LEFT),
                IconResources.LoadTextureIconsDir(IconResources.kICON_HAND_RIGHT)
            };

            m_focusIcon = IconResources.LoadTextureIconsDir(IconResources.kICON_FOCUS);
        }

        private void DrawTabs()
        {
            var tabMode = m_tabMode;

            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {
                var toolbarbutton = new GUIStyle("toolbarbutton");

                if (GUILayout.Toggle(tabMode == TabMode.FingerControls, "Controls", toolbarbutton))
                {
                    tabMode = TabMode.FingerControls;
                }

                if (GUILayout.Toggle(tabMode == TabMode.AnimationExport, "Export Animation", toolbarbutton))
                {
                    tabMode = TabMode.AnimationExport;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (tabMode != m_tabMode)
                    {
                        m_tabMode = tabMode;
                        GUI.FocusControl(string.Empty);
                    }
                }
            }
        }

        private void DrawHandEditMode()
        {
            //GUILayout.Label("Hand", "BoldLabel");
            GUILayout.Space(12f);
            
            var editMode = m_handEditMode;

            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(250)))
            {
                var style = new GUIStyle("toolbarbutton");
                style.fixedHeight = 32f; 

                if (GUILayout.Toggle(editMode == HandEditMode.Both, new GUIContent(m_handIcons[0], "Both"), style))
                {
                    editMode = HandEditMode.Both;
                }

                if (GUILayout.Toggle(editMode == HandEditMode.Left, new GUIContent(m_handIcons[1], "Left"), style))
                {
                    editMode = HandEditMode.Left;
                }

                if (GUILayout.Toggle(editMode == HandEditMode.Right, new GUIContent(m_handIcons[2], "Right"), style))
                {
                    editMode = HandEditMode.Right;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (editMode != m_handEditMode)
                    {
                        m_handEditMode = editMode;
                        GUI.FocusControl(string.Empty);
                    }
                }
                
                GUILayout.Space(8f);
                
                if (GUILayout.Button(new GUIContent(m_focusIcon, "Focus"), style, GUILayout.Width(50f)))
                {
                    SetSceneViewCameraToHand();
                }                
            }
        }


        private void DrawHandPosePreset()
        {
            m_presetFoldout = EditorGUILayout.Foldout(m_presetFoldout, "Presets");
            if (m_presetFoldout)
            {
                if (m_prop_preset.objectReferenceValue == null)
                {
                    m_prop_preset.objectReferenceValue = HandPosePresetsAsset.GetDefaultGetPresetsAsset();
                }

                var presetsAsset = m_prop_preset.objectReferenceValue as HandPosePresetsAsset;

                EditorGUILayout.PropertyField(m_prop_preset);

                m_presetFilter = EditorGUILayout.TextField("Filter", m_presetFilter);

                m_presetScroll = GUILayout.BeginScrollView(m_presetScroll, GUI.skin.box, GUILayout.Height(174f));
                using (new EditorGUILayout.HorizontalScope())
                {
                    var presets = string.IsNullOrEmpty(m_presetFilter)
                        ? presetsAsset.SavedPresets
                        : presetsAsset.SavedPresets.Where(p => p.Name.ToLower().Contains(m_presetFilter.ToLower()));
                    foreach (var preset in presets)
                    {
                        if (GUILayout.Button(new GUIContent(preset.HandPoseImage, preset.Name), GUILayout.Width(150f),
                            GUILayout.Height(150f)))
                        {
                            switch (m_handEditMode)
                            {
                                case HandEditMode.Both:
                                    m_handPoseUtility.ApplyPreset(preset, HandType.LeftHand);
                                    m_handPoseUtility.ApplyPreset(preset, HandType.RightHand);
                                    break;
                                case HandEditMode.Left:
                                    m_handPoseUtility.ApplyPreset(preset, HandType.LeftHand);
                                    break;
                                case HandEditMode.Right:
                                    m_handPoseUtility.ApplyPreset(preset, HandType.RightHand);
                                    break;
                            }
                        }
                    }
                }

                GUILayout.EndScrollView();
            }
        }

        private void DrawFingerControls()
        {
            DrawHandEditMode();
            
            GUILayout.Space(12f);

            var allSpreadValue = AllSpread;
            var newAllSpreadValue = EditorGUILayout.Slider("Spread", allSpreadValue, -1f, 1f);
            if (allSpreadValue != newAllSpreadValue)
            {
                AllSpread = newAllSpreadValue;
            }

            var allFingersMuscleValue = AllFingersMuscle;
            var newAllFingersMuscleValue = EditorGUILayout.Slider("Muscles", allFingersMuscleValue, -1f, 1f);
            if (allFingersMuscleValue != newAllFingersMuscleValue)
            {
                AllFingersMuscle = newAllFingersMuscleValue;
            }

            GUILayout.Space(12f);

            var fingers = m_handEditMode != HandEditMode.Right
                ? m_handPoseUtility.LeftFingers
                : m_handPoseUtility.RightFingers;
            var bBoth = m_handEditMode == HandEditMode.Both;

            for (var i = 0; i < m_handPoseUtility.LeftFingers.Length; ++i)
            {
                var finger = fingers[i];
                m_foldouts[i] = EditorGUILayout.Foldout(m_foldouts[i], string.Format("{0} finger", finger.FingerName));
                if (m_foldouts[i])
                {
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        GUILayout.Space(4f);
                        var allMuscleValue = finger.MuscleAll;
                        var newAllMuscleValue = EditorGUILayout.Slider("All", allMuscleValue, -1f, 1f);
                        if (allMuscleValue != newAllMuscleValue)
                        {
                            finger.MuscleAll = newAllMuscleValue;
                            if (bBoth)
                            {
                                m_handPoseUtility.RightFingers[i].MuscleAll = newAllMuscleValue;
                            }
                        }

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.Slider(finger.PropSpread, -1f, 1f, "Spread");
                        GUILayout.Space(4f);
                        EditorGUILayout.Slider(finger.PropMuscle1, -1f, 1f, "1st");
                        EditorGUILayout.Slider(finger.PropMuscle2, -1f, 1f, "2nd");
                        EditorGUILayout.Slider(finger.PropMuscle3, -1f, 1f, "3rd");
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (bBoth)
                            {
                                m_handPoseUtility.RightFingers[i].Spread = finger.Spread;
                                m_handPoseUtility.RightFingers[i].Muscle1 = finger.Muscle1;
                                m_handPoseUtility.RightFingers[i].Muscle2 = finger.Muscle2;
                                m_handPoseUtility.RightFingers[i].Muscle3 = finger.Muscle3;
                            }
                        }
                    }
                }

                GUILayout.Space(8f);
            }

            DrawHandPosePreset();
        }

        private void DrawAnimationExport()
        {
            m_animationExportMode = (AnimationExportMode)EditorGUILayout.EnumPopup("Mode", m_animationExportMode);
            
            switch (m_animationExportMode)
            {
                case AnimationExportMode.Simple:
                    DrawAnimationExportSimple();
                    break;
                    case AnimationExportMode.Advanced:
                        DrawAnimationExportAdvanced();
                        break;
            }
        }

        private void DrawAnimationExportSimple()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save", GUILayout.Width(40f)))
                {
                    m_exportAnimationSec = 0.01f;
                    m_curve = new AnimationCurve(new Keyframe(0f, 1.0f), new Keyframe(1f, 1f));
                    SaveHandInAnimationClip();
                }
            }
        }

        private void DrawAnimationExportAdvanced()
        {
            m_handAnimationExport = (HandEditMode)EditorGUILayout.EnumPopup("Hand", m_handAnimationExport);
            m_exportAnimationSec = EditorGUILayout.FloatField("Animation Length(sec)", m_exportAnimationSec);
            m_curve = EditorGUILayout.CurveField("Animation Curve", m_curve);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save", GUILayout.Width(40f)))
                {
                    SaveHandInAnimationClip();
                }
            }
        }

        private void DrawSavePresetButton()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (m_presetSaveState == PresetSaveState.None)
                {
                    if (GUILayout.Button("Open Preset Save Tool in SceneView", GUILayout.Width(220f)))
                    {
                        BeginSaveCurrentToPreset();
                    }
                }
                else
                {
                    if (GUILayout.Button("Close Preset Save Tool in SceneView", GUILayout.Width(220f)))
                    {
                        EndSaveCurrentToPreset(false);
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (m_currentTarget != target || m_prop_preset == null)
            {
                ResetEditor();
            }

            GUILayout.Space(12f);
            DrawTabs();
            switch (m_tabMode)
            {
                case TabMode.FingerControls:
                    DrawFingerControls();
                    DrawSavePresetButton();

                    break;
                case TabMode.AnimationExport:
                    DrawAnimationExport();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                m_currentTarget.InitializeRuntimeControl();
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return m_presetSaveState != PresetSaveState.None;
        }

        private void SetSceneViewCameraToHand()
        {
            var animator = m_currentTarget.GetComponent<Animator>();
            var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand).position;

            Vector3 pivotPos;
            switch (m_handEditMode)
            {
                case HandEditMode.Left:
                    pivotPos = leftHand;
                    break;
                case HandEditMode.Right:
                    pivotPos = rightHand;
                    break;
                default:
                    pivotPos = Vector3.Lerp(leftHand, rightHand, 0.5f);
                    break;
            }

            SceneView.lastActiveSceneView.pivot = pivotPos;
            SceneView.lastActiveSceneView.Repaint();
        }

        private void HandleSceneViewMouseEvents()
        {
            if (Event.current.button != 0)
            {
                return;
            }

            var curRect = new Rect(m_selectPoint, new Vector2(kPresetIconSize, kPresetIconSize));
            var curPos = Event.current.mousePosition;
            if (!curRect.Contains(curPos))
            {
                return;
            }

            //mouse drag event handling.
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    m_selectMouseOffset = curPos - m_selectPoint;
                    HandleUtility.Repaint();
                    Event.current.Use();
                    break;
                case EventType.MouseDrag:
                case EventType.MouseUp:
                    m_selectPoint = curPos - m_selectMouseOffset;
                    HandleUtility.Repaint();
                    Event.current.Use();
                    break;
            }
        }

        private void BeginSaveCurrentToPreset()
        {
            if (m_presetSaveState != PresetSaveState.None)
            {
                return;
            }

            m_previousTool = Tools.current;
            Tools.current = Tool.View;
            m_presetSaveState = PresetSaveState.ToolIsOpen;
            m_newPresetName = "New Preset";
            SceneView.onSceneGUIDelegate += OnPresetImageShotTakerSceneViewGUI;
            SceneView.lastActiveSceneView.Repaint();

            m_selectPoint = new Vector2
            {
                x = (SceneView.lastActiveSceneView.position.width - kPresetIconSize) / 2f,
                y = (SceneView.lastActiveSceneView.position.height - kPresetIconSize) / 2f
            };
        }

        private void OnPresetIconCapturePostRenderScreenView(Camera camera)
        {
            if (m_presetSaveState != PresetSaveState.Capturing)
            {
                Camera.onPostRender -= OnPresetIconCapturePostRenderScreenView;
                return;
            }

            if (camera == SceneView.lastActiveSceneView.camera)
            {
                var currentRT = RenderTexture.active;
                RenderTexture.active = camera.targetTexture;

                // for upper-resolution display such as Retina, pixel vs screen coordinate is different.
                var resolutionRatio = Mathf.Round(SceneView.lastActiveSceneView.camera.pixelHeight /
                                                  SceneView.lastActiveSceneView.position.height);

                // Mac: Camera coordinate Y is inverted from mouse coordinate
                var cameraY = (Application.platform == RuntimePlatform.WindowsEditor) ?
                    resolutionRatio * m_selectPoint.y : 
                    camera.targetTexture.height - resolutionRatio * (m_selectPoint.y + kPresetIconSize);
                var cameraX = resolutionRatio * m_selectPoint.x;

                var texSize = Mathf.FloorToInt(kPresetIconSize * resolutionRatio);

                // Make a new texture and read the active Render Texture into it.
                m_presetCaptureIcon = new Texture2D(texSize, texSize);
                m_presetCaptureIcon.ReadPixels(new Rect(cameraX, cameraY, texSize, texSize), 0, 0);
                m_presetCaptureIcon.Apply();
                m_presetCaptureIcon.name = m_newPresetName;

                RenderTexture.active = currentRT;

                EndSaveCurrentToPreset(true);
            }
        }

        private void EndSaveCurrentToPreset(bool capture)
        {
            Tools.current = m_previousTool;

            SceneView.onSceneGUIDelegate -= OnPresetImageShotTakerSceneViewGUI;
            m_presetSaveState = PresetSaveState.None;
            SceneView.lastActiveSceneView.Repaint();
            Repaint();

            if (capture)
            {
                if (m_presetCaptureIcon.width != kPresetIconSize)
                {
                    var scale = (float) kPresetIconSize / (float) m_presetCaptureIcon.width;
                    m_presetCaptureIcon = TextureUtility.CreateScaledTexture(m_presetCaptureIcon, scale);
                }

                var handType = m_handEditMode != HandEditMode.Right ? HandType.LeftHand : HandType.RightHand;
                
                m_handPoseUtility.SaveCurrentToPreset(
                    m_prop_preset.objectReferenceValue as HandPosePresetsAsset,
                    handType,
                    m_newPresetName, m_presetCaptureIcon);                
            }
        }

        private void OnPresetImageShotTakerSceneViewGUI(SceneView sceneView)
        {
            if (m_presetSaveState != PresetSaveState.ToolIsOpen)
            {
                return;
            }

            Handles.BeginGUI();
            HandleSceneViewMouseEvents();

            var tbLabel = new GUIStyle(EditorStyles.toolbar);
            tbLabel.alignment = TextAnchor.MiddleCenter;

            var closePos = new Rect(m_selectPoint.x + kPresetIconSize - 20f, m_selectPoint.y - 20f, 20f, 20f);
            if (GUI.Button(closePos, "X", tbLabel))
            {
                EndSaveCurrentToPreset(false);
            }

            var sel = new Rect(m_selectPoint.x, m_selectPoint.y, 128f, 128f);
            GUI.Label(sel, string.Empty, "SelectionRect");

            var namePos = new Rect(m_selectPoint.x, m_selectPoint.y + kPresetIconSize + 4f, 128f, 20f);
            m_newPresetName = EditorGUI.TextField(namePos, m_newPresetName);

            var buttonPos = new Rect(namePos.x, namePos.y + 22f, 128f, 20f);
            if (GUI.Button(buttonPos, "Save to Preset"))
            {
                m_presetSaveState = PresetSaveState.Capturing;
                Camera.onPostRender += OnPresetIconCapturePostRenderScreenView;
            }

            Handles.EndGUI();
        }

        private void SaveHandInAnimationClip()
        {
            var savePath = EditorUtility.SaveFilePanelInProject("Save New Animation", "New Animation", "anim",
                "Saving Finger Pose as Animation");
            if (string.IsNullOrEmpty(savePath))
            {
                return;
            }

            var handNames = new [] {"LeftHand", "RightHand"};
            var fingerProperties = new[] {m_handPoseUtility.LeftFingers, m_handPoseUtility.RightFingers};
            var newAnimation = new AnimationClip();

            for (var i = 0; i < 2; ++i)
            {
                if (m_handAnimationExport == HandEditMode.Right && i == 0)
                {
                    continue;
                }
                if (m_handAnimationExport == HandEditMode.Left && i == 1)
                {
                    continue;
                }
                
                var hand = handNames[i];
                var fingers = fingerProperties[i];
                
                foreach (var finger in fingers)
                {
                    var spread = new AnimationCurve();
                    var muscle1 = new AnimationCurve();
                    var muscle2 = new AnimationCurve();
                    var muscle3 = new AnimationCurve();

                    foreach (var k in m_curve.keys)
                    {
                        var newKey = k;
                        newKey.time = k.time * m_exportAnimationSec;
                        newKey.value = Mathf.Lerp(0f, finger.Spread, k.value);
                        spread.AddKey(newKey);
                        newKey.value = Mathf.Lerp(0f, finger.Muscle1, k.value);
                        muscle1.AddKey(newKey);
                        newKey.value = Mathf.Lerp(0f, finger.Muscle2, k.value);
                        muscle2.AddKey(newKey);
                        newKey.value = Mathf.Lerp(0f, finger.Muscle3, k.value);
                        muscle3.AddKey(newKey);
                    }

                    newAnimation.SetCurve("", typeof(Animator), string.Format("{0}.{1}.Spread", hand, finger.FingerName),
                        spread);
                    newAnimation.SetCurve("", typeof(Animator),
                        string.Format("{0}.{1}.1 Stretched", hand, finger.FingerName), muscle1);
                    newAnimation.SetCurve("", typeof(Animator),
                        string.Format("{0}.{1}.2 Stretched", hand, finger.FingerName), muscle2);
                    newAnimation.SetCurve("", typeof(Animator),
                        string.Format("{0}.{1}.3 Stretched", hand, finger.FingerName), muscle3);
                }
            }


            AssetDatabase.CreateAsset(newAnimation, savePath);
            AssetDatabase.SaveAssets();
        }
    }
}
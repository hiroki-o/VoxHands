using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Vox.Hands
{
    [CustomPropertyDrawer(typeof(HandControllerPlayableBehaviour))]
    public class HandControllerPlayableDrawer : PropertyDrawer
    {
        private enum HandEditMode
        {
            Both,
            Left,
            Right
        }

        private HandPoseDataEditorUtility m_hand;
        private SerializedProperty m_prop_presets;

        private bool m_presetFoldout;
        private string m_presetFilter;
        private Vector2 m_presetScroll;

        private Texture2D[] m_handIcons;

        private bool m_isInitialized;
        private HandEditMode m_handEditMode = HandEditMode.Both;

        private float AllSpread
        {
            get
            {
                switch (m_handEditMode)
                {
                    case HandEditMode.Both:
                        return m_hand.AllSpread;
                    case HandEditMode.Left:
                        return m_hand.AllLeftSpread;
                    default:
                        return m_hand.AllRightSpread;
                }
            }

            set
            {
                switch (m_handEditMode)
                {
                    case HandEditMode.Both:
                        m_hand.AllSpread = value;
                        break;
                    case HandEditMode.Left:
                        m_hand.AllLeftSpread = value;
                        break;
                    default:
                        m_hand.AllRightSpread = value;
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
                        return m_hand.AllFingersMuscle;
                    case HandEditMode.Left:
                        return m_hand.AllLeftFingersMuscle;
                    default:
                        return m_hand.AllRightFingersMuscle;
                }
            }

            set
            {
                switch (m_handEditMode)
                {
                    case HandEditMode.Both:
                        m_hand.AllFingersMuscle = value;
                        break;
                    case HandEditMode.Left:
                        m_hand.AllLeftFingersMuscle = value;
                        break;
                    default:
                        m_hand.AllRightFingersMuscle = value;
                        break;
                }
            }
        }        
        
        private void Initialize(Rect position, SerializedProperty property, GUIContent label)
        {
            m_hand = new HandPoseDataEditorUtility(property, "leftHandPose.", "rightHandPose.");
            m_prop_presets = property.FindPropertyRelative("presets");

            m_handIcons = new[]
            {
                IconResources.LoadTextureIconsDir(IconResources.kICON_HAND_BOTH),
                IconResources.LoadTextureIconsDir(IconResources.kICON_HAND_LEFT),
                IconResources.LoadTextureIconsDir(IconResources.kICON_HAND_RIGHT)
            };

            m_isInitialized = true;
        }

        private Rect DrawHandEditMode(Rect position)
        {
            //GUILayout.Label("Hand", "BoldLabel");
            GUILayout.Space(12f);

            var editMode = m_handEditMode;

            const float kButtonWidth = 80f;
            const float kButtonHeight = 32f;
            const float kTabBottomSpace = 8f;
            const float kIndentPixel = 16f;

            var style = new GUIStyle("toolbarbutton");
            style.fixedHeight = kButtonHeight;

            EditorGUI.BeginChangeCheck();
            var buttonRect = new Rect(position.x + kIndentPixel, position.y + kTabBottomSpace, kButtonWidth, kButtonHeight);
            if (GUI.Toggle(buttonRect, editMode == HandEditMode.Both, new GUIContent(m_handIcons[0], "Both"), style))
            {
                editMode = HandEditMode.Both;
            }

            buttonRect.x += kButtonWidth;
            if (GUI.Toggle(buttonRect, editMode == HandEditMode.Left, new GUIContent(m_handIcons[1], "Left"), style))
            {
                editMode = HandEditMode.Left;
            }

            buttonRect.x += kButtonWidth;
            if (GUI.Toggle(buttonRect, editMode == HandEditMode.Right, new GUIContent(m_handIcons[2], "Right"), style))
            {
                editMode = HandEditMode.Right;
            }

            if (EditorGUI.EndChangeCheck())
            {
                m_handEditMode = editMode;
            }

            return new Rect(position.x, position.y + kButtonHeight + kTabBottomSpace * 2, position.width,
                position.height - kButtonHeight - kTabBottomSpace * 2);
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int fieldCount = 36;
            return fieldCount * EditorGUIUtility.singleLineHeight + 200f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!m_isInitialized)
            {
                Initialize(position, property, label);
            }

            position = DrawHandEditMode(position);

            var presets = m_prop_presets.objectReferenceValue as HandPosePresetsAsset;
            if (presets == null)
            {
                presets = HandPosePresetsAsset.GetDefaultGetPresetsAsset();
                m_prop_presets.objectReferenceValue = presets;
            }

            var singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(singleFieldRect, m_prop_presets);
            position.y += EditorGUIUtility.singleLineHeight;
            position.height -= EditorGUIUtility.singleLineHeight;

            DrawFingerControls(position, presets);
        }

        public void DrawFingerControls(Rect position, HandPosePresetsAsset presetsAsset)
        {
            var singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            var allSpreadValue =  AllSpread;
            var newAllSpreadValue = EditorGUI.Slider(singleFieldRect, "Spread All", allSpreadValue, -1f, 1f);
            if (allSpreadValue != newAllSpreadValue)
            {
                AllSpread = newAllSpreadValue;
            }

            var allFingersMuscleValue = AllFingersMuscle;
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            var newAllFingersMuscleValue = EditorGUI.Slider(singleFieldRect, "Muscles All", allFingersMuscleValue, -1f, 1f);
            if (allFingersMuscleValue != newAllFingersMuscleValue)
            {
                AllFingersMuscle = newAllFingersMuscleValue;
            }

            singleFieldRect.y += 12f;
            
            var fingers = m_handEditMode != HandEditMode.Right
                ? m_hand.LeftFingers
                : m_hand.RightFingers;
            var bBoth = m_handEditMode == HandEditMode.Both;

            for (var i = 0; i < m_hand.LeftFingers.Length; ++i)
            {
                var finger = fingers[i];
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Space(4f);
                    var allMuscleValue = finger.MuscleAll;
                    singleFieldRect.y += EditorGUIUtility.singleLineHeight;
                    var newAllMuscleValue = EditorGUI.Slider(singleFieldRect, finger.FingerName + " All", allMuscleValue, -1f, 1f);
                    if (allMuscleValue != newAllMuscleValue)
                    {
                        finger.MuscleAll = newAllMuscleValue;
                        if (bBoth)
                        {
                            m_hand.RightFingers[i].MuscleAll = newAllMuscleValue;
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    singleFieldRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.Slider(singleFieldRect, finger.PropSpread, -1f, 1f, finger.FingerName + " Spread");
                    singleFieldRect.y += 4f;
                    singleFieldRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.Slider(singleFieldRect, finger.PropMuscle1, -1f, 1f, finger.FingerName + " 1st");
                    singleFieldRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.Slider(singleFieldRect, finger.PropMuscle2, -1f, 1f, finger.FingerName + " 2nd");
                    singleFieldRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.Slider(singleFieldRect, finger.PropMuscle3, -1f, 1f, finger.FingerName + " 3rd");
                    singleFieldRect.y += 8f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (bBoth)
                        {
                            m_hand.RightFingers[i].Spread = finger.Spread;
                            m_hand.RightFingers[i].Muscle1 = finger.Muscle1;
                            m_hand.RightFingers[i].Muscle2 = finger.Muscle2;
                            m_hand.RightFingers[i].Muscle3 = finger.Muscle3;
                        }
                    }
                }
            }

            singleFieldRect.y += 32f;
            DrawHandPosePreset(
                new Rect(singleFieldRect.x, singleFieldRect.y, position.width,
                    position.height - (singleFieldRect.y - position.y)),
                presetsAsset);
        }

        private void DrawHandPosePreset(Rect position, HandPosePresetsAsset presetsAsset)
        {
            var singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            GUI.Label(singleFieldRect, "Presets", "BoldLabel");
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            m_presetFilter = EditorGUI.TextField(singleFieldRect, "Filter", m_presetFilter);
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;

            singleFieldRect.y += 12f;

            var presets = string.IsNullOrEmpty(m_presetFilter)
                ? presetsAsset.SavedPresets
                : presetsAsset.SavedPresets.Where(p => p.Name.ToLower().Contains(m_presetFilter.ToLower()));

            var posRect = new Rect(position.x, singleFieldRect.y, position.width, 174f);
            var viewRect = new Rect(0f, 0f, presets.Count() * 154f, 150f);

            m_presetScroll = GUI.BeginScrollView(posRect, m_presetScroll, viewRect, true, false);

            var buttonRect = new Rect(0f, 0f, 150f, 150f);

            foreach (var preset in presets)
            {
                if (GUI.Button(buttonRect, new GUIContent(preset.HandPoseImage, preset.Name)))
                {
                    switch (m_handEditMode)
                    {
                        case HandEditMode.Both:
                            m_hand.ApplyPreset(preset, HandType.LeftHand);
                            m_hand.ApplyPreset(preset, HandType.RightHand);
                            break;
                        case HandEditMode.Left:
                            m_hand.ApplyPreset(preset, HandType.LeftHand);
                            break;
                        case HandEditMode.Right:
                            m_hand.ApplyPreset(preset, HandType.RightHand);
                            break;
                    }
                }

                buttonRect.x += 154f;
            }

            GUI.EndScrollView();
        }
    }
}
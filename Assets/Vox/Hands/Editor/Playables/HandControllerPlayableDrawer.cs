using UnityEditor;
using UnityEngine;

namespace Vox.Hands
{
    [CustomPropertyDrawer(typeof(HandControllerPlayableBehaviour))]
    public class HandControllerPlayableDrawer : PropertyDrawer
    {
        private HandPoseDataEditorUtility m_utility;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int fieldCount = 35;
            return fieldCount * EditorGUIUtility.singleLineHeight + 200f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (m_utility == null)
            {
                m_utility = new HandPoseDataEditorUtility(property, "handPose.");
            }
            
            m_utility.DrawFingerControls(position);
                        
//            var singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
//            EditorGUI.PropertyField(singleFieldRect, bindValue01Prop);
//            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
//            EditorGUI.PropertyField(singleFieldRect, bindValue02Prop);
        }
    }
}
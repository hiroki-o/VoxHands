using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Vox.Hands {
	[CustomEditor(typeof(HandController))]
	public class HandControllerEditor : Editor {

		private class FingerProperties
		{
			private readonly SerializedProperty m_prop_spread;
			private readonly SerializedProperty m_prop_muscle1;
			private readonly SerializedProperty m_prop_muscle2;
			private readonly SerializedProperty m_prop_muscle3;
			private readonly string m_fingerName;

			public FingerProperties(SerializedObject serializedObject, string fingerName)
			{
				m_fingerName = fingerName;
				m_prop_spread = serializedObject.FindProperty("m_handPoseData." + fingerName.ToLower() + ".spread");
				m_prop_muscle1 = serializedObject.FindProperty("m_handPoseData." + fingerName.ToLower() + ".muscle1");
				m_prop_muscle2 = serializedObject.FindProperty("m_handPoseData." + fingerName.ToLower() + ".muscle2");
				m_prop_muscle3 = serializedObject.FindProperty("m_handPoseData." + fingerName.ToLower() + ".muscle3");
			}

			public float Spread
			{
				get => m_prop_spread.floatValue;
				set => m_prop_spread.floatValue = value;
			}

			public float Muscle1
			{
				get => m_prop_muscle1.floatValue;
				set => m_prop_muscle1.floatValue = value;
			}

			public float Muscle2
			{
				get => m_prop_muscle2.floatValue;
				set => m_prop_muscle2.floatValue = value;
			}

			public float Muscle3
			{
				get => m_prop_muscle3.floatValue;
				set => m_prop_muscle3.floatValue = value;
			}

			public SerializedProperty PropSpread => m_prop_spread;
			public SerializedProperty PropMuscle1 => m_prop_muscle1;
			public SerializedProperty PropMuscle2 => m_prop_muscle2;
			public SerializedProperty PropMuscle3 => m_prop_muscle3;

			public float MuscleAll
			{
				get => (m_prop_muscle1.floatValue + m_prop_muscle2.floatValue + m_prop_muscle3.floatValue) / 3.0f;
				set =>
					m_prop_muscle1.floatValue =
						m_prop_muscle2.floatValue =
							m_prop_muscle3.floatValue = value;
			}

			public string FingerName => m_fingerName;
		}

		private HandController m_currentTarget;
		private GUIContent m_empty;
		private FingerProperties[] m_fingers;
		private SerializedProperty m_prop_handType;
		private bool[] m_foldouts;

		private float AllSpread
		{
			get => m_fingers.Sum(f => f.Spread) / m_fingers.Length;
			set
			{
				foreach (var finger in m_fingers)
				{
					finger.Spread = value;
				}
			}
		}
		
		private float AllFingersMuscle
		{
			get => m_fingers.Sum(f => f.MuscleAll) / m_fingers.Length;
			set
			{
				foreach (var finger in m_fingers)
				{
					finger.MuscleAll = value;
				}
			}
		}

		private void ResetEditor()
		{
			m_currentTarget = target as HandController;
			m_empty = new GUIContent();
			m_prop_handType = serializedObject.FindProperty("m_handType");

			m_fingers = new[]
			{
				new FingerProperties(serializedObject, "Thumb"), 
				new FingerProperties(serializedObject, "Index"), 
				new FingerProperties(serializedObject, "Middle"), 
				new FingerProperties(serializedObject, "Ring"), 
				new FingerProperties(serializedObject, "Little")
			};
			
			m_foldouts = new bool[m_fingers.Length];
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			if (m_currentTarget != target)
			{
				ResetEditor();
			}

			EditorGUILayout.PropertyField(m_prop_handType);

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

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Focus", GUILayout.Width(50f)))
				{
					SetSceneViewCameraToHand();
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Save in AnimationClip"))
				{
					SaveHandInAnimationClip();
				}
			}

			GUILayout.Space(12f);

			for (var i = 0; i < m_fingers.Length; ++i)
			{
				var finger = m_fingers[i];
				m_foldouts[i] = EditorGUILayout.Foldout(m_foldouts[i], $"{finger.FingerName} finger");
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
						}

						EditorGUILayout.Slider(finger.PropSpread, -1f, 1f, "Spread");
						GUILayout.Space(4f);
						EditorGUILayout.Slider(finger.PropMuscle1, -1f, 1f, "1st");
						EditorGUILayout.Slider(finger.PropMuscle2, -1f, 1f, "2nd");
						EditorGUILayout.Slider(finger.PropMuscle3, -1f, 1f, "3rd");
					}
				}
				GUILayout.Space(8f);
			}

			serializedObject.ApplyModifiedProperties();
			if (GUI.changed)
			{
				m_currentTarget.InitializeRuntimeControl();
			}
		}

		private void SetSceneViewCameraToHand()
		{
			var hand = m_prop_handType.enumValueIndex == 0 ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
			var animator = m_currentTarget.GetComponent<Animator>();

			SceneView.lastActiveSceneView.pivot = animator.GetBoneTransform(hand).position;
			SceneView.lastActiveSceneView.rotation = Quaternion.identity;

			SceneView.lastActiveSceneView.Repaint();

		}

		private void SaveHandInAnimationClip()
		{
			var savePath = EditorUtility.SaveFilePanelInProject("Save New Animation", "New Animation", "anim", "Saving Finger Pose as Animation");
			if (string.IsNullOrEmpty(savePath))
			{
				return;
			}			
			
			var hand = m_prop_handType.enumValueIndex == 0 ? "LeftHand" : "RightHand";
			var newAnimation = new AnimationClip();

			foreach (var finger in m_fingers)
			{
				var spread = new AnimationCurve();
				var muscle1 = new AnimationCurve();
				var muscle2 = new AnimationCurve();
				var muscle3 = new AnimationCurve();

				spread.AddKey(0f, finger.Spread);
				muscle1.AddKey(0f, finger.Muscle1);
				muscle2.AddKey(0f, finger.Muscle2);
				muscle3.AddKey(0f, finger.Muscle3);

				spread.AddKey(1f, finger.Spread);
				muscle1.AddKey(1f, finger.Muscle1);
				muscle2.AddKey(1f, finger.Muscle2);
				muscle3.AddKey(1f, finger.Muscle3);
				
				newAnimation.SetCurve("", typeof(Animator), $"{hand}.{finger.FingerName}.Spread", spread);
				newAnimation.SetCurve("", typeof(Animator), $"{hand}.{finger.FingerName}.1 Stretched", muscle1);
				newAnimation.SetCurve("", typeof(Animator), $"{hand}.{finger.FingerName}.2 Stretched", muscle2);
				newAnimation.SetCurve("", typeof(Animator), $"{hand}.{finger.FingerName}.3 Stretched", muscle3);
			}
			
			AssetDatabase.CreateAsset(newAnimation, savePath);
			AssetDatabase.SaveAssets();
		}
	}
}


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Vox.Hands {
	
	[Serializable]
	public class FingerProperties
	{
		private readonly SerializedProperty m_prop_spread;
		private readonly SerializedProperty m_prop_muscle1;
		private readonly SerializedProperty m_prop_muscle2;
		private readonly SerializedProperty m_prop_muscle3;
		private readonly string m_fingerName;

		public FingerProperties(SerializedObject serializedObject, string rootPropertyPath, string fingerName)
		{
			m_fingerName = fingerName;
			m_prop_spread = serializedObject.FindProperty(rootPropertyPath + fingerName.ToLower() + ".spread");
			m_prop_muscle1 = serializedObject.FindProperty(rootPropertyPath + fingerName.ToLower() + ".muscle1");
			m_prop_muscle2 = serializedObject.FindProperty(rootPropertyPath + fingerName.ToLower() + ".muscle2");
			m_prop_muscle3 = serializedObject.FindProperty(rootPropertyPath + fingerName.ToLower() + ".muscle3");
		}
		
		public FingerProperties(SerializedProperty serializedProperty, string rootPropertyPath, string fingerName)
		{
			m_fingerName = fingerName;
			m_prop_spread = serializedProperty.FindPropertyRelative(rootPropertyPath + fingerName.ToLower() + ".spread");
			m_prop_muscle1 = serializedProperty.FindPropertyRelative(rootPropertyPath + fingerName.ToLower() + ".muscle1");
			m_prop_muscle2 = serializedProperty.FindPropertyRelative(rootPropertyPath + fingerName.ToLower() + ".muscle2");
			m_prop_muscle3 = serializedProperty.FindPropertyRelative(rootPropertyPath + fingerName.ToLower() + ".muscle3");
		}

		public float Spread
		{
			get { return m_prop_spread.floatValue; }
			set { m_prop_spread.floatValue = value; }
		}

		public float Muscle1
		{
			get { return m_prop_muscle1.floatValue; }
			set { m_prop_muscle1.floatValue = value; }
		}

		public float Muscle2
		{
			get { return m_prop_muscle2.floatValue; }
			set { m_prop_muscle2.floatValue = value; }
		}

		public float Muscle3
		{
			get
			{
				return m_prop_muscle3.floatValue;
			}
			set { m_prop_muscle3.floatValue = value; }
		}

		public SerializedProperty PropSpread
		{
			get
			{
				return m_prop_spread;
			}
		}

		public SerializedProperty PropMuscle1
		{
			get
			{
				return m_prop_muscle1;
			}
		}

		public SerializedProperty PropMuscle2
		{
			get
			{
				return m_prop_muscle2;
			}
		}

		public SerializedProperty PropMuscle3
		{
			get
			{
				return m_prop_muscle3;
			}
		}

		public float MuscleAll
		{
			get { return (m_prop_muscle1.floatValue + m_prop_muscle2.floatValue + m_prop_muscle3.floatValue) / 3.0f; }
			set
			{
				m_prop_muscle1.floatValue =
					m_prop_muscle2.floatValue =
						m_prop_muscle3.floatValue = value;
			}
		}

		public string FingerName
		{
			get
			{
				return m_fingerName;
			}
		}
	}
	
	[Serializable]
	public class HandPoseDataEditorUtility {

		private FingerProperties[] m_fingers;
		private bool[] m_foldouts;
		private bool m_presetFoldout;
		private string m_presetFilter;
		private Vector2 m_presetScroll;

		public FingerProperties[] Fingers
		{
			get
			{
				return m_fingers;
			}
		}

		private float AllSpread
		{
			get { return m_fingers.Sum(f => f.Spread) / m_fingers.Length; }
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
			get { return m_fingers.Sum(f => f.MuscleAll) / m_fingers.Length; }
			set
			{
				foreach (var finger in m_fingers)
				{
					finger.MuscleAll = value;
				}
			}
		}

		public HandPoseDataEditorUtility(SerializedObject serializedObject, string rootPropertyPath)
		{
			m_fingers = new[]
			{
				new FingerProperties(serializedObject, rootPropertyPath, "Thumb"), 
				new FingerProperties(serializedObject, rootPropertyPath, "Index"), 
				new FingerProperties(serializedObject, rootPropertyPath, "Middle"), 
				new FingerProperties(serializedObject, rootPropertyPath, "Ring"), 
				new FingerProperties(serializedObject, rootPropertyPath, "Little")
			};
			
			m_foldouts = new bool[m_fingers.Length];
		}
		
		public HandPoseDataEditorUtility(SerializedProperty serializedProperty, string rootPropertyPath)
		{
			m_fingers = new[]
			{
				new FingerProperties(serializedProperty, rootPropertyPath, "Thumb"), 
				new FingerProperties(serializedProperty, rootPropertyPath, "Index"), 
				new FingerProperties(serializedProperty, rootPropertyPath, "Middle"), 
				new FingerProperties(serializedProperty, rootPropertyPath, "Ring"), 
				new FingerProperties(serializedProperty, rootPropertyPath, "Little")
			};
			
			m_foldouts = new bool[m_fingers.Length];
		}
		

		public void DrawFingerControls(HandPosePresetsAsset presetsAsset)
		{
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

			for (var i = 0; i < m_fingers.Length; ++i)
			{
				var finger = m_fingers[i];
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

			DrawHandPosePreset(presetsAsset);
		}

		public void DrawFingerControls(Rect position, HandPosePresetsAsset presetsAsset)
		{
            var singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			
			var allSpreadValue = AllSpread;
			var newAllSpreadValue = EditorGUI.Slider(singleFieldRect, "Spread", allSpreadValue, -1f, 1f);
			if (allSpreadValue != newAllSpreadValue)
			{
				AllSpread = newAllSpreadValue;
			}
			
			var allFingersMuscleValue = AllFingersMuscle;
			singleFieldRect.y += EditorGUIUtility.singleLineHeight;
			var newAllFingersMuscleValue = EditorGUI.Slider(singleFieldRect,"Muscles", allFingersMuscleValue, -1f, 1f);
			if (allFingersMuscleValue != newAllFingersMuscleValue)
			{
				AllFingersMuscle = newAllFingersMuscleValue;
			}

			singleFieldRect.y += 12f;

			for (var i = 0; i < m_fingers.Length; ++i)
			{
				var finger = m_fingers[i];
				singleFieldRect.y += 4f;
				var allMuscleValue = finger.MuscleAll;
				singleFieldRect.y += EditorGUIUtility.singleLineHeight;
				var newAllMuscleValue = EditorGUI.Slider(singleFieldRect,finger.FingerName + " All", allMuscleValue, -1f, 1f);
				if (allMuscleValue != newAllMuscleValue)
				{
					finger.MuscleAll = newAllMuscleValue;
				}

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
			}
			singleFieldRect.y += 32f;

			DrawHandPosePreset(new Rect(singleFieldRect.x, singleFieldRect.y, position.width, position.height - (singleFieldRect.y - position.y)), 
				presetsAsset);
		}
		
		private void DrawHandPosePreset(HandPosePresetsAsset presetsAsset)
		{
			m_presetFoldout = EditorGUILayout.Foldout(m_presetFoldout, "Presets");
			if (m_presetFoldout)
			{
				m_presetFilter = EditorGUILayout.TextField("Filter", m_presetFilter);

				m_presetScroll = GUILayout.BeginScrollView(m_presetScroll, GUI.skin.box, GUILayout.Height(174f));
				using (new EditorGUILayout.HorizontalScope())
				{
					var presets = string.IsNullOrEmpty(m_presetFilter)
						? presetsAsset.SavedPresets
						: presetsAsset.SavedPresets.Where(p => p.Name.ToLower().Contains(m_presetFilter.ToLower()));
					foreach (var preset in presets)
					{
						if (GUILayout.Button(new GUIContent(preset.HandPoseImage, preset.Name), GUILayout.Width(150f), GUILayout.Height(150f)))
						{
							ApplyPreset(preset);
						}						
					}					
				}

				GUILayout.EndScrollView();
			}
		}
		
		private void DrawHandPosePreset(Rect position, HandPosePresetsAsset presetsAsset)
		{
			var singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			GUI.Label(singleFieldRect,"Presets", "BoldLabel");
			singleFieldRect.y += EditorGUIUtility.singleLineHeight;
			m_presetFilter = EditorGUI.TextField(singleFieldRect,"Filter", m_presetFilter);
			singleFieldRect.y += EditorGUIUtility.singleLineHeight;

			singleFieldRect.y += 12f;

			var presets = string.IsNullOrEmpty(m_presetFilter)
				? presetsAsset.SavedPresets
				: presetsAsset.SavedPresets.Where(p => p.Name.ToLower().Contains(m_presetFilter.ToLower()));
			
			var posRect = new Rect(position.x, singleFieldRect.y, position.width, 174f);
			var viewRect = new Rect(0f, 0f, presets.Count() * 154f, 150f);

			m_presetScroll = GUI.BeginScrollView( posRect, m_presetScroll, viewRect, true, false);
			
			var buttonRect = new Rect(0f, 0f, 150f, 150f);
			
			foreach (var preset in presets)
			{
				if (GUI.Button(buttonRect, new GUIContent(preset.HandPoseImage, preset.Name)))
				{
					ApplyPreset(preset);
				}
				buttonRect.x += 154f;
			}					

			GUI.EndScrollView();
		}
		
		
		private void ApplyPreset(HandPosePreset preset)
		{
			var pose = preset.HandPoseData;
			for (var f = 0; f < HandPoseData.HumanFingerCount; ++f)
			{
				m_fingers[f].Spread	= pose[f].spread;
				m_fingers[f].Muscle1 = pose[f].muscle1;
				m_fingers[f].Muscle2 = pose[f].muscle2;
				m_fingers[f].Muscle3 = pose[f].muscle3;
			}
		}

		public void SaveCurrentToPreset(HandPosePresetsAsset presetsAsset, string name, Texture2D icon)
		{
			var pose = new HandPoseData
			{
				thumb =
				{
					spread = m_fingers[0].Spread,
					muscle1 = m_fingers[0].Muscle1,
					muscle2 = m_fingers[0].Muscle2,
					muscle3 = m_fingers[0].Muscle3
				},
				index =
				{
					spread = m_fingers[1].Spread,
					muscle1 = m_fingers[1].Muscle1,
					muscle2 = m_fingers[1].Muscle2,
					muscle3 = m_fingers[1].Muscle3
				},
				middle =
				{
					spread = m_fingers[2].Spread,
					muscle1 = m_fingers[2].Muscle1,
					muscle2 = m_fingers[2].Muscle2,
					muscle3 = m_fingers[2].Muscle3
				},
				ring =
				{
					spread = m_fingers[3].Spread,
					muscle1 = m_fingers[3].Muscle1,
					muscle2 = m_fingers[3].Muscle2,
					muscle3 = m_fingers[3].Muscle3
				},
				little =
				{
					spread = m_fingers[4].Spread,
					muscle1 = m_fingers[4].Muscle1,
					muscle2 = m_fingers[4].Muscle2,
					muscle3 = m_fingers[4].Muscle3
				}
			};

			var newPreset = new HandPosePreset(name, ref pose, icon);			
			presetsAsset.AddNewPreset(newPreset);
		}
	}
}


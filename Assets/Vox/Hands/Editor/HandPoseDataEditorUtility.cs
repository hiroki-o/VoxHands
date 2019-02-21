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

		private readonly FingerProperties[] m_leftFingers;
		private readonly FingerProperties[] m_rightFingers;

		public FingerProperties[] LeftFingers
		{
			get
			{
				return m_leftFingers;
			}
		}

		public FingerProperties[] RightFingers
		{
			get
			{
				return m_rightFingers;
			}
		}
		
		public float AllSpread
		{
			get { return (AllLeftSpread + AllRightSpread) /2f; }
			set
			{
				AllLeftSpread = value;
				AllRightSpread = value;
			}
		}
		
		public float AllFingersMuscle
		{
			get { return (AllLeftFingersMuscle + AllRightFingersMuscle) /2f; }
			set
			{
				AllLeftFingersMuscle = value;
				AllRightFingersMuscle = value;
			}
		}
		
		public float AllLeftSpread
		{
			get { return m_leftFingers.Sum(f => f.Spread) / m_leftFingers.Length; }
			set
			{
				foreach (var finger in m_leftFingers)
				{
					finger.Spread = value;
				}
			}
		}
		
		public float AllLeftFingersMuscle
		{
			get { return m_leftFingers.Sum(f => f.MuscleAll) / m_leftFingers.Length; }
			set
			{
				foreach (var finger in m_leftFingers)
				{
					finger.MuscleAll = value;
				}
			}
		}
		
		public float AllRightSpread
		{
			get { return m_rightFingers.Sum(f => f.Spread) / m_rightFingers.Length; }
			set
			{
				foreach (var finger in m_rightFingers)
				{
					finger.Spread = value;
				}
			}
		}
		
		public float AllRightFingersMuscle
		{
			get { return m_rightFingers.Sum(f => f.MuscleAll) / m_rightFingers.Length; }
			set
			{
				foreach (var finger in m_rightFingers)
				{
					finger.MuscleAll = value;
				}
			}
		}

		public HandPoseDataEditorUtility(SerializedObject serializedObject, string leftRootPropertyPath, string rightRootPropertyPath)
		{
			m_leftFingers = new[]
			{
				new FingerProperties(serializedObject, leftRootPropertyPath, "Thumb"), 
				new FingerProperties(serializedObject, leftRootPropertyPath, "Index"), 
				new FingerProperties(serializedObject, leftRootPropertyPath, "Middle"), 
				new FingerProperties(serializedObject, leftRootPropertyPath, "Ring"), 
				new FingerProperties(serializedObject, leftRootPropertyPath, "Little")
			};
			m_rightFingers = new[]
			{
				new FingerProperties(serializedObject, rightRootPropertyPath, "Thumb"), 
				new FingerProperties(serializedObject, rightRootPropertyPath, "Index"), 
				new FingerProperties(serializedObject, rightRootPropertyPath, "Middle"), 
				new FingerProperties(serializedObject, rightRootPropertyPath, "Ring"), 
				new FingerProperties(serializedObject, rightRootPropertyPath, "Little")
			};
		}
		
		public HandPoseDataEditorUtility(SerializedProperty serializedProperty, string leftRootPropertyPath, string rightRootPropertyPath)
		{
			m_leftFingers = new[]
			{
				new FingerProperties(serializedProperty, leftRootPropertyPath, "Thumb"), 
				new FingerProperties(serializedProperty, leftRootPropertyPath, "Index"), 
				new FingerProperties(serializedProperty, leftRootPropertyPath, "Middle"), 
				new FingerProperties(serializedProperty, leftRootPropertyPath, "Ring"), 
				new FingerProperties(serializedProperty, leftRootPropertyPath, "Little")
			};
			m_rightFingers = new[]
			{
				new FingerProperties(serializedProperty, rightRootPropertyPath, "Thumb"), 
				new FingerProperties(serializedProperty, rightRootPropertyPath, "Index"), 
				new FingerProperties(serializedProperty, rightRootPropertyPath, "Middle"), 
				new FingerProperties(serializedProperty, rightRootPropertyPath, "Ring"), 
				new FingerProperties(serializedProperty, rightRootPropertyPath, "Little")
			};
		}
				
		public void ApplyPreset(HandPosePreset preset, HandType hand)
		{
			var pose = preset.HandPoseData;
			if (hand == HandType.LeftHand)
			{
				for (var f = 0; f < HandPoseData.HumanFingerCount; ++f)
				{
					m_leftFingers[f].Spread	= pose[f].spread;
					m_leftFingers[f].Muscle1 = pose[f].muscle1;
					m_leftFingers[f].Muscle2 = pose[f].muscle2;
					m_leftFingers[f].Muscle3 = pose[f].muscle3;
				}
			}
			else
			{
				for (var f = 0; f < HandPoseData.HumanFingerCount; ++f)
				{
					m_rightFingers[f].Spread	= pose[f].spread;
					m_rightFingers[f].Muscle1 = pose[f].muscle1;
					m_rightFingers[f].Muscle2 = pose[f].muscle2;
					m_rightFingers[f].Muscle3 = pose[f].muscle3;
				}
			}
		}

		public void SaveCurrentToPreset(HandPosePresetsAsset presetsAsset, HandType hand, string name, Texture2D icon)
		{
			HandPoseData pose;

			if (hand == HandType.LeftHand)
			{
				pose = new HandPoseData
				{
					thumb =
					{
						spread  = m_leftFingers[0].Spread,
						muscle1 = m_leftFingers[0].Muscle1,
						muscle2 = m_leftFingers[0].Muscle2,
						muscle3 = m_leftFingers[0].Muscle3
					},
					index =
					{
						spread  = m_leftFingers[1].Spread,
						muscle1 = m_leftFingers[1].Muscle1,
						muscle2 = m_leftFingers[1].Muscle2,
						muscle3 = m_leftFingers[1].Muscle3
					},
					middle =
					{
						spread  = m_leftFingers[2].Spread,
						muscle1 = m_leftFingers[2].Muscle1,
						muscle2 = m_leftFingers[2].Muscle2,
						muscle3 = m_leftFingers[2].Muscle3
					},
					ring =
					{
						spread  = m_leftFingers[3].Spread,
						muscle1 = m_leftFingers[3].Muscle1,
						muscle2 = m_leftFingers[3].Muscle2,
						muscle3 = m_leftFingers[3].Muscle3
					},
					little =
					{
						spread  = m_leftFingers[4].Spread,
						muscle1 = m_leftFingers[4].Muscle1,
						muscle2 = m_leftFingers[4].Muscle2,
						muscle3 = m_leftFingers[4].Muscle3
					}
				};
			}
			else
			{
				pose = new HandPoseData
				{
					thumb =
					{
						spread  = m_rightFingers[0].Spread,
						muscle1 = m_rightFingers[0].Muscle1,
						muscle2 = m_rightFingers[0].Muscle2,
						muscle3 = m_rightFingers[0].Muscle3
					},
					index =
					{
						spread  = m_rightFingers[1].Spread,
						muscle1 = m_rightFingers[1].Muscle1,
						muscle2 = m_rightFingers[1].Muscle2,
						muscle3 = m_rightFingers[1].Muscle3
					},
					middle =
					{
						spread  = m_rightFingers[2].Spread,
						muscle1 = m_rightFingers[2].Muscle1,
						muscle2 = m_rightFingers[2].Muscle2,
						muscle3 = m_rightFingers[2].Muscle3
					},
					ring =
					{
						spread  = m_rightFingers[3].Spread,
						muscle1 = m_rightFingers[3].Muscle1,
						muscle2 = m_rightFingers[3].Muscle2,
						muscle3 = m_rightFingers[3].Muscle3
					},
					little =
					{
						spread  = m_rightFingers[4].Spread,
						muscle1 = m_rightFingers[4].Muscle1,
						muscle2 = m_rightFingers[4].Muscle2,
						muscle3 = m_rightFingers[4].Muscle3
					}
				};
			}

			var newPreset = new HandPosePreset(name, ref pose, icon);			
			presetsAsset.AddNewPreset(newPreset);
		}
	}
}


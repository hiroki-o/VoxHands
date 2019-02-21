using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

		private const int kPresetIconSize = 128;
		
		private HandController m_currentTarget;
		private SerializedProperty m_prop_handType;
		private SerializedProperty m_prop_preset;
		private HandPoseDataEditorUtility m_handPoseUtility;

		[SerializeField] private TabMode m_tabMode = TabMode.FingerControls;

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
		
		private void ResetEditor()
		{
			m_currentTarget = target as HandController;
			m_prop_handType = serializedObject.FindProperty("m_handType");
			m_prop_preset = serializedObject.FindProperty("m_preset");
			m_handPoseUtility = new HandPoseDataEditorUtility(serializedObject, "m_handPoseData.");
			m_presetSaveState = PresetSaveState.None;
		}

		private void DrawTabs()
		{
			var tabMode = m_tabMode;

			EditorGUI.BeginChangeCheck();
			using (new EditorGUILayout.HorizontalScope()) {
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

		private void DrawAnimationExport()
		{
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
			
			if (m_currentTarget != target || m_prop_handType == null)
			{
				ResetEditor();
			}

			if (m_prop_preset.objectReferenceValue == null)
			{
				m_prop_preset.objectReferenceValue = HandPosePresetsAsset.GetDefaultGetPresetsAsset();
			}

			EditorGUILayout.PropertyField(m_prop_preset);
			GUILayout.Space(8f);			
			EditorGUILayout.PropertyField(m_prop_handType);

			if (GUILayout.Button("Focus", GUILayout.Width(50f)))
			{
				SetSceneViewCameraToHand();
			}
			
			GUILayout.Space(12f);
			DrawTabs();
			switch (m_tabMode)
			{
				case TabMode.FingerControls:
					m_handPoseUtility.DrawFingerControls(m_prop_preset.objectReferenceValue as HandPosePresetsAsset);
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
		
		public override bool RequiresConstantRepaint() {
			return m_presetSaveState != PresetSaveState.None;
		}

		private void SetSceneViewCameraToHand()
		{
			var hand = m_prop_handType.enumValueIndex == 0 ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
			var animator = m_currentTarget.GetComponent<Animator>();

			SceneView.lastActiveSceneView.pivot = animator.GetBoneTransform(hand).position;
			SceneView.lastActiveSceneView.Repaint();
		}

		private void HandleSceneViewMouseEvents()
		{
			if (Event.current.button != 0)
			{
				return;
			}
			var curRect = new Rect(m_selectPoint, new Vector2(kPresetIconSize,kPresetIconSize));
			var curPos = Event.current.mousePosition;
			if(!curRect.Contains(curPos))
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
				var resolutionRatio = Mathf.Round(SceneView.lastActiveSceneView.camera.pixelHeight / SceneView.lastActiveSceneView.position.height);
				
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
					var scale = (float)kPresetIconSize / (float)m_presetCaptureIcon.width;
					m_presetCaptureIcon = TextureUtility.CreateScaledTexture(m_presetCaptureIcon, scale);
				}
				
				m_handPoseUtility.SaveCurrentToPreset(
					m_prop_preset.objectReferenceValue as HandPosePresetsAsset, 
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
			if (GUI.Button(closePos,"X", tbLabel))
			{
				EndSaveCurrentToPreset(false);
			}
			
			var sel = new Rect(m_selectPoint.x, m_selectPoint.y, 128f, 128f);
			GUI.Label(sel, string.Empty, "SelectionRect");
			
			var namePos = new Rect(m_selectPoint.x, m_selectPoint.y + kPresetIconSize + 4f, 128f, 20f);
			m_newPresetName = EditorGUI.TextField(namePos, m_newPresetName);
			
			var buttonPos = new Rect(namePos.x, namePos.y + 22f, 128f, 20f);
			if (GUI.Button(buttonPos,"Save to Preset"))
			{
				m_presetSaveState = PresetSaveState.Capturing;
				Camera.onPostRender += OnPresetIconCapturePostRenderScreenView;
			}
			
			Handles.EndGUI();
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

			foreach (var finger in m_handPoseUtility.Fingers)
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


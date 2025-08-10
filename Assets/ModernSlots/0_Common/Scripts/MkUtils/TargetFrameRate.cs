using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif
/*
	18.03.2024
	05.02.2020
 */

namespace Mkey
{
	public class TargetFrameRate : MonoBehaviour
	{
		[SerializeField]
		private int targetFrameRate = 35;
		[SerializeField]
		private bool defaultFrameRate = false;

		#region regular
		void Awake()
		{
			Validate();
			Application.targetFrameRate = targetFrameRate;
		}

		private void OnValidate()
		{
			Validate();
		}
		#endregion regular

		private void Validate()
		{
			targetFrameRate = (defaultFrameRate) ? -1: Mathf.Max(-1, targetFrameRate);
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(TargetFrameRate))]
	public class TargetFrameRateEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			GUILayout.Space(8);
			if (LinkLabel(new GUIContent("Help for target frame rate")))
			{
				Application.OpenURL("https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html");
			}
		}

		private bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
		{
			var position = GUILayoutUtility.GetRect(label, EditorStyles.linkLabel, options);

			Handles.BeginGUI();
			Handles.color = EditorStyles.linkLabel.normal.textColor;
			Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
			Handles.color = Color.white;
			Handles.EndGUI();

			EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
			return GUI.Button(position, label, EditorStyles.linkLabel);
		}
	}
#endif
}
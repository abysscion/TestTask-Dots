using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
	/// <summary> Script keeps camera ortho size focused around target renderers </summary>
	[ExecuteAlways]
	public class CameraSizeAdjuster : MonoBehaviour
	{
		[SerializeField] private List<Renderer> targetRenderers = new();

		private void Awake()
		{
			UpdateCameraSize();
		}

		private void OnRectTransformDimensionsChange()
		{
			UpdateCameraSize();
		}

		public void UpdateCameraSize()
		{
			if (Camera.main)
				Utility.SetCameraOrtographicSizeToFitRenderers(Camera.main, targetRenderers, true);
		}

		[UnityEditor.CustomEditor(typeof(CameraSizeAdjuster))]
		private class CameraSizeKeeperEditor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();
				if (GUILayout.Button("Update camera size"))
					(target as CameraSizeAdjuster).UpdateCameraSize();
			}
		}
	}
}


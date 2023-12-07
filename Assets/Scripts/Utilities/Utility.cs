using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
	public static class Utility
	{
		public static bool IsMaskContainsLayer(LayerMask mask, int layer)
		{
			return mask == (mask | (1 << layer));
		}

		public static bool TryDetectComponentUnderMouseViaRaycast<T>(LayerMask mask, out T component) where T : Component
		{
			component = null;

			Camera cam = Camera.main;
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, mask.value);

			foreach (RaycastHit hit in hits)
			{
				if (hit.transform.TryGetComponent(out component))
					return true;
			}
			return false;
		}

		public static void GetMinMaxAmongRenderers(IReadOnlyCollection<Renderer> renderers, out Vector2 min, out Vector2 max)
		{
			var atLeastOneRendererValid = false;
			max = new Vector2(float.MinValue, float.MinValue);
			min = new Vector2(float.MaxValue, float.MaxValue);
			foreach (Renderer renderer in renderers)
			{
				if (!renderer)
					continue;
				atLeastOneRendererValid = true;
				Vector3 minPos = renderer.bounds.min;
				Vector3 maxPos = renderer.bounds.max;
				min.x = minPos.x < min.x ? minPos.x : min.x;
				min.y = minPos.z < min.y ? minPos.z : min.y;
				max.x = maxPos.x > max.x ? maxPos.x : max.x;
				max.y = maxPos.z > max.y ? maxPos.z : max.y;
			}
			if (!atLeastOneRendererValid)
			{
				min = new Vector2(0f, 0f);
				max = new Vector2(0f, 0f);
			}
		}

		public static (Vector2 min, Vector2 max) GetMinMaxAroundRectTransforms(IReadOnlyCollection<RectTransform> rectTransforms)
		{
			var worldMinPos = new Vector2 { x = float.MaxValue, y = float.MaxValue };
			var worldMaxPos = new Vector2 { x = float.MinValue, y = float.MinValue };
			var cornersBuf = new Vector3[4];
			foreach (RectTransform rt in rectTransforms)
			{
				rt.GetWorldCorners(cornersBuf);
				foreach (Vector3 corner in cornersBuf)
				{
					worldMinPos.x = Mathf.Min(corner.x, worldMinPos.x);
					worldMinPos.y = Mathf.Min(corner.y, worldMinPos.y);
					worldMaxPos.x = Mathf.Max(corner.x, worldMaxPos.x);
					worldMaxPos.y = Mathf.Max(corner.y, worldMaxPos.y);
				}
			}
			return (worldMinPos, worldMaxPos);
		}

		public static void FitOneToManyRectTransforms(RectTransform target, IReadOnlyCollection<RectTransform> others)
		{
			if (others.Count == 0)
				return;

			(Vector2 worldMinPos, Vector2 worldMaxPos) = GetMinMaxAroundRectTransforms(others);
			Vector3 canvasMin = target.worldToLocalMatrix.MultiplyPoint(worldMinPos);
			Vector3 canvasMax = target.worldToLocalMatrix.MultiplyPoint(worldMaxPos);
			var worldWidth = Mathf.Abs(worldMaxPos.x - worldMinPos.x);
			var worldHeight = Mathf.Abs(worldMaxPos.y - worldMinPos.y);
			var canvasWidth = Mathf.Abs(canvasMax.x - canvasMin.x);
			var canvasHeight = Mathf.Abs(canvasMax.y - canvasMin.y);
			target.pivot = Vector2.one * 0.5f;
			target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvasWidth);
			target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvasHeight);
			target.position = new Vector3(worldMinPos.x + worldWidth * 0.5f, worldMinPos.y + worldHeight * 0.5f, 0f);
		}

		public static void SetCameraOrtographicSizeToFitRenderers(Camera cam, IReadOnlyCollection<Renderer> renderers, bool is2dPreset = false)
		{
			GetMinMaxAmongRenderers(renderers, out Vector2 min, out Vector2 max);
			Transform camTf = cam.transform;
			var desiredUnitsWidth = max.x - min.x;
			var desiredUnitsHeight = max.y - min.y;
			var camY = camTf.position.y;
			var camPos = new Vector2(min.x + desiredUnitsWidth * 0.5f, min.y + desiredUnitsHeight * 0.5f);
			var desiredVerticalSize = desiredUnitsHeight * 0.5f;
			var desiredHorizontalSize = (desiredUnitsWidth * 0.5f) / ((float)Screen.width / Screen.height);

			cam.orthographicSize = desiredVerticalSize > desiredHorizontalSize ? desiredVerticalSize : desiredHorizontalSize;
			camTf.position = is2dPreset ? new Vector3(camPos.x, camPos.y, camTf.position.z) : new Vector3(camPos.x, camY, camPos.y);
		}

		public static Vector2 GetSpriteRendererWorldSize(SpriteRenderer sr, bool ignoreScale = false)
		{
			var width = (ignoreScale ? 1f : sr.transform.localScale.x) * sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
			var height = (ignoreScale ? 1f : sr.transform.localScale.y) * sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
			return new Vector2(width, height);
		}
	}
}

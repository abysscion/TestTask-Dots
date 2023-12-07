using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Core.Field.FieldControllerComboChecker;

namespace Core.Field
{
	public class FieldControllerAnimator : MonoBehaviour
	{
		[SerializeField] private FieldController fieldController;
		[SerializeField] private AnimationCurve shrinkAnimationCurve;

		public void StartShrinkAnimation(CellsComboNode root, CellsComboNode last, Action completedCallback = null)
		{
			StopAllCoroutines();
			StartCoroutine(CompleteComboCoroutine(root, last, completedCallback));
		}

		private IEnumerator CompleteComboCoroutine(CellsComboNode root, CellsComboNode last, Action completedCallback = null)
		{
			var duration = shrinkAnimationCurve.keys[shrinkAnimationCurve.length - 1].time;
			var timeElapsed = 0f;
			Vector3 initialCellScale = root.cell.transform.localScale;
			CellsComboNode tmpNode = root;

			//shrink combo cells =====================================================
			while (timeElapsed < duration)
			{
				tmpNode = root;
				var completionPercent = shrinkAnimationCurve.Evaluate(timeElapsed);
				var curCellScale = Vector3.Lerp(initialCellScale, Vector3.zero, completionPercent);
				while (tmpNode != null)
				{
					tmpNode.cell.transform.localScale = curCellScale;
					tmpNode = tmpNode.next;
				}
				timeElapsed += Time.deltaTime;
				yield return null;
			}

			//remember cells we need to "move" =====================================================
			var shrinkedCells = new HashSet<FieldCell>();
			var cellsToFallAmountDict = new Dictionary<FieldCell, int>();
			tmpNode = root;
			while (tmpNode != null)
			{
				shrinkedCells.Add(tmpNode.cell);
				tmpNode = tmpNode.next;
			}
			foreach (var cell in shrinkedCells)
			{
				for (var i = cell.Position.y; i < fieldController.FieldSize.y; i++)
				{
					var targetCell = fieldController.GetCell(cell.Position.x, i);
					if (shrinkedCells.Contains(targetCell))
						continue;
					if (!cellsToFallAmountDict.TryGetValue(targetCell, out var _))
						cellsToFallAmountDict.Add(targetCell, 0);
					cellsToFallAmountDict[targetCell]++;
				}
			}
			var newCellToNewColorsDict = new Dictionary<FieldCell, DotColor>();
			foreach (var cell in cellsToFallAmountDict.Keys)
			{
				var fallAmount = cellsToFallAmountDict[cell];
				var newCell = fieldController.GetCell(cell.Position.x, cell.Position.y - fallAmount);
				var newColor = cell.DotColor;
				newCellToNewColorsDict.Add(newCell, newColor);
			}

			//recolor cells as they are moved ======================================================
			yield return new WaitForSeconds(0.75f);
			foreach (var cell in cellsToFallAmountDict.Keys)
				cell.transform.localScale = Vector3.zero;
			foreach (var cell in newCellToNewColorsDict.Keys)
			{
				cell.SetColor(newCellToNewColorsDict[cell]);
				cell.transform.localScale = initialCellScale;
			}

			//recolor cells as they are moved =====================================================
			yield return new WaitForSeconds(0.75f);
			for (var y = 0; y < fieldController.FieldSize.y; y++)
			{
				for (var x = 0; x < fieldController.FieldSize.x; x++)
				{
					var cell = fieldController.GetCell(x, y);
					if ((initialCellScale - cell.transform.localScale).sqrMagnitude <= Mathf.Epsilon)
						continue;
					cell.SetColor(DotColorExtension.GetRandomDotColor());
					cell.transform.localScale = initialCellScale;
				}
			}
			
			completedCallback?.Invoke();
		}
	}
}

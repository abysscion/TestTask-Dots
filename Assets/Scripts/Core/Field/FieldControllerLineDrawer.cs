using System.Collections;
using UnityEngine;
using static Core.Field.FieldControllerComboChecker;

namespace Core.Field
{
	public class FieldControllerLineDrawer : MonoBehaviour
	{
		[SerializeField] private FieldControllerComboChecker fieldComboChecker;
		[SerializeField] private FieldController fieldController;
		[SerializeField] private LineRenderer dynamicLineRenderer;
		[SerializeField] private LineRenderer staticLineRenderer;

		private Coroutine _lineDrawingCoroutine;

		private void Start()
		{
			staticLineRenderer.positionCount = 0;
			staticLineRenderer.widthMultiplier = fieldController.FieldCellSize * 0.5f;
			staticLineRenderer.enabled = false;
			dynamicLineRenderer.enabled = false;
			fieldComboChecker.ComboStarted += OnComboStarted;
			fieldComboChecker.ComboEnded += OnComboEnded;
			fieldComboChecker.ComboChanged += OnComboChanged;
		}

		private void OnComboChanged(CellsComboNode root, CellsComboNode current)
		{
			if (_lineDrawingCoroutine == null)
				return;

			CellsComboNode node = root.next;
			staticLineRenderer.positionCount = 1;
			staticLineRenderer.SetPosition(0, (Vector2)root.cell.transform.position);
			while (node != null)
			{
				staticLineRenderer.positionCount++;
				staticLineRenderer.SetPosition(staticLineRenderer.positionCount - 1, (Vector2)node.cell.transform.position);
				dynamicLineRenderer.SetPosition(0, (Vector2)node.cell.transform.position);
				node = node.next;
			}
		}

		private void OnComboStarted(CellsComboNode root)
		{
			staticLineRenderer.positionCount = 1;
			staticLineRenderer.SetPosition(0, (Vector2)root.cell.transform.position);
			staticLineRenderer.startColor = root.cell.DotColor.DotColorToColor();
			staticLineRenderer.endColor = root.cell.DotColor.DotColorToColor();

			dynamicLineRenderer.positionCount = 2;
			dynamicLineRenderer.SetPosition(0, (Vector2)root.cell.transform.position);
			dynamicLineRenderer.startColor = root.cell.DotColor.DotColorToColor();
			dynamicLineRenderer.endColor = root.cell.DotColor.DotColorToColor();

			if (_lineDrawingCoroutine != null)
				StopCoroutine(_lineDrawingCoroutine);
			_lineDrawingCoroutine = StartCoroutine(LineDrawingCoroutine());

			dynamicLineRenderer.enabled = true;
			staticLineRenderer.enabled = true;
		}

		private void OnComboEnded(CellsComboNode root, CellsComboNode last)
		{
			if (_lineDrawingCoroutine != null)
				StopCoroutine(_lineDrawingCoroutine);
			staticLineRenderer.enabled = false;
			dynamicLineRenderer.enabled = false;
		}

		private IEnumerator LineDrawingCoroutine()
		{
			while (true)
			{
				if (Input.touchCount == 0)
					yield return null;
				Touch touch = Input.GetTouch(0);
				if (touch.phase != TouchPhase.Moved)
					yield return null;
				dynamicLineRenderer.SetPosition(1, (Vector2)(Camera.main.ScreenToWorldPoint(touch.position)));
				yield return null;
			}
		}
	}
}

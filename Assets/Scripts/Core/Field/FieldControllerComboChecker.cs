using System.Collections.Generic;
using UnityEngine;

namespace Core.Field
{
	public class FieldControllerComboChecker : MonoBehaviour
	{
		[SerializeField] private FieldController fieldController;

		private CellsComboNode _currentComboNode;
		private CellsComboNode _rootComboNode;

		private HashSet<FieldCell> _visitedCells = new();

		public event System.Action<CellsComboNode> ComboStarted;
		/// <summary> T1 is root node, T2 is current node </summary>
		public event System.Action<CellsComboNode, CellsComboNode> ComboChanged;
		/// <summary> T1 is root node, T2 is current node </summary>
		public event System.Action<CellsComboNode, CellsComboNode> ComboEnded;

		private void Start()
		{
			fieldController.CellPressed += OnCellPressed;
			fieldController.CellReleased += OnCellReleased;
			fieldController.CellEntered += OnCellEntered;
			fieldController.CellExited += OnCellExited;
		}

		private void OnCellExited(FieldCell cell)
		{

		}

		private void OnCellEntered(FieldCell cell)
		{
			if (_currentComboNode == null)
				return;

			if (!cell.IsAdjacentToOtherCell(_currentComboNode.cell)
				|| cell.DotColor != _currentComboNode.cell.DotColor
				|| _visitedCells.Contains(cell))
			{
				return;
			}

			_visitedCells.Add(cell);
			var newNode = new CellsComboNode(cell);
			_currentComboNode.next = newNode;
			_currentComboNode = newNode;
			ComboChanged?.Invoke(_rootComboNode, _currentComboNode);
		}

		private void OnCellPressed(FieldCell cell)
		{
			_visitedCells.Clear();
			_rootComboNode = _currentComboNode = new CellsComboNode(cell);
			_visitedCells.Add(cell);
			ComboStarted?.Invoke(_rootComboNode);
		}

		private void OnCellReleased(FieldCell cell)
		{
			//CellsComboNode node = _rootComboNode;
			//var str = "";
			//while (node != null)
			//{
			//	str += $"{node.cell.Position} => ";
			//	node = node.next;
			//}
			//Debug.Log("Combo: " + str);
			ComboEnded?.Invoke(_rootComboNode, _currentComboNode);
		}

		public class CellsComboNode
		{
			public FieldCell cell;
			public CellsComboNode next;

			public CellsComboNode(FieldCell cell)
			{
				this.cell = cell;
			}
		}
	}
}

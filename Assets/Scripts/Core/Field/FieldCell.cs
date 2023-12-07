using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Field
{
	[RequireComponent(typeof(Collider2D))]
	public class FieldCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		[SerializeField] private SpriteRenderer cellFrame;
		[SerializeField] private SpriteRenderer dotView;

		public event Action<FieldCell> OnPressed;
		public event Action<FieldCell> OnReleased;
		public event Action<FieldCell> OnEntered;
		public event Action<FieldCell> OnExited;

		public SpriteRenderer CellFrame => cellFrame;
		public DotColor DotColor { get; private set; }
		public Vector2Int Position { get; private set; }

		public void OnPointerEnter(PointerEventData eventData) => OnEntered?.Invoke(this);

		public void OnPointerExit(PointerEventData eventData) => OnExited?.Invoke(this);

		public void OnPointerDown(PointerEventData eventData) => OnPressed?.Invoke(this);

		public void OnPointerUp(PointerEventData eventData) => OnReleased?.Invoke(this);

		public void Init(Vector2Int pos, DotColor color)
		{
			Position = pos;
			SetColor(color);
		}

		public void SetColor(DotColor color)
		{
			DotColor = color;
			dotView.color = color.DotColorToColor();
		}
	}

	public static class FieldCellExtensions
	{
		public static bool IsAdjacentToOtherCell(this FieldCell cell, FieldCell otherCell)
		{
			return (cell.Position - otherCell.Position).magnitude == 1f;
		}
	}
}

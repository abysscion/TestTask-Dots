using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static Core.Field.FieldControllerComboChecker;

namespace Core.Field
{
	public class FieldController : MonoBehaviour
	{
		[SerializeField] private FieldControllerComboChecker comboChecker;
		[SerializeField] private FieldControllerAnimator fieldAnimator;
		[SerializeField] private SpriteRenderer fieldFrame;
		[SerializeField] private EventSystem eventSystem;
		[SerializeField] private FieldCell cellPrefab;
		[SerializeField] private Vector2Int fieldSize = new(6, 6);

		public event Action<int> PlayerScoreChanged;
		public event Action<FieldCell> CellPressed;
		public event Action<FieldCell> CellReleased;
		public event Action<FieldCell> CellEntered;
		public event Action<FieldCell> CellExited;

		private const int MinComboLength = 2;
		private const string ScorePrefsKey = "player_score";
		private FieldCell[,] _field = new FieldCell[0, 0];
		private int _playerScore;

		public Vector2Int FieldSize => fieldSize;
		public float FieldCellSize => _field.Length > 0 ? _field[0, 0].transform.localScale.x : 0f;

		public int PlayerScore
		{
			get => _playerScore;
			private set
			{
				_playerScore = value;
				PlayerScoreChanged?.Invoke(_playerScore);
			}
		}

		private void Awake()
		{
			ClearCells();
			CreateField();

			PlayerScore = PlayerPrefs.HasKey(ScorePrefsKey) ? PlayerPrefs.GetInt(ScorePrefsKey) : 0;
			comboChecker.ComboEnded += OnComboEnded;
		}

		private void OnApplicationQuit()
		{
			PlayerPrefs.SetInt(ScorePrefsKey, PlayerScore);
		}

		public FieldCell GetCell(int x, int y) => _field[x, y];

		private void CreateField()
		{
			Vector2 fieldWorldSize = Utilities.Utility.GetSpriteRendererWorldSize(fieldFrame);
			var targetCellPixelSize = new Vector2(fieldWorldSize.x / fieldSize.x, fieldWorldSize.y / fieldSize.y);
			Vector2 cellWorldSize = Utilities.Utility.GetSpriteRendererWorldSize(cellPrefab.CellFrame, true);
			var targetCellScale = new Vector2(targetCellPixelSize.x / cellWorldSize.x, targetCellPixelSize.y / cellWorldSize.y);
			Vector3 cellsSpawnPos = fieldFrame.transform.position + (Vector3)fieldWorldSize * -0.5f + (Vector3)targetCellPixelSize * 0.5f;
			_field = new FieldCell[fieldSize.x, fieldSize.y];
			for (var y = 0; y < fieldSize.y; y++)
			{
				for (var x = 0; x < fieldSize.x; x++)
				{
					FieldCell cell = Instantiate(cellPrefab, transform);
					cell.Init(new Vector2Int(x, y), DotColorExtension.GetRandomDotColor());
					cell.name = $"CellTile [{x}, {y}]";
					cell.transform.localPosition = new Vector3(cellsSpawnPos.x + x * targetCellPixelSize.x, cellsSpawnPos.y + y * targetCellPixelSize.y, 0f);
					cell.transform.localScale = targetCellScale;
					cell.OnPressed += OnCellPressed;
					cell.OnReleased += OnCellReleased;
					cell.OnEntered += OnCellEntered;
					cell.OnExited += OnCellExited;
					_field[x, y] = cell;
				}
			}
		}

		private void ClearCells()
		{
			for (var i = transform.childCount - 1; i > -1; i--)
			{
				if (Application.isPlaying)
					Destroy(transform.GetChild(i).gameObject);
				else
					DestroyImmediate(transform.GetChild(i).gameObject);
			}
		}

		private void OnComboEnded(CellsComboNode root, CellsComboNode last)
		{
			var comboCounter = CalculateComboSize(root);
			if (comboCounter < MinComboLength)
				return;

			eventSystem.enabled = false;
			fieldAnimator.StartShrinkAnimation(root, last, EnableInput);
			PlayerScore += Mathf.CeilToInt(Mathf.Pow(5 * comboCounter, 1 + comboCounter * 0.1f));
		}

		private int CalculateComboSize(CellsComboNode root)
		{
			CellsComboNode tmpNode = root;
			var counter = 0;
			while (tmpNode != null)
			{
				counter++;
				tmpNode = tmpNode.next;
			}
			return counter;
		}

		private void EnableInput()
		{
			eventSystem.enabled = true;
		}

		private void OnCellPressed(FieldCell cell) => CellPressed?.Invoke(cell);
		private void OnCellReleased(FieldCell cell) => CellReleased?.Invoke(cell);
		private void OnCellExited(FieldCell cell) => CellExited?.Invoke(cell);
		private void OnCellEntered(FieldCell cell) => CellEntered?.Invoke(cell);

#if UNITY_EDITOR
		[UnityEditor.CustomEditor(typeof(FieldController))]
		public class FieldControllerEditor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();
				if (GUILayout.Button("Regenerate field"))
				{
					var fieldController = target as FieldController;
					fieldController.ClearCells();
					fieldController.CreateField();
				}
			}
		}
#endif
	}
}

/*
			private void GenerateCellsWithGapIncluded(SpriteRenderer cellPrefab, int fieldWidth, int fieldHeight)
			{
				var fieldBG = FieldBackgroundProperty.objectReferenceValue as SpriteRenderer;
				var fieldCellsGap = FieldCellsGapProperty.vector2Value;
				var fieldPixelSize = GetRealSpriteRendererSize(fieldBG);
				var fieldPixelSizeWithGap = new Vector2()
				{
					x = fieldPixelSize.x - ((fieldWidth - 1) * fieldCellsGap.x),
					y = fieldPixelSize.y - ((fieldHeight - 1) * fieldCellsGap.y),
				};
				var targetCellPixelSize = new Vector2(fieldPixelSizeWithGap.x / fieldWidth, fieldPixelSizeWithGap.y / fieldHeight);
				var realCellPixelSize = GetRealSpriteRendererSize(cellPrefab, true);
				var cellScale = new Vector2(targetCellPixelSize.x / realCellPixelSize.x, targetCellPixelSize.y / realCellPixelSize.y);
				var cellsSpawnPos = fieldBG.transform.position + (Vector3)fieldPixelSize * -0.5f + (Vector3)targetCellPixelSize * 0.5f;
				var cellTotalOffset = new Vector2(targetCellPixelSize.x + fieldCellsGap.x, targetCellPixelSize.y + fieldCellsGap.y);
				for (var y = 0; y < fieldHeight; y++)
				{
					for (var x = 0; x < fieldWidth; x++)
					{
						var cell = PrefabUtility.InstantiatePrefab(cellPrefab.gameObject, _scriptTf) as GameObject;
						cell.name = $"CellTile [{x}, {y}]";
						cell.transform.localPosition = new Vector3(cellsSpawnPos.x + x * cellTotalOffset.x, cellsSpawnPos.y + y * cellTotalOffset.y, 0f);
						cell.transform.localScale = cellScale;
					}
				}
			}
			*/

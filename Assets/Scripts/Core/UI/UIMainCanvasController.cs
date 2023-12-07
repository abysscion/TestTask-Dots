using Core.Field;
using TMPro;
using UnityEngine;

namespace Core.UI
{
	public class UIMainCanvasController : MonoBehaviour
	{
		[SerializeField] private FieldController fieldController;
		[SerializeField] private TMP_Text labelScore;

		private void Start()
		{
			fieldController.PlayerScoreChanged += OnPlayerScoreChanged;
			OnPlayerScoreChanged(fieldController.PlayerScore);
		}

		private void OnPlayerScoreChanged(int newValue)
		{
			labelScore.text = $"{fieldController.PlayerScore}";
		}
	}
}

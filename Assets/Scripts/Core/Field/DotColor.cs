using UnityEngine;

namespace Core.Field
{
	public enum DotColor
	{
		Red,
		Green,
		Blue,
		Purple,
		Yellow,
		Brown
	}

	public static class DotColorExtension
	{
		public static Color DotColorToColor(this DotColor dotColor) => dotColor switch
		{
			DotColor.Red => Color.red,
			DotColor.Green => Color.green,
			DotColor.Blue => Color.blue,
			DotColor.Purple => new Color(0.5f, 0f, 0.5f),
			DotColor.Yellow => new Color(1f, 1f, 0f),
			DotColor.Brown => new Color(0.8f, 0.4f, 0f),
			_ => throw new System.NotImplementedException()
		};

		public static DotColor GetRandomDotColor()
		{
			System.Array values = System.Enum.GetValues(typeof(DotColor));
			return (DotColor)values.GetValue(Random.Range(0, values.Length));
		}
	}
}

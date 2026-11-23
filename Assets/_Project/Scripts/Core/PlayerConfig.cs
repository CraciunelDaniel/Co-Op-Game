using UnityEngine;

namespace Core
{
	[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Game/Player Config")]
	public class PlayerConfig : ScriptableObject
	{
		[field: SerializeField] public float MoveSpeed { get; private set; } = 6f;
		[field: SerializeField] public float Gravity { get; private set; } = -20f;
		[field: SerializeField] public float MouseSensitivity { get; private set; } = -0.15f;
		[field: SerializeField] public float MaxPitchAngle { get; private set; } = 80f;

		[field: SerializeField] public Color[] ColorPalette { get; private set; } =
		{
			Color.white, Color.red, Color.green,
			Color.blue, Color.yellow, Color.cyan,
			Color.magenta
		};
	}
}

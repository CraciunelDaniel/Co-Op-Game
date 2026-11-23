using UnityEngine;

namespace Core
{
	[CreateAssetMenu(fileName = "InteractionConfig", menuName = "Game/Interaction Config")]
	public class InteractionConfig : ScriptableObject
	{
		[field:SerializeField] public float MaxGrabDistance {get;private set;}=3.5f;
		[field: SerializeField] public float SpawnDistance { get; private set; } = 2.0f;
		[field: SerializeField] public float ThrowForce { get; private set; } = 12f;
		[field: SerializeField] public float HoldFollowSpeed { get; private set; } = 20f; 
		[field:SerializeField] public LayerMask InteractionLayer{get; private set; }
	}
}

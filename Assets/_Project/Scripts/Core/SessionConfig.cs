using UnityEngine;

namespace Core
{
	public enum TransportType
	{
		Utp,
		Steam
	}
	[CreateAssetMenu(fileName = "SessionConfig", menuName = "Game/SessionConfig")]
	public class SessionConfig : ScriptableObject
	{
		[field: SerializeField] public TransportType DefaultTransport { get;private set; }= TransportType.Utp;
		[field: SerializeField] public int MaxPlayers { get;private  set; } = 4;
		[field: SerializeField] public string DefaultIp{get;private set;} = "127.0.0.1";
		[field: SerializeField] public ushort DefaultPort { get; private set; } = 7777;
		[field: SerializeField] public string MenuSceneName{get;private set;} = "Menu";
		[field: SerializeField] public string GameplaySceneName{get;private set;} = "Gameplay";
	}
}

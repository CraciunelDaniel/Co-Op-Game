using System;

namespace Scripts.Core
{
	[Serializable]
	public struct ConnectionPayload
	{
		public string PlayerId;
		public string PlayerName;
		public string BuildVersion;
	}

	public readonly struct ConnectedPlayerInfo
	{
		public ulong ClientId {get;}
		public string PlayerId {get;}
		public string PlayerName {get;}

		public ConnectedPlayerInfo(ulong clientId, string playerId, string playerName)
		{
			ClientId = clientId;
			PlayerId = playerId;
			PlayerName = playerName;
		}
	}
}

using System;
using System.Collections.Generic;

namespace Core
{
	public readonly struct LobbyPlayerInfo
	{
		public ulong ClientId { get; }
		public string Name { get; }
		public bool IsHost { get; }
		public int PingMs { get; }

		public LobbyPlayerInfo(ulong clientId, string name, bool isHost,int pingMs)
		{
			ClientId = clientId;
			Name = name;
			IsHost = isHost;
			PingMs = pingMs;
		}
	}

	public interface ISessionStateService
	{
		bool IsGameStarted { get; }
		IReadOnlyList<LobbyPlayerInfo> Players { get; }

		event Action OnPlayersChanged;
		event Action<bool> OnLoadingScreenToggled;

		void StartGame();
		void KickPlayer(ulong clientId);
	}
}

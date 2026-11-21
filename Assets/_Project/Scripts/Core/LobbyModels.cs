using System;
using System.Collections.Generic;

namespace Scripts.Core
{
	public readonly struct LobbyPlayerInfo
	{
		public ulong ClientId { get; }
		public string Name { get; }
		public bool IsHost { get; }

		public LobbyPlayerInfo(ulong clientId, string name, bool isHost)
		{
			ClientId = clientId;
			Name = name;
			IsHost = isHost;
		}
	}

	public interface ISessionStateService
	{
		bool IsGameStarted { get; }
		IReadOnlyList<LobbyPlayerInfo> Players { get; }

		event Action OnPlayersChanged;
		event Action<bool> OnLoadingScreenToggled;

		void StartGame();
	}
}

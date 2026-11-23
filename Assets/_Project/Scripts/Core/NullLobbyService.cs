using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
	public class NullLobbyService : ILobbyService
	{
		public bool IsSupported => false;
		public string CurrentLobbyId => string.Empty;
		public event Action<string> OnJoinRequested;

		public Task<bool> CreateLobbyAsync(LobbySettings settings) => Task.FromResult(false);

		public Task<string> JoinLobbyAsync(string lobbyId) => Task.FromResult(string.Empty);

		public void UpdateSessionState(int playerCount, int maxPlayers, bool inGame) { }

		public void LeaveLobby() { }
		public void OpenInviteOverlay() { }
	}

	public class NullRichPresenceService : IRichPresenceService
	{
		public void SetStatus(string statusKey, string value) { }
		public void Clear() { }
	}

	public class LoggingAchievementService : IAchievementService
	{
		public void IncrementStat(string statName, int amount = 1)
		{
			Debug.Log($"[Stats]{statName}+={amount}");
		}

		public void Unlock(string achievementId)
		{
			Debug.Log($"[Achievements] Unlock {achievementId}");
		}

		public void StoreStats()
		{
			Debug.Log($"[Stats] StoreStats");
		}
	}
}
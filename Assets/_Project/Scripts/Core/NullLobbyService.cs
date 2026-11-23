using System;
using System.Threading.Tasks;

namespace Core
{
	public class NullLobbyService : ILobbyService
	{
		public bool IsSupported => false;
		public string CurrentLobbyId => string.Empty;
		public event Action<string> OnJoinRequested;
		public Task<bool> CreateLobbyAsync(int maxPlayers) => Task.FromResult(false);
		public Task<string> JoinLobbyAsync(string lobbyId) => Task.FromResult(string.Empty);
		public void LeaveLobby() { }
		public void OpenInviteOverlay() { }
	}

	public class NullRichPresenceService : IRichPresenceService
	{
		public void SetStatus(string statusKey, string value) { }
		public void Clear() { }
	}

	public class NullAchievementService : IAchievementService
	{
		public void Unlock(string achievementId) { }
	}
}
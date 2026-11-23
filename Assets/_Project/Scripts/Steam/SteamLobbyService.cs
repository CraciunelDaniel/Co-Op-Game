using System;
using System.Threading.Tasks;
using Core;
using Steamworks;
using Steamworks.Data;

namespace Steam
{
	public class SteamLobbyService : ILobbyService, IDisposable
	{
		private const string NAME_KEY = "name";
		private const string STATE_KEY = "state";
		private const string CLOSED_KEY = "closed";
		private const string HOST_ADDRESS_KEY = "HostAddress";

		private Lobby? _currentLobby;

		public bool IsSupported => true;
		private bool IsLobbyOwner => _currentLobby.HasValue && _currentLobby.Value.Owner.Id == SteamClient.SteamId;
		public string CurrentLobbyId => _currentLobby?.Id.Value.ToString() ?? string.Empty;
		public event Action<string> OnJoinRequested;

		public SteamLobbyService()
		{
			SteamFriends.OnGameLobbyJoinRequested += HandleLobbyJoinRequested;
		}

		public async Task<bool> CreateLobbyAsync(LobbySettings lobbySettings)
		{
			if (!SteamClient.IsValid) return false;

			var created = await SteamMatchmaking.CreateLobbyAsync(lobbySettings.MaxPlayers);
			if (!created.HasValue) return false;

			var lobby = created.Value;
			if (lobbySettings.Visibility == LobbyVisibility.FriendsOnly)
				lobby.SetFriendsOnly();
			else lobby.SetPrivate();

			lobby.SetData(NAME_KEY, lobbySettings.Name);
			lobby.SetData(HOST_ADDRESS_KEY, SteamClient.SteamId.Value.ToString());
			lobby.SetData(STATE_KEY, "lobby");
			lobby.SetJoinable(true);
			_currentLobby = lobby;

			return true;
		}

		public async Task<string> JoinLobbyAsync(string lobbyIdStr)
		{
			if (!SteamClient.IsValid || !ulong.TryParse(lobbyIdStr?.Trim(), out ulong lobbyId))
				return string.Empty;

			var lobby = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
			if (!lobby.HasValue) return string.Empty;

			_currentLobby = lobby.Value;
			if (_currentLobby.Value.GetData(CLOSED_KEY) == "1")
				return string.Empty;
			return _currentLobby.Value.GetData(HOST_ADDRESS_KEY) ?? string.Empty;
		}

		public void UpdateSessionState(int playerCount, int maxPlayers, bool inGame)
		{
			if (!IsLobbyOwner) return;

			var lobby = _currentLobby.Value;
			lobby.SetJoinable(playerCount < maxPlayers);
			lobby.SetData(STATE_KEY, inGame ? "in_game" : "lobby");
		}

		public string GetHostSteamId()
		{
			return _currentLobby?.GetData(HOST_ADDRESS_KEY) ?? string.Empty;
		}

		public void LeaveLobby()
		{
			if (IsLobbyOwner)
			{
				_currentLobby.Value.SetJoinable(false);
				_currentLobby.Value.SetData(CLOSED_KEY, "1");
			}

			_currentLobby?.Leave();
			_currentLobby = null;
		}

		public void OpenInviteOverlay()
		{
			if (_currentLobby.HasValue)
			{
				SteamFriends.OpenGameInviteOverlay(_currentLobby.Value.Id);
			}
		}

		private async void HandleLobbyJoinRequested(Lobby lobby, SteamId friendId)
		{
			var joined = await lobby.Join();
			if (joined == RoomEnter.Success)
			{
				_currentLobby = lobby;
				string hostSteamId = GetHostSteamId();
				if (!string.IsNullOrEmpty(hostSteamId))
				{
					OnJoinRequested?.Invoke(hostSteamId);
				}
			}
		}

		public void Dispose()
		{
			SteamFriends.OnGameLobbyJoinRequested -= HandleLobbyJoinRequested;
		}
	}
}
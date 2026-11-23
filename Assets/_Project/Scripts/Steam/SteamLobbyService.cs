using System;
using System.Threading.Tasks;
using Core;
using Steamworks;
using Steamworks.Data;

namespace Steam
{
	public class SteamLobbyService:ILobbyService,IDisposable
	{
		
		private const string HostAddressKey = "HostAddress";
		private Lobby? _currentLobby;

		public bool IsSupported => true;
		public string CurrentLobbyId =>_currentLobby?.Id.Value.ToString()??string.Empty;
		public event Action<string> OnJoinRequested;

		public SteamLobbyService()
		{
			SteamFriends.OnGameLobbyJoinRequested += HandleLobbyJoinRequested;
		}
		

		public async Task<bool> CreateLobbyAsync(int maxPlayers)
		{
			if (!SteamClient.IsValid) return false;

			var lobby = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
			if (!lobby.HasValue) return false;
			
			_currentLobby = lobby;
			_currentLobby.Value.SetPublic();
			_currentLobby.Value.SetJoinable(true);
			_currentLobby.Value.SetData(HostAddressKey,SteamClient.SteamId.Value.ToString());
			return true;
		}
		public async Task<string> JoinLobbyAsync(string lobbyIdStr)
		{
			if (!SteamClient.IsValid || !ulong.TryParse(lobbyIdStr?.Trim(), out ulong lobbyId))
				return string.Empty;

			var lobby = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
			if (!lobby.HasValue) return string.Empty;

			_currentLobby = lobby.Value;
			return _currentLobby.Value.GetData(HostAddressKey) ?? string.Empty;
		}

		public string GetHostSteamId()
		{
			return _currentLobby?.GetData(HostAddressKey)??string.Empty;
		}
		public void LeaveLobby()
		{
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
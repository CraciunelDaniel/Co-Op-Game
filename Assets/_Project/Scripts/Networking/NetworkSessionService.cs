using System;
using System.Text;
using System.Threading.Tasks;
using Core;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
	public class NetworkSessionService:INetworkSessionService,IDisposable
	{
		private readonly NetworkManager _networkManager;
		private readonly ITransportConfigurator _transportConfigurator;
		private readonly IPlayerIdentityProvider _identityProvider;
		private readonly ConnectionApprovalHandler _approvalHandler;
		private readonly SessionConfig _config;
		private readonly ILobbyService _lobbyService;
		
		private bool _isConnected;
		
		public string LastDisconnectReason { get; private set; } 
		public bool IsHost =>_networkManager.IsHost;
		public bool IsConnected =>_networkManager.IsClient && _networkManager.IsConnectedClient;
		public event Action OnConnected;
		public event Action<string> OnDisconnected;

		public NetworkSessionService(NetworkManager networkManager,
			ITransportConfigurator transportConfigurator,
			IPlayerIdentityProvider identityProvider,
			ConnectionApprovalHandler approvalHandler,
			SessionConfig config,
			ILobbyService lobbyService)
		{
			_networkManager = networkManager;
			_transportConfigurator = transportConfigurator;
			_identityProvider = identityProvider;
			_approvalHandler = approvalHandler;
			_config=config;
			_lobbyService=lobbyService;
			
			_networkManager.OnClientConnectedCallback += HandleClientConnected;
			_networkManager.OnClientDisconnectCallback += HandleClientDisconnected;

			_lobbyService.OnJoinRequested += HandleLobbyJoinRequested;
		}
		public void StartHost(ushort port)
		{
			LastDisconnectReason = string.Empty;
			
			PrepareConnectionPayload();
			_transportConfigurator.ConfigureForHost(port);
			_approvalHandler.Clear();
			_networkManager.ConnectionApprovalCallback = _approvalHandler.Approve;
			
			_isConnected = true;
			if (!_networkManager.StartHost())
			{
				_isConnected = false;
				OnDisconnected?.Invoke("Failed to start host.");
			}

			if (_lobbyService.IsSupported)
			{
				_=CreateLobbyForHostAsync();
			}
		}

		private async Task CreateLobbyForHostAsync()
		{
			await _lobbyService.CreateLobbyAsync(_config.MaxPlayers);
		}
		public void StartClient(string address, ushort port)
		{
			LastDisconnectReason = string.Empty;
			
			PrepareConnectionPayload();
			_transportConfigurator.ConfigureForClient(address, port);
			
			_isConnected = true;
			if (!_networkManager.StartClient())
			{
				_isConnected = false;
				OnDisconnected?.Invoke("Failed to start client.");
			}
			
		}
		public void Shutdown()
		{
			_isConnected = false;
			_approvalHandler.Clear();
			if (_networkManager.IsListening)
			{
				_networkManager.Shutdown();
			}
			_lobbyService.LeaveLobby();
			ReturnToMenu();
		}

		private void PrepareConnectionPayload()
		{
			var payload = new ConnectionPayload
			{
				PlayerId = _identityProvider.PlayerId,
				PlayerName = _identityProvider.DisplayName,
				BuildVersion = Application.version
			};
			string json = JsonUtility.ToJson(payload);
			_networkManager.NetworkConfig.ConnectionData=Encoding.UTF8.GetBytes(json);
		}

		private void HandleLobbyJoinRequested(string hostSteamId)
		{
			if (_networkManager.IsListening)
			{
				_networkManager.Shutdown();
			}
			StartClient(hostSteamId,0);
		}
		private void HandleClientConnected(ulong clientId)
		{
			if (clientId == _networkManager.LocalClientId)
			{
				_isConnected = false;
				OnConnected?.Invoke();
			}
		}

		private void HandleClientDisconnected(ulong clientId)
		{
			if(_networkManager.IsServer)
			{
				_approvalHandler.RemoveClient(clientId);
			}

			if (clientId == _networkManager.LocalClientId)
			{
				string reason = _networkManager.DisconnectReason;
				if (string.IsNullOrEmpty(reason))
				{
					reason= _isConnected?"Connection timed out.": "Disconnected from host.";
				}
				_isConnected = false;
				LastDisconnectReason = reason;
				_lobbyService.LeaveLobby();
				OnDisconnected?.Invoke(reason);
				
				
				ReturnToMenu();
			}
		}

		private void ReturnToMenu()
		{
			if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != _config.MenuSceneName)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				UnityEngine.SceneManagement.SceneManager.LoadScene(_config.MenuSceneName);
			}
		}

		public void Dispose()
		{
			if (_networkManager != null)
			{
				_networkManager.OnClientConnectedCallback -= HandleClientConnected;
				_networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
				_lobbyService.OnJoinRequested -= HandleLobbyJoinRequested;
				_networkManager.ConnectionApprovalCallback = null;
			}
		}
	}
}

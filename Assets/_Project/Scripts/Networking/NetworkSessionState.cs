using System;
using System.Collections.Generic;
using Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{
	public class NetworkSessionState : NetworkBehaviour
	{
		public static NetworkSessionState Instance { get; private set; }

		private readonly NetworkList<NetworkPlayerData> _players = new();
		private readonly NetworkVariable<bool> _isGameStarted=new (false);

		private readonly HashSet<ulong> _pendingSceneLoadClients = new();
		private bool _isWaitingForSceneLoad;
		private const float PING_UPDATE_INTERVAL = 1f;
		private float _pingTimer;
		
		private ConnectionApprovalHandler _approvalHandler;
		private SessionConfig _config;
		
		public bool IsGameStarted => _isGameStarted.Value;

		public event Action OnPlayersChanged;
		public event Action<bool> OnLoadingScreenToggled;

		public void InitializeServerDependencies(ConnectionApprovalHandler approvalHandler,SessionConfig config)
		{
			_approvalHandler = approvalHandler;
			_config = config;
		}

		private void Update()
		{
			if(!IsServer||!IsSpawned)return;
			
			_pingTimer += Time.unscaledDeltaTime;
			if(_pingTimer<PING_UPDATE_INTERVAL)return;
			_pingTimer = 0;

			UpdateClientsPing();
		}

		public override void OnNetworkSpawn()
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

			_players.OnListChanged += HandlePlayersListChanged ;

			if (IsServer)
			{
				foreach ( var kvp in _approvalHandler.ConnectedPlayers)
				{
					AddPlayersToList(kvp.Key, kvp.Value.PlayerName);
				}

				NetworkManager.OnClientConnectedCallback += HandleClientConnected;
				NetworkManager.OnClientDisconnectCallback += HandleClientDisconnected;
				NetworkManager.SceneManager.OnLoadComplete += HandleClientSceneLoadComplete;
			}
			
			OnPlayersChanged?.Invoke();
		}

		public override void OnNetworkDespawn()
		{
			_players.OnListChanged -= HandlePlayersListChanged;
			
			if(IsServer &&NetworkManager!=null)
			{
				NetworkManager.OnClientConnectedCallback -= HandleClientConnected;
				NetworkManager.OnClientDisconnectCallback -= HandleClientDisconnected;

				if (NetworkManager.SceneManager != null)
				{
					NetworkManager.SceneManager.OnLoadComplete -= HandleClientSceneLoadComplete;
				}
			}
			if(Instance==this) Instance = null;
		}
		

		public List<LobbyPlayerInfo> GetPlayersSnapshot()
		{
			var result = new List<LobbyPlayerInfo>(_players.Count);
			foreach (var p in _players)
			{
				result.Add(new LobbyPlayerInfo(p.ClientId,p.Name.ToString(),p.ClientId==NetworkManager.ServerClientId,p.PingMs));
			}
			return result;
		}

		public void ServerStartGame()
		{
			if(!IsServer||_isGameStarted.Value) return;
			
			_isGameStarted.Value = true;
			_isWaitingForSceneLoad=true;
			_pendingSceneLoadClients.Clear();

			foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
			{
				_pendingSceneLoadClients.Add(clientId);
			}
			
			SetLoadingScreenToggledRpc(true);
			NetworkManager.SceneManager.LoadScene(_config.GameplaySceneName, LoadSceneMode.Single);
		}

		public void ServerKickPlayer(ulong clientId)
		{
			if(!IsServer|| clientId==NetworkManager.ServerClientId) return;
			
			NetworkManager.DisconnectClient(clientId,"You were kicked by the host.");
		}
		private void HandleClientConnected(ulong clientId)
		{
			if (_approvalHandler.ConnectedPlayers.TryGetValue(clientId, out var info))
			{
				AddPlayersToList(clientId,info.PlayerName);
			}
		}

		private void HandleClientDisconnected(ulong clientId)
		{
			for (int i = 0; i < _players.Count; i++)
			{
				if (_players[i].ClientId == clientId)
				{
					_players.RemoveAt(i);
					break;
				}
			}

			if (_isWaitingForSceneLoad && _pendingSceneLoadClients.Remove(clientId))
			{
				CheckAllClientsLoaded();
			}
		}

		private void HandleClientSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
		{
			if(!_isWaitingForSceneLoad||sceneName != _config.GameplaySceneName)return;

			if (_pendingSceneLoadClients.Remove(clientId))
			{
					CheckAllClientsLoaded();
			}
		}
		private void CheckAllClientsLoaded()
		{
			if (_pendingSceneLoadClients.Count == 0)
			{
				_isWaitingForSceneLoad = false;
				SetLoadingScreenToggledRpc(false);
			}
		}
		
		private void AddPlayersToList(ulong clientId, string playerName)
		{
			for (int i = 0; i < _players.Count; i++)
			{
				if(_players[i].ClientId == clientId)
					return;
			}

			_players.Add(new NetworkPlayerData()
			{
				ClientId = clientId,
				Name = playerName
			});
		}
		private void HandlePlayersListChanged(NetworkListEvent<NetworkPlayerData> changeEvent)
		{
			OnPlayersChanged?.Invoke();
		}
		[Rpc(SendTo.ClientsAndHost)]
		private void SetLoadingScreenToggledRpc(bool visible)
		{
			OnLoadingScreenToggled?.Invoke(visible);
		}

		private void UpdateClientsPing()
		{
			var transport = NetworkManager.NetworkConfig.NetworkTransport;
			if(transport == null)return;

			for (int i = 0; i < _players.Count; i++)
			{
				var data= _players[i];
				ushort rtt = data.ClientId == NetworkManager.ServerClientId
					? (ushort) 0
					: (ushort) Mathf.Clamp((int) transport.GetCurrentRtt(data.ClientId), 0, 9999);
				if (data.PingMs != rtt)
				{
					data.PingMs = rtt;
					_players[i] = data;
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using Scripts.Core;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;

namespace Scripts.Networking
{
	public class SessionStateService : ISessionStateService, IDisposable
	{
		private readonly NetworkManager _networkManager;
		private readonly ConnectionApprovalHandler _approvalHandler;
		private readonly SessionConfig _config;
		private readonly NetworkSessionState _sessionStatePrefab;

		private NetworkSessionState _activeState;

		public bool IsGameStarted => _activeState != null && _activeState.IsGameStarted;

		public IReadOnlyList<LobbyPlayerInfo> Players =>
			_activeState != null ? _activeState.GetPlayersSnapshot() : Array.Empty<LobbyPlayerInfo>();

		public event Action OnPlayersChanged;
		public event Action<bool> OnLoadingScreenToggled;
		
		public SessionStateService(NetworkManager networkManager,
			ConnectionApprovalHandler approvalHandler,
			SessionConfig config,
			NetworkSessionState sessionStatePrefab)
		{
			_networkManager = networkManager;
			_approvalHandler = approvalHandler;
			_config = config;
			_sessionStatePrefab = sessionStatePrefab;
			
			_networkManager.OnServerStarted += HandleServerStarted;
			_networkManager.OnClientConnectedCallback+= HandleClientConnected;
			_networkManager.OnClientDisconnectCallback+= HandleClientDisconnected;
		}

		public void StartGame()
		{
			_activeState.ServerStartGame();
		}

		private void HandleServerStarted()
		{
			var prefabInstance = UnityEngine.Object.Instantiate(_sessionStatePrefab.gameObject);

			var instance = prefabInstance.GetComponent<NetworkSessionState>();
			instance.InitializeServerDependencies(_approvalHandler,_config);
			instance.NetworkObject.Spawn(destroyWithScene:false);
			BindState(instance);
		}
		private void HandleClientConnected(ulong clientId)
		{
			if (clientId == _networkManager.LocalClientId && !_networkManager.IsServer)
			{
				if (NetworkSessionState.Instance != null)
				{
					BindState(NetworkSessionState.Instance);
				}
			}
		}
		private void HandleClientDisconnected(ulong clientId)
		{
			if (clientId == _networkManager.LocalClientId)
			{
				UnbindState();
				OnLoadingScreenToggled?.Invoke(false);
				OnPlayersChanged?.Invoke();
			}
		}
		private void BindState(NetworkSessionState state)
		{
			if(_activeState ==state)return;
			UnbindState();
			
			_activeState = state;
			_activeState.OnPlayersChanged += HandlePlayersChanged;
			_activeState.OnLoadingScreenToggled += HandleLoadingScreenToggled;
			OnPlayersChanged?.Invoke();
		}
		private void UnbindState()
		{
			if (_activeState == null) return;
			_activeState.OnPlayersChanged -= HandlePlayersChanged;
			_activeState.OnLoadingScreenToggled -= HandleLoadingScreenToggled;
			_activeState = null;
		}

		private void HandlePlayersChanged() => OnPlayersChanged?.Invoke();
		private void HandleLoadingScreenToggled(bool visible) => OnLoadingScreenToggled?.Invoke(visible);
		public void Dispose()
		{
			UnbindState();
			if (_networkManager != null)
			{
				_networkManager.OnServerStarted -= HandleServerStarted;
				_networkManager.OnClientConnectedCallback -= HandleClientConnected;
				_networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
			}
		}
	}
}
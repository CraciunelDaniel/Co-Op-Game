using System;
using System.Collections.Generic;
using Core;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Networking
{
	public class SessionStateService : ISessionStateService, IDisposable, INetworkPrefabInstanceHandler
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

			_networkManager.PrefabHandler.AddHandler(_sessionStatePrefab.gameObject, this);
			_networkManager.OnServerStarted += HandleServerStarted;
			_networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
		}

		public void StartGame()
		{
			_activeState?.ServerStartGame();
		}

		public void KickPlayer(ulong clientId)
		{
			_activeState?.ServerKickPlayer(clientId);
		}

		private void HandleServerStarted()
		{
			var instance = UnityEngine.Object.Instantiate(_sessionStatePrefab);
			instance.InitializeServerDependencies(_approvalHandler, _config);
			BindState(instance);
			instance.NetworkObject.Spawn(destroyWithScene: false);
		}

		public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
		{
			var instance = UnityEngine.Object.Instantiate(_sessionStatePrefab, position, rotation);
			BindState(instance);
			return instance.NetworkObject;
		}

		public void Destroy(NetworkObject networkObject)
		{
			if (_activeState != null && _activeState.NetworkObject == networkObject)
			{
				UnbindState();
				OnPlayersChanged?.Invoke();
			}

			Object.Destroy(networkObject.gameObject);
		}

		private void HandleClientDisconnected(ulong clientId)
		{
			if (clientId != _networkManager.LocalClientId) return;

			UnbindState();
			OnLoadingScreenToggled?.Invoke(false);
			OnPlayersChanged?.Invoke();
		}

		private void BindState(NetworkSessionState state)
		{
			if (_activeState == state) return;
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
				_networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
			}
		}
	}
}
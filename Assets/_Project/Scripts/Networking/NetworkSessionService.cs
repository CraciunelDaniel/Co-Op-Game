using System;
using System.Text;
using Scripts.Core;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Networking
{
	public class NetworkSessionService:INetworkSessionService,IDisposable
	{
		private readonly NetworkManager _networkManager;
		private readonly ITransportConfigurator _transportConfigurator;
		private readonly IPlayerIdentityProvider _identityProvider;
		private readonly ConnectionApprovalHandler _approvalHandler;
		
		private bool _isConnected;
		
		public bool IsHost =>_networkManager.IsHost;
		public bool IsConnected =>_networkManager.IsClient && _networkManager.IsConnectedClient;
		public event Action OnConnected;
		public event Action<string> OnDisconnected;

		public NetworkSessionService(NetworkManager networkManager,
			ITransportConfigurator transportConfigurator,
			IPlayerIdentityProvider identityProvider,
			ConnectionApprovalHandler approvalHandler)
		{
			_networkManager = networkManager;
			_transportConfigurator = transportConfigurator;
			_identityProvider = identityProvider;
			_approvalHandler = approvalHandler;

			_networkManager.OnClientConnectedCallback += HandleClientConnected;
		}
		public void StartHost(ushort port)
		{
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
		}
		public void StartClient(string address, ushort port)
		{
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
				OnDisconnected?.Invoke(reason);
			}
		}

	
		public void Dispose()
		{
			if (_networkManager != null)
			{
				_networkManager.OnClientConnectedCallback -= HandleClientConnected;
				_networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
				_networkManager.ConnectionApprovalCallback = null;
			}
		}
	}
}

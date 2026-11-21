using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scripts.Core;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Networking
{
	public class ConnectionApprovalHandler
	{
		private readonly NetworkManager _networkManager;
		private readonly SessionConfig _config;
		private readonly Dictionary<ulong, ConnectedPlayerInfo> _connectedPlayers = new();

		public IReadOnlyDictionary<ulong, ConnectedPlayerInfo> ConnectedPlayers => _connectedPlayers;

		public ConnectionApprovalHandler(NetworkManager networkManager, SessionConfig config)
		{
			_networkManager = networkManager;
			_config = config;
		}

		public void Approve(NetworkManager.ConnectionApprovalRequest request,
			NetworkManager.ConnectionApprovalResponse response)
		{
			var staleClientIds = _connectedPlayers.Keys.Where(id => !_networkManager.ConnectedClientsIds.Contains(id)).ToList();
			foreach (ulong staleClientId in staleClientIds)
			{
				_connectedPlayers.Remove(staleClientId);
			}
			if (request.Payload == null || request.Payload.Length == 0)
			{
				Deny(response, "Missing connection payload.");
				return;
			}

			string json = Encoding.UTF8.GetString(request.Payload);
			var payload = JsonUtility.FromJson<ConnectionPayload>(json);

			if (payload.BuildVersion != Application.version)
			{
				Deny(response, $"Version mismatch. Server: {Application.version}, Client: {payload.BuildVersion}");
			}

			if (_connectedPlayers.Count >= _config.MaxPlayers)
			{
				Deny(response, "Session is full.");
				return;
			}

			if (_connectedPlayers.Values.Any(p => p.PlayerId == payload.PlayerId))
			{
				Deny(response, "Player is already in the session.");
				return;
			}

			string safeName = string.IsNullOrWhiteSpace(payload.PlayerName)
				? $"Player_{request.ClientNetworkId}"
				: payload.PlayerName;

			_connectedPlayers[request.ClientNetworkId] =
				new ConnectedPlayerInfo(request.ClientNetworkId, payload.PlayerId, safeName);

			response.Approved = true;
			response.CreatePlayerObject = false;
			response.Pending = false;
		}

		public void RemoveClient(ulong clientId) 
		{
			_connectedPlayers.Remove(clientId);
		}

		public void Clear()
		{
			_connectedPlayers.Clear();
		}
		private static void Deny(NetworkManager.ConnectionApprovalResponse response, string reason)
		{
			response.Approved = false;
			response.Reason = reason;
			response.Pending = false;
		}
	}
}
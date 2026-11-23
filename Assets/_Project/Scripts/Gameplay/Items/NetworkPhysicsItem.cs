using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Items
{
	public class NetworkPhysicsItem:NetworkBehaviour 
	{
		public const ulong NoHolderId=ulong.MaxValue;
		
		[SerializeField] private Rigidbody _rigidbody;
		[SerializeField] private Collider _collider;
		
		private NetworkVariable<ulong> _holderClientId;
		
		public bool IsHeld=>_holderClientId.Value!=NoHolderId;
		public ulong HolderClientId => _holderClientId.Value;

		private void Awake()
		{
			_rigidbody??=GetComponent<Rigidbody>();
			_collider??=GetComponent<Collider>();
			_holderClientId=new NetworkVariable<ulong>(NoHolderId);
		}

		public override void OnNetworkSpawn()
		{
			_holderClientId.OnValueChanged += HandleHolderChanged;
			ApplyPhysicsState(_holderClientId.Value);

			if (IsServer)
			{
				NetworkManager.OnClientDisconnectCallback += HandleClientDisconnected;
			}
		}

		public override void OnNetworkDespawn()
		{
			_holderClientId.OnValueChanged -= HandleHolderChanged;
			
			if(IsServer&&NetworkManager!=null)
			{
				NetworkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
			}
		}

		public bool TryServerGrab(ulong clientId)
		{
			if(!IsServer||IsHeld) return false;
			
			_holderClientId.Value = clientId;
			ApplyPhysicsState(clientId);
			
			NetworkObject.ChangeOwnership(clientId);
			return true;
		}

		public void ServerDrop()
		{
			if(!IsServer||!IsHeld) return;
			
			ReleaseToServer(Vector3.zero);
		}

		public void ServerThrow(Vector3 impulse)
		{
			if(!IsServer||!IsHeld) return;
			
			ReleaseToServer(impulse);
		}
		

		public void ServerDelete()
		{
			if(!IsServer) return;
			NetworkObject.Despawn();
		}

		private void ReleaseToServer(Vector3 impulse)
		{
			_holderClientId.Value = NoHolderId;
			NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
			ApplyPhysicsState(NoHolderId);

			if (impulse.sqrMagnitude > 0.001f)
			{
				_rigidbody.linearVelocity = Vector3.zero;
				_rigidbody.angularVelocity = Vector3.zero;
				_rigidbody.AddForce(impulse, ForceMode.Impulse);
			}
		}
		private void HandleClientDisconnected(ulong clientId)
		{
			if (IsServer && _holderClientId.Value == clientId)
			{
				ServerDrop();
			}
		}

		private void HandleHolderChanged(ulong oldHolderId, ulong newHolderId)
		{
			ApplyPhysicsState(newHolderId);
		}
		private void ApplyPhysicsState(ulong holderId)
		{
			bool held= holderId != NoHolderId;

			_rigidbody.isKinematic = !IsServer || held;
			_collider.enabled = !held;
			
			_collider.isTrigger = !IsServer;
		}

	
	}
}
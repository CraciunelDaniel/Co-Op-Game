using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Gameplay.Items
{
	public class NetworkPhysicsItem : NetworkBehaviour
	{
		public const ulong NoHolderId = ulong.MaxValue;

		[SerializeField] private Rigidbody _rigidbody;
		[SerializeField] private Collider _collider;
		[SerializeField] private NetworkTransform _networkTransform;

		private NetworkVariable<ulong> _holderClientId;

		public bool IsHeld => _holderClientId.Value != NoHolderId;
		public ulong HolderClientId => _holderClientId.Value;

		private void Awake()
		{
			_rigidbody ??= GetComponent<Rigidbody>();
			_collider ??= GetComponent<Collider>();
			_holderClientId = new NetworkVariable<ulong>(NoHolderId);
		}

		public override void OnNetworkSpawn()
		{
			if (IsServer)
				_rigidbody.sleepThreshold = 0.02f;
			
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

			if (IsServer && NetworkManager != null)
			{
				NetworkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
			}
		}

		public bool TryServerGrab(ulong clientId)
		{
			if (!IsServer || IsHeld) return false;

			_holderClientId.Value = clientId;
			ApplyPhysicsState(clientId);

			NetworkObject.ChangeOwnership(clientId);
			return true;
		}

		public void ServerDrop(Vector3 releasePos, Quaternion releaseRot)
		{
			if (!IsServer || !IsHeld) return;
			ReleaseToServer(releasePos, releaseRot, Vector3.zero);
		}

		public void ServerThrow(Vector3 releasePos, Quaternion releaseRot, Vector3 impulse)
		{
			if (!IsServer || !IsHeld) return;
			ReleaseToServer(releasePos, releaseRot, impulse);
		}

		public void ServerDelete()
		{
			if (!IsServer) return;
			NetworkObject.Despawn();
		}

		private void ReleaseToServer(Vector3 releasePos, Quaternion releaseRot, Vector3 impulse)
		{
			_holderClientId.Value = NoHolderId;
			NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
			if (_networkTransform != null && _networkTransform.CanCommitToTransform)
			{
				_networkTransform.Teleport(releasePos, releaseRot, transform.localScale);
			}
			else
			{
				transform.SetPositionAndRotation(releasePos, releaseRot);
			}

			ApplyPhysicsState(NoHolderId);
			_rigidbody.position = releasePos;
			_rigidbody.rotation = releaseRot;
			_rigidbody.linearVelocity = Vector3.zero;
			_rigidbody.angularVelocity = Vector3.zero;

			if (impulse.sqrMagnitude > 0.001f)
			{
				_rigidbody.AddForce(impulse, ForceMode.Impulse);
			}
		}

		private void HandleClientDisconnected(ulong clientId)
		{
			if (IsServer && _holderClientId.Value == clientId)
			{
				ServerDrop(transform.position, transform.rotation);
			}
		}

		private void HandleHolderChanged(ulong oldHolderId, ulong newHolderId)
		{
			ApplyPhysicsState(newHolderId);
		}

		private void ApplyPhysicsState(ulong holderId)
		{
			bool held = holderId != NoHolderId;

			_rigidbody.isKinematic = !IsServer || held;
			_collider.enabled = !held;
			_collider.isTrigger = false;
		}
	}
}
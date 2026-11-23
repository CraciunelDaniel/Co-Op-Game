using System;
using Core;
using Gameplay.Items;
using Scripts.Core;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Player
{
	public class PlayerInteraction : NetworkBehaviour
	{
		[SerializeField] private Transform _cameraTransform;
		[SerializeField] private Transform _holdPoint;

		private IPlayerInputService _inputService;
		private InteractionConfig _config;
		private NetworkPhysicsItem _itemPrefab;

		private NetworkVariable<NetworkObjectReference> _heldItemRef;
		private NetworkPhysicsItem _cachedHeldItem;

		private void Awake()
		{
			_heldItemRef = new NetworkVariable<NetworkObjectReference>();
		}

		public void Initialize(IPlayerInputService inputService, InteractionConfig interactionConfig, NetworkPhysicsItem itemPrefab)
		{
			_inputService = inputService;
			_config = interactionConfig;
			_itemPrefab = itemPrefab;
			
			bool isOwner = IsOwner;
			enabled = isOwner;
			
			if (isOwner && _inputService != null)
			{
				_inputService.OnSpawnItemPressed += HandleSpawnInput;
				_inputService.OnGrabOrDropPressed += HandleGrabOrDropInput;
				_inputService.OnThrowPressed += HandleThrowInput;
				_inputService.OnDeleteItemPressed += HandleDeleteInput;
			}
		}

		public override void OnNetworkSpawn()
		{
			_heldItemRef.OnValueChanged += HandleHeldItemChanged;
			ResolveHeldItem(_heldItemRef.Value);
		
		}

		public override void OnNetworkDespawn()
		{
			_heldItemRef.OnValueChanged -= HandleHeldItemChanged;

			if (IsOwner && _inputService != null)
			{
				_inputService.OnSpawnItemPressed -= HandleSpawnInput;
				_inputService.OnGrabOrDropPressed -= HandleGrabOrDropInput;
				_inputService.OnThrowPressed -= HandleThrowInput;
				_inputService.OnDeleteItemPressed -= HandleDeleteInput;
			}
		}

		private void LateUpdate()
		{
			if (_cachedHeldItem != null && _cachedHeldItem.IsOwner && _holdPoint != null)
			{
				float t = 1f - Mathf.Exp(-_config.HoldFollowSpeed * Time.deltaTime);
				_cachedHeldItem.transform.SetPositionAndRotation(Vector3.Lerp(_cachedHeldItem.transform.position, _holdPoint.position, t),
					Quaternion.Slerp(_cachedHeldItem.transform.rotation, _holdPoint.rotation, t));
			}
		}

		private void HandleSpawnInput()
		{
			Vector3 spawnPos = _cameraTransform.position + _cameraTransform.forward * _config.SpawnDistance;
			RequestSpawnItemRpc(spawnPos);
		}

		private void HandleGrabOrDropInput()
		{
			if (_cachedHeldItem != null)
			{
				RequestDropItemRpc();
				return;
			}

			if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _config.MaxGrabDistance, _config.InteractionLayer))
			{
				if (hit.collider.TryGetComponent(out NetworkPhysicsItem item) && !item.IsHeld)
				{
					RequestGrabItemRpc(item.NetworkObject);
				}
			}
		}

		private void HandleThrowInput()
		{
			if (_cachedHeldItem == null) return;
			RequestThrowItemRpc(_cameraTransform.forward);
		}

		private void HandleDeleteInput()
		{
			if (_cachedHeldItem == null) return;
			RequestDeleteItemRpc();
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
		private void RequestSpawnItemRpc(Vector3 requestedPos)
		{
			if (_itemPrefab == null) return;

			Vector3 origin = transform.position + Vector3.up * 1.2f;
			Vector3 offset = requestedPos - origin;
			if (offset.magnitude >= _config.SpawnDistance + 1.0f)
			{
				requestedPos = origin + offset.normalized * _config.SpawnDistance;
			}

			var instance = Instantiate(_itemPrefab, requestedPos, Quaternion.identity);
			instance.NetworkObject.Spawn(true);
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
		private void RequestGrabItemRpc(NetworkObjectReference itemRef)
		{
			if (TryGetServerHeldItem(out _)) return;

			if (!itemRef.TryGet(out NetworkObject netObj)) return;
			if (!netObj.TryGetComponent(out NetworkPhysicsItem item)) return;

			if (item.IsHeld) return;
			float distance = Vector3.Distance(transform.position, item.transform.position);
			if (distance > _config.MaxGrabDistance + 1.5f) return;

			if (item.TryServerGrab(OwnerClientId))
			{
				_heldItemRef.Value = itemRef;
			}
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
		private void RequestDropItemRpc()
		{
			if (!TryGetServerHeldItem(out var item)) return;

			item.ServerDrop();
			_heldItemRef.Value = default;
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
		private void RequestThrowItemRpc(Vector3 direction)
		{
			if (!TryGetServerHeldItem(out var item)) return;
			if (direction.sqrMagnitude < 0.001f) direction = transform.forward;

			item.ServerThrow(direction.normalized * _config.ThrowForce);
			_heldItemRef.Value = default;
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
		private void RequestDeleteItemRpc()
		{
			if (!TryGetServerHeldItem(out var item)) return;

			_heldItemRef.Value = default;
			item.ServerDelete();
		}

		private bool TryGetServerHeldItem(out NetworkPhysicsItem item)
		{
			item = null;
			if (_heldItemRef.Value.TryGet(out NetworkObject netObj) && netObj.TryGetComponent(out NetworkPhysicsItem found) && found.HolderClientId == OwnerClientId)
			{
				item = found;
				return true;
			}

			return false;
		}

		private void HandleHeldItemChanged(NetworkObjectReference _, NetworkObjectReference newRef)
		{
			ResolveHeldItem(newRef);
		}

		private void ResolveHeldItem(NetworkObjectReference itemRef)
		{
			if (itemRef.TryGet(out NetworkObject netObj))
			{
				_cachedHeldItem = netObj.GetComponent<NetworkPhysicsItem>();
			}
			else
			{
				_cachedHeldItem = null;
			}
		}
	}
}
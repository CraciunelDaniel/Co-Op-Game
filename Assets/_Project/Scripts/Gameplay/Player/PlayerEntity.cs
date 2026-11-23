using System;
using System.Linq;
using Core;
using Gameplay.Items;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay.Player
{
	public class PlayerEntity : NetworkBehaviour
	{
		[SerializeField] private Camera _playerCamera;
		[SerializeField] private AudioListener _audioListener;
		[SerializeField] private PlayerMotor _motor;
		[SerializeField] private PlayerColorSync _colorSync;
		[SerializeField] private PlayerInteraction _playerInteraction;
		
		private IPlayerInputService _inputService;
		private PlayerConfig _config;

		private void Awake()
		{
			_motor ??= GetComponent<PlayerMotor>();
			_colorSync ??= GetComponent<PlayerColorSync>();
			_playerInteraction ??= GetComponent<PlayerInteraction>();
		}

		[Inject]
		public void Construct(IPlayerInputService inputService, PlayerConfig config,InteractionConfig interactionConfig,NetworkPhysicsItem itemPrefab)
		{
			_inputService = inputService;
			_config = config;
			_colorSync.Initialize(config);
			_playerInteraction.Initialize(inputService,interactionConfig,itemPrefab);
		}

		public override void OnNetworkSpawn()
		{
			if (_inputService == null)
			{
				var scope = FindObjectsByType<LifetimeScope>(FindObjectsSortMode.None)
					.FirstOrDefault(s => s.gameObject.scene == gameObject.scene);
				scope?.Container.Inject(this);
			}

			bool isOwner = IsOwner;
			if(_playerCamera!=null) _playerCamera.enabled = isOwner;
			if(_audioListener!=null) _audioListener.enabled = isOwner;
			
			_motor.Initialize(_config,isOwner,IsServer);

			if (isOwner)
			{
				_inputService.EnableGameplayInput();
				_inputService.OnChangeColorPressed += HandleChangeColorPressed;
			}
		}

		public override void OnNetworkDespawn()
		{
			if (IsOwner && _inputService != null)
			{
				_inputService.OnChangeColorPressed -= HandleChangeColorPressed;
				_inputService.DisableGameplayInput();
			}
		}

		private void Update()
		{
			if(!IsOwner || _inputService == null)return;
			
			_motor.ApplyLock(_inputService.LookInput);
			_motor.ApplyMove(_inputService.MoveInput,Time.deltaTime);
		}

		private void HandleChangeColorPressed()
		{
			_colorSync.RequestNextRandomColor();
		}
	}
}

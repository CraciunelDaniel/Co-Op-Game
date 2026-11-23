using System;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Player
{
	public class PlayerMotor:MonoBehaviour
	{
		[SerializeField] private Transform _cameraPivot;
		[SerializeField] private CharacterController _controller;
		
		private PlayerConfig _config;
		
		private float _verticalVelocity;
		private float _cameraPitch;

		private void Awake()
		{
			_controller??= GetComponent<CharacterController>();
		}

		public void Initialize(PlayerConfig config, bool isLocalOwner,bool isServer)
		{
			_config = config;
			_controller.enabled = isLocalOwner||isServer;
		}

		public void ApplyLock(Vector2 lockDelta)
		{
			transform.Rotate(Vector3.up, -lockDelta.x*_config.MouseSensitivity);
			
			_cameraPitch= Mathf.Clamp(_cameraPitch-lockDelta.y*_config.MouseSensitivity,
				-_config.MaxPitchAngle, _config.MaxPitchAngle);
			if (_cameraPivot != null)
			{
				_cameraPivot.localRotation = Quaternion.Euler(-_cameraPitch,0f,0f);
			}
		}

		public void ApplyMove(Vector2 moveInput, float deltaTime)
		{
			Vector3 move=transform.right *moveInput.x+transform.forward*moveInput.y;

			if (_controller.isGrounded && _verticalVelocity < 0f)
			{
				_verticalVelocity = -2f;
			}

			_verticalVelocity += _config.Gravity * deltaTime;
			Vector3 velocity = move*_config.MoveSpeed+Vector3.up*_verticalVelocity;
			
			_controller.Move(velocity*deltaTime);
		}
	}
}
using System;
using Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Input
{
	public class PlayerInputService : IPlayerInputService, GameInputActions.IPlayerActions,GameInputActions.IUIActions,IDisposable
	{
		private GameInputActions _inputActions;
		public Vector2 MoveInput { get; private set; }
		public Vector2 LookInput => _inputActions.Player.Look.ReadValue<Vector2>();

		public event Action OnChangeColorPressed;
		public event Action OnSpawnItemPressed;
		public event Action OnGrabOrDropPressed;
		public event Action OnThrowPressed;
		public event Action OnDeleteItemPressed;
		public event Action OnTogglePlayerListPressed;

		public PlayerInputService()
		{
			_inputActions = new GameInputActions();
			_inputActions.Player.SetCallbacks(this);
			_inputActions.UI.SetCallbacks(this);
			_inputActions.UI.Enable();
		}

		public void EnableGameplayInput()
		{
			_inputActions.Player.Enable();
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		public void DisableGameplayInput()
		{
			_inputActions.Player.Disable();
			MoveInput = Vector2.zero;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		public void OnMove(InputAction.CallbackContext context)
		{
			MoveInput = context.ReadValue<Vector2>();
		}

		public void OnLook(InputAction.CallbackContext context) { }

		public void OnJump(InputAction.CallbackContext context) { }
		public void OnSprint(InputAction.CallbackContext context) { }

		public void OnChangeColor(InputAction.CallbackContext context)
		{
			if (context.performed) OnChangeColorPressed?.Invoke();
		}

		public void OnSpawnItem(InputAction.CallbackContext context)
		{
			if (context.performed) OnSpawnItemPressed?.Invoke(); 
		}

		public void OnGrabOrDrop(InputAction.CallbackContext context)
		{
			if (context.performed) OnGrabOrDropPressed?.Invoke();
		}

		public void OnThrow(InputAction.CallbackContext context)
		{
			if (context.performed) OnThrowPressed?.Invoke();
		}

		public void OnDeleteItem(InputAction.CallbackContext context)
		{
			if (context.performed) OnDeleteItemPressed?.Invoke();
		}
		public void OnTogglePlayerList(InputAction.CallbackContext context)
		{
			if(context.performed) OnTogglePlayerListPressed?.Invoke();
		}
		public void Dispose()
		{
			_inputActions.Player.SetCallbacks(null);
			_inputActions.UI.SetCallbacks(null);
			_inputActions.UI.Disable();
			DisableGameplayInput();
			_inputActions.Dispose();
		}

	
	}
}
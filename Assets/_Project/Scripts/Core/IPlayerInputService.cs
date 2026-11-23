using System;
using UnityEngine;

namespace Core
{
	public interface IPlayerInputService 
	{
		Vector2 MoveInput { get; }
		Vector2 LookInput { get; }

		event Action OnChangeColorPressed;
		event Action OnSpawnItemPressed;
		event Action OnGrabOrDropPressed;
		event Action OnThrowPressed;
		event Action OnDeleteItemPressed;
		event Action OnTogglePlayerListPressed;

		void EnableGameplayInput();
		void DisableGameplayInput();

	}
}  

using System;
using UnityEngine;

namespace Scripts.Core
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

		void EnableGameplayInput();
		void DisableGameplayInput();

	}
}  

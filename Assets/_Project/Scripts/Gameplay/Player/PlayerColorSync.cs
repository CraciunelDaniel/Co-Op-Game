using Core;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Player
{
	public class PlayerColorSync : NetworkBehaviour
	{
		[SerializeField] private Renderer _bodyRenderer;

		private readonly NetworkVariable<Color> _networkColor = new();
		
		private int _currentColorIndex = -1;
		private PlayerConfig _config;

		public void Initialize(PlayerConfig config)
		{
			_config = config;
		}

		public override void OnNetworkSpawn()
		{
			_networkColor.OnValueChanged += HandleColorChanged;
			ApplyColor(_networkColor.Value);
		}

		public override void OnNetworkDespawn()
		{
			_networkColor.OnValueChanged -= HandleColorChanged;
		}

		public void RequestNextRandomColor()
		{
			if(!IsOwner)return;
			RequestRandomColorRpc();
		}
		[Rpc(SendTo.Server)]
		private void RequestRandomColorRpc()
		{
			if (_config == null || _config.ColorPalette.Length == 0) return;

			if (_config.ColorPalette.Length == 1)
			{
				_currentColorIndex = 0;
				_networkColor.Value = _config.ColorPalette[0];
				return;
			}

			int nextIndex = Random.Range(0, _config.ColorPalette.Length - 1);
			if (nextIndex >= _currentColorIndex) nextIndex++;

			_currentColorIndex = nextIndex;
			_networkColor.Value = _config.ColorPalette[_currentColorIndex];
		}
		private void HandleColorChanged(Color previousColor, Color newColor)
		{
			ApplyColor(newColor);
		}

		private void ApplyColor(Color color)
		{
			if(_bodyRenderer==null)return;
			
			_bodyRenderer.material.color = color;
		}
	}
}

using Core;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Player
{
	public class PlayerColorSync : NetworkBehaviour
	{
		[SerializeField] private Renderer _bodyRenderer;

		private readonly NetworkVariable<byte> _colorIndex = new();

		private int _currentColorIndex = -1;
		private PlayerConfig _config;

		public void Initialize(PlayerConfig config)
		{
			_config = config;
			ApplyColor(_colorIndex.Value);
		}

		public override void OnNetworkSpawn()
		{
			_colorIndex.OnValueChanged += HandleColorChanged;
			ApplyColor(_colorIndex.Value);
		}

		public override void OnNetworkDespawn()
		{
			_colorIndex.OnValueChanged -= HandleColorChanged;
		}

		public void RequestNextRandomColor()
		{
			if (!IsOwner) return;
			RequestRandomColorRpc();
		}

		[Rpc(SendTo.Server)]
		private void RequestRandomColorRpc()
		{
			int count = _config.ColorPalette.Length;
			if (count <= 1) return;

			int next = Random.Range(0, count - 1);
			if (next >= _colorIndex.Value) next++;
			_colorIndex.Value = (byte) next;
		}

		private void HandleColorChanged(byte _, byte index)
		{
			ApplyColor(index);
		}

		private void ApplyColor(byte index)
		{
			if (_bodyRenderer == null || _config == null || index >= _config.ColorPalette.Length) return;

			_bodyRenderer.material.color = _config.ColorPalette[index];
		}
	}
}
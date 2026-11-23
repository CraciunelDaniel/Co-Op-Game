using System;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class PlayerListRowView : MonoBehaviour
	{
		[SerializeField] private TMP_Text _nameText;
		[SerializeField] private TMP_Text _pingText;
		[SerializeField] private Button _kickButton;

		private ulong _targetId;
		private event Action<ulong> OnKick = null;

		private void OnDestroy()
		{
			Unbind();
		}

		public void Bind(LobbyPlayerInfo info, bool canKick, Action<ulong> onKickClicked)
		{
			string hostTag = info.IsHost ? " [Host]" : "";
			_nameText.text = $"{info.Name}{hostTag})";
			_pingText.text = info.IsHost ? "0 ms" : $"{info.PingMs} ms";
			UpdatePingColor(info);

			bool showKick = canKick && !info.IsHost;
			_kickButton.gameObject.SetActive(showKick);
			_kickButton.onClick.RemoveListener(KickPlayer);
			OnKick = null;

			if (showKick)
			{
				_targetId = info.ClientId;
				OnKick = onKickClicked;
				_kickButton.onClick.AddListener(KickPlayer);
			}
		}

		public void Unbind()
		{
			_targetId = 0;
			OnKick = null;
			_kickButton.onClick.RemoveListener(KickPlayer);
		}

		private void UpdatePingColor(LobbyPlayerInfo info)
		{
			if (info.PingMs >= 200)
			{
				_pingText.color = Color.red;
			}
			else if (info.PingMs >= 100)
			{
				_pingText.color = Color.yellow;
			}
			else
			{
				_pingText.color = Color.green;
			}
		}

		private void KickPlayer()
		{
			OnKick?.Invoke(_targetId);
		}
	}
}
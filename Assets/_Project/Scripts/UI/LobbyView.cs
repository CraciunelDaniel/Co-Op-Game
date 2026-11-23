using System.Text;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI
{
	public class LobbyView : MonoBehaviour
	{
		[SerializeField] private GameObject _connectionPanel;
		[SerializeField] private GameObject _lobbyPanel;
		[SerializeField] private TMP_Text _playersListText;
		[SerializeField] private Button _startGameButton;
		[SerializeField] private Button _leaveButton;
		[SerializeField] private Button _inviteFriend;

		private INetworkSessionService _sessionService;
		private ISessionStateService _sessionState;
		private ILobbyService _lobbyService;

		[Inject]
		public void Construct(INetworkSessionService sessionService, ISessionStateService sessionState, ILobbyService lobbyService)
		{
			_sessionService = sessionService;
			_sessionState = sessionState;
			_lobbyService = lobbyService;
		}

		private void Start()
		{
			_startGameButton.onClick.AddListener(OnStartGameClicked);
			_leaveButton.onClick.AddListener(OnLeaveClicked);
			bool isSupported = _lobbyService.IsSupported;
			if (isSupported)
			{
				_inviteFriend.onClick.AddListener(OnInviteFriendClicked);
			}

			_inviteFriend.gameObject.SetActive(_lobbyService.IsSupported);

			_sessionService.OnConnected += HandleConnected;
			_sessionService.OnDisconnected += HandleDisconnected;
			_sessionState.OnPlayersChanged += RefreshPlayersList;

			ShowLobby(false);
		}

		private void OnInviteFriendClicked()
		{
			_lobbyService.OpenInviteOverlay();
		}

		private void OnDestroy()
		{
			_startGameButton.onClick.RemoveListener(OnStartGameClicked);
			_leaveButton.onClick.RemoveListener(OnLeaveClicked);
			_inviteFriend.onClick.RemoveListener(OnInviteFriendClicked);
			
			if (_sessionService != null)
			{
				_sessionService.OnConnected -= HandleConnected;
				_sessionService.OnDisconnected -= HandleDisconnected;
			}

			if (_sessionState != null)
			{
				_sessionState.OnPlayersChanged -= RefreshPlayersList;
			}
			
		}

		private void HandleConnected()
		{
			ShowLobby(true);
			_startGameButton.gameObject.SetActive(_sessionService.IsHost);
			RefreshPlayersList();
		}

		private void HandleDisconnected(string _)
		{
			ShowLobby(false);
		}

		private void RefreshPlayersList()
		{
			var sb = new StringBuilder();
			foreach (var player in _sessionState.Players)
			{
				string tag = player.IsHost ? " [Host]" : "";
				sb.AppendLine($"• {player.Name}{tag}");
			}

			_playersListText.text = sb.ToString();
		}

		private void OnStartGameClicked()
		{
			_startGameButton.interactable = false;
			_sessionState.StartGame();
		}

		private void OnLeaveClicked()
		{
			_sessionService.Shutdown();
			ShowLobby(false);
		}

		private void ShowLobby(bool visible)
		{
			_connectionPanel.SetActive(!visible);
			_lobbyPanel.SetActive(visible);
			if (visible) _startGameButton.interactable = true;
		}
	}
}
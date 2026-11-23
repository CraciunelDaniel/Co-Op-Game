using System;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using Button = UnityEngine.UI.Button;

namespace UI
{
	public class GameplayPlayerListView : MonoBehaviour
	{
		[SerializeField] private GameObject _view;
		[SerializeField] private Transform _rowsContainer;
		[SerializeField] private PlayerListRowView _rowPrefab;
		[SerializeField] private Button _leaveButton;

		private ISessionStateService _sessionState;
		private INetworkSessionService _sessionService;
		private IPlayerInputService _playerInputService;

		private readonly List<PlayerListRowView> _rowPool = new();
		private readonly List<PlayerListRowView> _spawnedRows = new();
		private bool _isOpen;

		[Inject]
		public void Construct(ISessionStateService sessionState, INetworkSessionService sessionService, IPlayerInputService playerInputService)
		{
			_sessionState = sessionState;
			_sessionService = sessionService;
			_playerInputService = playerInputService;
		}

		private void Start()
		{
			_view.SetActive(false);
			_leaveButton.onClick.AddListener(OnLeaveClicked);
			
			_sessionState.OnPlayersChanged += RefreshList;
			_playerInputService.OnTogglePlayerListPressed += TogglePanel;
		}

		private void OnDestroy()
		{
			if(_sessionState!=null) _sessionState.OnPlayersChanged-=RefreshList;
			if(_playerInputService!=null) _playerInputService.OnTogglePlayerListPressed-=TogglePanel;
		}

		private void TogglePanel()
		{
			_isOpen = !_isOpen;
			_view.SetActive(_isOpen);

			if (_isOpen)
			{
				_playerInputService.DisableGameplayInput();
				RefreshList();
			}
			else
			{
				_playerInputService.EnableGameplayInput();
			}
		}

		private void RefreshList()
		{
			if (!_isOpen) return;

			var players = _sessionState.Players;
			bool isHost = _sessionService.IsHost;

			while (_rowPool.Count < players.Count)
			{
				var row = Instantiate(_rowPrefab, _rowsContainer);
				_rowPool.Add(row);
			}

			for (int i = 0; i < _rowPool.Count; i++)
			{
				if (i < players.Count)
				{
					_rowPool[i].gameObject.SetActive(true);
					_rowPool[i].Bind(players[i], isHost, HandleKickClicked);
				}
				else
				{
					_rowPool[i].Unbind();
					_rowPool[i].gameObject.SetActive(false);
				}
			}
		}

		private void HandleKickClicked(ulong clientId)
		{
			_sessionState.KickPlayer(clientId);
		}

		private void OnLeaveClicked()
		{
			_sessionService.Shutdown();
		}
	}
}
using System;
using Core;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Networking
{
	public class RichPresenceController : IInitializable, IDisposable
    {
        private const string StatusKey = "status";

        private readonly IRichPresenceService _richPresence;
        private readonly INetworkSessionService _sessionService;
        private readonly ISessionStateService _sessionState;
        private readonly SessionConfig _config;

        public RichPresenceController(
            IRichPresenceService richPresence,
            INetworkSessionService sessionService,
            ISessionStateService sessionState,
            SessionConfig config)
        {
            _richPresence = richPresence;
            _sessionService = sessionService;
            _sessionState = sessionState;
            _config = config;
        }

        public void Initialize()
        {
            _sessionService.OnConnected += UpdateStatus;
            _sessionService.OnDisconnected += HandleDisconnected;
            _sessionState.OnPlayersChanged += UpdateStatus;
            SceneManager.activeSceneChanged += HandleSceneChanged;

            _richPresence.SetStatus(StatusKey, "In Menu");
        }

        private void HandleDisconnected(string _)
        {
            _richPresence.SetStatus(StatusKey, "In Menu");
        }

        private void HandleSceneChanged(Scene _, Scene next)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (!_sessionService.IsConnected && !_sessionService.IsHost)
            {
                _richPresence.SetStatus(StatusKey, "In Menu");
                return;
            }

            bool inMenu = SceneManager.GetActiveScene().name == _config.MenuSceneName;
            int count = _sessionState.Players.Count;
            string state = inMenu ? "In Lobby" : "In Game";

            _richPresence.SetStatus(StatusKey, $"{state} ({count}/{_config.MaxPlayers})");
        }

        public void Dispose()
        {
            _sessionService.OnConnected -= UpdateStatus;
            _sessionService.OnDisconnected -= HandleDisconnected;
            _sessionState.OnPlayersChanged -= UpdateStatus;
            SceneManager.activeSceneChanged -= HandleSceneChanged;
            _richPresence.Clear();
        }
    }
}
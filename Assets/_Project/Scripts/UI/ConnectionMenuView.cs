using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI
{
    public class ConnectionMenuView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_InputField _ipInput;
        [SerializeField] private TMP_InputField _portInput;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private TMP_Text _statusText;

        private INetworkSessionService _sessionService;
        private IPlayerIdentityProvider _identityProvider;
        private SessionConfig _config;

        [Inject]
        public void Construct(
            INetworkSessionService sessionService,
            IPlayerIdentityProvider identityProvider,
            SessionConfig config)
        {
            _sessionService = sessionService;
            _identityProvider = identityProvider;
            _config = config;
        }

        private void Start()
        {
            _nameInput.text = _identityProvider.DisplayName;
            _ipInput.text = _config.DefaultIp;
            _portInput.text = _config.DefaultPort.ToString();
            _statusText.text = string.Empty;

            _hostButton.onClick.AddListener(OnHostClicked);
            _joinButton.onClick.AddListener(OnJoinClicked);

            _sessionService.OnConnected += HandleConnected;
            _sessionService.OnDisconnected += HandleDisconnected;

            if (string.IsNullOrEmpty(_sessionService.LastDisconnectReason))
            {
                _statusText.text = _sessionService.LastDisconnectReason;
            }
        }

        private void OnDestroy()
        {
            if (_sessionService != null)
            {
                _sessionService.OnConnected -= HandleConnected;
                _sessionService.OnDisconnected -= HandleDisconnected;
            }
        }

        private void OnHostClicked()
        {
            _identityProvider.SetDisplayName(_nameInput.text);
            ushort port = ParsePort();
            _statusText.text = "Starting host...";
            _sessionService.StartHost(port);
        }

        private void OnJoinClicked()
        {
            _identityProvider.SetDisplayName(_nameInput.text);
            ushort port = ParsePort();
            _statusText.text = $"Connecting to {_ipInput.text}:{port}...";
            _sessionService.StartClient(_ipInput.text, port);
        }

        private void HandleConnected()
        {
            _statusText.text = _sessionService.IsHost ? "Host started!" : "Connected to host!";
        }

        private void HandleDisconnected(string reason)
        {
            _statusText.text = $"Disconnected: {reason}";
        }

        private ushort ParsePort()
        {
            return ushort.TryParse(_portInput.text, out ushort port) ? port : _config.DefaultPort;
        }
    }
}
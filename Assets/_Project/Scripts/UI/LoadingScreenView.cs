using Scripts.Core;
using UnityEngine;
using VContainer;

namespace Scripts.UI
{
    public class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject _rootPanel;

        private ISessionStateService _sessionState;

        [Inject]
        public void Construct(ISessionStateService sessionState)
        {
            _sessionState = sessionState;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _rootPanel.SetActive(false);
        }

        private void Start()
        {
            _sessionState.OnLoadingScreenToggled += HandleLoadingToggled;
        }

        private void OnDestroy()
        {
            if (_sessionState != null)
            {
                _sessionState.OnLoadingScreenToggled -= HandleLoadingToggled;
            }
        }

        private void HandleLoadingToggled(bool visible)
        {
            _rootPanel.SetActive(visible);
        }
    }
}


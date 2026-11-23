using System;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using VContainer.Unity;

namespace Steam
{
	public class SteamLifecycleService : IDisposable
	{
		private const uint SPACEWAR_APP_ID = 480;
		private NetworkManager _networkManager;

		public bool TryInitialize()
		{
			if (SteamClient.IsValid) return true;
			try
			{
				SteamClient.Init(SPACEWAR_APP_ID, false);
				Debug.Log($"Steam Service initialized {SteamClient.Name}({SteamClient.SteamId})");
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to initialize Steam Service {e.Message}");
				return false;
			}
		}

		public void Attach(NetworkManager networkManager)
		{
			if (_networkManager != null)
				return;

			_networkManager = networkManager;
			_networkManager.OnServerStopped += HandleNetworkStopped;
			_networkManager.OnClientStopped += HandleNetworkStopped;
		}

		private void HandleNetworkStopped(bool _)
		{
			TryInitialize();
		}

		public void Dispose()
		{
			if (_networkManager != null)
			{
				_networkManager.OnServerStopped -= HandleNetworkStopped;
				_networkManager.OnClientStopped -= HandleNetworkStopped;
				_networkManager = null;
			}

			if (SteamClient.IsValid)
			{
				SteamClient.Shutdown();
			}
		}
	}
}
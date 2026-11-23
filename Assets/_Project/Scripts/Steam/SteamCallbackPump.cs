using Steamworks;
using Unity.Netcode;
using VContainer.Unity;

namespace Steam
{
	public class SteamCallbackPump : ITickable
	{
		private readonly NetworkManager _networkManager;

		public SteamCallbackPump(NetworkManager networkManager)
		{
			_networkManager = networkManager;
		}

		public void Tick()
		{
			if (SteamClient.IsValid && !_networkManager.IsListening)
			{
				SteamClient.RunCallbacks();
			}
		}
	}
}
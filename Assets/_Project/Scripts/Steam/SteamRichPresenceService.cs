using Core;
using Steamworks;

namespace Steam
{
	public class SteamRichPresenceService:IRichPresenceService
	{
		public void SetStatus(string statusKey, string value)
		{
			if(!SteamClient.IsValid)return;
			SteamFriends.SetRichPresence(statusKey, value);
		}

		public void Clear()
		{
			if(!SteamClient.IsValid)return;
			SteamFriends.ClearRichPresence();
		}
	}
}
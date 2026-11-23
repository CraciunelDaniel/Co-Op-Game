using Core;
using Steamworks;
using Steamworks.Data;

namespace Steam
{
	public class SteamAchievementService : IAchievementService
	{
		/// <summary>
		/// test Unlock("ACH_WIN_ONE_GAME")
		/// </summary>
		/// <param name="achievementId"></param>
		public void Unlock(string achievementId)
		{
			if (!SteamClient.IsValid) return;
			
			var ach=new Achievement(achievementId);
			if (ach.State)
			{
				ach.Trigger();
				SteamUserStats.StoreStats();
			}
		}
		
	}
}
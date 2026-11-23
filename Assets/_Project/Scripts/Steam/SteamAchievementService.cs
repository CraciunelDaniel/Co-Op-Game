using Core;
using Steamworks;
using Steamworks.Data;

namespace Steam
{
	public class SteamAchievementService : IAchievementService
	{
		private bool _hasPendingChanges;

		public void IncrementStat(string statName, int amount = 1)
		{
			if(!SteamClient.IsValid)return;
			SteamUserStats.AddStat(statName, amount);
			_hasPendingChanges = true;
		}
		/// <summary>
		/// test Unlock("ACH_WIN_ONE_GAME")
		/// </summary>
		/// <param name="achievementId"></param>
		public void Unlock(string achievementId)
		{
			if (!SteamClient.IsValid) return;

			var ach = new Achievement(achievementId);
			if (!ach.State)
			{
				ach.Trigger(false);
				_hasPendingChanges = true;
			}
		}

		public void StoreStats()
		{
			if (!SteamClient.IsValid||!_hasPendingChanges) return;
			SteamUserStats.StoreStats();
			_hasPendingChanges = false;
		}
	}
}
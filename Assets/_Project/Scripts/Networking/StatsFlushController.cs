using System;
using Core;
using UnityEngine;
using VContainer.Unity;

namespace Networking
{
	public class StatsFlushController:IInitializable,IDisposable
	{
		private readonly IAchievementService _achievementService;
		private readonly INetworkSessionService _sessionService;

		public StatsFlushController(IAchievementService achievementService, INetworkSessionService sessionService)
		{
			_achievementService = achievementService;
			_sessionService = sessionService;
		}
		public void Initialize()
		{
			_sessionService.OnDisconnected += HandleSessionEnded;
			Application.quitting += _achievementService.StoreStats;
		}

		private void HandleSessionEnded(string _)
		{
			_achievementService.StoreStats();
		}

		public void Dispose()
		{
			_sessionService.OnDisconnected -= HandleSessionEnded;
			Application.quitting -= _achievementService.StoreStats;
		}
	}
}
using System;
using Core;
using Netcode.Transports.Facepunch;
using Networking;
using Steam;
using Transports;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Bootstrap
{
	public class BootstrapLifetimeScope : LifetimeScope
	{
		[SerializeField] private NetworkManager _networkManager;
		[SerializeField] private SessionConfig _sessionConfig;
		[SerializeField] private NetworkSessionState _sessionStatePrefab;

		[SerializeField] private UnityTransport _unityTransport;
		[SerializeField] private FacepunchTransport _facepunchTransport;

		private readonly SteamLifecycleService _steamLifecycleService = new();

		protected override void Awake()
		{
			DontDestroyOnLoad(gameObject);
			base.Awake();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			_steamLifecycleService.Dispose();
		}

		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterInstance(_networkManager);
			builder.RegisterInstance(_sessionConfig);
			builder.RegisterInstance(_sessionStatePrefab);

			ConfigureTransport(builder);

			builder.Register<ConnectionApprovalHandler>(Lifetime.Singleton);
			builder.Register<INetworkSessionService, NetworkSessionService>(Lifetime.Singleton);
			builder.Register<ISessionStateService, SessionStateService>(Lifetime.Singleton);
			builder.RegisterEntryPoint<RichPresenceController>(Lifetime.Singleton);
			builder.RegisterEntryPoint<StatsFlushController>(Lifetime.Singleton);

			builder.RegisterBuildCallback(_ => { SceneManager.LoadScene(_sessionConfig.MenuSceneName); });
		}

		private void ConfigureTransport(IContainerBuilder builder)
		{
			var transportType = ResolveTransportType(_sessionConfig.DefaultTransport);
			if (transportType == TransportType.Steam && !_steamLifecycleService.TryInitialize())
			{
				transportType = TransportType.Utp;
			}

			switch (transportType)
			{
				case TransportType.Steam:
					ConfigureSteam(builder);
					break;
				case TransportType.Utp:
				default:
					ConfigureUtp(builder);
					break;
			}
		}

		private void ConfigureSteam(IContainerBuilder builder)
		{
			_networkManager.NetworkConfig.NetworkTransport = _facepunchTransport;
			_steamLifecycleService.Attach(_networkManager);
			
			builder.RegisterEntryPoint<SteamCallbackPump>(Lifetime.Singleton);
			builder.Register<ITransportConfigurator, SteamTransportConfigurator>(Lifetime.Singleton);
			builder.Register<IPlayerIdentityProvider, SteamIdentityProvider>(Lifetime.Singleton);
			builder.Register<ILobbyService, SteamLobbyService>(Lifetime.Singleton);
			builder.Register<IRichPresenceService, SteamRichPresenceService>(Lifetime.Singleton);
			builder.Register<IAchievementService, SteamAchievementService>(Lifetime.Singleton);
		}

		private void ConfigureUtp(IContainerBuilder builder)
		{
			_networkManager.NetworkConfig.NetworkTransport = _unityTransport;

			builder.Register<ITransportConfigurator, UtpTransportConfigurator>(Lifetime.Singleton);
			builder.Register<IPlayerIdentityProvider, LocalPlayerIdentityProvider>(Lifetime.Singleton);
			builder.Register<ILobbyService, NullLobbyService>(Lifetime.Singleton);
			builder.Register<IRichPresenceService, NullRichPresenceService>(Lifetime.Singleton);
			builder.Register<IAchievementService, LoggingAchievementService>(Lifetime.Singleton);
		}

		private static TransportType ResolveTransportType(TransportType fallback)
		{
			var args = Environment.GetCommandLineArgs();
			for (int i = 0; i < args.Length - 1; i++)
			{
				if (args[i].Equals("-transport", StringComparison.OrdinalIgnoreCase) && Enum.TryParse(args[i + 1], true, out TransportType parsed))
				{
					return parsed;
				}
			}

			return fallback;
		}
	}
}
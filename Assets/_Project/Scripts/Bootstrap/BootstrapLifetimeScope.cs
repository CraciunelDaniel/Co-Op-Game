using System;
using Core;
using Networking;
using Transports;
using Unity.Netcode;
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

		protected override void Awake()
		{
			DontDestroyOnLoad(gameObject);
			base.Awake();
		}

		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterInstance(_networkManager);
			builder.RegisterInstance(_sessionConfig);
			builder.RegisterInstance(_sessionStatePrefab);


			var transportType = ResolveTransportType(_sessionConfig.DefaultTransport);

			switch (transportType)
			{
				case TransportType.Utp:
				default:
					builder.Register<ITransportConfigurator, UtpTransportConfigurator>(Lifetime.Singleton);
					builder.Register<IPlayerIdentityProvider, LocalPlayerIdentityProvider>(Lifetime.Singleton);
					break;
			}

			builder.Register<ConnectionApprovalHandler>(Lifetime.Singleton);
			builder.Register<INetworkSessionService, NetworkSessionService>(Lifetime.Singleton);
			builder.Register<ISessionStateService, SessionStateService>(Lifetime.Singleton);

			builder.RegisterBuildCallback(_ => { SceneManager.LoadScene(_sessionConfig.MenuSceneName); });
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

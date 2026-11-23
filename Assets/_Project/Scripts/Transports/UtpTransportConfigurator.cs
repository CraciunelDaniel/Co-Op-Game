using Core;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Transports
{
	public class UtpTransportConfigurator : ITransportConfigurator
	{
		private readonly NetworkManager _networkManager;
		public UtpTransportConfigurator(NetworkManager networkManager)
		{
			_networkManager = networkManager;
		}
		public void ConfigureForHost(ushort port)
		{
			var utp = GetOrAddTransport();
			utp.SetConnectionData("0.0.0.0", port); 
			_networkManager.NetworkConfig.NetworkTransport = utp; 
		}
		public void ConfigureForClient(string address, ushort port)
		{ 
			var utp = GetOrAddTransport();
			utp.SetConnectionData(address, port);
			_networkManager.NetworkConfig.NetworkTransport = utp;
		}

		private UnityTransport GetOrAddTransport()
		{
			if (!_networkManager.TryGetComponent(out UnityTransport utp))
			{
				utp=_networkManager.gameObject.AddComponent<UnityTransport>();
			}
			return utp;
		}
	}
}

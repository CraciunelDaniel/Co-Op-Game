using Core;
using Netcode.Transports.Facepunch;
using Unity.Netcode;

namespace Steam
{
	public class SteamTransportConfigurator:ITransportConfigurator
	{
		private readonly FacepunchTransport _transport;

		public SteamTransportConfigurator(NetworkManager networkManager)
		{
			networkManager.TryGetComponent(out _transport);
		}
		public void ConfigureForHost(ushort port)
		{
			
		}
		public void ConfigureForClient(string address, ushort port)
		{
			if (ulong.TryParse(address, out ulong targetSteamId))
			{
				_transport.targetSteamId = targetSteamId;
			}
		}
	}
}
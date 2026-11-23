using System;

namespace Core
{
   public interface IPlayerIdentityProvider
   {
      string PlayerId { get; }
      string DisplayName { get; }
      void SetDisplayName(string name);
   }
   public interface ITransportConfigurator
   {
      void ConfigureForHost(ushort port);
      void ConfigureForClient(string address,ushort port);
   }
   public interface INetworkSessionService
   {
      bool IsHost { get; }
      bool IsConnected {get;}
      string LastDisconnectReason {get;}
      
      event Action OnConnected;
      event Action<string> OnDisconnected;
      
      void StartHost(ushort port);
      void StartClient(string address, ushort port);
      void Shutdown();
   }
}

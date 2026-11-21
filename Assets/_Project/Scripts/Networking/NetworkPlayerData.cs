using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Networking
{
    public struct NetworkPlayerData : INetworkSerializable,IEquatable<NetworkPlayerData>
    {
        public ulong ClientId;
        public FixedString64Bytes Name;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Name);
        }
        public bool Equals(NetworkPlayerData other)
        { 
            return ClientId== other.ClientId  && Name.Equals(other.Name);
        }
    }
}

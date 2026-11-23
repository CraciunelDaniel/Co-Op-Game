using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Gameplay.Spawning
{
    public class PlayerSpawnService : IStartable, IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly NetworkObject _playerPrefab;
        private readonly Transform[] _spawnPoints;
        private readonly string _gameplaySceneName;

        private readonly HashSet<ulong> _spawnedClients = new();
        private readonly NetworkVariable<ushort> _nextSpawnIndex=new();

        public PlayerSpawnService(NetworkManager networkManager, NetworkObject playerPrefab
            , Transform[] spawnPoints, string gameplaySceneName)
        {
            _networkManager = networkManager;
            _playerPrefab = playerPrefab;
            _spawnPoints = spawnPoints;
            _gameplaySceneName = gameplaySceneName;
        }
        public void Start()
        {
            if(!_networkManager.IsServer)return;

            foreach (ulong clientId in _networkManager.ConnectedClientsIds)
            {
                SpawnForClient(clientId);
            }

            _networkManager.SceneManager.OnLoadComplete += HandleClientSceneLoadComplete;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
        }
        
        private void SpawnForClient(ulong clientId)
        {
            if(_spawnedClients.Contains(clientId))return;
        
            GetNextSpawnTransform(out Vector3 pos,out Quaternion rot);
            var instance=UnityEngine.Object.Instantiate(_playerPrefab, pos, rot);
            instance.SpawnAsPlayerObject(clientId,true);
        
            _spawnedClients.Add(clientId);
        }

        private void GetNextSpawnTransform(out Vector3 position, out Quaternion rotation)
        {
            if (_spawnedClients == null || _spawnPoints.Length == 0)
            {
                position=new Vector3(_nextSpawnIndex.Value*2f,1f,0f);
                rotation = Quaternion.identity;
            }
            else
            {
                Transform point=_spawnPoints[_nextSpawnIndex.Value%_spawnPoints.Length];
                position=point.position;
                rotation=point.rotation;
            }
            _nextSpawnIndex.Value++;
        }
        private void HandleClientDisconnected(ulong clientId)
        {
            _spawnedClients.Remove(clientId);
        }

        private void HandleClientSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (sceneName == _gameplaySceneName)
            {
                SpawnForClient(clientId);
            }
        }

        public void Dispose()
        {
            if(_networkManager==null||!_networkManager.IsServer) return;;

            if (_networkManager.SceneManager != null)
            {
                _networkManager.SceneManager.OnLoadComplete -= HandleClientSceneLoadComplete;
            }
            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }
}

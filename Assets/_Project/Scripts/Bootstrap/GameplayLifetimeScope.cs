using Core;
using Gameplay.Input;
using Gameplay.Items;
using Gameplay.Spawning;
using Scripts.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Bootstrap
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        [Header("Configs & Input")]
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private InteractionConfig _interactionConfig;
        [SerializeField] private InputActionAsset _inputActions;
           
        [Header("Prefabs & Spawning")]
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private NetworkPhysicsItem _itemPrefab;
        [SerializeField] private Transform[] _spawnPoints;
        

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_playerConfig);
            builder.RegisterInstance(_interactionConfig);
            builder.RegisterInstance(_inputActions);
            builder.RegisterInstance(_playerPrefab);
            builder.RegisterInstance(_itemPrefab);

            builder.Register<IPlayerInputService, PlayerInputService>(Lifetime.Singleton);

            builder.RegisterEntryPoint<PlayerSpawnService>(Lifetime.Scoped)
                .WithParameter(_playerPrefab)
                .WithParameter(_spawnPoints)
                .WithParameter(gameObject.scene.name);
        } 
    }
}

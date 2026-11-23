using System;
using Steamworks;
using UnityEngine;
using VContainer.Unity;

namespace Steam
{
    public class SteamLifecycleService : IInitializable,ITickable, IDisposable
    {
        private const uint SPACEWAR_APP_ID = 480;
        public bool IsInitialized { get; private set; }
        public void Initialize()
        {
            try
            {
                SteamClient.Init(SPACEWAR_APP_ID,false);
                IsInitialized = true;
                Debug.Log($"Steam Service initialized {SteamClient.Name}({SteamClient.SteamId})");
            }
            catch (Exception e)
            {
                IsInitialized = false;
                Debug.LogError($"Failed to initialize Steam Service {e.Message}");
            }
        }
        public void Tick()
        {
            if (IsInitialized)
            {
                SteamClient.RunCallbacks();
            }
        }
        public void Dispose()
        {
            if (IsInitialized)
            {
                SteamClient.Shutdown();
                IsInitialized = false;
            }
        }
    }
}

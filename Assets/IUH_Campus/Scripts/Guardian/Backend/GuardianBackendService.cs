using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IUHCampus.Guardian.Backend
{
    public sealed class GuardianBackendService : MonoBehaviour
    {
        [SerializeField] private GuardianBackendConfig config;
        private static GuardianBackendService owner;
        private CancellationTokenSource lifetime;
        private bool ownsConfig;
        public GuardianApiClient Client { get; private set; }
        public GuardianEventOutbox Outbox { get; private set; }

        private void Awake()
        {
            if (owner != null && owner != this) { Destroy(this); return; }
            owner = this;
            DontDestroyOnLoad(gameObject);
            if (config == null) { config = ScriptableObject.CreateInstance<GuardianBackendConfig>(); ownsConfig = true; }
            Client = new GuardianApiClient(config);
            Outbox = new GuardianEventOutbox(config, Client);
            lifetime = new CancellationTokenSource();
            Run();
        }

        private async void Run()
        {
            try
            {
                while (!lifetime.IsCancellationRequested)
                { await Outbox.FlushAsync(lifetime.Token); await Task.Delay(1000, lifetime.Token); }
            }
            catch (OperationCanceledException) { }
            catch (Exception error) { Debug.LogError("[IUH Guardian] Backend outbox stopped: " + error.GetType().Name); }
        }

        private async void OnApplicationPause(bool paused)
        {
            if (paused || lifetime == null || lifetime.IsCancellationRequested) return;
            try { await Outbox.FlushAsync(lifetime.Token); }
            catch (OperationCanceledException) { }
            catch (Exception error) { Debug.LogError("[IUH Guardian] Backend resume flush failed: " + error.GetType().Name); }
        }

        private void OnDestroy()
        {
            lifetime?.Cancel(); lifetime?.Dispose();
            if (owner == this) owner = null;
            if (ownsConfig) Destroy(config);
        }
    }
}

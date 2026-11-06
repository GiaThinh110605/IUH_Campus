using System;
using UnityEngine;

namespace IUHCampus.Guardian.Backend
{
    [CreateAssetMenu(menuName = "IUH Campus/Guardian Backend/Config")]
    public sealed class GuardianBackendConfig : ScriptableObject
    {
        public const string UrlPreference = "iuh.guardian.backend_url";
        public string baseUrl = "http://127.0.0.1:5001";
        [Min(1)] public int timeoutSeconds = 15;
        [Min(0.1f)] public float retryBaseSeconds = 2;
        [Min(1)] public float maxBackoffSeconds = 300;
        [Min(1)] public int maxAttempts = 8;
        [Min(1)] public int maxImageSide = 640;
        [Range(1, 100)] public int jpgQuality = 80;
        public bool useFakeScan = false;
        public string[] fakeInfestedClasses = { "laptop" };
        public string[] fakeLabels = { "laptop", "laptop", "chair", "chair" };
        public string fakeHint = "Fake: electronics";
        public IGuardianScanService CreateScanService() => useFakeScan
            ? new FakeScanService(fakeInfestedClasses, fakeLabels, fakeHint)
            : new BackendScanService(new GuardianApiClient(this));
        public string BaseUrl => PlayerPrefs.GetString(UrlPreference, baseUrl).TrimEnd('/');

        public static void SetBaseUrl(string value)
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https") || !string.IsNullOrEmpty(uri.UserInfo))
                throw new ArgumentException("An HTTP(S) URL without credentials is required.");
            PlayerPrefs.SetString(UrlPreference, value.TrimEnd('/'));
            PlayerPrefs.Save();
        }
    }
}

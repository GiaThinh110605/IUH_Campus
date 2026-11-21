using UnityEngine;

namespace IUHCampus.Guardian
{
    [DefaultExecutionOrder(1000)]
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class IUHAmbientParticles : MonoBehaviour
    {
        Transform cameraTransform;

        void Awake()
        {
            if (Application.isMobilePlatform)
                gameObject.SetActive(false);
        }

        void Start()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                enabled = false;
                return;
            }

            cameraTransform = mainCamera.transform;
            transform.position = cameraTransform.position;
            GetComponent<ParticleSystem>().Play();
        }

        void LateUpdate()
        {
            if (cameraTransform != null)
                transform.position = cameraTransform.position;
        }
    }
}

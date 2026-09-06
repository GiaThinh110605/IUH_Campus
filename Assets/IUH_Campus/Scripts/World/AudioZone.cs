using UnityEngine;

namespace IUHCampus.World
{
    public class AudioZone : MonoBehaviour
    {
        public string zoneId = "AUDIO_Courtyard";
        public AudioSource ambientAudioSource;
        [Range(0f, 1f)]
        public float targetVolume = 0.5f;
        public float fadeDuration = 1.5f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && ambientAudioSource != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeAudio(targetVolume));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && ambientAudioSource != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeAudio(0f));
            }
        }

        private System.Collections.IEnumerator FadeAudio(float target)
        {
            float start = ambientAudioSource.volume;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                ambientAudioSource.volume = Mathf.Lerp(start, target, elapsed / fadeDuration);
                yield return null;
            }
            ambientAudioSource.volume = target;
        }
    }
}

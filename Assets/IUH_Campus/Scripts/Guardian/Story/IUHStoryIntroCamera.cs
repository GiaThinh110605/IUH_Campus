using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace IUHCampus.Guardian.Story
{
    public sealed class IUHStoryIntroCamera : MonoBehaviour
    {
        Action done;
        Button skip;
        bool completed;
        public static void Play(IReadOnlyList<IUHStoryScript.CameraShot> shots, string caption, IUHStoryDialogueUI ui, Action onDone)
        {
            var go = new GameObject("Story Intro Camera", typeof(Camera));
            var camera = go.GetComponent<Camera>();
            var original = Camera.main;
            if (original != null)
            {
                camera.CopyFrom(original);
                camera.depth = original.depth + 100;
            }

            var runner = go.AddComponent<IUHStoryIntroCamera>();
            runner.done = onDone;
            runner.skip = ui.CreateSkipButton(runner.Finish);
            Time.timeScale = 0;
            ui.ShowCaption(caption, 8);
            runner.StartCoroutine(runner.Run(shots));
        }

        IEnumerator Run(IReadOnlyList<IUHStoryScript.CameraShot> shots)
        {
            foreach (var shot in shots)
            {
                for (float t = 0; t < shot.seconds; t += Time.unscaledDeltaTime)
                {
                    transform.position = shot.position + Vector3.right * (Mathf.SmoothStep(0, 1, t / shot.seconds) * 3);
                    transform.LookAt(shot.lookAt);
                    yield return null;
                }
            }

            Finish();
        }

        void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) Finish();
#elif ENABLE_LEGACY_INPUT_MANAGER
            if(Input.GetKeyDown(KeyCode.Escape)) Finish();
#endif
        }

        void Finish()
        {
            if (completed)
                return;
            completed = true;
            Time.timeScale = 1;
            if (skip != null)
                Destroy(skip.gameObject);
            var callback = done;
            done = null;
            Destroy(gameObject);
            callback?.Invoke();
        }

        void OnDisable()
        {
            Finish();
        }
    }
}

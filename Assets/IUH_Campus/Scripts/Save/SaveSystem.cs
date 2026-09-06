using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using IUHCampus.Player;
using IUHCampus.World;

namespace IUHCampus.Save
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        public const string SaveFileName = "IUH_Campus_Save.json";

        public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        private void Awake()
        {
            Instance = this;
        }

        public static bool SaveGame()
        {
            try
            {
                SaveContainer container = new SaveContainer();
                container.timestamp = DateTime.UtcNow.ToString("o");

                // 1. Capture Player Data
                PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
                if (player != null)
                {
                    PlayerState state = player.GetComponent<PlayerState>();
                    container.player.SetPose(player.transform.position, player.transform.rotation);
                    if (state != null)
                    {
                        container.player.currentSpawnPointId = state.currentSpawnPointId;
                    }
                }

                // 2. Capture World Objects
                if (WorldState.Instance != null)
                {
                    var allStates = WorldState.Instance.GetAllStates();
                    foreach (var kvp in allStates)
                    {
                        container.worldObjects.Add(new WorldObjectRecord(kvp.Key, kvp.Value));
                    }
                }

                string json = JsonUtility.ToJson(container, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[SaveSystem] Lưu game thành công vào: {SaveFilePath}\n{json}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Lỗi khi lưu game: {ex.Message}");
                return false;
            }
        }

        public static bool LoadGame()
        {
            try
            {
                if (!File.Exists(SaveFilePath))
                {
                    Debug.LogWarning($"[SaveSystem] Không tìm thấy file save tại: {SaveFilePath}");
                    return false;
                }

                string json = File.ReadAllText(SaveFilePath);
                SaveContainer container = JsonUtility.FromJson<SaveContainer>(json);

                // 1. Restore Player Pose
                PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
                if (player != null && container.player != null)
                {
                    player.Teleport(container.player.GetPosition(), container.player.GetRotation());
                    PlayerState state = player.GetComponent<PlayerState>();
                    if (state != null)
                    {
                        state.currentSpawnPointId = container.player.currentSpawnPointId;
                    }
                }

                // 2. Restore World Object States
                if (WorldState.Instance != null && container.worldObjects != null)
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    foreach (var rec in container.worldObjects)
                    {
                        dict[rec.objectId] = rec.stateValue;
                    }
                    WorldState.Instance.RestoreStates(dict);
                }

                Debug.Log($"[SaveSystem] Tải game thành công từ: {SaveFilePath} (Timestamp: {container.timestamp})");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Lỗi khi tải game: {ex.Message}");
                return false;
            }
        }

        public static bool HasSaveFile()
        {
            return File.Exists(SaveFilePath);
        }

        public static void DeleteSaveFile()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("[SaveSystem] Đã xóa file save.");
            }
        }
    }
}

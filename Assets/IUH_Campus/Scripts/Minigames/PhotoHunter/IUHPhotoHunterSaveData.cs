using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IUHCampus.Minigames.PhotoHunter
{
    [Serializable]
    public class PhotoRecord
    {
        public string targetId;
        public string displayName;
        public string timestamp;
        public string filePath;
        public int scoreEarned;

        public PhotoRecord() { }

        public PhotoRecord(string id, string name, string path, int score)
        {
            targetId = id;
            displayName = name;
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            filePath = path;
            scoreEarned = score;
        }

        public PhotoRecord Clone()
        {
            return new PhotoRecord
            {
                targetId = this.targetId,
                displayName = this.displayName,
                timestamp = this.timestamp,
                filePath = this.filePath,
                scoreEarned = this.scoreEarned
            };
        }
    }

    [Serializable]
    public class PhotoHunterSaveContainer
    {
        public string gameVersion = "1.0.0";
        public string lastSavedTimestamp;
        public int totalScore = 0;
        public List<string> completedTargetIds = new List<string>();
        public List<PhotoRecord> photoRecords = new List<PhotoRecord>();

        public PhotoHunterSaveContainer Clone()
        {
            var clone = new PhotoHunterSaveContainer
            {
                gameVersion = this.gameVersion,
                lastSavedTimestamp = this.lastSavedTimestamp,
                totalScore = this.totalScore,
                completedTargetIds = new List<string>(this.completedTargetIds)
            };

            foreach (var rec in this.photoRecords)
            {
                if (rec != null) clone.photoRecords.Add(rec.Clone());
            }

            return clone;
        }

        /// <summary>
        /// Validates records against active targets, sanitizing nulls, duplicates, and defunct IDs.
        /// Recomputes totalScore consistently. Returns true if sanitized successfully.
        /// </summary>
        public bool SanitizeAndValidate(IEnumerable<string> validTargetIds, int scorePerTarget = 100)
        {
            var validSet = validTargetIds != null ? new HashSet<string>(validTargetIds) : null;
            var cleanIds = new List<string>();
            var seen = new HashSet<string>();

            if (completedTargetIds != null)
            {
                foreach (var id in completedTargetIds)
                {
                    if (string.IsNullOrEmpty(id)) continue;
                    if (seen.Contains(id)) continue;
                    if (validSet != null && validSet.Count > 0 && !validSet.Contains(id)) continue;

                    seen.Add(id);
                    cleanIds.Add(id);
                }
            }

            completedTargetIds = cleanIds;

            // Clean photo records
            var cleanRecords = new List<PhotoRecord>();
            var seenRecordTargets = new HashSet<string>();
            if (photoRecords != null)
            {
                foreach (var rec in photoRecords)
                {
                    if (rec == null || string.IsNullOrEmpty(rec.targetId)) continue;
                    if (seenRecordTargets.Contains(rec.targetId)) continue;
                    if (!seen.Contains(rec.targetId)) continue;

                    seenRecordTargets.Add(rec.targetId);
                    cleanRecords.Add(rec);
                }
            }

            photoRecords = cleanRecords;
            totalScore = completedTargetIds.Count * scorePerTarget;
            return true;
        }

        /// <summary>
        /// Attempts to register a photo capture idempotently.
        /// Returns true if this was a new completion and points were rewarded.
        /// Returns false if already recorded before (preventing duplicate rewards).
        /// </summary>
        public bool RecordCompletion(string targetId, string displayName, string filePath, int rewardScore, out int awardedPoints)
        {
            awardedPoints = 0;
            if (string.IsNullOrEmpty(targetId)) return false;

            if (completedTargetIds.Contains(targetId))
            {
                // Already recorded - do not award points again
                return false;
            }

            completedTargetIds.Add(targetId);
            awardedPoints = rewardScore;
            totalScore += rewardScore;

            photoRecords.RemoveAll(r => r.targetId == targetId);
            photoRecords.Add(new PhotoRecord(targetId, displayName, filePath, rewardScore));
            lastSavedTimestamp = DateTime.UtcNow.ToString("o");
            return true;
        }
    }

    /// <summary>
    /// Thread-safe and persistent data storage dedicated to Photo Hunter minigame.
    /// Preserves player progress and photo album across play sessions without altering global saves.
    /// </summary>
    public static class IUHPhotoHunterStorage
    {
        public const string SaveFileName = "IUH_PhotoHunter_Save.json";
        public const string MetadataFileName = "IUH_PhotoHunter_Metadata.json";
        public const string PhotosDirectoryName = "IUH_PhotoHunter";

        private static string s_CustomStorageDirectory = null;
        private static bool s_SimulateWriteFailureForTest = false;

        public static void SetCustomStorageDirectoryForTest(string dir)
        {
            s_CustomStorageDirectory = dir;
        }

        public static void SetSimulateWriteFailureForTest(bool simulateFailure)
        {
            s_SimulateWriteFailureForTest = simulateFailure;
        }

        public static string BaseDirectory => string.IsNullOrEmpty(s_CustomStorageDirectory) ? Application.persistentDataPath : s_CustomStorageDirectory;
        public static string SaveFilePath => Path.Combine(BaseDirectory, SaveFileName);
        public static string MetadataFilePath => Path.Combine(BaseDirectory, MetadataFileName);
        public static string PhotosDirectory => Path.Combine(BaseDirectory, PhotosDirectoryName);

        public static string EnsurePhotosDirectory()
        {
            string dir = PhotosDirectory;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        /// <summary>
        /// Writes save progress. Returns true only if the primary save file wrote successfully.
        /// </summary>
        public static bool SaveProgress(PhotoHunterSaveContainer container)
        {
            if (container == null) return false;
            if (s_SimulateWriteFailureForTest)
            {
                Debug.LogWarning("[PhotoHunter Storage] Giả lập lỗi ghi đĩa cho kiểm thử transaction rollback.");
                return false;
            }
            string temporaryPath = null;
            try
            {
                container.lastSavedTimestamp = DateTime.UtcNow.ToString("o");
                string json = JsonUtility.ToJson(container, true);
                Directory.CreateDirectory(BaseDirectory);
                temporaryPath = SaveFilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
                File.WriteAllText(temporaryPath, json);
                // Keep the previous save intact until the replacement is complete.
                if (File.Exists(SaveFilePath)) File.Replace(temporaryPath, SaveFilePath, null);
                else File.Move(temporaryPath, SaveFilePath);

                // Mirror metadata export (non-critical; failure logged as warning only)
                try
                {
                    File.WriteAllText(MetadataFilePath, json);
                }
                catch (Exception metaEx)
                {
                    Debug.LogWarning($"[PhotoHunter Storage] Ghi file metadata phụ thất bại (không ảnh hưởng save chính): {metaEx.Message}");
                }

                Debug.Log($"[PhotoHunter Storage] Đã lưu tiến trình vào: {SaveFilePath} (Điểm: {container.totalScore}, Đã chụp: {container.completedTargetIds.Count})");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhotoHunter Storage] Lỗi nghiêm trọng khi lưu tiến trình: {ex.Message}");
                return false;
            }
            finally
            {
                if (temporaryPath != null && File.Exists(temporaryPath))
                {
                    try { File.Delete(temporaryPath); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
        }

        public static PhotoHunterSaveContainer LoadProgress()
        {
            try
            {
                if (!File.Exists(SaveFilePath))
                {
                    return new PhotoHunterSaveContainer();
                }

                string json = File.ReadAllText(SaveFilePath);
                var container = JsonUtility.FromJson<PhotoHunterSaveContainer>(json);
                if (container == null) container = new PhotoHunterSaveContainer();
                return container;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhotoHunter Storage] Lỗi khi đọc file save: {ex.Message}");
                return new PhotoHunterSaveContainer();
            }
        }

        public static void ResetProgress()
        {
            try
            {
                if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
                if (File.Exists(MetadataFilePath)) File.Delete(MetadataFilePath);
                Debug.Log("[PhotoHunter Storage] Đã đặt lại tiến trình Photo Hunter.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PhotoHunter Storage] Không thể xóa file save: {ex.Message}");
            }
        }

        public static bool HasSaveFile()
        {
            return File.Exists(SaveFilePath);
        }
    }
}

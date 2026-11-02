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
                completedTargetIds = new List<string>(this.completedTargetIds??new List<string>())
            };

            foreach (var rec in this.photoRecords??new List<PhotoRecord>())
            {
                if (rec != null) clone.photoRecords.Add(rec.Clone());
            }

            return clone;
        }

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
                    if (validSet != null && !validSet.Contains(id)) continue;

                    seen.Add(id);
                    cleanIds.Add(id);
                }
            }

            completedTargetIds = cleanIds;

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

        public bool RecordCompletion(string targetId, string displayName, string filePath, int rewardScore, out int awardedPoints)
        {
            awardedPoints = 0;
            if (string.IsNullOrEmpty(targetId)) return false;
            if(completedTargetIds==null)completedTargetIds=new List<string>();
            if(photoRecords==null)photoRecords=new List<PhotoRecord>();

            if (completedTargetIds.Contains(targetId))
            {

                int originalReward=photoRecords.Find(r=>r!=null&&r.targetId==targetId)?.scoreEarned??rewardScore;
                photoRecords.RemoveAll(r=>r==null||r.targetId==targetId);
                photoRecords.Add(new PhotoRecord(targetId,displayName,filePath,originalReward));
                lastSavedTimestamp=DateTime.UtcNow.ToString("o");
                return false;
            }

            completedTargetIds.Add(targetId);
            awardedPoints = rewardScore;
            totalScore += rewardScore;

            photoRecords.RemoveAll(r => r==null||r.targetId == targetId);
            photoRecords.Add(new PhotoRecord(targetId, displayName, filePath, rewardScore));
            lastSavedTimestamp = DateTime.UtcNow.ToString("o");
            return true;
        }
    }

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

                if (File.Exists(SaveFilePath)) File.Replace(temporaryPath, SaveFilePath, null);
                else File.Move(temporaryPath, SaveFilePath);

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

        public static bool ResetProgress()
        {

            return SaveProgress(new PhotoHunterSaveContainer());
        }

        public static bool IsManagedJpeg(string path)
        {
            if(string.IsNullOrEmpty(path))return false;
            try
            {
                string full=Path.GetFullPath(path);string directory=Path.GetFullPath(BaseDirectory).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)+Path.DirectorySeparatorChar;
                var comparison=Application.platform==RuntimePlatform.WindowsEditor||Application.platform==RuntimePlatform.WindowsPlayer?StringComparison.OrdinalIgnoreCase:StringComparison.Ordinal;
                return full.StartsWith(directory,comparison)&&(string.Equals(Path.GetExtension(full),".jpg",StringComparison.OrdinalIgnoreCase)||string.Equals(Path.GetExtension(full),".jpeg",StringComparison.OrdinalIgnoreCase));
            }
            catch(ArgumentException){return false;}catch(NotSupportedException){return false;}catch(PathTooLongException){return false;}
        }

        public static bool HasSaveFile()
        {
            return File.Exists(SaveFilePath);
        }
    }
}

using System;
using System.Globalization;
using UnityEngine;

namespace IUHCampus.Guardian.Backend
{
    public static class GuardianSession
    {
        public static string Id { get; private set; } = Guid.NewGuid().ToString();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void BeginSession() => Id = Guid.NewGuid().ToString();
    }

    public sealed class GuardianEvent
    {
        public string EventId { get; }
        public string Json { get; }
        internal GuardianEvent(string id, string json) { EventId = id; Json = json; }
    }

    public static class GuardianEventFactory
    {
        [Serializable] private sealed class Header
        {
            public string event_id, session_id, quest_id, milestone_id, event_type, occurred_at;
        }
        [Serializable] private sealed class Scan { public string label; }
        [Serializable] private sealed class Upgrade { public string branch; public int level; }

        private static GuardianEvent Create(string quest, string milestone, string kind, string payload = null)
        {
            var header = new Header { event_id = Guid.NewGuid().ToString(), session_id = GuardianSession.Id,
                quest_id = quest, milestone_id = milestone, event_type = kind,
                occurred_at = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) };
            string json = JsonUtility.ToJson(header);
            if (payload != null) json = json.Substring(0, json.Length - 1) + ",\"payload\":" + payload + "}";
            return new GuardianEvent(header.event_id, json);
        }

        public static GuardianEvent ScanConfirmed(string quest, string milestone, string label, float confidence)
        {
            if (float.IsNaN(confidence) || float.IsInfinity(confidence)) throw new ArgumentException("Confidence must be finite.");
            string payload = JsonUtility.ToJson(new Scan { label = label });
            // Preserve 0.70f as JSON 0.7 instead of JsonUtility's widened 0.699999988 value.
            payload = payload.Substring(0, payload.Length - 1) + ",\"confidence\":" + confidence.ToString("R", CultureInfo.InvariantCulture) + "}";
            return Create(quest, milestone, "scan_confirmed", payload);
        }
        public static GuardianEvent CombatWon(string quest, string milestone) => Create(quest, milestone, "combat_won");
        public static GuardianEvent CombatLost(string quest, string milestone) => Create(quest, milestone, "combat_lost");
        public static GuardianEvent UpgradePurchased(string quest, string milestone, string branch, int level)
        {
            if ((branch != "attack" && branch != "defense" && branch != "vitality") || level < 1 || level > 2)
                throw new ArgumentException("Invalid upgrade branch or level.");
            return Create(quest, milestone, "upgrade_purchased", JsonUtility.ToJson(new Upgrade { branch = branch, level = level }));
        }
    }
}

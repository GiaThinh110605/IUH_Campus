using System.Text.RegularExpressions;

namespace IUHCampus.Guardian.Backend
{
    public static class GuardianCombatReport
    {
        private static readonly Regex Identifier = new Regex(@"\A[A-Za-z0-9_-]{1,64}\z");

        public static GuardianEvent Create(bool won, string questId, string milestoneId)
        {
            if (questId == null || milestoneId == null ||
                !Identifier.IsMatch(questId) || !Identifier.IsMatch(milestoneId)) return null;

            return won ? GuardianEventFactory.CombatWon(questId, milestoneId)
                : GuardianEventFactory.CombatLost(questId, milestoneId);
        }
    }
}

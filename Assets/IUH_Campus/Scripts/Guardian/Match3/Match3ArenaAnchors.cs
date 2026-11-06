using UnityEngine;

namespace IUHCampus.Guardian.Match3
{
    /// <summary>
    /// Lives in the IUH_CombatArena scene and tells <see cref="Match3BattleStage"/> where the
    /// fighters stand and which camera films them. The center's +X axis points from the student to the monster.
    /// </summary>
    public class Match3ArenaAnchors : MonoBehaviour
    {
        [SerializeField] private Transform m_Center;
        [SerializeField] private Camera m_Camera;

        public Transform Center => m_Center != null ? m_Center : transform;
        public Camera ArenaCamera => m_Camera;
    }
}

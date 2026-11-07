using UnityEngine;

namespace IUHCampus.Guardian.Match3
{
    /// <summary>Slow spin and bob for the floating data cubes in the combat arena.</summary>
    public class IUHDataSpin : MonoBehaviour
    {
        [SerializeField] private Vector3 m_SpinDegreesPerSecond = new Vector3(0f, 25f, 10f);
        [SerializeField] private float m_BobHeight = 0.4f;
        [SerializeField] private float m_BobSpeed = 0.6f;

        private Vector3 m_Origin;
        private float m_Phase;

        private void Awake()
        {
            m_Origin = transform.localPosition;
            m_Phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            transform.Rotate(m_SpinDegreesPerSecond * Time.deltaTime, Space.Self);
            transform.localPosition = m_Origin + Vector3.up * (Mathf.Sin(Time.time * m_BobSpeed + m_Phase) * m_BobHeight);
        }
    }
}

using UnityEngine;

namespace IUHCampus.World
{
    public enum SurfaceType
    {
        Concrete,
        Tile,
        Asphalt,
        Wood,
        Metal
    }

    public class SurfaceIdentifier : MonoBehaviour
    {
        public SurfaceType surfaceType = SurfaceType.Concrete;
    }
}

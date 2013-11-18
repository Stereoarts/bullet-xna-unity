#if UNITY
using UnityEngine;
#else
using Microsoft.Xna.Framework;
#endif

namespace BulletXNA.BulletCollision.CollisionShapes.GImpact
{
    public static class GeometeryOperations
    {
#if UNITY
        public static void bt_edge_plane(ref Vector3  e1,ref Vector3  e2, ref Vector3 normal, out Plane plane)
#else
        public static void bt_edge_plane(ref Vector3  e1,ref Vector3  e2, ref Vector3 normal, out Vector4 plane)
#endif
        {
	        Vector3 planenormal = Vector3.Cross(e2-e1,normal);
	        planenormal.Normalize();
#if UNITY
			plane = new Plane(planenormal,Vector3.Dot(e2,planenormal));
#else
	        plane = new Vector4(planenormal,Vector3.Dot(e2,planenormal));
#endif
        }

    }
}

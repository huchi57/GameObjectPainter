using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanFox.GameObjectPainter
{
    public static class GeometryExtensions
    {
        public static bool IsPointInCylinder(this Vector3 point, Vector3 surface1Center, Vector3 surface2Center, float cylinderRadius)
        {
            var pointProjectedOnCylinderAxis = surface1Center + Vector3.Project(point - surface1Center, surface2Center - surface1Center);
            if (Vector3.Dot(pointProjectedOnCylinderAxis - surface1Center, surface2Center - surface1Center) <= 0)
            {
                return false;
            }
            if (Vector3.Dot(pointProjectedOnCylinderAxis - surface2Center, surface1Center - surface2Center) <= 0)
            {
                return false;
            }
            return Vector3.Distance(pointProjectedOnCylinderAxis, point) < cylinderRadius;
        }

        public static bool IsPointInTetrahedron(this Vector3 point, Vector3 shapeVertex0, Vector3 shapeVertex1, Vector3 shapeVertex2, Vector3 shapeVertex3)
        {
            // A tetrahedron (triangular pyramid) is consisted with 4 triangles.
            var plane = new Plane(shapeVertex0, shapeVertex1, shapeVertex2);
            if (!plane.SameSide(point, shapeVertex3))
            {
                return false;
            }
            plane = new Plane(shapeVertex0, shapeVertex1, shapeVertex3);
            if (!plane.SameSide(point, shapeVertex2))
            {
                return false;
            }
            plane = new Plane(shapeVertex0, shapeVertex2, shapeVertex3);
            if (!plane.SameSide(point, shapeVertex1))
            {
                return false;
            }
            plane = new Plane(shapeVertex1, shapeVertex2, shapeVertex3);
            if (!plane.SameSide(point, shapeVertex0))
            {
                return false;
            }

            // Special case, tetrahedron height = 0: it is essentially a plane.
            if (Vector3.Distance(shapeVertex0, plane.ClosestPointOnPlane(shapeVertex0)).IsApproximatelyZero())
            {
                return false;
            }
            return true;
        }

        public static bool IsPointInTetrahedron(this Vector3 point, params Vector3[] shapeVertices)
        {
            if (shapeVertices.IsNullOrEmpty() || shapeVertices.Length != 4)
            {
                return false;
            }
            return point.IsPointInTetrahedron(shapeVertices[0], shapeVertices[1], shapeVertices[2], shapeVertices[3]);
        }

        public static bool IsPointInTetrahedron(this Vector3 point, IList<Vector3> shapeVertices)
        {
            if (shapeVertices.IsNullOrEmpty() || shapeVertices.Count != 4)
            {
                return false;
            }
            return point.IsPointInTetrahedron(shapeVertices[0], shapeVertices[1], shapeVertices[2], shapeVertices[3]);
        }

        public static bool IsPointInConvexPolygon(this Vector3 point, params Vector3[] shapeVertices)
        {
            if (shapeVertices.IsNullOrEmpty())
            {
                return false;
            }

            // Special case for 1 point.
            if (shapeVertices.Length == 1)
            {
                return Vector3.Distance(point, shapeVertices[0]).IsApproximatelyZero();
            }

            // Special case for 2 points (a line).
            if (shapeVertices.Length == 2)
            {
                var pointProjectedOnLine = shapeVertices[0] + Vector3.Project(point - shapeVertices[0], shapeVertices[1] - shapeVertices[0]);
                return Vector3.Distance(point, pointProjectedOnLine).IsApproximatelyZero();
            }

            // Special case for 3 points (a plane).
            if (shapeVertices.Length == 3)
            {
                var plane = new Plane(shapeVertices[0], shapeVertices[1], shapeVertices[2]);
                return Vector3.Distance(point, plane.ClosestPointOnPlane(point)).IsApproximatelyZero();
            }

            // Base case (a tetrahedron).
            if (shapeVertices.Length == 4)
            {
                return point.IsPointInTetrahedron(shapeVertices);
            }

            // General case (shapes with 5 or more vertices can be break down into different tetrahedrons).
            for (int i = 0; i < shapeVertices.Length - 3; i++)
            {
                for (int j = i + 1; j < shapeVertices.Length - 2; j++)
                {
                    for (int k = j + 1; k < shapeVertices.Length - 1; k++)
                    {
                        for (int l = k + 1; l < shapeVertices.Length; l++)
                        {
                            if (point.IsPointInTetrahedron(shapeVertices[i], shapeVertices[j], shapeVertices[k], shapeVertices[l]))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsPointInConvexPolygon(this Vector3 point, IList<Vector3> shapeVertices)
        {
            if (shapeVertices.IsNullOrEmpty())
            {
                return false;
            }
            return point.IsPointInConvexPolygon(shapeVertices.ToArray());
        }
    }
}

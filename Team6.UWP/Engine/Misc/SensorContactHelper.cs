using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Team6.Engine.Misc
{
    public static class SensorContactHelper
    {
        private enum ContactType
        {
            NotSupported,
            Polygon,
            PolygonAndCircle,
            Circle,
            EdgeAndPolygon,
            EdgeAndCircle,
            ChainAndPolygon,
            ChainAndCircle,
        }

        private static ContactType[,] _registers = new[,]
                                                      {
                                                           {
                                                               ContactType.Circle,
                                                               ContactType.EdgeAndCircle,
                                                               ContactType.PolygonAndCircle,
                                                               ContactType.ChainAndCircle,
                                                           },
                                                           {
                                                               ContactType.EdgeAndCircle,
                                                               ContactType.NotSupported,
                                                               // 1,1 is invalid (no ContactType.Edge)
                                                               ContactType.EdgeAndPolygon,
                                                               ContactType.NotSupported,
                                                               // 1,3 is invalid (no ContactType.EdgeAndLoop)
                                                           },
                                                           {
                                                               ContactType.PolygonAndCircle,
                                                               ContactType.EdgeAndPolygon,
                                                               ContactType.Polygon,
                                                               ContactType.ChainAndPolygon,
                                                           },
                                                           {
                                                               ContactType.ChainAndCircle,
                                                               ContactType.NotSupported,
                                                               // 3,1 is invalid (no ContactType.EdgeAndLoop)
                                                               ContactType.ChainAndPolygon,
                                                               ContactType.NotSupported,
                                                               // 3,3 is invalid (no ContactType.Loop)
                                                           },
                                                       };

        public static Manifold CalculateContactPoints(this Contact contact, out Vector2 worldNormal, out FixedArray2<Vector2> worldPoints)
        {
            // Farseer does not generate manifolds for sensors
            // => taken from farseer source code, performing manifold collision calculation for sensors
            if (!(contact.FixtureA.IsSensor || contact.FixtureB.IsSensor))
            {
                contact.GetWorldManifold(out worldNormal, out worldPoints);
                return contact.Manifold;
            }

            Shape shapeA = contact.FixtureA.Shape;
            Transform transformA;
            contact.FixtureA.Body.GetTransform(out transformA);

            Shape shapeB = contact.FixtureB.Shape;
            Transform transformB;
            contact.FixtureB.Body.GetTransform(out transformB);

            Manifold manifold = new Manifold();

            EdgeShape edgeShape;

            switch (_registers[(int)shapeA.ShapeType, (int)shapeB.ShapeType])
            {
                case ContactType.Polygon:
                    Collision.CollidePolygons(ref manifold, (PolygonShape)shapeA, ref transformA, (PolygonShape)shapeB, ref transformB);
                    break;
                case ContactType.PolygonAndCircle:
                    Collision.CollidePolygonAndCircle(ref manifold, (PolygonShape)shapeA, ref transformA, (CircleShape)shapeB, ref transformB);
                    break;
                case ContactType.EdgeAndCircle:
                    Collision.CollideEdgeAndCircle(ref manifold, (EdgeShape)shapeA, ref transformA, (CircleShape)shapeB, ref transformB);
                    break;
                case ContactType.EdgeAndPolygon:
                    Collision.CollideEdgeAndPolygon(ref manifold, (EdgeShape)shapeA, ref transformA, (PolygonShape)shapeB, ref transformB);
                    break;
                case ContactType.ChainAndCircle:
                    ChainShape chain = (ChainShape)shapeA;
                    edgeShape = chain.GetChildEdge(contact.ChildIndexA);
                    Collision.CollideEdgeAndCircle(ref manifold, edgeShape, ref transformA, (CircleShape)shapeB, ref transformB);
                    break;
                case ContactType.ChainAndPolygon:
                    ChainShape loop2 = (ChainShape)shapeA;
                    edgeShape = loop2.GetChildEdge(contact.ChildIndexA);
                    Collision.CollideEdgeAndPolygon(ref manifold, edgeShape, ref transformA, (PolygonShape)shapeB, ref transformB);
                    break;
                case ContactType.Circle:
                    Collision.CollideCircles(ref manifold, (CircleShape)shapeA, ref transformA, (CircleShape)shapeB, ref transformB);
                    break;
            }

            ContactSolver.WorldManifold.Initialize(ref manifold, ref transformA, shapeA.Radius, ref transformB, shapeB.Radius, out worldNormal, out worldPoints);
            return manifold;
        }

    }
}

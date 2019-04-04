using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Misc;

namespace Team6.Engine.Components
{
    public class PhysicsComponent : Component, IDebugDrawable
    {
        protected Color debugShapeColor = Color.Coral;

        public Shape Shape { get; private set; }
        public Fixture Fixture { get; private set; }

        public PhysicsComponent(Shape shape = null)
        {
            this.Shape = shape ?? new CircleShape(0.5f, 1.0f);
        }


        public override void Initialize()
        {
            base.Initialize();

            this.Fixture = Entity.Body.CreateFixture(this.Shape);
            this.Fixture.Body.LocalCenter = new Vector2(0,0);
        }

        public void ReplaceShape(Shape newShape)
        {
            Entity.Body.DestroyFixture(Fixture);
            Shape = newShape;
            this.Fixture = Entity.Body.CreateFixture(newShape);
        }

        public virtual void DebugDraw(DebugRenderer renderer, float elapsedSeconds, float totalSeconds)
        {
            var body = Entity.Body;
            switch (Fixture.Shape.ShapeType)
            {
                case ShapeType.Circle:
                    var circle = (CircleShape)Fixture.Shape;
                    var radius = circle.Radius;
                    renderer.DebugDrawCircle(body.Position + circle.Position, radius, color: debugShapeColor);
                    break;

                case ShapeType.Polygon:
                    var polygon = (PolygonShape)Fixture.Shape;

                    var transformedPos = polygon.Vertices.Select(v =>
                    {
                        return Vector2.Transform(v, Matrix.CreateRotationZ(body.Rotation));
                    }).ToArray();


                    renderer.DebugDrawPolygon(transformedPos, body.Position, color: debugShapeColor);
                    break;
            }

            // Draw border
            if (body.FixtureList.Count > 1 && body.BodyType == BodyType.Static)
            {
                foreach (var f in body.FixtureList)
                {
                    if (f.Shape is PolygonShape)
                    {
                        var polygon = (PolygonShape)f.Shape;
                        renderer.DebugDrawPolygon(polygon.Vertices, f.Body.Position);
                    }
                }
            }
        }

    }
}

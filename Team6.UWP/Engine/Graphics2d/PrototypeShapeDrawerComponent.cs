using System;
using System.Linq;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Team6.Engine.Components;

namespace Team6.Engine.Graphics2d
{
	public class PrototypeShapeDrawerComponent : Component, IDrawableComponent
	{
	    private Vertices shape;
	    private Color color;
	    private float layerDepth;

		public PrototypeShapeDrawerComponent(Vertices vertices, Color? color = null, float layerDepth = 0)
		{
		    this.color = color ?? Color.Chartreuse;
		    this.shape = vertices;
		    this.layerDepth = layerDepth;
		}

	    public void Draw(SpriteBatch spriteBatch, float elapsedSeconds, float totalSeconds)
	    {
	        var transformedPos = shape.Select(v =>
	        {
	            return Vector2.Transform(v, Matrix.CreateRotationZ(this.Entity.Body.Rotation));
	        }).ToArray();

	        this.Entity.Game.Debug.DebugDrawPolygon(transformedPos, this.Entity.Body.Position, color, false, layerDepth);
	    }
	}
}

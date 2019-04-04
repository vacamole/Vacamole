using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Common;
using Team6.Engine.UI;

namespace Team6.Engine.Misc
{
    public class DebugRenderer
    {

        public Texture2D DebugRectangle { get; private set; }
        /// <summary>
        /// Font used to render debug stuff
        /// </summary>
        public MultiSizeSpriteFont DebugFont { get; private set; }

        public SpriteBatch SpriteBatch { get; private set; }


        public DebugRenderer(SpriteBatch spriteBatch, MainGame game)
        {
            this.SpriteBatch = spriteBatch;
            DebugFont = new MultiSizeSpriteFont(game, MultiSizeSpriteFontDefinition.Create(game, "Fonts\\DebugFont\\DebugFont").WithDefaultAvailableSizes());
            DebugRectangle = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            DebugRectangle.SetData(new[] { Color.White });
        }

        public void DebugDrawPoint(Vector2 point, Color? color = null)
        {
            var drawColor = color ?? Color.Coral;

            SpriteBatch.Draw(DebugRectangle, point, null,
                drawColor, 0, new Vector2(0.5f, 0.5f), 0.2f, SpriteEffects.None, 0);
        }

        public void DebugDrawLine(Vector2 a, Vector2 b, Vector2 offset = default(Vector2), Color? color = null, float layerDepth=1)
        {
            var drawColor = color ?? Color.Coral;
            var length = (a - b).Length();
            var rot = (float)Math.Atan2((b - a).Y, (b - a).X);
            var center = (a + b) / 2;
            SpriteBatch.Draw(DebugRectangle,
                center + offset, null,
                drawColor, rot, new Vector2(0.5f, 0.5f), new Vector2(length, 0.05f),
                SpriteEffects.None, (0.5f - layerDepth * 0.5f));
        }

        public void DebugDrawPolygon(Vertices vertices, Vector2 offset = default(Vector2), Color? color = null,
            bool drawNormal = true)
        {
            DebugDrawPolygon(vertices.Select(v => new Vector2(v.X, v.Y)).ToArray(), offset, color, drawNormal);
        }

        public void DebugDrawPolygon(Vector2[] vertices, Vector2 offset = default(Vector2), Color? color = null, bool drawNormal=true, float layerDepth=1)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                var a = vertices[i % vertices.Length];
                var b = vertices[(i + 1) % vertices.Length];
                DebugDrawLine(a, b, offset, color, layerDepth);
                if (drawNormal)
                {
                    // Since winding matters, draw the normal
                    var middle = (a + b) / 2;
                    var norm = Vector3.Normalize(Vector3.Cross(new Vector3((b - a), 0), Vector3.UnitZ));
                    DebugDrawLine(middle, middle + 0.3f * (new Vector2(norm.X, norm.Y)), offset, color, layerDepth);
                }
            }
        }

        public void DebugDrawCircle(Vector2 center, float radius, int numberOfSegments = 36, Color? color = null)
        {
            for (int i = 0; i < numberOfSegments; i++)
            {
                var a = i % numberOfSegments;
                var b = (i + 1) % numberOfSegments;
                float ax = (float)(radius * Math.Sin((float)a / numberOfSegments * 2.0 * Math.PI));
                float ay = (float)(radius * Math.Cos((float)a / numberOfSegments * 2.0 * Math.PI));
                float bx = (float)(radius * Math.Sin((float)b / numberOfSegments * 2.0 * Math.PI));
                float by = (float)(radius * Math.Cos((float)b / numberOfSegments * 2.0 * Math.PI));
                DebugDrawLine(new Vector2(ax, ay), new Vector2(bx, by), center, color);
            }
        }
    }
}

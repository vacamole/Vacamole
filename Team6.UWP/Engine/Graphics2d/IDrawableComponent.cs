using System;
using Microsoft.Xna.Framework.Graphics;

namespace Team6.Engine.Graphics2d
{
	public interface IDrawableComponent
	{
		void Draw(SpriteBatch spriteBatch, float elapsedSeconds, float totalSeconds);
	}
}
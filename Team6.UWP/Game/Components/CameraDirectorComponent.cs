using System;
using System.Collections.Generic;
using System.Linq;
using Comora;
using Microsoft.Xna.Framework;
using Team6.Engine.Components;
using Team6.Engine.Content;
using Team6.Game.Entities;
using Team6.Game.Misc;

namespace Team6.Game.Components
{
	public class CameraDirectorComponent : Component, IUpdateableComponent, IUnloadContent
	{
	    private readonly List<PlayerEntity> players;
	    private const float maxWidth = GameConstants.ScreenWidth;
	    private const float maxHeight = GameConstants.ScreenHeight;
	    private Vector2 lastPosition;
	    private Vector2 lastScreenSize = new Vector2(maxWidth, maxHeight);
	    private Vector2 lastPositionOffset;

	    public CameraDirectorComponent(List<PlayerEntity> players, Camera cam)
		{
		    this.players = players;
		    lastPosition = cam?.Position ?? default(Vector2);
		    lastScreenSize = new Vector2(cam?.Width ?? maxWidth, cam?.Height ?? maxHeight);
		    lastPositionOffset = cam?.PositionOffset ?? lastScreenSize/2.0f;
		}

	    public void Update(float elapsedSeconds, float totalSeconds)
	    {
	        var ratio = Entity.Game.GraphicsDevice.Viewport.AspectRatio;
	        var cam = Entity.Game.Camera;

	        // zoom in
	        var minMaxPos = players.Aggregate(Tuple.Create(new Vector2(100000, 100000), new Vector2(-100000, -100000)),
	            (c, p) =>
	            {
	                var po = p.Body.Position;
	                var minV = new Vector2(Math.Min(po.X, c.Item1.X), Math.Min(po.Y, c.Item1.Y));
	                var maxV = new Vector2(Math.Max(po.X, c.Item2.X), Math.Max(po.Y, c.Item2.Y));
	                return Tuple.Create(minV, maxV);
	            });

	        var wh = minMaxPos.Item2 - minMaxPos.Item1;
            // constrain within min/max zoom
            var ww = Math.Min(Math.Max(wh.X + 5, maxWidth / 2.0f), maxWidth);
            var hh = Math.Min(Math.Max(wh.Y + 5, maxHeight / 2.0f), maxHeight);
	        // take bigger side as actual w/h
	        var w = Math.Max(ww, hh*ratio);
	        var h = Math.Max(hh, ww/ratio);

	        var newWH = new Vector2(w, h);
	        var cameraWH = Vector2.Lerp(lastScreenSize, newWH, 2.5f * elapsedSeconds);
	        cam.Width = cameraWH.X;
	        cam.Height = cameraWH.Y;
	        cam.PositionOffset = Vector2.Lerp(-cameraWH / 2, lastPositionOffset, 2.5f * elapsedSeconds);
	        lastPositionOffset = cam.PositionOffset;
	        lastScreenSize = cameraWH;


	        // move cam to center of bounds
	        var centerPos = (minMaxPos.Item2 + minMaxPos.Item1) / 2.0f;
            // don't go outside the game bounds
	        var newPos = Vector2.Clamp(centerPos, new Vector2(-maxWidth + w, -maxHeight + h)/2, new Vector2(maxWidth - w, maxHeight - h)/2);

	        cam.Position = Vector2.Lerp(lastPosition, newPos, 2.5f * elapsedSeconds);
	        lastPosition = cam.Position;
	    }

	    public void UnloadContent()
	    {
	        /*this.Entity.Game.Camera.Position = Vector2.Zero;
	        this.Entity.Game.Camera.Width = 32;
	        this.Entity.Game.Camera.Height = 18;
	        this.Entity.Game.Camera.PositionOffset = new Vector2(-16, -9);*/
	    }
	}
}

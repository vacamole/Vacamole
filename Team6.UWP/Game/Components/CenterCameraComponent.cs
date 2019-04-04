using System;
using System.Linq;
using Comora;
using Microsoft.Xna.Framework;
using Team6.Engine.Components;
using Team6.Engine.Content;
using Team6.Game.Entities;
using Team6.Game.Misc;

namespace Team6.Game.Components
{
    public class CenterCameraComponent : Component, IUpdateableComponent
    {
        private const float screenWidth = GameConstants.ScreenWidth;
        private const float screenHeight = GameConstants.ScreenHeight;
        private Vector2 lastPosition = default(Vector2);
        private Vector2 lastWH;
        private Vector2 lastPositionOffset;
        private Vector2 centerPosition = new Vector2(0, 0);

        public CenterCameraComponent(Camera cam)
        {
            lastPosition = cam?.Position ?? default(Vector2);
            lastWH = new Vector2(cam?.Width ?? screenWidth, cam?.Height ?? screenHeight);
            lastPositionOffset = cam?.PositionOffset ?? lastWH / 2.0f;
        }

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            var ratio = Entity.Game.GraphicsDevice.Viewport.AspectRatio;
            var cam = Entity.Game.Camera;

            // constrain within min/max zoom
            var ww = screenWidth;
            var hh = screenHeight;

            // take bigger side as actual w/h
            var w = Math.Max(ww, hh * ratio);
            var h = Math.Max(hh, ww / ratio);

            var newWH = new Vector2(w, h);
            var cameraWH = Vector2.Lerp(lastWH, newWH, 2.5f * elapsedSeconds);
            cam.Width = cameraWH.X;
            cam.Height = cameraWH.Y;
            cam.PositionOffset = Vector2.Lerp(-cameraWH / 2, lastPositionOffset, 2.5f * elapsedSeconds);
            lastPositionOffset = cam.PositionOffset;
            lastWH = cameraWH;

            cam.Position = Vector2.Lerp(lastPosition, centerPosition, 2.5f * elapsedSeconds);
            lastPosition = cam.Position;
        }
    }
}

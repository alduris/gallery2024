using System.Collections.Generic;
using UnityEngine;

namespace Gallery2024.Graphics
{
    internal interface IPlayerGraphicsExtension
    {
        public void Update();
        public void InitiateSprites(int startSprite, List<FSprite> sprites);
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer);
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette);
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos);
    }
}

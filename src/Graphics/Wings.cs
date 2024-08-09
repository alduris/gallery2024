using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gallery2024.Graphics
{
    internal class Wings : IPlayerGraphicsExtension
    {
        const int TIME_SWITCH = 12;
        const float ALPHA_RATE = 1f / 10f;
        const float FLICKER_CHANCE = 1f / 280f;

        public PlayerGraphics owner;
        public int startSprite;

        public int timeSinceSwitch = 0;
        public float[,,] wingData; // feather, side, data (0 -> alpha, 1 -> lastAlpha, 2 -> mode [-1 -> fade out, 0 -> flicker, 1 -> fade in])
        private bool flying = false;
        private bool lastFlying = false;

        public Wings(PlayerGraphics owner)
        {
            this.owner = owner;

            wingData = new float[3, 2, 3];
            flying = owner.player.monkAscension;
            lastFlying = flying;
            for (int i = 0; i < wingData.GetLength(0); i++)
            {
                for (int j = 0; j < wingData.GetLength(1); j++)
                {
                    if (flying)
                    {
                        // Already flying
                        wingData[i, j, 0] = 1f;
                        wingData[i, j, 1] = 1f;
                        wingData[i, j, 2] = Random.value < 0.05f ? 0f : 1f;
                    }
                    else
                    {
                        // Not flying
                        wingData[i, j, 0] = 0f;
                        wingData[i, j, 1] = 0f;
                        wingData[i, j, 2] = -1f;
                    }
                }
            }
        }

        public void Update()
        {
            // Update flight tracking
            lastFlying = flying;
            flying = owner.player.monkAscension;

            if (lastFlying != flying)
                timeSinceSwitch = 0;
            else
                timeSinceSwitch++;

            // Update wings
            for (int i = 0; i < wingData.GetLength(0); i++)
            {
                for (int j = 0; j < wingData.GetLength(1); j++)
                {
                    wingData[i, j, 1] = wingData[i, j, 0];

                    if (timeSinceSwitch < TIME_SWITCH)
                    {
                        if ((!flying || wingData[i, j, 2] == -1f) && Random.value < Custom.LerpMap(timeSinceSwitch, 0, TIME_SWITCH, 0.25f, 0.99f))
                            wingData[i, j, 2] = flying ? (Random.value < 0.95f ? 1f : 0f) : -1f;
                    }
                    else if (wingData[i, j, 2] != -1f)
                    {
                        if (flying)
                        {
                            if (wingData[i, j, 2] == 1f && Random.value < FLICKER_CHANCE)
                            {
                                wingData[i, j, 2] = 0f;
                            }
                            else if (Random.value < 0.25f)
                            {
                                wingData[i, j, 2] = 1f;
                            }
                        }
                        else
                        {
                            wingData[i, j, 2] = -1f;
                        }
                    }

                    if (wingData[i, j, 2] == -1f)
                    {
                        wingData[i, j, 0] = Mathf.Max(0f, wingData[i, j, 0] - ALPHA_RATE);
                    }
                    else if (wingData[i, j, 2] == 1f)
                    {
                        wingData[i, j, 0] = Mathf.Min(1f, wingData[i, j, 0] + ALPHA_RATE);
                    }
                    else
                    {
                        wingData[i, j, 0] = Random.value;
                    }
                }
            }
        }

        public void InitiateSprites(int startSprite, List<FSprite> sprites)
        {
            this.startSprite = startSprite;
            for (int i = 0; i < wingData.GetLength(0); i++)
            {
                for (int j = 0; j < wingData.GetLength(1); j++)
                {
                    sprites.Add(new FSprite("EZ0", true) { anchorY = 0f });
                }
            }
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            FSprite furthestBack = sLeaser.sprites[0];
            for (int i = 1; i < sLeaser.sprites.Length; i++)
            {
                if (sLeaser.sprites[i].depth < furthestBack.depth)
                    furthestBack = sLeaser.sprites[i];
            }

            newContainer ??= rCam.ReturnFContainer("Midground");
            for (int i = 0; i < wingData.GetLength(0) * wingData.GetLength(1); i++)
            {
                newContainer.AddChild(sLeaser.sprites[startSprite + i]);
                if (i == 0)
                {
                    sLeaser.sprites[startSprite + i].MoveBehindOtherNode(furthestBack);
                }
                else
                {
                    sLeaser.sprites[startSprite + i].MoveBehindOtherNode(sLeaser.sprites[startSprite + i - 1]);
                }
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            int k = startSprite;
            for (int i = 0; i < wingData.GetLength(0); i++)
            {
                for (int j = 0; j < wingData.GetLength(1); j++, k++)
                {
                    sLeaser.sprites[k].color = Color.Lerp(Color.white, palette.fogColor, (float)i / wingData.GetLength(0) / 3f);
                }
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < wingData.GetLength(0) * wingData.GetLength(1); i++)
            {
                sLeaser.sprites[startSprite + i].isVisible = owner.player.room != null;
            }
            if (owner.player.room == null)
            {
                return;
            }

            Vector2 upperBodyDrawPos = Vector2.Lerp(owner.drawPositions[0, 1], owner.drawPositions[0, 0], timeStacker);
            Vector2 lowerBodyDrawPos = Vector2.Lerp(owner.drawPositions[1, 1], owner.drawPositions[1, 0], timeStacker);
            Vector2 basePosition = Vector2.Lerp(upperBodyDrawPos, lowerBodyDrawPos, 0.25f);
            Vector2 dir = Custom.DirVec(upperBodyDrawPos, lowerBodyDrawPos);
            Vector2 perp = Custom.PerpendicularVector(dir);
            const float DIST = 12f;
            const float ANGLE = 15f;
            const float ANGLEOFFSET = 10f;

            int k = startSprite;
            for (int i = 0; i < wingData.GetLength(0); i++)
            {
                float u = i / (wingData.GetLength(0) - 1f);
                for (int j = 0; j < wingData.GetLength(1); j++, k++)
                {
                    var flip = (j % 2 == 0 ? -1 : 1);
                    var wingDir = Custom.RotateAroundOrigo(perp * DIST * flip, ((i - (wingData.GetLength(0) / 2f)) * ANGLE + ANGLEOFFSET) * flip);
                    var wingFac = Mathf.Lerp(wingData[i, j, 1], wingData[i, j, 0], timeStacker);
                    var sprite = sLeaser.sprites[k];
                    sprite.alpha = wingFac;
                    sprite.SetPosition(basePosition + wingDir - camPos);
                    sprite.rotation = wingDir.GetAngle() + 90f;
                    (sprite.scaleX, sprite.scaleY) = (0.75f, Mathf.Lerp(Mathf.Lerp(0.6f, 0.3f, u), Mathf.Lerp(1.1f, 0.75f, u), wingFac));

                    if (wingData[i, j, 2] == 0f)
                    {
                        sprite.shader = rCam.game.rainWorld.Shaders["Hologram"];
                    }
                    else
                    {
                        sprite.shader = FShader.defaultShader;
                    }
                }
            }
        }

    }
}

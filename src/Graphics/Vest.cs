using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Gallery2024.Graphics
{
    internal class Vest : IPlayerGraphicsExtension
    {
        public PlayerGraphics owner;
        private readonly int divs;
        public Vector2[,,] clothPoints; // [x, y, i] where i = 0 -> pos, i = 1 -> lastPos, i = 2 -> vel
        public bool needsReset;
        private int startSprite;

        public Vest(PlayerGraphics owner)
        {
            divs = 4;
            this.owner = owner;
            clothPoints = new Vector2[divs, divs, 3];
            needsReset = true;
        }

        public void Update()
        {
            if (owner.player.room == null)
            {
                needsReset = true;
                return;
            }

            if (needsReset)
            {
                for (int i = 0; i < divs; i++)
                {
                    for (int j = 0; j < divs; j++)
                    {
                        clothPoints[i, j, 1] = owner.player.bodyChunks[1].pos;
                        clothPoints[i, j, 0] = owner.player.bodyChunks[1].pos;
                        clothPoints[i, j, 2] *= 0f;
                    }
                }
                needsReset = false;
            }

            Vector2 bodyPos = Vector2.Lerp(owner.head.pos, owner.player.bodyChunks[1].pos, 0.75f); // 75% between head and torso
            if (owner.player.bodyMode == Player.BodyModeIndex.Crawl)
            {
                bodyPos += new Vector2(0f, 4f);
            }
            else if (owner.player.bodyMode == Player.BodyModeIndex.Stand)
            {
                bodyPos += new Vector2(0f, Mathf.Sin(owner.player.animationFrame / 3f * Mathf.PI) * 2f);
            }

            Vector2 dir = Custom.DirVec(owner.player.bodyChunks[1].pos, owner.player.bodyChunks[0].pos) * 1.6f;
            Vector2 perp = Custom.PerpendicularVector(dir);
            for (int i = 0; i < divs; i++)
            {
                for (int j = 0; j < divs; j++)
                {
                    // float u = Mathf.InverseLerp(0f, divs - 1, i);
                    float v = Mathf.InverseLerp(0f, divs - 1, j);
                    clothPoints[i, j, 1] = clothPoints[i, j, 0];
                    clothPoints[i, j, 0] += clothPoints[i, j, 2];
                    clothPoints[i, j, 2] *= 0.95f; // stiffness
                    clothPoints[i, j, 2].y -= 1.1f * owner.player.EffectiveRoomGravity; // gravity effect

                    Vector2 idealPos = IdealPosForPoint(i, j, bodyPos, dir, perp);
                    Vector3 rot = Vector3.Slerp(-dir, Custom.DirVec(bodyPos, idealPos), v) * (0.01f + 0.9f * v);
                    clothPoints[i, j, 2] += new Vector2(rot.x, rot.y);
                    float dist = Vector2.Distance(clothPoints[i, j, 0], idealPos);
                    float maxDist = Mathf.Lerp(0f, 1f, v); // strength of bounce ?
                    Vector2 idealDir = Custom.DirVec(clothPoints[i, j, 0], idealPos);
                    if (dist > maxDist)
                    {
                        clothPoints[i, j, 0] -= (maxDist - dist) * idealDir * (1f - v / 1.4f);
                        clothPoints[i, j, 2] -= (maxDist - dist) * idealDir * (1f - v / 1.4f);
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        IntVector2 neighbor = new IntVector2(i, j) + Custom.fourDirections[k];
                        if (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.x < divs && neighbor.y < divs)
                        {
                            dist = Vector2.Distance(clothPoints[i, j, 0], clothPoints[neighbor.x, neighbor.y, 0]);
                            idealDir = Custom.DirVec(clothPoints[i, j, 0], clothPoints[neighbor.x, neighbor.y, 0]);
                            float neighborDist = Vector2.Distance(idealPos, IdealPosForPoint(neighbor.x, neighbor.y, bodyPos, dir, perp));
                            clothPoints[i, j, 2] -= (neighborDist - dist) * idealDir * 0.05f;
                            clothPoints[neighbor.x, neighbor.y, 2] += (neighborDist - dist) * idealDir * 0.05f;
                        }
                    }
                }
            }
        }

        private Vector2 IdealPosForPoint(int x, int y, Vector2 bodyPos, Vector2 dir, Vector2 perp)
        {
            float u = Mathf.InverseLerp(0f, divs - 1, x);
            float v = Mathf.InverseLerp(0f, divs - 1, y);
            return bodyPos
                + Mathf.Lerp(-1f, 1f, u) * perp * 1.2f * Mathf.Lerp(9f, 9f, v)
                //                                 top width ^   ^ bottom width
                + dir * Mathf.Lerp(8f, -1f, v) * (1f + Mathf.Sin(Mathf.PI * u) * 0.25f * Mathf.Lerp(-1f, 1f, v));
            //          top height ^   ^ bottom height
        }

        public Color VestColor()
        {
            return ExplorerGraphics.GetColor(owner, "Vest", ExplorerGraphics.DefaultVestColor);
        }

        public void InitiateSprites(int startSprite, List<FSprite> sprites)
        {
            this.startSprite = startSprite;
            sprites.Add(TriangleMesh.MakeGridMesh("Explorer24VestUnder", divs - 1));
            sprites.Add(TriangleMesh.MakeGridMesh("Explorer24Vest", divs - 1));
            for (int i = 0; i < divs; i++)
            {
                for (int j = 0; j < divs; j++)
                {
                    clothPoints[i, j, 0] = owner.player.firstChunk.pos;
                    clothPoints[i, j, 1] = owner.player.firstChunk.pos;
                    clothPoints[i, j, 2] = new Vector2(0f, 0f);
                }
            }
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            var container = newContainer ?? rCam.ReturnFContainer("Midground");
            container.AddChild(sLeaser.sprites[startSprite]);
            container.AddChild(sLeaser.sprites[startSprite + 1]);
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            for (int i = 0; i < divs; i++)
            {
                for (int j = 0; j < divs; j++)
                {
                    (sLeaser.sprites[startSprite] as TriangleMesh).verticeColors[j * divs + i] = Color.white; // under
                    (sLeaser.sprites[startSprite + 1] as TriangleMesh).verticeColors[j * divs + i] = VestColor(); // lines
                }
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[startSprite].isVisible = sLeaser.sprites[startSprite + 1].isVisible = owner.player.room != null;
            if (!sLeaser.sprites[startSprite].isVisible)
            {
                return;
            }

            float lookOffset;
            Vector2 upperBodyDrawPos = Vector2.Lerp(owner.drawPositions[0, 1], owner.drawPositions[0, 0], timeStacker);
            Vector2 lowerBodyDrawPos = Vector2.Lerp(owner.drawPositions[1, 1], owner.drawPositions[1, 0], timeStacker);
            if (owner.player.bodyMode == Player.BodyModeIndex.Stand && owner.player.input[0].x != 0)
            {
                // "Borrow" the calculations the code does to figure out which direction to put the player's face
                Vector2 headPos = Vector2.Lerp(owner.head.lastPos, owner.head.pos, timeStacker);

                Vector2 lookDir = (headPos - Vector2.Lerp(lowerBodyDrawPos, upperBodyDrawPos, 0.5f)).normalized;
                lookOffset = lookDir.x > 0 ? 1f : -1f;
            }
            else if (owner.player.bodyMode == Player.BodyModeIndex.Crawl)
            {
                // just always "look" in a certain direction because it's only correct in one direction if I use the stand code
                lookOffset = 1f;
            }
            else
            {
                // Rotate in direction of looking
                lookOffset = Vector2.Lerp(owner.lastLookDir, owner.lookDirection, timeStacker).normalized.x;
            }

            for (int x = 0; x < divs; x++)
            {
                float u = x / (divs - 1f);
                for (int y = 0; y < divs; y++)
                {
                    float v = y / (divs - 1f);
                    var clothPoint = Vector2.Lerp(clothPoints[x, y, 1], clothPoints[x, y, 0], timeStacker);
                    Vector2 offsetDir = owner.player.bodyMode == Player.BodyModeIndex.Crawl ? Vector2.down : Vector2.right;
                    clothPoint += offsetDir * (lookOffset * Mathf.Sin(Mathf.PI * u) * 5f); // account for look direction
                    if (owner.player.bodyMode == Player.BodyModeIndex.Stand && owner.player.input[0].x != 0) // account for run direction or something
                    {
                        var bodyAngle = upperBodyDrawPos - lowerBodyDrawPos;
                        clothPoint += Mathf.Lerp(1f, -1.5f, v) * Vector2.right * bodyAngle.x * Mathf.Pow(bodyAngle.normalized.y, 2f) * 0.65f;
                    }
                    (sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(x * divs + y, clothPoint - camPos);
                    (sLeaser.sprites[startSprite + 1] as TriangleMesh).MoveVertice(x * divs + y, clothPoint - camPos);
                }
            }
        }
    }
}

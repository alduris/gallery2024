using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;

namespace Gallery2024
{
    internal class Vest
    {
        public PlayerGraphics owner;
        private readonly int divs;
        public Vector2[,,] clothPoints;
        public bool needsReset;
        public readonly int totalSprites = 2;
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

            Vector2 bodyPos = Vector2.Lerp(owner.head.pos, owner.player.bodyChunks[1].pos, 0.75f);
            if (owner.player.bodyMode == Player.BodyModeIndex.Crawl)
            {
                bodyPos += new Vector2(0f, 4f);
            }

            Vector2 walkBob = default;
            if (owner.player.bodyMode == Player.BodyModeIndex.Stand)
            {
                bodyPos += new Vector2(0f, Mathf.Sin(owner.player.animationFrame / 3f * Mathf.PI) * 2f);
                walkBob = new Vector2(0f, -11f + Mathf.Sin(owner.player.animationFrame / 3f * Mathf.PI) * -2.5f);
            }

            Vector2 dir = Custom.DirVec(owner.player.bodyChunks[1].pos, owner.player.bodyChunks[0].pos + Custom.DirVec(default(Vector2), owner.player.bodyChunks[0].vel) * 5f) * 1.6f;
            Vector2 perp = Custom.PerpendicularVector(dir);
            for (int i = 0; i < divs; i++)
            {
                for (int j = 0; j < divs; j++)
                {
                    Mathf.InverseLerp(0f, divs - 1, i);
                    float t = Mathf.InverseLerp(0f, divs - 1, j);
                    clothPoints[i, j, 1] = clothPoints[i, j, 0];
                    clothPoints[i, j, 0] += clothPoints[i, j, 2];
                    clothPoints[i, j, 2] *= 0.999f;
                    clothPoints[i, j, 2].y -= 1.1f * owner.player.EffectiveRoomGravity;

                    Vector2 idealPos = IdealPosForPoint(i, j, bodyPos, dir, perp) + walkBob * (-1f * t);
                    Vector3 rot = Vector3.Slerp(-dir, Custom.DirVec(bodyPos, idealPos), t) * (0.01f + 0.9f * t);
                    clothPoints[i, j, 2] += new Vector2(rot.x, rot.y);
                    float num = Vector2.Distance(clothPoints[i, j, 0], idealPos);
                    float num2 = Mathf.Lerp(0f, 9f, t);
                    Vector2 a = Custom.DirVec(clothPoints[i, j, 0], idealPos);
                    if (num > num2)
                    {
                        clothPoints[i, j, 0] -= (num2 - num) * a * (1f - t / 1.4f);
                        clothPoints[i, j, 2] -= (num2 - num) * a * (1f - t / 1.4f);
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        IntVector2 intVector = new IntVector2(i, j) + Custom.fourDirections[k];
                        if (intVector.x >= 0 && intVector.y >= 0 && intVector.x < divs && intVector.y < divs)
                        {
                            num = Vector2.Distance(clothPoints[i, j, 0], clothPoints[intVector.x, intVector.y, 0]);
                            a = Custom.DirVec(clothPoints[i, j, 0], clothPoints[intVector.x, intVector.y, 0]);
                            float num3 = Vector2.Distance(idealPos, IdealPosForPoint(intVector.x, intVector.y, bodyPos, dir, perp));
                            clothPoints[i, j, 2] -= (num3 - num) * a * 0.05f;
                            clothPoints[intVector.x, intVector.y, 2] += (num3 - num) * a * 0.05f;
                        }
                    }
                }
            }
        }

        private Vector2 IdealPosForPoint(int x, int y, Vector2 bodyPos, Vector2 dir, Vector2 perp)
        {
            float num = Mathf.InverseLerp(0f, divs - 1, x);
            float t = Mathf.InverseLerp(0f, divs - 1, y);
            return bodyPos + Mathf.Lerp(-1f, 1f, num) * perp * Mathf.Lerp(9f, 11f, t) + dir * Mathf.Lerp(8f, -9f, t) * (1f + Mathf.Sin((float)Math.PI * num) * 0.35f * Mathf.Lerp(-1f, 1f, t));
        }

        public Color VestColor()
        {
            return ExplorerGraphics.GetColor(owner, "Vest", ExplorerGraphics.DefaultVestColor);
        }

        public void InitiateSprites(int startSprite, List<FSprite> sprites)
        {
            this.startSprite = startSprite;
            sprites.Add(TriangleMesh.MakeGridMesh("Futile_White", divs - 1));
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

        public void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[startSprite].isVisible = owner.player.room != null;
            if (!sLeaser.sprites[startSprite].isVisible)
            {
                return;
            }
            for (int i = 0; i < divs; i++)
            {
                for (int j = 0; j < divs; j++)
                {
                    (sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * divs + j, Vector2.Lerp(clothPoints[i, j, 1], clothPoints[i, j, 0], timeStacker) - camPos);
                    (sLeaser.sprites[startSprite + 1] as TriangleMesh).MoveVertice(i * divs + j, Vector2.Lerp(clothPoints[i, j, 1], clothPoints[i, j, 0], timeStacker) - camPos);
                }
            }
        }
    }
}

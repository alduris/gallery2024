using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RWCustom;
using SlugBase.DataTypes;
using UnityEngine;

namespace Gallery2024.Graphics
{
    public class ExplorerGraphics
    {
        internal static ConditionalWeakTable<PlayerGraphics, ExplorerGraphics> grafCWT = new();

        public static readonly Color DefaultBodyColor = Custom.hexToColor("c6c6c6");
        public static readonly Color DefaultMudColor = Custom.hexToColor("754848");
        public static readonly Color DefaultVestColor = Custom.hexToColor("e8a240");

        private int tailMudSprite = -1;
        private Vest vest;

        private WeakReference<RoomCamera.SpriteLeaser> sLeaserRef;
        public ExplorerGraphics(RoomCamera.SpriteLeaser leaser)
        {
            sLeaserRef = new(leaser);
        }

        public void InitiateSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (!sLeaserRef.TryGetTarget(out var leaser) || leaser != sLeaser || sLeaser.deleteMeNextFrame)
            {
                grafCWT.Remove(self);
                return;
            }

            var initialLen = sLeaser.sprites.Length;
            List<FSprite> newSprites = [];

            // Map tail texture
            // Partial credit: Pocky-raisin
            if (sLeaser.sprites[2] is TriangleMesh oldTail)
            {
                var tail = new TriangleMesh("Explorer24Tail", oldTail.triangles, false, false);
                for (var i = tail.vertices.Length - 1; i >= 0; i--)
                {
                    var perc = i / 2 / (float)(tail.vertices.Length / 2);

                    Vector2 uv;
                    if (i % 2 == 0)
                        uv = new Vector2(perc, 0f);
                    else if (i < tail.vertices.Length - 1)
                        uv = new Vector2(perc, 1f);
                    else
                        uv = new Vector2(1f, 0f);

                    // Map UV values to the element
                    uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
                    uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

                    tail.UVvertices[i] = uv;
                }
                tailMudSprite = initialLen + newSprites.Count;
                newSprites.Add(tail);
            }

            // Vest
            vest = new Vest(self);
            vest.InitiateSprites(initialLen + newSprites.Count, newSprites);

            // Add new sprites to sprite leaser
            Array.Resize(ref sLeaser.sprites, initialLen + newSprites.Count);
            for (int i = 0; i < newSprites.Count; i++)
            {
                sLeaser.sprites[initialLen + i] = newSprites[i];
            }
        }

        public void AddToContainer(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer) // I know how to spell thankfully
        {
            if (!sLeaserRef.TryGetTarget(out var leaser) || leaser != sLeaser || sLeaser.deleteMeNextFrame)
            {
                grafCWT.Remove(self);
                return;
            }

            if (tailMudSprite > -1)
                (newContainer ?? sLeaser.sprites[2].container).AddChild(sLeaser.sprites[tailMudSprite]);
            vest.AddToContainer(sLeaser, rCam, newContainer);
        }

        public void ApplyPalette(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (!sLeaserRef.TryGetTarget(out var leaser) || leaser != sLeaser || sLeaser.deleteMeNextFrame)
            {
                grafCWT.Remove(self);
                return;
            }

            // Mud coloring
            var mudColor = GetColor(self, "Mud", DefaultMudColor);
            sLeaser.sprites[8].color = mudColor; // right hand
            if (tailMudSprite > -1) sLeaser.sprites[tailMudSprite].color = mudColor;

            // Vest
            vest.ApplyPalette(sLeaser, rCam, palette);
        }

        public void DrawSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!sLeaserRef.TryGetTarget(out var leaser) || leaser != sLeaser || sLeaser.deleteMeNextFrame)
            {
                grafCWT.Remove(self);
                return;
            }

            // Tail overlap
            if (tailMudSprite > -1)
            {
                var origTail = sLeaser.sprites[2] as TriangleMesh;
                var mudTail = sLeaser.sprites[tailMudSprite] as TriangleMesh;

                for (int i = 0; i < origTail.vertices.Length; i++)
                {
                    mudTail.vertices[i] = origTail.vertices[i];
                }

                mudTail.MoveInFrontOfOtherNode(origTail);
            }

            // Vest
            vest.DrawSprite(sLeaser, rCam, timeStacker, camPos);

            // Face
            string faceName = "Explorer24" + sLeaser.sprites[9].element.name;
            if (Futile.atlasManager.DoesContainElementWithName(faceName))
            {
                sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(faceName);
            }
        }

        public void Update(PlayerGraphics self)
        {
            vest.Update();
        }

        public static Color GetColor(PlayerGraphics graf, string name, Color defaultColor)
        {
            try
            {
                return PlayerColor.GetCustomColor(graf, name);
            }
            catch
            {
                return defaultColor;
            }
        }

        public static void LoadAtlases()
        {
            Futile.atlasManager.LoadAtlas("atlases/Explorer24Tail");
            Futile.atlasManager.LoadAtlas("atlases/Explorer24Vest");
            Futile.atlasManager.LoadAtlas("atlases/Explorer24Eyes");
        }

        public static class Hooks
        {
            public static void Apply()
            {
                On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
                On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
                On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
                On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
                On.PlayerGraphics.Update += PlayerGraphics_Update;
            }

            private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
            {
                orig(self);

                if (self.player.SlugCatClass == Plugin.Slugcat && grafCWT.TryGetValue(self, out var graf))
                {
                    graf.Update(self);
                }
            }

            private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                orig(self, sLeaser, rCam);

                if (self.player.SlugCatClass == Plugin.Slugcat)
                {
                    if (grafCWT.TryGetValue(self, out _))
                    {
                        grafCWT.Remove(self);
                    }
                    var graf = new ExplorerGraphics(sLeaser);
                    grafCWT.Add(self, graf);
                    graf.InitiateSprites(self, sLeaser, rCam);
                    self.AddToContainer(sLeaser, rCam, null);
                }
            }

            private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
            {
                orig(self, sLeaser, rCam, newContatiner);

                if (self.player.SlugCatClass == Plugin.Slugcat && grafCWT.TryGetValue(self, out var graf))
                {
                    graf.AddToContainer(self, sLeaser, rCam, newContatiner);
                }
            }

            private static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                orig(self, sLeaser, rCam, palette);

                if (self.player.SlugCatClass == Plugin.Slugcat && grafCWT.TryGetValue(self, out var graf))
                {
                    graf.ApplyPalette(self, sLeaser, rCam, palette);
                }
            }

            private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                orig(self, sLeaser, rCam, timeStacker, camPos);

                if (self.player.SlugCatClass == Plugin.Slugcat && grafCWT.TryGetValue(self, out var graf))
                {
                    graf.DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
                }
            }
        }
    }
}

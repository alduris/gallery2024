using System;
using RWCustom;
using UnityEngine;

namespace Gallery2024
{
    internal class Counter : CosmeticSprite
    {
        private const int ACTIVATION_TIME = 20;
        private int activeTimer = 0;
        private int active = 0;
        private int lastActive = 0;
        private readonly WeakReference<Player> playerRef;

        private FLabel progLabel;
        private FLabel authorLabel;
        private FLabel promptLabel;

        public Counter(Player player, Room room)
        {
            this.playerRef = new(player);
            this.room = room;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            lastActive = active;
            if (activeTimer > 0 && active < ACTIVATION_TIME)
            {
                active++;
            }
            else if (activeTimer == 0 && active > 0)
            {
                active--;
            }

            if (activeTimer > 0)
            {
                activeTimer--;
            }

            if (!playerRef.TryGetTarget(out var player) || player.room != room)
            {
                Destroy();
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            activeTimer = 0;
            active = 0;
            lastActive = 0;
        }

        public void Activate()
        {
            activeTimer = 160; // 4 seconds
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [
                // Circle
                new FSprite("Futile_White", true) { shader = rCam.room.game.rainWorld.Shaders["VectorCircleFadable"] },
                new FSprite("Futile_White", true) { shader = rCam.room.game.rainWorld.Shaders["VectorCircleFadable"] },
                new FSprite("Futile_White", true) { shader = rCam.room.game.rainWorld.Shaders["HoldButtonCircle"] }
            ];
            sLeaser.containers = [new FContainer()];

            // Labels
            var entry = Data.GetRoomData(room.abstractRoom.name);
            progLabel = new FLabel(Custom.GetDisplayFont(), Data.VisitedCount + "/" + Data.TotalRoomCount) { alignment = FLabelAlignment.Center, scale = 0.8f };
            sLeaser.containers[0].AddChild(progLabel);
            authorLabel = new FLabel(Custom.GetDisplayFont(), entry.author) { alignment = FLabelAlignment.Center, scale = 0.8f };
            sLeaser.containers[0].AddChild(authorLabel);
            promptLabel = new FLabel(Custom.GetFont(), entry.prompts) { alignment = FLabelAlignment.Center, scale = 1f };
            sLeaser.containers[0].AddChild(promptLabel);

            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Water"));
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            sLeaser.RemoveAllSpritesFromContainer();
            base.AddToContainer(sLeaser, rCam, newContatiner);
            foreach (var container in sLeaser.containers)
            {
                newContatiner.AddChild(container);
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            if (slatedForDeletetion || !playerRef.TryGetTarget(out var player))
            {
                foreach (var sprite in sLeaser.sprites)
                {
                    sprite.isVisible = false;
                }
                foreach (var container in sLeaser.containers)
                {
                    container.isVisible = false;
                }
                return;
            }

            // Get stuff
            var activeFac = Mathf.Lerp(lastActive, active, timeStacker) / ACTIVATION_TIME;
            var sinAFac = Mathf.Sin(activeFac * Mathf.PI / 2);
            var sigAFac = 1f / (1f + Mathf.Exp(-5 * (activeFac - 0.5f)));

            // Counter circle
            const float CIRCLE_SCALE = 80f;
            const float PROG_SCALE = CIRCLE_SCALE * 0.95f;
            const float INNER_SCALE = CIRCLE_SCALE * 0.9f - 5f;
            float scale = UpperHalf(CIRCLE_SCALE * sinAFac, CIRCLE_SCALE) / 12f;
            float progScale = UpperHalf(PROG_SCALE * sinAFac, PROG_SCALE) / 12f;
            float innerScale = UpperHalf(INNER_SCALE * sinAFac, INNER_SCALE) / 12f;
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[i].scale = (i == 0) ? innerScale : scale;
                sLeaser.sprites[i].color = new Color(0f, 0f, sinAFac);
                sLeaser.sprites[i].alpha = 2f / Mathf.Max(2f, sLeaser.sprites[i].scale * 10f);
            }
            sLeaser.sprites[2].scale = progScale;
            sLeaser.sprites[2].alpha = sigAFac * ((float)Data.VisitedCount / Data.TotalRoomCount);
            sLeaser.sprites[2].isVisible = active != 0 && lastActive != 0;

            var posFac =
                (
                    Vector2.Lerp(player.bodyChunks[0].lastPos, player.bodyChunks[0].pos, timeStacker)
                    + Vector2.Lerp(player.bodyChunks[1].lastPos, player.bodyChunks[1].pos, timeStacker)
                ) / 2f
                - camPos;
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                var offset = Vector2.zero; // Vector2.one * sLeaser.sprites[i].element.sourcePixelSize * sLeaser.sprites[i].scale;
                sLeaser.sprites[i].SetPosition(posFac - offset);
            }

            // Labels (yippee)
            progLabel.alpha = authorLabel.alpha = promptLabel.alpha = sinAFac;

            const float RADIUS = CIRCLE_SCALE / 2 + 5f;
            var textFac = RADIUS * sinAFac * 0.25f + RADIUS * 0.75f;
            progLabel.SetPosition(posFac + new Vector2(0f, textFac + 30f * progLabel.scale)); // 30 = large text size
            authorLabel.SetPosition(posFac - new Vector2(0f, textFac + 30f * authorLabel.scale));
            promptLabel.SetPosition(posFac - new Vector2(0f, textFac + 30f * authorLabel.scale + 15f * promptLabel.scale)); // 15 = small text size, 6 = margin

            static float UpperHalf(float x, float m) => m / 2f + (x / 2f);
        }
    }
}

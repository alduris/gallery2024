using HUD;
using RWCustom;
using UnityEngine;

namespace Gallery2024
{
    internal class GRRoomMarker : Map.FadeInMarker
    {
        public bool discovered;

        public GRRoomMarker(Map map, int room) : base(map, room, Vector2.zero, 3f)
        {
            inRoomPos = map.mapData.SizeOfRoom(room).ToVector2() * 10f;
            discovered = Data.IsVisited(RainWorld.roomIndexToName[room]);
            symbolSprite = new FSprite("Sandbox_QuestionMark", true);
            map.inFrontContainer.AddChild(symbolSprite);
            symbolSprite.isVisible = false;
            Plugin.Logger.LogDebug(RainWorld.roomIndexToName[room] + " : " + discovered);
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);

            bkgFade.isVisible = map.visible && !discovered;
            symbolSprite.isVisible = map.visible && !discovered;
            if (!map.visible || discovered)
            {
                return;
            }
            float num = Mathf.Lerp(map.lastFade, map.fade, timeStacker); // * Mathf.Lerp(lastFade, fade, timeStacker); // screw you RW code setting fade to 0 because it's "not discovered"
            Vector2 vector = map.RoomToMapPos(inRoomPos, room, timeStacker);
            bkgFade.x = vector.x;
            bkgFade.y = vector.y;
            bkgFade.alpha = num * 0.5f;
            symbolSprite.x = vector.x;
            symbolSprite.y = vector.y;
            symbolSprite.alpha = num;
            symbolSprite.color = Color.Lerp(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White), 0.5f + 0.5f * Mathf.Sin((map.counter + timeStacker) / 14f));
            bkgFade.scale = 12.5f;
        }

        public override void Update()
        {
            fadeInSpeed = 0.1f;
            base.Update();

            discovered = Data.IsVisited(RainWorld.roomIndexToName[room]);
        }
    }
}

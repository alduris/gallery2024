using Menu.Remix.MixedUI;
using UnityEngine;

namespace Gallery2024
{
    internal class Options : OptionInterface
    {
        public Configurable<bool> AlwaysCredits;
        public Configurable<bool> AllowFlight;
        public Configurable<bool> AlwaysNewRoom;

        public Configurable<string> VisitedRooms;

        public Options()
        {
            AlwaysCredits = config.Bind("StreamMode", false);
            AllowFlight = config.Bind("AllowFlilght", true);
            AlwaysNewRoom = config.Bind("AlwaysNewRoom", true);
            VisitedRooms = config.Bind("RoomSave", "[]");
        }

        public override void Initialize()
        {
            Tabs = [new(this)];
            var tab = Tabs[0];

            OpHoldButton resetButton;

            tab.AddItems(
                new OpCheckBox(AlwaysCredits, new(10f, 560f)) { description = "Always show credits" },
                new OpLabel(40f, 560f, "Always show credits", false),
                new OpCheckBox(AllowFlight, new(10f, 530f)) { description = "Allow flight" },
                new OpLabel(40f, 530f, "Allow flight", false),
                new OpCheckBox(AlwaysNewRoom, new(10f, 500f)) { description = "Always spawn in new room, if any unvisited" },
                new OpLabel(40f, 500f, "Always spawn in unvisited room", false),
                resetButton = new OpHoldButton(new Vector2(10f, 470f), new Vector2(120f, 24f), "RESET PROGRESS")
            );
            resetButton.OnPressDone += ResetButton_OnPressDone;
        }

        private void ResetButton_OnPressDone(UIfocusable trigger)
        {
            Data.Reset();
        }
    }
}

using Menu.Remix.MixedUI;
using UnityEngine;

namespace Gallery2024
{
    internal class Options : OptionInterface
    {
        public Configurable<bool> StreamMode;
        public Configurable<bool> AllowFlight;

        public Configurable<string> VisitedRooms;

        public Options()
        {
            StreamMode = config.Bind("StreamMode", false);
            AllowFlight = config.Bind("AllowFlilght", true);
            VisitedRooms = config.Bind("RoomSave", "[]");
        }

        public override void Initialize()
        {
            Tabs = [new(this)];
            var tab = Tabs[0];

            var resetButton = new OpHoldButton(new Vector2(10f, 500f), new Vector2(80f, 24f), "RESET");
            resetButton.OnPressDone += ResetButton_OnPressDone;

            tab.AddItems(
                new OpCheckBox(StreamMode, new(10f, 560f)) { description = "Always show credits" },
                new OpLabel(40f, 560f, "Always show credits", false),
                new OpCheckBox(AllowFlight, new(10f, 530f)) { description = "Allow flight" },
                new OpLabel(40f, 530f, "Allow flight", false),
                resetButton
            );
        }

        private void ResetButton_OnPressDone(UIfocusable trigger)
        {
            Data.Reset();
        }
    }
}

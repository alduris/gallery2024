using Menu.Remix.MixedUI;

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
            AllowFlight = config.Bind("AllowFlilght", false);
            VisitedRooms = config.Bind("RoomSave", "[]");
        }

        public override void Initialize()
        {
            Tabs = [new(this)];
            var tab = Tabs[0];

            tab.AddItems(
                new OpCheckBox(StreamMode, new(10f, 560f)) { description = "Always show credits" },
                new OpLabel(40f, 560f, "Always show credits", false),
                new OpCheckBox(AllowFlight, new(10f, 530f)) { description = "Allow flight" },
                new OpLabel(40f, 530f, "Allow flight", false)
            );
        }
    }
}

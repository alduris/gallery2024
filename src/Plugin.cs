using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Gallery2024;

[BepInPlugin("Community.Gallery2", "Community Gallery Region 2", "1.0")]
sealed class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    public bool IsInit;

    public static SlugcatStats.Name Slugcat;

    public void OnEnable()
    {
        Logger = base.Logger;
        On.RainWorld.OnModsInit += OnModsInit;

    }

    public void OnDisable()
    {
        if (!IsInit) return;

        Slugcat.Unregister();
        Slugcat = null;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (IsInit) return;
        IsInit = true;

        Slugcat = new("Explorer 2024", true);

        Logger.LogDebug("Ready to explore!");
    }
}

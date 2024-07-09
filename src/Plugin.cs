using BepInEx;
using BepInEx.Logging;
using RWCustom;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using UnityEngine;
using static MonoMod.InlineRT.MonoModRule;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Gallery2024;

[BepInPlugin("Community.Gallery2", "Community Gallery Region 2024", "1.0")]
sealed class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    public bool IsInit;

    public static SlugcatStats.Name Slugcat = new("Explorer 2024", false);

    private readonly ConditionalWeakTable<AbstractCreature, MutBox<WorldCoordinate>> lastSafePosCWT = new();

    public void OnEnable()
    {
        Logger = base.Logger;
        On.RainWorld.OnModsInit += OnModsInit;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (IsInit) return;
        IsInit = true;

        On.AbstractCreature.Update += AbstractCreature_Update;
        On.Player.Update += Player_Update;
        On.Player.NewRoom += Player_NewRoom;
        On.Player.SpitOutOfShortCut += Player_SpitOutOfShortCut;
        On.SaveState.GetStoryDenPosition += SaveState_GetStoryDenPosition;
        On.RainCycle.Update += RainCycle_Update;
        On.MultiplayerUnlocks.ClassUnlocked += MultiplayerUnlocks_ClassUnlocked;

        Logger.LogDebug("Ready to explore!");
    }

    private bool MultiplayerUnlocks_ClassUnlocked(On.MultiplayerUnlocks.orig_ClassUnlocked orig, MultiplayerUnlocks self, SlugcatStats.Name classID)
    {
        return classID != Slugcat && orig(self, classID);
    }

    private void RainCycle_Update(On.RainCycle.orig_Update orig, RainCycle self)
    {
        orig(self);
        if (self.world?.region?.name == "GR" && self.timer > self.cycleLength / 2)
        {
            self.timer = self.cycleLength / 2;
        }
    }

    private string SaveState_GetStoryDenPosition(On.SaveState.orig_GetStoryDenPosition orig, SlugcatStats.Name slugcat, out bool isVanilla)
    {
        if (slugcat == Slugcat)
        {
            isVanilla = false;
            return Data.Rooms.Keys.ToArray()[Random.Range(0, Data.Rooms.Count)];
        }

        return orig(slugcat, out isVanilla);
    }

    private void Player_SpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        orig(self, pos, newRoom, spitOutAllSticks);
        if (self.SlugCatClass == Slugcat && !self.dead)
        {
            if (lastSafePosCWT.TryGetValue(self.abstractCreature, out var box))
                box.Value = self.room.GetWorldCoordinate(pos);
            else
                lastSafePosCWT.Add(self.abstractCreature, new(self.room.GetWorldCoordinate(pos)));
        }
    }

    private void Player_NewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom)
    {
        orig(self, newRoom);
        if (self.SlugCatClass == Slugcat && !self.dead)
        {
            var pos = self.mainBodyChunk.pos;
            if (lastSafePosCWT.TryGetValue(self.abstractCreature, out var box))
                box.Value = self.room.GetWorldCoordinate(pos);
            else
                lastSafePosCWT.Add(self.abstractCreature, new(self.room.GetWorldCoordinate(pos)));
        }
    }

    private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (self.SlugCatClass == Slugcat)
        {
            self.airInLungs = 1f;

            // Funny flight mode
            if (self.input[0].jmp && self.input[1].jmp && self.input[0].pckp && self.input[1].pckp)
            {
                self.monkAscension = !self.monkAscension;
            }

            if (self.monkAscension)
            {
                self.dead = false;
                self.stun = 0;

                self.buoyancy = 0f;
                self.godDeactiveTimer = 0f;
                self.animation = Player.AnimationIndex.None;
                self.bodyMode = Player.BodyModeIndex.Default;
                self.gravity = 0f;
                self.airFriction = 0.7f;
                float num = 2.75f;
                if (self.killWait >= 0.2f && !self.forceBurst)
                {
                    self.airFriction = 0.1f;
                    self.bodyChunks[0].vel = Custom.RNV() * Mathf.Lerp(0f, 20f, self.killWait);
                    num = 0f;
                }
                if (self.input[0].y > 0)
                {
                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + num;
                    self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + (num - 1f);
                }
                else if (self.input[0].y < 0)
                {
                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - num;
                    self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - (num - 1f);
                }
                if (self.input[0].x > 0)
                {
                    self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + num;
                    self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + (num - 1f);
                }
                else if (self.input[0].x < 0)
                {
                    self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - num;
                    self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - (num - 1f);
                }
            }

            // Respawn on death
            if (!lastSafePosCWT.TryGetValue(self.abstractCreature, out _) && self.room != null && !self.dead && self.mainBodyChunk.pos != Vector2.zero)
            {
                lastSafePosCWT.Add(self.abstractCreature, new(self.room.GetWorldCoordinate(self.mainBodyChunk.pos)));
            }
        }
    }

    private void AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
    {
        orig(self, time);

        if (
            self.creatureTemplate.type == CreatureTemplate.Type.Slugcat
            && self.state is PlayerState ps && ps.slugcatCharacter == Slugcat && (ps.dead || ps.permaDead)
            && lastSafePosCWT.TryGetValue(self, out var box))
        {
            // Destroy the player for realz
            var safePos = box.Value;
            self.realizedCreature?.Destroy();
            self.Abstractize(safePos);

            // Make the game forget we have dieded
            ps.alive = true;
            ps.permaDead = false;
            ps.permanentDamageTracking = 0f;

            // Recreate player, good as new!
            self.RealizeInRoom();

            // Reset HUD
            foreach (var cam in self.world.game.cameras)
            {
                if (cam.hud != null && (cam.hud.owner as Player)?.abstractCreature == self)
                {
                    if (cam.hud.textPrompt != null)
                        cam.hud.textPrompt.gameOverMode = false;

                    cam.hud.owner = self.realizedObject as Player;
                }
            }

            // Play sound and add shockwave
            (self.realizedCreature as Player).PlayHUDSound(SoundID.SS_AI_Give_The_Mark_Boom);
            self.Room.realizedRoom.AddObject(new ShockWave(self.realizedCreature.mainBodyChunk.pos, 160f, 0.07f, 9, true));
        }
    }
}

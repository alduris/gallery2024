using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

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

        try
        {
            On.RainWorldGame.Update += RainWorldGame_Update;
            On.Player.Update += Player_Update;
            On.Player.NewRoom += Player_NewRoom;
            On.Player.SpitOutOfShortCut += Player_SpitOutOfShortCut;
            On.SaveState.GetStoryDenPosition += SaveState_GetStoryDenPosition;
            On.RainCycle.Update += RainCycle_Update;
            On.MultiplayerUnlocks.ClassUnlocked += MultiplayerUnlocks_ClassUnlocked;
            On.ArtificialIntelligence.StaticRelationship += ArtificialIntelligence_StaticRelationship;
            On.DaddyCorruption.BulbNibbleAtChunk += DaddyCorruption_BulbNibbleAtChunk;
            On.DaddyCorruption.Bulb.Update += Bulb_Update;
            On.AbstractCreature.RealizeInRoom += AbstractCreature_RealizeInRoom;

            Logger.LogDebug("Ready to explore!");
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }

    private void AbstractCreature_RealizeInRoom(On.AbstractCreature.orig_RealizeInRoom orig, AbstractCreature self)
    {
        if (self.creatureTemplate.type == CreatureTemplate.Type.Slugcat && self.state is PlayerState ps && ps.slugcatCharacter == Slugcat)
            self.tentacleImmune = true;

        orig(self);
    }

    private void Bulb_Update(On.DaddyCorruption.Bulb.orig_Update orig, DaddyCorruption.Bulb self)
    {
        orig(self);
        if (self.eatChunk?.owner is Player p && p.SlugCatClass == Slugcat)
        {
            self.eatChunk = null;
        }
    }

    private void DaddyCorruption_BulbNibbleAtChunk(On.DaddyCorruption.orig_BulbNibbleAtChunk orig, DaddyCorruption self, DaddyCorruption.Bulb bulb, BodyChunk chunk)
    {
        if (chunk.owner is Player p && p.SlugCatClass == Slugcat) return;

        orig(self, bulb, chunk);
    }

    private CreatureTemplate.Relationship ArtificialIntelligence_StaticRelationship(On.ArtificialIntelligence.orig_StaticRelationship orig, ArtificialIntelligence self, AbstractCreature otherCreature)
    {
        if (otherCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && otherCreature.state is PlayerState ps && ps.slugcatCharacter == Slugcat)
            return new(CreatureTemplate.Relationship.Type.DoesntTrack, 1f);

        return orig(self, otherCreature);
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
            if (self.input[0].jmp && self.input[1].jmp && !self.input[2].jmp && self.input[0].pckp && self.input[1].pckp && !self.input[2].pckp)
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
            if (
                (!lastSafePosCWT.TryGetValue(self.abstractCreature, out var box) || box.Value.room != self.room?.abstractRoom.index)
                && self.room != null
                && !self.dead && self.mainBodyChunk.pos != Vector2.zero)
            {
                lastSafePosCWT.Add(self.abstractCreature, new(self.room.GetWorldCoordinate(self.mainBodyChunk.pos)));
            }
        }
    }

    private void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);
        if (!self.IsStorySession) return;

        for (int i = 0; i < self.Players.Count; i++)
        {
            var player = self.Players[i];
            var state = player.state as PlayerState;

            if (state.slugcatCharacter == Slugcat && (state.dead || state.permaDead) && lastSafePosCWT.TryGetValue(player, out var box))
            {
                // Destroy the player for realz
                var safePos = box.Value;
                player.realizedCreature?.Destroy();
                player.Abstractize(safePos);

                // Make the game forget we have dieded
                state.alive = true;
                state.permaDead = false;
                state.permanentDamageTracking = 0f;
                player.slatedForDeletion = false;

                // Recreate player, good as new!
                player.RealizeInRoom();

                // Also push them slightly out of the hole so they don't immediately go through it sometimes
                var offset = player.Room.realizedRoom.ShorcutEntranceHoleDirection(safePos.Tile).ToVector2() * -10f;
                foreach (var chunk in player.realizedCreature.bodyChunks)
                {
                    chunk.pos += offset;
                    chunk.lastPos += offset;
                    chunk.lastLastPos += offset;
                }

                // Reset HUD
                foreach (var cam in self.world.game.cameras)
                {
                    if (cam.hud != null && (cam.hud.owner as Player)?.abstractCreature == player)
                    {
                        if (cam.hud.textPrompt != null)
                            cam.hud.textPrompt.gameOverMode = false;

                        cam.hud.owner = player.realizedObject as Player;
                    }
                }

                // Play sound and add shockwave
                (player.realizedCreature as Player).PlayHUDSound(SoundID.SS_AI_Give_The_Mark_Boom);
                player.Room.realizedRoom.AddObject(new ShockWave(player.realizedCreature.mainBodyChunk.pos, 160f, 0.07f, 9, true));
            }
        }
    }
}

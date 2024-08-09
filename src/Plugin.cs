using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using Gallery2024.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Gallery2024;

[BepInPlugin(MOD_ID, "Community Gallery Region 2024", "1.0")]
sealed class Plugin : BaseUnityPlugin
{
	public const string MOD_ID = "Community.Gallery2";
	public static new ManualLogSource Logger;
	public bool IsInit;

	public static SlugcatStats.Name Slugcat = new("Explorer 2024", false);
	public static Options OI = null;

	private readonly ConditionalWeakTable<AbstractCreature, MutBox<WorldCoordinate>> lastSafePosCWT = new();
	private readonly ConditionalWeakTable<Player, Counter> creditCWT = new();

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
			On.RainWorldGame.Update += PlayerRespawnHook;
			On.Player.Update += PlayerAbilitiesHook;
			On.Player.NewRoom += UpdateVisitedRoomsHook;
			On.Player.SpitOutOfShortCut += SaveSafePositionHook;
			On.SaveState.GetStoryDenPosition += RandomSpawnRoomHook;
			On.RainCycle.Update += RainCycleFreezeHook;
			On.MultiplayerUnlocks.ClassUnlocked += NoPlayInArenaHook;
			On.ArtificialIntelligence.StaticRelationship += NoRelationshipsHook;
			On.DaddyCorruption.BulbNibbleAtChunk += CorruptionCannotEatMeHook;
			On.DaddyCorruption.Bulb.Update += CorruptionCannotGrabMeHook;
			On.AbstractCreature.RealizeInRoom += TentacleImmunityHook;
			On.RainWorldGame.SpawnPlayers_bool_bool_bool_bool_WorldCoordinate += RoomSpawnPosFixHook;
			On.Player.ClassMechanicsSaint += ClassMechanicsSaintDisableHook;
			On.Player.Destroy += DestroyCreditsHook;
			On.HUD.FoodMeter.Update += NoShowFoodMeterHook;
            On.HUD.KarmaMeter.Update += NoShowKarmaMeterHook;
            On.Menu.SlugcatSelectMenu.UpdateStartButtonText += StartButtonRenameHook;
			_ = new ILHook(typeof(HUD.Map).GetProperty(nameof(HUD.Map.discoverTexture)).GetSetMethod(), MapDiscoverTextureMemLeakFixQuestionMarkHook);
			
			// Map stuff
			LoadShaders(self);
			On.HUD.Map.OnMapConnection.Update += FadeMapConnectionHook;
			On.HUD.Map.Update += GRMapShaderHook;
            On.HUD.Map.ctor += CreateMapMarkers;
            On.HUD.Map.FadeInMarker.SetInvisible += FadeInMarker_SetInvisible;
            On.HUD.Map.ResetNotRevealedMarkers += Map_ResetNotRevealedMarkers;
			
			// L4 fix stolen from mergefix
			On.RoomCamera.MoveCamera2 += RoomCamera_MoveCamera2;
			On.RoomCamera.MoveCamera_Room_int += RoomCamera_MoveCamera_Room_int;

			// Player graphics stuff
			ExplorerGraphics.LoadAtlases();
			ExplorerGraphics.Hooks.Apply();

			Logger.LogInfo("Ready to explore!");
		}
		catch (Exception e)
		{
			Logger.LogError(e);
		}

		OI = new Options();
		MachineConnector.SetRegisteredOI(MOD_ID, OI);
	}

	private void MapDiscoverTextureMemLeakFixQuestionMarkHook(ILContext il)
	{
		// You'd think I could just use a normal Hook, not an ILHook. Unfortunately fuck you, the Hook implementation is broken for property setters.
		var c = new ILCursor(il);
		c.Emit(OpCodes.Ldarg_0);
		c.EmitDelegate((HUD.Map self) =>
		{
			if (self.hud.rainWorld.progression.mapDiscoveryTextures.TryGetValue(self.mapData.regionName, out var oldTex))
			{
				Destroy(oldTex);
			}
		});
	}

    private void StartButtonRenameHook(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText orig, Menu.SlugcatSelectMenu self)
    {
		orig(self);
		if (self.slugcatPages[self.slugcatPageIndex].slugcatNumber == Slugcat)
		{
			self.startButton.menuLabel.text = self.Translate("PLAY");
		}
    }

    private void Map_ResetNotRevealedMarkers(On.HUD.Map.orig_ResetNotRevealedMarkers orig, HUD.Map self)
    {
		orig(self);
		self.notRevealedFadeMarkers = self.notRevealedFadeMarkers.Where(x => x is not GRRoomMarker).ToList();
    }

    private void FadeInMarker_SetInvisible(On.HUD.Map.FadeInMarker.orig_SetInvisible orig, HUD.Map.FadeInMarker self)
    {
		if (self is not GRRoomMarker) orig(self);
    }

    private void CreateMapMarkers(On.HUD.Map.orig_ctor orig, HUD.Map self, HUD.HUD hud, HUD.Map.MapData mapData)
    {
        orig(self, hud, mapData);

        if (self.RegionName != "GR") return;

        for (int i = 0; i < mapData.roomIndices.Length; i++)
		{
			self.mapObjects.Add(new GRRoomMarker(self, mapData.roomIndices[i]));
		}
    }

    private void LoadShaders(RainWorld rainWorld)
	{
		AssetBundle assetBundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assets/mapshader"));
		rainWorld.Shaders["VisibleMap"] = FShader.CreateShader("VisibleMap", assetBundle.LoadAsset<Shader>("Assets/VisibleMap.shader"));
	}

	private static void RoomCamera_MoveCamera_Room_int(On.RoomCamera.orig_MoveCamera_Room_int orig, RoomCamera self, Room newRoom, int camPos)
    {
        orig(self, newRoom, camPos);
        if (!System.IO.File.Exists(WorldLoader.FindRoomFile(newRoom.abstractRoom.name, false, "_" + (camPos + 1).ToString() + "_bkg.png")))
        { self.preLoadedBKG = null; }
    }

    private static void RoomCamera_MoveCamera2(On.RoomCamera.orig_MoveCamera2 orig, RoomCamera self, string roomName, int camPos)
    {
        orig(self, roomName, camPos);
        if (!System.IO.File.Exists(WorldLoader.FindRoomFile(roomName, false, "_" + (camPos + 1).ToString() + "_bkg.png")))
        { self.preLoadedBKG = null; }
    }
	
	private void GRMapShaderHook(On.HUD.Map.orig_Update orig, HUD.Map self)
	{
		orig(self);
		
		if (self.RegionName != "GR") return;
		
		for (int i = 0; i < self.mapSprites.Length; i++)
		{
			self.mapSprites[i].shader = self.hud.rainWorld.Shaders["VisibleMap"];
		}
	}

	
	private void FadeMapConnectionHook(On.HUD.Map.OnMapConnection.orig_Update orig, HUD.Map.OnMapConnection self)
	{
		orig(self);
		
		if (self.map.RegionName != "GR") return;
		
		// Set minimum alpha
		self.revealA = Mathf.Max(0.15f, self.revealA);
		self.revealB = Mathf.Max(0.15f, self.revealB);
		self.lastRevealA = Mathf.Max(0.15f, self.lastRevealA);
		self.lastRevealB = Mathf.Max(0.15f, self.lastRevealB);
	}

    private void NoShowKarmaMeterHook(On.HUD.KarmaMeter.orig_Update orig, HUD.KarmaMeter self)
    {
		orig(self);
        if (self.hud.owner is Player player && player.SlugCatClass == Slugcat)
        {
            self.fade = 0f;
            self.lastFade = 0f;
        }
    }

	private void NoShowFoodMeterHook(On.HUD.FoodMeter.orig_Update orig, HUD.FoodMeter self)
	{
		orig(self);
		if (self.hud.owner is Player player && player.SlugCatClass == Slugcat)
		{
			self.fade = 0f;
			self.lastFade = 0f;
		}
	}

	private void DestroyCreditsHook(On.Player.orig_Destroy orig, Player self)
	{
		orig(self);
		creditCWT.Remove(self);
	}

	private void ClassMechanicsSaintDisableHook(On.Player.orig_ClassMechanicsSaint orig, Player self)
	{
		if (self.SlugCatClass != Slugcat) orig(self);
	}

	private AbstractCreature RoomSpawnPosFixHook(On.RainWorldGame.orig_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate orig, RainWorldGame self, bool player1, bool player2, bool player3, bool player4, WorldCoordinate location)
	{
		// Anyone in the gallery region will spawn from the first node (will be a room entrance)
		if (RainWorld.roomIndexToName.ContainsKey(location.room) && RainWorld.roomIndexToName[location.room].StartsWith("GR_"))
		{
			location = new WorldCoordinate(location.room, -1, -1, 0);
		}
		return orig(self, player1, player2, player3, player4, location);
	}

	private void TentacleImmunityHook(On.AbstractCreature.orig_RealizeInRoom orig, AbstractCreature self)
	{
		if (self.creatureTemplate.type == CreatureTemplate.Type.Slugcat && self.state is PlayerState ps && ps.slugcatCharacter == Slugcat)
			self.tentacleImmune = true; // no tentacles today

		orig(self);
	}

	private void CorruptionCannotGrabMeHook(On.DaddyCorruption.Bulb.orig_Update orig, DaddyCorruption.Bulb self)
	{
		orig(self);
		if (self.eatChunk?.owner is Player p && p.SlugCatClass == Slugcat)
		{
			self.eatChunk = null; // bulb no grab gallery slugcat
		}
	}

	private void CorruptionCannotEatMeHook(On.DaddyCorruption.orig_BulbNibbleAtChunk orig, DaddyCorruption self, DaddyCorruption.Bulb bulb, BodyChunk chunk)
	{
		if (chunk.owner is Player p && p.SlugCatClass == Slugcat) return; // placed rot doesn't get dinner today

		orig(self, bulb, chunk);
	}

	private CreatureTemplate.Relationship NoRelationshipsHook(On.ArtificialIntelligence.orig_StaticRelationship orig, ArtificialIntelligence self, AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && otherCreature.state is PlayerState ps && ps.slugcatCharacter == Slugcat)
			return new(CreatureTemplate.Relationship.Type.DoesntTrack, 1f); // everything will not track (intended for stuck daddys)

		return orig(self, otherCreature);
	}

	private bool NoPlayInArenaHook(On.MultiplayerUnlocks.orig_ClassUnlocked orig, MultiplayerUnlocks self, SlugcatStats.Name classID)
	{
		return classID != Slugcat && orig(self, classID); // cannot play gallery slugcat in arena
	}

	private void RainCycleFreezeHook(On.RainCycle.orig_Update orig, RainCycle self)
	{
		orig(self);
		if (self.world?.region?.name == "GR" && self.timer > self.cycleLength / 2)
		{
			self.timer = self.cycleLength / 2; // no going past half cycle
		}
	}

	private string RandomSpawnRoomHook(On.SaveState.orig_GetStoryDenPosition orig, SlugcatStats.Name slugcat, out bool isVanilla)
	{
		if (slugcat == Slugcat)
		{
			// Pick a random GR room to spawn in at start of cycle
			Random.State state = Random.state;
			Random.InitState((int)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()); // I hope this doesn't crash due to casting errors after 03:14:07 UTC on 19 January 2038
			isVanilla = false;
			string room = Data.Rooms.Keys.ToArray()[Random.Range(0, Data.Rooms.Count)];
			if (Data.IsVisited(room) && OI.AlwaysNewRoom.Value)
			{
				var possibleRooms = Data.Rooms.Keys.Where(x => !Data.IsVisited(x)).ToArray();
				if (possibleRooms.Length > 0)
				{
					room = possibleRooms[Random.Range(0, possibleRooms.Length)];
				}
			}
			Data.UpdateVisited(room);
			Random.state = state;
			return room;
		}

		return orig(slugcat, out isVanilla);
	}

	private void SaveSafePositionHook(On.Player.orig_SpitOutOfShortCut orig, Player self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		orig(self, pos, newRoom, spitOutAllSticks);

		// Save last visited shortcut hole for respawning
		if (self.SlugCatClass == Slugcat && !self.dead)
		{
			if (lastSafePosCWT.TryGetValue(self.abstractCreature, out var box))
			{
				box.Value = self.room.GetWorldCoordinate(pos);
			}
			else
			{
				lastSafePosCWT.Add(self.abstractCreature, new(self.room.GetWorldCoordinate(pos)));
			}
		}
	}

	private void UpdateVisitedRoomsHook(On.Player.orig_NewRoom orig, Player self, Room newRoom)
	{
		orig(self, newRoom);

		// I think this is redundant with the spitting out of shortcut code but just in case, save a position
		if (self.SlugCatClass == Slugcat && !self.dead)
		{
			Data.UpdateVisited(newRoom.abstractRoom.name);
		}
	}

	private void PlayerAbilitiesHook(On.Player.orig_Update orig, Player self, bool eu)
	{
		orig(self, eu);

		if (self.SlugCatClass == Slugcat && self.room != null)
		{
			if (self.room != null && self.room.game.IsStorySession && self.room.world.name != "GR")
			{
				if (self.State.alive || Random.value < 0.02f)
				{
					self.Die();
					var launchDir = Custom.RNV() * 100f;

                    for (int i = 0; i < self.bodyChunks.Length; i++)
					{
						self.bodyChunks[i].vel += launchDir;
					}
					var pos = self.firstChunk.pos;
                    self.room.AddObject(new Explosion(self.room, self, pos, 7, Random.Range(200f, 300f), 6f, 2f, 280f, 0.25f, self, 1f, 160f, 1f));
                    self.room.AddObject(new Explosion.ExplosionLight(pos, 280f, 1f, 7, self.ShortCutColor()));
                    self.room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, new Color(1f, 1f, 1f)));
                    self.room.AddObject(new ExplosionSpikes(self.room, pos, 14, 30f, 9f, 7f, 170f, self.ShortCutColor()));
                    self.room.AddObject(new ShockWave(pos, 330f, 0.045f, 5, false));
					return;
                }
			}
			self.airInLungs = 1f;

			// Room credit
			if (self.input[0].mp)
			{
				if (!creditCWT.TryGetValue(self, out Counter counter) || counter.slatedForDeletetion)
				{
					counter = new Counter(self, self.room);
					self.room.AddObject(counter);
					creditCWT.Remove(self);
					creditCWT.Add(self, counter);
				}
				counter.Activate();
			}

			// Funny flight mode
			if (self.wantToJump > 0 && self.canJump <= 0 && self.input[0].pckp && self.input[1].pckp)
			{
				self.monkAscension = OI.AllowFlight.Value && !self.monkAscension;
				self.wantToJump = 0;
				self.PlayHUDSound(self.monkAscension ? SoundID.SS_AI_Give_The_Mark_Boom : SoundID.HUD_Pause_Game);
                // self.room.AddObject(new ShockWave(self.mainBodyChunk.pos, 160f, 0.07f, 9, false));
                for (int i = 0; i < 12; i++)
                {
                    self.room.AddObject(new WaterDrip(self.bodyChunks[1].pos, Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(5f, 20f, Random.value), false));
                }
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
				float speed = 2.75f;
				if (self.input[0].y > 0)
				{
					self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + speed;
					self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + (speed - 1f);
				}
				else if (self.input[0].y < 0)
				{
					self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - speed;
					self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - (speed - 1f);
				}
				if (self.input[0].x > 0)
				{
					self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + speed;
					self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + (speed - 1f);
				}
				else if (self.input[0].x < 0)
				{
					self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - speed;
					self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - (speed - 1f);
				}
			}

			// Respawn on death
			if (self.room != null && !self.dead && self.mainBodyChunk.pos != Vector2.zero)
			{
				if (!lastSafePosCWT.TryGetValue(self.abstractCreature, out var box))
				{
					lastSafePosCWT.Add(self.abstractCreature, new(self.room.GetWorldCoordinate(self.mainBodyChunk.pos)));
				}
				else if (box.Value.room != self.room?.abstractRoom.index || !box.Value.TileDefined)
				{
					box.Value = self.room.GetWorldCoordinate(self.mainBodyChunk.pos);
				}
			}
		}
	}

	private void PlayerRespawnHook(On.RainWorldGame.orig_Update orig, RainWorldGame self)
	{
		orig(self);
		if (!self.IsStorySession) return;

		for (int i = 0; i < self.Players.Count; i++)
		{
			var player = self.Players[i];
			var state = player.state as PlayerState;

			if (state.slugcatCharacter == Slugcat && (state.dead || state.permaDead) && lastSafePosCWT.TryGetValue(player, out var box) && (player.Room?.world?.name ?? "GR") == "GR")
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

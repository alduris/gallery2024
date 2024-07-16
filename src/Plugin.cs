using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
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
			On.RainWorldGame.SpawnPlayers_bool_bool_bool_bool_WorldCoordinate += RainWorldGame_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate;
			On.Player.ClassMechanicsSaint += Player_ClassMechanicsSaint;
			On.Player.Destroy += Player_Destroy;
			On.HUD.FoodMeter.Update += FoodMeter_Update;
			
			// Map stuff
			LoadShaders(self);
			On.HUD.Map.OnMapConnection.Update += OnMapConnection_Update;
			On.HUD.Map.Update += Map_Update;

			Logger.LogInfo("Ready to explore!");
		}
		catch (Exception e)
		{
			Logger.LogError(e);
		}

		OI = new Options();
		MachineConnector.SetRegisteredOI(MOD_ID, OI);
	}
	
	private void LoadShaders(RainWorld rainWorld)
	{
		AssetBundle assetBundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assets/mapshader"));
		rainWorld.Shaders["VisibleMap"] = FShader.CreateShader("VisibleMap", assetBundle.LoadAsset<Shader>("Assets/VisibleMap.shader"));
	}

	
	private void Map_Update(On.HUD.Map.orig_Update orig, HUD.Map self)
	{
		orig(self);
		
		if (self.RegionName != "GR") return;
		
		for (int i = 0; i < self.mapSprites.Length; i++)
		{
			self.mapSprites[i].shader = self.hud.rainWorld.Shaders["VisibleMap"];
		}
	}

	
	private void OnMapConnection_Update(On.HUD.Map.OnMapConnection.orig_Update orig, HUD.Map.OnMapConnection self)
	{
		orig(self);
		
		if (self.map.RegionName != "GR") return;
		
		// Set minimum alpha
		self.revealA = Mathf.Max(0.15f, self.revealA);
		self.revealB = Mathf.Max(0.15f, self.revealB);
		self.lastRevealA = Mathf.Max(0.15f, self.lastRevealA);
		self.lastRevealB = Mathf.Max(0.15f, self.lastRevealB);
	}

	private void FoodMeter_Update(On.HUD.FoodMeter.orig_Update orig, HUD.FoodMeter self)
	{
		orig(self);
		if (self.hud.owner is Player player && player.SlugCatClass == Slugcat)
		{
			self.fade = 0f;
			self.lastFade = 0f;
		}
	}

	private void Player_Destroy(On.Player.orig_Destroy orig, Player self)
	{
		orig(self);
		creditCWT.Remove(self);
	}

	private void Player_ClassMechanicsSaint(On.Player.orig_ClassMechanicsSaint orig, Player self)
	{
		if (self.SlugCatClass != Slugcat) orig(self);
	}

	private AbstractCreature RainWorldGame_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate(On.RainWorldGame.orig_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate orig, RainWorldGame self, bool player1, bool player2, bool player3, bool player4, WorldCoordinate location)
	{
		// Anyone in the gallery region will spawn from the first node (will be a room entrance)
		if (RainWorld.roomIndexToName.ContainsKey(location.room) && RainWorld.roomIndexToName[location.room].StartsWith("GR_"))
		{
			location = new WorldCoordinate(location.room, -1, -1, 0);
		}
		return orig(self, player1, player2, player3, player4, location);
	}

	private void AbstractCreature_RealizeInRoom(On.AbstractCreature.orig_RealizeInRoom orig, AbstractCreature self)
	{
		if (self.creatureTemplate.type == CreatureTemplate.Type.Slugcat && self.state is PlayerState ps && ps.slugcatCharacter == Slugcat)
			self.tentacleImmune = true; // no tentacles today

		orig(self);
	}

	private void Bulb_Update(On.DaddyCorruption.Bulb.orig_Update orig, DaddyCorruption.Bulb self)
	{
		orig(self);
		if (self.eatChunk?.owner is Player p && p.SlugCatClass == Slugcat)
		{
			self.eatChunk = null; // bulb no grab gallery slugcat
		}
	}

	private void DaddyCorruption_BulbNibbleAtChunk(On.DaddyCorruption.orig_BulbNibbleAtChunk orig, DaddyCorruption self, DaddyCorruption.Bulb bulb, BodyChunk chunk)
	{
		if (chunk.owner is Player p && p.SlugCatClass == Slugcat) return; // placed rot doesn't get dinner today

		orig(self, bulb, chunk);
	}

	private CreatureTemplate.Relationship ArtificialIntelligence_StaticRelationship(On.ArtificialIntelligence.orig_StaticRelationship orig, ArtificialIntelligence self, AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && otherCreature.state is PlayerState ps && ps.slugcatCharacter == Slugcat)
			return new(CreatureTemplate.Relationship.Type.DoesntTrack, 1f); // everything will not track (intended for stuck daddys)

		return orig(self, otherCreature);
	}

	private bool MultiplayerUnlocks_ClassUnlocked(On.MultiplayerUnlocks.orig_ClassUnlocked orig, MultiplayerUnlocks self, SlugcatStats.Name classID)
	{
		return classID != Slugcat && orig(self, classID); // cannot play gallery slugcat in arena
	}

	private void RainCycle_Update(On.RainCycle.orig_Update orig, RainCycle self)
	{
		orig(self);
		if (self.world?.region?.name == "GR" && self.timer > self.cycleLength / 2)
		{
			self.timer = self.cycleLength / 2; // no going past half cycle
		}
	}

	private string SaveState_GetStoryDenPosition(On.SaveState.orig_GetStoryDenPosition orig, SlugcatStats.Name slugcat, out bool isVanilla)
	{
		if (slugcat == Slugcat)
		{
			// Pick a random GR room to spawn in at start of cycle
			Random.State state = Random.state;
			Random.InitState((int)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()); // I hope this doesn't crash due to casting errors after 03:14:07 UTC on 19 January 2038
			isVanilla = false;
			string room = Data.Rooms.Keys.ToArray()[Random.Range(0, Data.Rooms.Count)];
			Data.UpdateVisited(room);
			Random.state = state;
			return room;
		}

		return orig(slugcat, out isVanilla);
	}

	private void Player_SpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
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

	private void Player_NewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom)
	{
		orig(self, newRoom);

		// I think this is redundant with the spitting out of shortcut code but just in case, save a position
		if (self.SlugCatClass == Slugcat && !self.dead)
		{
			Data.UpdateVisited(newRoom.abstractRoom.name);
		}
	}

	private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
	{
		orig(self, eu);

		if (self.SlugCatClass == Slugcat && self.room != null)
		{
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

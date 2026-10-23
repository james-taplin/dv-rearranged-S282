using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

namespace RearrangedS282
{
	public static class Main
	{
		public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
		public static string ModPath { get; private set; }
		public static Settings settings { get; private set; }

		public static bool IsGaugeModInstalled { get; private set; }
#if B99_ALL_STANDARD
		private static bool showArrangementSettings;
#endif

		// Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
		private static bool Load(UnityModManager.ModEntry modEntry)
		{
			Logger = modEntry.Logger;
			ModPath = modEntry.Path;
#if B99_STAGE1
			// Stage 1 validates the B99 assembly without running unported geometry/physics
			// or writing arrangement data into a player's save. Remove this build gate
			// only when the stage 2 runtime update path has been validated.
			modEntry.OnGUI = entry => GUILayout.Label(
				"B99 stage 1 build baseline. Wheel rearrangement is not active yet.");
			Logger.Log("B99 stage 1 baseline loaded. No Harmony patches applied; stock locomotives retained.");
			return true;
#elif B99_STAGE2 || B99_ALL_STANDARD
			// Register B99 integrations explicitly. Legacy B98 global patches stay disabled.
			try
			{
				var stage2Harmony = new Harmony(modEntry.Info.Id);
#if B99_ALL_STANDARD
				settings = Settings.Load<Settings>(modEntry);
				if (settings.MigrateSpawnPool()) settings.Save(modEntry);
				settings.OnChange();
				modEntry.OnSaveGUI = OnSaveGUI;
#if B99_DIAGNOSTICS
				stage2Harmony.CreateClassProcessor(typeof(PowerDiagnosticsTick)).Patch();
#endif
				ArticulatedSimulation.Register(stage2Harmony);
				BigEndOiling.Register(stage2Harmony);
				ArticulatedEffects.Register(stage2Harmony);
				DeltaTrailingTruckCompatibility.Register(stage2Harmony);
				ConventionalSpawn.RegisterPatches(stage2Harmony);
				stage2Harmony.CreateClassProcessor(typeof(WheelRearranger.CurveSquealPatch)).Patch();
#if B99_DIAGNOSTICS
				PowerDiagnostics.CurveSampler = CurveDiagnostics.Sample;
				PowerDiagnostics.CurveSessionInfo = CurveDiagnostics.SessionInfo;
				modEntry.OnUpdate = (entry, delta) => PowerDiagnostics.Update();
				Application.quitting += () => PowerDiagnostics.Stop("game exiting");
#endif
				// Live geometry and save owners cannot safely outlive their Harmony hooks.
				modEntry.OnUnload = entry => { Logger.Log("Restart the game to unload Rearranged S282."); return false; };
#endif
				stage2Harmony.CreateClassProcessor(typeof(CommsRadio_Awake_Patch)).Patch();
				modEntry.OnGUI = entry => {
                    GUILayout.Label("Running Rearranged S282 " + typeof(Main).Assembly.GetName().Version);
#if B99_ALL_STANDARD
					GUILayout.Label("48 ARRANGEMENTS — ARTICULATED TRIPLEX AND OPTIONAL POWERED TENDER\n" +
						"Use Wheel Rearranger on the comms radio. Stop and close the regulator first.\n" +
						"Selections are saved with each locomotive. Loaded arrangements wait until switching is safe.\n" +
						"First conversion may take a few seconds to prepare rods.");
					var radio = CommsRadio_Awake_Patch.rearranger;
					if (radio != null)
					{
						if (radio.SettingsTarget != null) GUILayout.Label("Selected locomotive: " + radio.SettingsTarget.ID);
						if (radio.SettingsTarget != null &&
							GUILayout.Button("REAPPLY CURRENT ARRANGEMENT — highlighted S282")) radio.ReapplyFromSettings();
						if (GUILayout.Button("RESET WHEEL-ARRANGER RADIO SELECTION")) radio.ResetSelectionFromSettings();
					}
					DrawPoweredTenderSettings();
					if (CommsRadioS282Rearranger.ReapplyStatus != null) GUILayout.Label(CommsRadioS282Rearranger.ReapplyStatus);
					if (DeltaTrailingTruckCompatibility.Status != null) GUILayout.Label(DeltaTrailingTruckCompatibility.Status);
#if B99_DIAGNOSTICS
					GUILayout.Space(8);
					GUILayout.Label(PowerDiagnostics.Status);
					if (PowerDiagnostics.Latest != null) GUILayout.Label(PowerDiagnostics.Latest);
					if (PowerDiagnostics.LastPath != null) GUILayout.Label(PowerDiagnostics.LastPath);
					if (PowerDiagnostics.Recording)
					{
						if (GUILayout.Button("STOP AND SAVE POWER DIAGNOSTICS")) PowerDiagnostics.Stop();
					}
					else if (!Stage2RuntimeChecks.Running && GUILayout.Button("START POWER DIAGNOSTICS — all loaded S282s")) PowerDiagnostics.Begin();
#endif
#else
					GUILayout.Label(
					"B99 stage 2: select a stationary S282 with the Wheel Rearranger radio.\n" +
					"Stock 2-8-2 and 2-8-4 only. Close the regulator before switching.\n" +
					"Selections are temporary and are not written to saves.");
#endif
#if B99_DIAGNOSTICS || B99_STRUCTURAL_DIAGNOSTICS
					GUILayout.Label(Stage2RuntimeChecks.Status);
					if (!Stage2RuntimeChecks.Running
#if B99_ALL_STANDARD && B99_DIAGNOSTICS
						&& !PowerDiagnostics.Recording
#endif
						&& GUILayout.Button("RUN STRUCTURAL TEST — two stationary S282s"))
						Stage2RuntimeChecks.Begin();
#endif
#if B99_ALL_STANDARD
					GUILayout.Space(8);
					showArrangementSettings = GUILayout.Toggle(showArrangementSettings, "Show arrangement and random-spawn settings");
					if (showArrangementSettings)
					{
						GUILayout.Label("Random spawning affects new S282s only. Saved choices are kept. An empty pool uses stock 2-8-2.");
						settings.Draw(entry);
					}
#endif
				};
#if B99_ALL_STANDARD
				Logger.Log($"B99 expanded trial: {WheelArrangement.SupportedCount} choices including the 4-14-4, articulated 2-6-6-2 Mallet and 2-2-2-2-2 triplex, polished Pacific/Hudson Walschaerts, 2-6-0/2-4-0, giant-driver 2-2-2 clearance prototype and the retained original catalogue.");
#else
				Logger.Log("B99 stage 2: stock / 2-8-4 runtime trial. Persistence and other arrangements disabled.");
#endif
				return true;
			}
			catch (Exception ex)
			{
				new Harmony(modEntry.Info.Id).UnpatchAll(modEntry.Info.Id);
				Logger.LogException("Stage 2 radio initialization failed", ex);
				return false;
			}
#else
			Harmony harmony = null;

			settings = Settings.Load<Settings>(modEntry);
			settings.OnChange();
			modEntry.OnGUI = OnGUI;
			modEntry.OnSaveGUI = OnSaveGUI;

			IsGaugeModInstalled = UnityModManager.FindMod("Gauge") is not null;

#if DEBUG
			Harmony.DEBUG = true;
#endif

			try
			{
				harmony = new Harmony(modEntry.Info.Id);
				harmony.PatchAll();

			}
			catch (Exception ex)
			{
				modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
				//harmony?.UnpatchAll();
				return false;
			}

			return true;
#endif
		}

#if B99_ALL_STANDARD
        private static bool showPoweredTenderSettings;
        private static void DrawPoweredTenderSettings()
        {
            showPoweredTenderSettings = GUILayout.Toggle(showPoweredTenderSettings, "Powered tender — arrangement options (test)");
            if (!showPoweredTenderSettings) return;
            GUILayout.Label("Checked layouts use the Erie powered S282 tender. Applies on spawn/reload or REAPPLY above. Stop and close the regulator first.\nThree-engine locomotive triplexes, Drivers not found and the giant prototype are excluded. Uncheck Erie here to test its locomotive without the tender engine.");
            settings.poweredTenderArrangements ??= new[] { 43 };
            for (int id = 0; id < WheelArrangement.SupportedCount; id++)
            {
                if (!PoweredTenderPolicy.Supports(id)) continue;
                bool previous = settings.PoweredTenderEnabled(id);
                bool selected = GUILayout.Toggle(previous, WheelArrangement.WheelArrangementNames[id] + " — powered tender");
                if (selected && !previous) settings.poweredTenderArrangements = settings.poweredTenderArrangements.Append(id).ToArray();
                if (!selected && previous) settings.poweredTenderArrangements = settings.poweredTenderArrangements.Where(x => x != id).ToArray();
            }
        }
#endif

		static void OnGUI(UnityModManager.ModEntry modEntry)
		{
			settings.Draw(modEntry);
			//GUILayout.BeginVertical();
			//settings.spawnRandomWA = GUILayout.Toggle(settings.spawnRandomWA, "Spawn S282 with random wheel arrangement");

		}

		static void OnSaveGUI(UnityModManager.ModEntry modEntry)
		{
			settings.Save(modEntry);
		}
	}
}

using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.IO;    //For data read/write methods
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;

using HarmonyLib;

using QModManager.API.ModLoading;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.Auroresource {
	[QModCore]
	public static class AuroresourceMod {

		public const string MOD_KEY = "ReikaKalseki.Auroresource";

		//public static readonly ModLogger logger = new ModLogger();
		public static readonly Assembly modDLL = Assembly.GetExecutingAssembly();

		public static readonly Config<ARConfig.ConfigEntries> config = new Config<ARConfig.ConfigEntries>(modDLL);

		public static readonly XMLLocale locale = new XMLLocale(modDLL, "XML/locale.xml");
		public static readonly XMLLocale pdaLocale = new XMLLocale(modDLL, "XML/pda.xml");
		public static readonly XMLLocale voLocale = new XMLLocale(modDLL, "XML/vo.xml");

		public static readonly Vector3 jailbreakPedestalLocation = new Vector3(420, -93.3F, 1153);

		public static DrillableMeteorite dunesMeteor;
		public static LavaDome lavaPitCenter;
		public static PrecursorJailbreakingConsole console;
		public static ScannerRoomMeteorPlanner meteorDetector;

		public static StoryGoal laserCutterJailbroken;

		public static TechType detectorUnlock = TechType.BaseMapRoom;

		[QModPatch]
		public static void Load() {
			config.load();

			HarmonySystem harmony = new HarmonySystem(MOD_KEY, modDLL, typeof(ARPatches));
			harmony.apply();

			ModVersionCheck.getFromGitVsInstall("Auroresource", modDLL, "Auroresource").register();
			SNUtil.checkModHash(modDLL);

			locale.load();
			pdaLocale.load();
			voLocale.load();

			addPDAEntries();

			dunesMeteor = new DrillableMeteorite();
			dunesMeteor.register();
			lavaPitCenter = new LavaDome();
			lavaPitCenter.register(10);
			console = new PrecursorJailbreakingConsole(locale.getEntry("JailBreakConsole"));
			console.register();

			meteorDetector = new ScannerRoomMeteorPlanner();
			meteorDetector.Patch();

			laserCutterJailbroken = new StoryGoal("lasercutterjailbreak", Story.GoalType.Story, 0f);

			FallingMaterialSystem.instance.register();

			PDAMessagePrompts.instance.addPDAMessage(voLocale.getEntry("auroracut"));
			PDAMessagePrompts.instance.addPDAMessage(voLocale.getEntry("jailbreak"));

			GenUtil.registerWorldgen(new PositionedPrefab(dunesMeteor.ClassID, WorldUtil.DUNES_METEOR + (Vector3.down * 29)));
			GenUtil.registerWorldgen(new PositionedPrefab(lavaPitCenter.ClassID, WorldUtil.LAVA_DOME + (Vector3.down * 56)));
			GenUtil.registerWorldgen(new PositionedPrefab(VanillaCreatures.REAPER.prefab, new Vector3(-1125, -209, 1130)));

			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ARHooks).TypeHandle);

			DIHooks.onWorldLoadedEvent += () => {
				SNUtil.log("Adding resource data to motherlode PDA pages.", modDLL);
				dunesMeteor.updateLocale();
				lavaPitCenter.updateLocale();
			};

			StoryHandler.instance.addListener(s => {
				if (s == laserCutterJailbroken.key)
					PDAMessagePrompts.instance.trigger("jailbreak");
			});

			CustomLocaleKeyDatabase.registerKey(locale.getEntry("AuroraLaserCut"));
			CustomLocaleKeyDatabase.registerKey(locale.getEntry("AuroraLaserCutNeedsUnlock"));

			TechTypeMappingConfig<float>.loadInline("falling_materials", TechTypeMappingConfig<float>.FloatParser.instance, FallingMaterialSystem.instance.addMaterial);

			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("triggerFallingDebris", FallingMaterialSystem.instance.spawnItem);
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("queueFallingDebris", FallingMaterialSystem.instance.queueSpawn);
		}

		[QModPostPatch]
		public static void PostLoad() {
			if (detectorUnlock != TechType.None)
				TechnologyUnlockSystem.instance.addDirectUnlock(detectorUnlock, meteorDetector.TechType);
		}

		public static void addPDAEntries() {
			foreach (XMLLocale.LocaleEntry e in pdaLocale.getEntries()) {
				PDAManager.PDAPage page = PDAManager.createPage(e);
				if (e.hasField("audio"))
					page.setVoiceover(e.getField<string>("audio"));
				if (e.hasField("header"))
					page.setHeaderImage(TextureManager.getTexture(modDLL, "Textures/PDA/" + e.getField<string>("header")));
				page.register();
			}
		}

	}
}

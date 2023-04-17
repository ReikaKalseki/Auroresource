﻿using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Reflection;
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Assets;

using Story;

namespace ReikaKalseki.Auroresource
{
  [QModCore]
  public static class AuroresourceMod {
    
    public const string MOD_KEY = "ReikaKalseki.Auroresource";
    
    //public static readonly ModLogger logger = new ModLogger();
	public static readonly Assembly modDLL = Assembly.GetExecutingAssembly();
    
    public static readonly Config<ARConfig.ConfigEntries> config = new Config<ARConfig.ConfigEntries>();
    
    public static readonly XMLLocale locale = new XMLLocale("XML/locale.xml");
    public static readonly XMLLocale pdaLocale = new XMLLocale("XML/pda.xml");
    public static readonly XMLLocale voLocale = new XMLLocale("XML/vo.xml");
    
    public static readonly Vector3 jailbreakPedestalLocation = new Vector3(420, -93.3F, 1153);
    
    public static DrillableMeteorite dunesMeteor;
    public static LavaDome lavaPitCenter;
    public static PrecursorJailbreakingConsole console;
    
    public static StoryGoal laserCutterJailbroken;

    [QModPatch]
    public static void Load() {
        config.load();
        
        Harmony harmony = new Harmony(MOD_KEY);
        Harmony.DEBUG = true;
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(modDLL.Location), "harmony-log.txt");
        FileLog.Log("Ran mod register, started harmony (harmony log)");
        SNUtil.log("Ran mod register, started harmony");
        try {
        	harmony.PatchAll(modDLL);
        }
        catch (Exception ex) {
			FileLog.Log("Caught exception when running patcher!");
			FileLog.Log(ex.Message);
			FileLog.Log(ex.StackTrace);
			FileLog.Log(ex.ToString());
        }
        
        ModVersionCheck.getFromGitVsInstall("Auroresource", modDLL, "Auroresource").register();
        
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
	    
	    laserCutterJailbroken = new StoryGoal("lasercutterjailbreak", Story.GoalType.Story, 0f);
	    
	    PDAMessagePrompts.instance.addPDAMessage(voLocale.getEntry("auroracut"));
        PDAMessagePrompts.instance.addPDAMessage(voLocale.getEntry("jailbreak"));
        
        GenUtil.registerWorldgen(new PositionedPrefab(dunesMeteor.ClassID, new Vector3(-1125, -409, 1130)));
        GenUtil.registerWorldgen(new PositionedPrefab(lavaPitCenter.ClassID, new Vector3(-273, -1355-56, -152)));
        GenUtil.registerWorldgen(new PositionedPrefab(VanillaCreatures.REAPER.prefab, new Vector3(-1125, -209, 1130)));
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ARHooks).TypeHandle);
        
        DIHooks.onWorldLoadedEvent += () => {
        	SNUtil.log("Adding resource data to motherlode PDA pages.", modDLL);
        	dunesMeteor.updateLocale();
        	lavaPitCenter.updateLocale();
        };
        
        StoryHandler.instance.addListener(s => {if (s == laserCutterJailbroken.key){PDAMessagePrompts.instance.trigger("jailbreak");}});
    }
    
    [QModPostPatch]
    public static void PostLoad() {
		Spawnable irid = ItemRegistry.instance.getItem("IRIDIUM");
		if (irid != null) {
			SNUtil.log("Found iridium ore. Adding to meteor drop list.");
			dunesMeteor.addDrop(irid.TechType, 15);
		}
    }
    
    public static void addPDAEntries() {
    	foreach (XMLLocale.LocaleEntry e in pdaLocale.getEntries()) {
			PDAManager.PDAPage page = PDAManager.createPage(e);
			if (e.hasField("audio"))
				page.setVoiceover(e.getField<string>("audio"));
			if (e.hasField("header"))
				page.setHeaderImage(TextureManager.getTexture(modDLL, "Textures/PDA/"+e.getField<string>("header")));
			page.register();
    	}
    }

  }
}

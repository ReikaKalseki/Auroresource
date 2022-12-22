using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
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
    
    public static DrillableMeteorite dunesMeteor;

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
        
        locale.load();
        pdaLocale.load();
        
        addPDAEntries();
	    
	    dunesMeteor = new DrillableMeteorite();
	    dunesMeteor.register();
	    
	    PDAMessagePrompts.instance.addPDAMessage("auroracut", "The Aurora is the property of the Alterra Corporation. Do not attempt salvage of the Aurora's materials.", "Sounds/auroracutwarn.ogg");
        
        GenUtil.registerWorldgen(new PositionedPrefab(dunesMeteor.ClassID, new Vector3(-1125, -409, 1130)));
        GenUtil.registerWorldgen(new PositionedPrefab(lavaPit.ClassID, new Vector3(-273, -1355-40, -152)));
        GenUtil.registerWorldgen(new PositionedPrefab(VanillaCreatures.REAPER.prefab, new Vector3(-1125, -209, 1130)));
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ARHooks).TypeHandle);
    }
    
    [QModPostPatch]
    public static void PostLoad() {
		Spawnable irid = ItemRegistry.instance.getItem("IRIDIUM");
		if (irid != null) {
			SNUtil.log("Found iridium ore. Adding to meteor drop list.");
			dunesMeteor.drops.addEntry(irid.TechType, 15);
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

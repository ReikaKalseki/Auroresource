using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;

using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Auroresource {
	
	public static class ARHooks {
	    
	    static ARHooks() {
			DIHooks.itemTooltipEvent += generateItemTooltips;
	    }
		
		public static void generateItemTooltips(StringBuilder sb, TechType tt, GameObject go) {
			if (tt == TechType.LaserCutter && Story.StoryGoalManager.main.completedGoals.Contains(AuroresourceMod.laserCutterJailbroken.key)) {
				TooltipFactory.WriteDescription(sb, "\nDevice firmware has been modified to circumvent proscribed usage limitations.");
			}
		}
	    
	    public static void onAuroraSpawn(CrashedShipExploder ex) {
	    	Sealed s = ex.gameObject.EnsureComponent<Sealed>();
	    	s._sealed = true;
	    	s.maxOpenedAmount = 250/AuroresourceMod.config.getFloat(ARConfig.ConfigEntries.SPEED); //was 150, comparedto vanilla 100
	    	s.openedEvent.AddHandler(ex.gameObject, new UWE.Event<Sealed>.HandleFunction(se => {
				bool unlock = Story.StoryGoalManager.main.completedGoals.Contains(AuroresourceMod.laserCutterJailbroken.key);
	    		se.openedAmount = 0;
	    		se._sealed = true;
	    		if (unlock) {
	    			InventoryUtil.addItem(TechType.ScrapMetal);
		    		PDAMessagePrompts.instance.trigger("auroracut");
	    		}
	    		//SNUtil.log("Cycled aurora laser cut: "+s.openedAmount);
	    	}));
			GenericHandTarget ht = ex.gameObject.EnsureComponent<GenericHandTarget>();
			ht.onHandHover = new HandTargetEvent();
			ht.onHandHover.AddListener(hte => {
				bool unlock = Story.StoryGoalManager.main.completedGoals.Contains(AuroresourceMod.laserCutterJailbroken.key);
				Pickupable held = Inventory.main.GetHeld();
	    		if (unlock) {
					HandReticle.main.SetProgress(s.GetSealedPercentNormalized());
					HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
			   		HandReticle.main.SetInteractText("AuroraLaserCut"); //is a locale key
			    }
			    else if (held && held.GetTechType() == TechType.LaserCutter) {
			    	HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
			   		HandReticle.main.SetInteractText("AuroraLaserCutNeedsUnlock"); //is a locale key
			    }
			    else {
			    	HandReticle.main.SetIcon(HandReticle.IconType.Default, 1f);
			    	HandReticle.main.SetInteractText("");
			    }
			});
			LanguageHandler.SetLanguageLine("AuroraLaserCut", "Use unlocked laser cutter to harvest metal salvage");
			LanguageHandler.SetLanguageLine("AuroraLaserCutNeedsUnlock", "Laser cutter firmware forbids dismantling Alterra property");
	    }
	    
	    public static GameObject getDrillableDrop(Drillable d) {
	    	PrefabIdentifier pi = d.gameObject.GetComponent<PrefabIdentifier>();
	    	if (pi) {
	    		DrillableResourceArea di = DrillableResourceArea.getResourceNode(pi.ClassId);
	    		if (di != null) {
	    			return di.getRandomResource();
	    		}
	    	}
	    	return d.ChooseRandomResource();
	    }
		
		public static void onBreakableResourceSpawn(BreakableResource src) {
			if (src.gameObject.GetComponent<ReefbackPlant>()) {
				src.numChances = 4;
			}
		}
	}
}

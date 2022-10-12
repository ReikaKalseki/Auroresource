using System;
using System.IO;
using System.Xml;
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
			
	    }
	    
	    public static void onAuroraSpawn(CrashedShipExploder ex) {
	    	Sealed s = ex.gameObject.EnsureComponent<Sealed>();
	    	s._sealed = true;
	    	s.maxOpenedAmount = 250/AuroresourceMod.config.getFloat(ARConfig.ConfigEntries.SPEED); //was 150, comparedto vanilla 100
	    	s.openedEvent.AddHandler(ex.gameObject, new UWE.Event<Sealed>.HandleFunction(se => {
	    		se.openedAmount = 0;
	    		se._sealed = true;
	    		GameObject scrap = CraftData.GetPrefabForTechType(TechType.ScrapMetal);
	    		scrap = UnityEngine.Object.Instantiate(scrap);
	    		scrap.SetActive(false);
	    		Inventory.main.ForcePickup(scrap.GetComponent<Pickupable>());
	    		PDAMessagePrompts.instance.trigger("auroracut");
	    		//SNUtil.log("Cycled aurora laser cut: "+s.openedAmount);
	    	}));
			GenericHandTarget ht = ex.gameObject.EnsureComponent<GenericHandTarget>();
			ht.onHandHover = new HandTargetEvent();
			ht.onHandHover.AddListener(hte => {
				HandReticle.main.SetInteractText("AuroraLaserCut"); //is a locale key
				HandReticle.main.SetProgress(s.GetSealedPercentNormalized());
				HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
			});
			Language.main.strings["AuroraLaserCut"] = "Use Laser Cutter to harvest metal salvage";
	    }
	    
	    public static GameObject getDrillableDrop(Drillable d) {
	    	PrefabIdentifier pi = d.gameObject.GetComponent<PrefabIdentifier>();
	    	if (pi && pi.ClassId == AuroresourceMod.dunesMeteor.ClassID)
	    		return AuroresourceMod.dunesMeteor.getRandomResource();
	    	return d.ChooseRandomResource();
	    }
		
		public static void onBreakableResourceSpawn(BreakableResource src) {
			if (src.gameObject.GetComponent<ReefbackPlant>()) {
				src.numChances = 4;
			}
		}
	}
}

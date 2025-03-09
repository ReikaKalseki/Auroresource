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
			SNUtil.log("Initializing ARHooks");
	    	DIHooks.onPlayerTickEvent += tickPlayer;
			DIHooks.itemTooltipEvent += generateItemTooltips;
	    	DIHooks.onItemPickedUpEvent += onItemPickedUp;
	    	DIHooks.scannerRoomTechTypeListingEvent += FallingMaterialSystem.instance.modifyScannableList;	    	
	    	DIHooks.scannerRoomTickEvent += FallingMaterialSystem.instance.tickMapRoom;
	    }
		
		public static void generateItemTooltips(StringBuilder sb, TechType tt, GameObject go) {
			if (tt == TechType.LaserCutter && Story.StoryGoalManager.main.completedGoals.Contains(AuroresourceMod.laserCutterJailbroken.key)) {
				TooltipFactory.WriteDescription(sb, "\nDevice firmware has been modified to circumvent proscribed usage limitations.");
			}
		}
		
	    public static void tickPlayer(Player ep) {	    
			float time = DayNightCycle.main.timePassedAsFloat;		    	
	    	float dT = Time.deltaTime;
	    	FallingMaterialSystem.instance.tick(time, dT);
		}
		
		public static void onItemPickedUp(DIHooks.ItemPickup ip) {
			Pickupable p = ip.item;
			FallingMaterialTag tag = p.GetComponentInParent<FallingMaterialTag>();
			if (tag) {
				p.transform.SetParent(null);
				UnityEngine.Object.Destroy(tag.gameObject);
			}
		}
		
		public static void onGeyserSpawn(Geyser g) {
			g.gameObject.EnsureComponent<GeyserMaterialSpawner>().geyser = g;
		}
		
		public static void onMapRoomSpawn(MapRoomFunctionality map) {
			if (Array.IndexOf(map.allowedUpgrades, AuroresourceMod.meteorDetector.TechType) < 0)
				typeof(MapRoomFunctionality).GetField("allowedUpgrades", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(map, map.allowedUpgrades.addToArray(AuroresourceMod.meteorDetector.TechType));
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

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.Auroresource {
	
	public class PrecursorJailbreakingConsole : Spawnable {
		
		private readonly XMLLocale.LocaleEntry locale;
	        
	    internal PrecursorJailbreakingConsole(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("81cf2223-455d-4400-bac3-a5bcd02b3638");
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			StoryHandTarget sh = world.EnsureComponent<StoryHandTarget>();
			sh.goal = AuroresourceMod.laserCutterJailbroken;
			sh.primaryTooltip = locale.getField<string>("tooltip");
			sh.secondaryTooltip = locale.getField<string>("tooltipSecondary");
			sh.isValidHandTarget = false;
			world.EnsureComponent<JailbreakingConsoleTag>();
			foreach (Renderer r in world.GetComponent<PrecursorComputerTerminal>().fx.GetComponentsInChildren<Renderer>()) {
				//r.materials[0].SetColor("_Color", new Color(0.8F, 0.25F, 1F));
				r.materials[0].SetColor("_Color", new Color(0.3F, 0.9F, 1F));
			}
			return world;
	    }
		
		public void register() {
			Patch();
			GenUtil.registerWorldgen(new PositionedPrefab(ClassID, AuroresourceMod.jailbreakPedestalLocation, Quaternion.Euler(0, 20, 0)));
		}
		
		class JailbreakingConsoleTag : MonoBehaviour {
			
			private StoryHandTarget target;
			private PrecursorComputerTerminal terminal;
			
			void Update() {
				if (!target)
					target = gameObject.GetComponent<StoryHandTarget>();
				if (!terminal)
					terminal = gameObject.GetComponent<PrecursorComputerTerminal>();
				
				if (target) {
					bool unlock = Story.StoryGoalManager.main.completedGoals.Contains(AuroresourceMod.laserCutterJailbroken.key);
					Pickupable held = Inventory.main.GetHeld();
					target.isValidHandTarget = !unlock && held && held.GetTechType() == TechType.LaserCutter;
					target.enabled = target.isValidHandTarget;
					terminal.enabled = target.enabled;
					target.secondaryTooltip = target.enabled ? AuroresourceMod.console.locale.getField<string>("tooltipSecondary") : AuroresourceMod.console.locale.getField<string>("tooltipDisabled");
				}
			}
			
		}
		
	}
}

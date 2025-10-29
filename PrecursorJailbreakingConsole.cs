using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
			sh.primaryTooltip = locale.getString("tooltip");
			sh.secondaryTooltip = locale.getString("tooltipSecondary");
			sh.informGameObject = world;
			sh.isValidHandTarget = false;
			world.EnsureComponent<JailbreakingConsoleTag>();
			foreach (Renderer r in world.GetComponent<PrecursorComputerTerminal>().fx.GetComponentsInChildren<Renderer>()) {
				//r.materials[0].SetColor("_Color", new Color(0.8F, 0.25F, 1F));
				r.materials[0].SetColor("_Color", new Color(0.3F, 0.9F, 1F));
			}
			return world;
		}

		public void register() {
			this.Patch();
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
					target.isValidHandTarget = !unlock && held && held.GetComponent<LaserCutter>();// held.GetTechType() == TechType.LaserCutter;
					target.enabled = target.isValidHandTarget;
					terminal.enabled = target.enabled;
					target.secondaryTooltip = target.enabled ? AuroresourceMod.console.locale.getString("tooltipSecondary") : AuroresourceMod.console.locale.getString("tooltipDisabled");
				}
			}

			void OnStoryHandTarget() {
				SNUtil.triggerUnlockPopup(new SNUtil.PopupData("Laser Cutter Upgraded", "Firmware Unlocked") { controlText = "Can now harvest directly from the Aurora's hull", graphic = () => SNUtil.getTechPopupSprite(TechType.LaserCutter) });
			}

		}

	}
}

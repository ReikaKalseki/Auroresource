using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

	public class ScannerRoomMeteorPlanner : BasicCraftingItem {

		internal ScannerRoomMeteorPlanner() : base(AuroresourceMod.locale.getEntry("ScannerRoomMeteorPlanner"), "6d1d97a5-75b8-49ef-8944-393d387a37a0") {
			unlockRequirement = TechType.Unobtanium;
			sprite = TextureManager.getSprite(AuroresourceMod.modDLL, "Textures/planner");
			craftingTime = 8;
			inventorySize = new Vector2int(2, 2);
			this.addIngredient(TechType.MapRoomCamera, 1);
			this.addIngredient(TechType.SeamothSonarModule, 1);
			this.addIngredient(TechType.TitaniumIngot, 1);
			this.addIngredient(TechType.Polyaniline, 1);
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.MapRoomUpgrades;
			}
		}

		public sealed override TechCategory CategoryForPDA {
			get {
				return TechCategory.MapRoomUpgrades;
			}
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.MapRoom;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[0];
			}
		}
	}
}

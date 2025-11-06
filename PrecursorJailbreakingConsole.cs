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

	public class PrecursorJailbreakingConsole : PrecursorStoryConsole {

		internal PrecursorJailbreakingConsole(XMLLocale.LocaleEntry e) : base(e, AuroresourceMod.laserCutterJailbroken) {
			setPopup(TechType.LaserCutter);
		}

		public override bool isUsable(StoryConsoleTag tag) {
			Pickupable held = Inventory.main.GetHeld();
			return held && held.GetComponent<LaserCutter>();// held.GetTechType() == TechType.LaserCutter;
		}

	}
}

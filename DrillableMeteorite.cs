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
	
	public class DrillableMeteorite : DrillableResourceArea {
		
		public DrillableMeteorite() : base(AuroresourceMod.locale.getEntry("DrillableMeteorite"), 20) {
			addDrop(TechType.Titanium, 400);
			addDrop(TechType.Copper, 250);
			addDrop(TechType.Nickel, 180);
			addDrop(TechType.Lead, 180);
			addDrop(TechType.Silver, 150);
			addDrop(TechType.Gold, 100);
			addDrop(TechType.MercuryOre, 40);
		}
			
	}
}

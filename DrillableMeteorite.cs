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

	public class DrillableMeteorite : DrillableResourceArea {

		public DrillableMeteorite() : base(AuroresourceMod.locale.getEntry("DrillableMeteorite"), 24) {
			this.addDrop(TechType.Titanium, 400);
			this.addDrop(TechType.Copper, 250);
			this.addDrop(TechType.Nickel, 180);
			this.addDrop(TechType.Lead, 180);
			this.addDrop(TechType.Silver, 150);
			this.addDrop(TechType.Gold, 100);
			this.addDrop(TechType.MercuryOre, 40);
		}

	}
}

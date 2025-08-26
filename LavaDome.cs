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

	public class LavaDome : DrillableResourceArea {

		public LavaDome() : base(AuroresourceMod.locale.getEntry("DrillableLavaDome"), 69) {
			this.addDrop(TechType.Quartz, 180);
			this.addDrop(TechType.AluminumOxide, 120);
			this.addDrop(TechType.Diamond, 80);
			this.addDrop(TechType.Magnetite, 50);
			this.addDrop(TechType.Sulphur, 30);
			this.addDrop(TechType.UraniniteCrystal, 30);
			this.addDrop(TechType.Kyanite, 10);
		}

	}
}

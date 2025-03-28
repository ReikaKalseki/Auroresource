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
	
	public class LavaDome : DrillableResourceArea {
		
		public LavaDome() : base(AuroresourceMod.locale.getEntry("DrillableLavaDome"), 69) {
			addDrop(TechType.Quartz, 180);
			addDrop(TechType.AluminumOxide, 120);
			addDrop(TechType.Diamond, 80);
			addDrop(TechType.Magnetite, 50);
			addDrop(TechType.Sulphur, 30);
			addDrop(TechType.UraniniteCrystal, 30);
			addDrop(TechType.Kyanite, 10);
		}
			
	}
}

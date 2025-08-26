using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Auroresource {
	public class ARConfig {
		public enum ConfigEntries {
			[ConfigEntry("Motherlode Harvesting Speed", typeof(float), 1F, 0.1F, 10, float.NaN)]SPEED,
			[ConfigEntry("Orbital Debris ReEntry Rate", typeof(float), 1F, 0.1F, 5, float.NaN)]REENTRY_RATE,
			[ConfigEntry("Orbital Debris ReEntry Warning Multiplier", typeof(float), 1F, 0.25F, 2, float.NaN)]REENTRY_WARNING,
			[ConfigEntry("Geyser Mineral Ejection Rate", typeof(float), 1F, 0.1F, 5, 0)]GEYSER_RESOURCE_RATE,
		}
	}
}

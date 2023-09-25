using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Auroresource
{
	public class ARConfig
	{		
		public enum ConfigEntries {
			[ConfigEntry("Motherlode Harvesting Speed", typeof(float), 1F, 0.1F, 10, 0)]SPEED,
			[ConfigEntry("Orbital Debris ReEntry Rate", typeof(float), 1F, 0.1F, 5, 0)]REENTRY_RATE,
			[ConfigEntry("Orbital Debris ReEntry Warning Multiplier", typeof(float), 1F, 0.25F, 2, 0)]REENTRY_WARNING,
		}
	}
}

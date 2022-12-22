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
		}
	}
}

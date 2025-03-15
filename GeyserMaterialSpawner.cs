using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.Auroresource
{
	public class GeyserMaterialSpawner : MonoBehaviour {
		
		private static readonly WeightedRandom<TechType> drops = new WeightedRandom<TechType>();
		private static readonly Dictionary<BiomeBase, float> biomeMultipliers = new Dictionary<BiomeBase, float>();
		
		internal Geyser geyser;
		
		private float nextMineralTime = -1;
		
		static GeyserMaterialSpawner() {
			drops.addEntry(TechType.Gold, 30);
			drops.addEntry(TechType.Copper, 20);
			drops.addEntry(TechType.Silver, 10);
			drops.addEntry(TechType.Lead, 30);
			drops.addEntry(TechType.Lithium, 40);
			drops.addEntry(TechType.Magnetite, 10);
			drops.addEntry(TechType.UraniniteCrystal, 5);
			drops.addEntry(TechType.Quartz, 20);
			
			biomeMultipliers[VanillaBiomes.SHALLOWS] = 0.8F;
			biomeMultipliers[VanillaBiomes.JELLYSHROOM] = 0.5F;
			biomeMultipliers[VanillaBiomes.UNDERISLANDS] = 0.25F;
			biomeMultipliers[VanillaBiomes.KOOSH] = 3.0F; //because very few, comparable to bottom of underislands
			
		}
		
		public static void addGeyserMineral(TechType tt, float weight) {
			drops.addEntry(tt, weight);
		}
		
		public static TechType getRandomMineral() {
			return drops.getRandomEntry();
		}
		
		public static void addBiomeRateMultiplier(BiomeBase bb, float rate) {
			biomeMultipliers[bb] = rate;
		}
		
		public static float getBiomeRateMultiplier(Vector3 pos) {
			return getBiomeRateMultiplier(BiomeBase.getBiome(pos));
		}
		
		public static float getBiomeRateMultiplier(BiomeBase bb) {
			return biomeMultipliers.ContainsKey(bb) ? biomeMultipliers[bb] : 1;
		}
			
		void Update() {
			if (nextMineralTime < 0)
				nextMineralTime = getRandomNextTime(0);
			if (geyser.erupting && !drops.isEmpty()) {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time >= nextMineralTime) {
					trySpawnMineral();
					nextMineralTime = getRandomNextTime(time); //set time no matter what, do not "queue" spawn
				}
			}
		}
		
		private bool trySpawnMineral() {
			if (WorldUtil.getObjectsNearMatching(transform.position, 25, isEjectedMineral).Count > 6)
				return false;
			GameObject go = ObjectUtil.lookupPrefab(getRandomMineral());
			if (go) {
				go = UnityEngine.Object.Instantiate(go, transform.position+Vector3.up*3.5F, UnityEngine.Random.rotationUniform);
				Rigidbody rb = go.GetComponent<Rigidbody>();
				if (rb) {
					rb.isKinematic = false;
					rb.constraints = RigidbodyConstraints.None;
					rb.velocity = MathUtil.getRandomVectorAround(Vector3.zero, 6).setY(UnityEngine.Random.Range(5F, 18F));
					rb.angularVelocity = MathUtil.getRandomVectorAround(Vector3.zero, 8);
				}
				return true;
			}
			return false;
		}
		
		private bool isEjectedMineral(GameObject go) {
			Pickupable pp = go.GetComponent<Pickupable>();
			if (!pp || !drops.hasEntry(pp.GetTechType()))
				return false;
			Rigidbody rb = go.GetComponent<Rigidbody>();
			return rb && !rb.isKinematic;
		}
		
		private float getRandomNextTime(float time) {
			return time+UnityEngine.Random.Range(90, 240)/(AuroresourceMod.config.getFloat(ARConfig.ConfigEntries.GEYSER_RESOURCE_RATE)*getBiomeRateMultiplier(transform.position));
		}
		
	}
}

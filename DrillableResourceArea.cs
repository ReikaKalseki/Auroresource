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
	
	public abstract class DrillableResourceArea : Spawnable {
		
		public static readonly float DURATION = 200*AuroresourceMod.config.getFloat(ARConfig.ConfigEntries.SPEED);
		
		private static readonly Dictionary<string, DrillableResourceArea> NODES = new Dictionary<string, DrillableResourceArea>();
		
		private readonly WeightedRandom<TechType> drops = new WeightedRandom<TechType>();
		public readonly XMLLocale.LocaleEntry locale;
		public readonly float radius;
		
		//public float harvestSpeedMultiplier = 1;
		
		public static DrillableResourceArea getResourceNode(string id) {
			return NODES.ContainsKey(id) ? NODES[id] : null;
		}
		
		protected DrillableResourceArea(XMLLocale.LocaleEntry e, float r) : base(e.key, e.name, e.desc) {
			locale = e;
			radius = r;
		}
		
		internal DrillableResourceArea addDrop(TechType drop, double weight) {
			drops.addEntry(drop, weight);
			return this;
		}
		
		public void register(int scanTime = 20) {
			Patch();
			SNUtil.addScanUnlock(this, scanTime, PDAManager.getPage(locale.pda));
			NODES[ClassID] = this;
		}
		
		public void updateLocale() {
			PDAManager.PDAPage page = PDAManager.getPage(locale.pda);
			page.append("\n\nPossible Materials:\n");
			foreach (TechType tt in drops.getValues()) {
				page.append(Language.main.strings[tt.AsString(false)]+": "+(drops.getProbability(tt)*100).ToString("0.0")+"%\n");
			}
		}
		
		public GameObject getRandomResource() {
			return ObjectUtil.lookupPrefab(drops.getRandomEntry());
		}
			
	    public override sealed GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(VanillaResources.LARGE_QUARTZ.prefab, true, false);
			if (world != null) {
				world.SetActive(false);
				world.EnsureComponent<TechTag>().type = TechType;
				world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				MeshRenderer[] r = world.GetComponentsInChildren<MeshRenderer>();
				for (int i = 1; i < r.Length; i++) {
					UnityEngine.Object.DestroyImmediate(r[i].gameObject);
				}
				SphereCollider sc = r[0].gameObject.EnsureComponent<SphereCollider>();
				sc.radius = radius;
				sc.center = Vector3.zero;
				world.EnsureComponent<InfinitelyDrillable>();
				Drillable dr = world.EnsureComponent<Drillable>();
				dr.Start();
				dr.health[0] = DURATION;//harvestSpeedMultiplier;
				dr.primaryTooltip = locale.getField<string>("tooltip");
				dr.secondaryTooltip = locale.getField<string>("tooltipSecondary");
				dr.minResourcesToSpawn = 1;
				dr.maxResourcesToSpawn = 1;
				dr.deleteWhenDrilled = false;
				dr.kChanceToSpawnResources = 1;
				world.SetActive(true);
				dr.onDrilled += (d) => {
					//SNUtil.writeToChat("Finished drilling "+d.health.Length+"|"+string.Join(",", d.health));
					d.health[0] = DURATION;//harvestSpeedMultiplier;
					d.GetComponentsInChildren<MeshRenderer>(true)[0].gameObject.SetActive(true);
				};
				return world;
			}
			else {
				SNUtil.writeToChat("Could not fetch template GO for "+this);
				return null;
			}
	    }
		
		class InfinitelyDrillable : MonoBehaviour {
			
			private Drillable drill;
			private GameObject innerObject;
			
			void Update() {
				if (!drill || !innerObject) {
					drill = gameObject.GetComponent<Drillable>();
					innerObject = drill.GetComponentsInChildren<MeshRenderer>(true)[0].gameObject;
				}
				if (drill.health[0] <= 0 || !innerObject.activeSelf) {
					drill.health[0] = DURATION;//harvestSpeedMultiplier;
					innerObject.SetActive(true);
				}
			}
			
		}
			
	}
}

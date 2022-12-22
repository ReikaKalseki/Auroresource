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
		
		private readonly WeightedRandom<TechType> drops = new WeightedRandom<TechType>();
		public readonly XMLLocale.LocaleEntry locale;
		
		protected DrillableResourceArea(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
		}
		
		protected DrillableResourceArea addDrop(TechType drop, double weight) {
			drops.addEntry(drop, weight);
			return this;
		}
		
		public void register() {
			Patch();
			SNUtil.addPDAEntry(this, 20, PDAManager.getPage(locale.pda));
		}
		
		public GameObject getRandomResource() {
			return CraftData.GetPrefabForTechType(drops.getRandomEntry(), true);
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
				sc.radius = 20F;
				sc.center = Vector3.zero;
				world.EnsureComponent<InfinitelyDrillable>();
				Drillable dr = world.EnsureComponent<Drillable>();
				dr.Start();
				dr.primaryTooltip = locale.getField<string>("tooltip");
				dr.secondaryTooltip = locale.getField<string>("tooltipSecondary");
				dr.minResourcesToSpawn = 1;
				dr.maxResourcesToSpawn = 1;
				dr.deleteWhenDrilled = false;
				dr.kChanceToSpawnResources = 1;
				world.SetActive(true);
				dr.onDrilled += (d) => {
					//SNUtil.writeToChat("Finished drilling "+d.health.Length+"|"+string.Join(",", d.health));
					d.health[0] = DURATION;
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
					drill.health[0] = DURATION;
					innerObject.SetActive(true);
				}
			}
			
		}
			
	}
}

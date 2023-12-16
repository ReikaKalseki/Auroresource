using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ReikaKalseki.DIAlterra;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.Auroresource {
	
	public abstract class DrillableResourceArea : Spawnable {
		
		public static readonly float DURATION = 200*AuroresourceMod.config.getFloat(ARConfig.ConfigEntries.SPEED);
		
		private static float maxRadius = -1;
		private static readonly Dictionary<string, DrillableResourceArea> NODES = new Dictionary<string, DrillableResourceArea>();
		
		private readonly WeightedRandom<TechType> drops = new WeightedRandom<TechType>();
		public readonly XMLLocale.LocaleEntry locale;
		public readonly float radius;
		
		//public float harvestSpeedMultiplier = 1;
		
		public static DrillableResourceArea getResourceNode(string id) {
			return NODES.ContainsKey(id) ? NODES[id] : null;
		}
		
		public static float getMaxRadius() {
			return maxRadius;
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
			SNUtil.addScanUnlock(TechType, FriendlyName, scanTime, PDAManager.getPage(locale.pda));
			NODES[ClassID] = this;
			maxRadius = Mathf.Max(maxRadius, radius);
		}
		
		public void updateLocale() {
			PDAManager.PDAPage page = PDAManager.getPage(locale.pda);
			page.append("\n\n"+locale.getField<string>("materialListHeader")+"\n");
			foreach (TechType tt in drops.getValues()) {
				page.append(Language.main.strings[tt.AsString(false)]+": "+(drops.getProbability(tt)*100).ToString("0.0")+"%\n");
			}
			if (InstructionHandlers.getTypeBySimpleName("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator") != null)
				page.append("\n\n"+locale.getField<string>("fcsNote"));
		}
		
		public List<TechType> getAllAvailableResources() {
			return new List<TechType>(drops.getValues());
		}
		
		public TechType getRandomResourceType() {
			return drops.getRandomEntry();
		}
		
		public GameObject getRandomResource() {
			return ObjectUtil.lookupPrefab(getRandomResourceType());
		}
			
	    public override sealed GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(VanillaResources.LARGE_QUARTZ.prefab, true, false);
			if (world != null) {
				world.name = ClassID;
				world.SetActive(false);
				world.EnsureComponent<TechTag>().type = TechType;
				world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
				MeshRenderer[] r = world.GetComponentsInChildren<MeshRenderer>();
				for (int i = 1; i < r.Length; i++) {
					UnityEngine.Object.DestroyImmediate(r[i].gameObject);
				}
				ObjectUtil.removeComponent<Collider>(world);
				SphereCollider sc = world.EnsureComponent<SphereCollider>();
				sc.radius = radius;
				sc.center = Vector3.zero;
				sc.isTrigger = true;
				world.EnsureComponent<DrillableResourceAreaTag>();
				Drillable dr = world.EnsureComponent<Drillable>();
				dr.Start();
				dr.health[0] = DURATION;//harvestSpeedMultiplier;
				dr.primaryTooltip = locale.getField<string>("tooltip");
				dr.secondaryTooltip = locale.getField<string>("tooltipSecondary");
				dr.minResourcesToSpawn = 1;
				dr.maxResourcesToSpawn = 1;
				dr.deleteWhenDrilled = false;
				dr.kChanceToSpawnResources = 1;
				world.layer = LayerID.Useable;
				world.GetComponent<Rigidbody>().mass = 1000000;
				//dr.resources = new Drillable.ResourceType[0]; //DO NOT DO - breaks prawn drill
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
		
		public class DrillableResourceAreaTag : SpecialDrillable {
			
			private Drillable drill;
			private GameObject innerObject;
			private Rigidbody body;
			/*
			void OnDisable() {
				gameObject.SetActive(true);
			}
			
			void OnDestroy() {
				if (Player.main) {
					GameObject put = ObjectUtil.createWorldObject(GetComponent<PrefabIdentifier>().ClassId);
					put.transform.SetParent(transform.parent);
					put.transform.position = transform.position;
					put.transform.rotation = transform.rotation;
					put.transform.localScale = transform.localScale;
					SNUtil.log("Intercepted attempted delete of "+this+", spawning new one");
				}
			}*/
			
			void Update() {
				if (!body)
					body = GetComponent<Rigidbody>();
				body.isKinematic = true;
				body.constraints = RigidbodyConstraints.FreezeAll;
				if (!drill || !innerObject) {
					drill = gameObject.GetComponent<Drillable>();
					innerObject = drill.GetComponentsInChildren<MeshRenderer>(true)[0].gameObject;
				}
				if (drill.health[0] <= 0 || !innerObject.activeSelf) {
					drill.health[0] = DURATION;//harvestSpeedMultiplier;
					innerObject.SetActive(true);
				}
				gameObject.layer = LayerID.Useable;
			}
			
			public override bool allowAutomatedGrinding() {
				return false;
			}
			
			public override bool canBeMoved() {
				return false;
			}
			
		}
			
	}
}

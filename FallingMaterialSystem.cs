using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Auroresource;

namespace ReikaKalseki.Auroresource {
	
	public class FallingMaterialSystem {
		
		public static readonly FallingMaterialSystem instance = new FallingMaterialSystem();
		
		private static readonly SoundManager.SoundData entrySound = SoundManager.registerSound(AuroresourceMod.modDLL, "debrisentry", "Sounds/debris-entry.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 9999);}, SoundSystem.masterBus);
		internal static readonly SoundManager.SoundData splashSound = SoundManager.registerSound(AuroresourceMod.modDLL, "debrissplash", "Sounds/debris-splash.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 9999);}, SoundSystem.masterBus);
		
		private readonly WeightedRandom<TechType> items = new WeightedRandom<TechType>();
		
		private float nextReEntry = -1;
		
		private FallingMaterialSystem() {
			
		}
		
		public void addMaterial(TechType item, float weight) {
			items.addEntry(item, weight);
		}
		
		public void clear() {
			items.clear();
		}
		
		internal void tick(float time, float dT) {
			if (items.isEmpty())
				return;
			if (nextReEntry <= 0) {
				scheduleNextReEntry(time);
			}
			else if (time >= nextReEntry) {
				//spawnItem();
				queueSpawn();
				scheduleNextReEntry(time);
			}
		}
		
		internal void queueSpawn() {
			if (items.isEmpty())
				return;
			GameObject go = ObjectUtil.createWorldObject(AuroresourceMod.fallingMaterialSpawner.ClassID);
			go.transform.position = MathUtil.getRandomVectorAround(Vector3.zero, new Vector3(1500, 0, 1500)).setY(-2);
			go.EnsureComponent<FallingMaterialSpawnerTag>().timeLeft = UnityEngine.Random.Range(5F, 15F)*60*AuroresourceMod.config.getFloat(ARConfig.ConfigEntries.REENTRY_WARNING);
		}
		
		private void scheduleNextReEntry(float time) {
			nextReEntry = time+UnityEngine.Random.Range(20F, 60F)*60/AuroresourceMod.config.getFloat(ARConfig.ConfigEntries.REENTRY_RATE); //default every 20-60 min
		}
		
		internal void spawnItem() {
			spawnItem(MathUtil.getRandomVectorAround(Vector3.zero, new Vector3(1500, 0, 1500)));
		}
		
		internal void spawnItem(Vector3 pos) {
			if (items.isEmpty())
				return;
			GameObject go = ObjectUtil.createWorldObject(AuroresourceMod.fallingMaterial.ClassID);
			go.transform.position = MathUtil.getRandomVectorAround(pos, new Vector3(100, 0, 100)).setY(UnityEngine.Random.Range(500F, 1500F));
			foreach (ParticleSystem p in go.GetComponentsInChildren<ParticleSystem>())
				p.Play();
			GameObject item = ObjectUtil.createWorldObject(items.getRandomEntry());
			item.transform.SetParent(go.transform);
			item.transform.localPosition = Vector3.zero;
			go.GetComponent<FallingMaterialTag>().velocity = MathUtil.getRandomVectorAround(Vector3.zero, 10).setY(-24);
			SoundManager.playSoundAt(entrySound, go.transform.position, false, 9999);
		}
	}
	
	public class FallingMaterial : Spawnable {
		
		internal FallingMaterial() : base("FallingMaterial", "", "") {
			
		}
		
		public override GameObject GetGameObject() {
			GameObject go = new GameObject("FallingMaterial(Clone)");
			GameObject meteor = UnityEngine.Object.Instantiate(VFXSunbeam.main.burningChunkPrefabs[1]);
			meteor.transform.SetParent(go.transform);
			meteor.transform.localPosition = Vector3.zero;
			meteor.transform.rotation = Quaternion.Euler(90, UnityEngine.Random.Range(0F, 360F), 0);
			ObjectUtil.removeComponent<VFXStopAfterSeconds>(meteor);
			ObjectUtil.removeComponent<VFXFallingChunk>(meteor);
			go.EnsureComponent<PrefabIdentifier>().classId = ClassID;
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
			go.EnsureComponent<FallingMaterialTag>();
			return go;
		}
		
	}
	
	public class FallingMaterialSpawner : Spawnable {
		
		internal FallingMaterialSpawner(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<FallingMaterialSpawnerTag>().addField("timeLeft"));
			};
		}
		
		public override GameObject GetGameObject() {
			GameObject go = new GameObject("FallingMaterialSpawner(Clone)");
			PrefabIdentifier pi = go.EnsureComponent<PrefabIdentifier>();
			pi.classId = ClassID;
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
			go.EnsureComponent<FallingMaterialSpawnerTag>();
			ResourceTracker rt = go.EnsureComponent<ResourceTracker>();
			rt.techType = TechType;
			rt.overrideTechType = TechType;
			rt.prefabIdentifier = pi;
			rt.pickupable = null;
			rt.rb = null;
			return go;
		}
		
	}
	
	class FallingMaterialTag : MonoBehaviour {
		
		internal Vector3 velocity = Vector3.down*24;
		
		private bool isDestroyed;
		
		void Update() {
			if (Ocean.main.GetDepthOf(gameObject) > 1) {
				velocity *= 0.88F;
				if (!isDestroyed && velocity.magnitude < 0.25) {
					isDestroyed = true;
					foreach (ParticleSystem p in GetComponentsInChildren<ParticleSystem>())
						p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
					GetComponentInChildren<Pickupable>().transform.SetParent(null);
					UnityEngine.Object.Destroy(gameObject, 3);
					SoundManager.playSoundAt(FallingMaterialSystem.splashSound, transform.position, false, 9999);
				}
			}
			else {
				float dT = Time.deltaTime;
				transform.position = transform.position+velocity*dT;
				transform.up = -velocity.normalized;
				velocity += Vector3.down*dT*2;
			}
		}
		
	}
	
	class FallingMaterialSpawnerTag : MonoBehaviour {
		
		internal float timeLeft = -1;
		
		void Update() {
			if (timeLeft >= 0) {
				timeLeft -= Time.deltaTime;
				if (timeLeft <= 0) {
					FallingMaterialSystem.instance.spawnItem(transform.position);
				}
			}
		}
		
	}
	
}

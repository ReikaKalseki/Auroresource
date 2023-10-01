﻿using System;

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
		
		private static readonly Vector3 auroraPoint1 = new Vector3(746, 0, -362);
		private static readonly Vector3 auroraPoint2 = new Vector3(1295, 0, 110);
		private static readonly float auroraPointRadius = 275;
		private static readonly Vector3 mountainIslandPoint1 = new Vector3(360, 0, 1040);
		private static readonly Vector3 mountainIslandPoint2 = new Vector3(347, 0, 909);
		private static readonly float mountainIslandPointRadius = 80;
		private static readonly Vector3 floatingIslandCenter = new Vector3(-747, 0, -1061);
		private static readonly float floatingIslandRadius = 150;
		
		private readonly WeightedRandom<TechType> items = new WeightedRandom<TechType>();
    
	    internal FallingMaterial fallingMaterial;
	    internal FallingMaterialSpawner fallingMaterialSpawner;
		
		private SignalManager.ModSignal signal;
		
		private float nextReEntry = -1;
		
		private FallingMaterialSpawnerTag currentSpawner = null;
		private FallingMaterialCountdownTag countdown = null;
		
		private string timerText;
		
		private FallingMaterialSystem() {

		}
		
		internal void register() {
			XMLLocale.LocaleEntry e = AuroresourceMod.locale.getEntry("FallingMaterialSpawner");
			timerText = e.getField<string>("timer");
			
		    fallingMaterial = new FallingMaterial();
		    fallingMaterial.Patch();
		    fallingMaterialSpawner = new FallingMaterialSpawner(e);
		    fallingMaterialSpawner.Patch();
	    
			signal = SignalManager.createSignal(e);
			signal.register(null, /*SpriteManager.Get(SpriteManager.Group.Pings, "Sunbeam")*/TextureManager.getSprite(AuroresourceMod.modDLL, "Textures/impact-signal"), Vector3.zero);
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
			
			if (!countdown) {
				uGUI_SunbeamCountdown find = UnityEngine.Object.FindObjectOfType<uGUI_SunbeamCountdown>();
				if (find) {
					GameObject go2 = UnityEngine.Object.Instantiate(find.gameObject);
					countdown = go2.EnsureComponent<FallingMaterialCountdownTag>();
					uGUI_SunbeamCountdown gui = go2.GetComponent<uGUI_SunbeamCountdown>();
					countdown.timerText = gui.countdownText;
					countdown.titleText = gui.countdownTitle;
					countdown.titleText.text = timerText;
					countdown.holder = gui.countdownHolder;
					countdown.gameObject.name = "FallingMaterialCountdown";
					countdown.transform.SetParent(find.transform.parent);
					countdown.transform.position = find.transform.position;
					countdown.transform.rotation = find.transform.rotation;
					countdown.transform.localScale = find.transform.localScale;
					countdown.holder.transform.position = find.countdownHolder.transform.position;
					countdown.holder.transform.rotation = find.countdownHolder.transform.rotation;
					countdown.holder.transform.localScale = find.countdownHolder.transform.localScale;
					ObjectUtil.removeComponent<uGUI_SunbeamCountdown>(go2);
				}
			}
			
			if (countdown && currentSpawner)
				countdown.setTime(currentSpawner.timeLeft);
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
			if (currentSpawner)
				return;
			GameObject go = ObjectUtil.createWorldObject(fallingMaterialSpawner.ClassID);
			go.transform.position = selectRandomPosition();
			currentSpawner = go.EnsureComponent<FallingMaterialSpawnerTag>();
			currentSpawner.timeLeft = UnityEngine.Random.Range(5F, 15F)*60*AuroresourceMod.config.getFloat(ARConfig.ConfigEntries.REENTRY_WARNING);
			countdown.setTime(currentSpawner.timeLeft);
		}
		
		private Vector3 selectRandomPosition() {
			Vector3 sel = MathUtil.getRandomVectorAround(Vector3.zero, new Vector3(1500, 0, 1500));
			while (isCloseToExclusion(sel)) {
				sel = MathUtil.getRandomVectorAround(Vector3.zero, new Vector3(1500, 0, 1500));
			}
			return sel.setY(-2);
		}
		
		private bool isCloseToExclusion(Vector3 sel) {
			if (Vector3.Distance(sel, floatingIslandCenter) <= floatingIslandRadius)
				return true;
			if (MathUtil.getDistanceToLineSegment(sel, mountainIslandPoint1, mountainIslandPoint2) <= mountainIslandPointRadius)
				return true;
			if (MathUtil.getDistanceToLineSegment(sel, auroraPoint1, auroraPoint2) <= auroraPointRadius)
				return true;
			return false;
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
			GameObject go = ObjectUtil.createWorldObject(fallingMaterial.ClassID);
			go.transform.position = MathUtil.getRandomVectorAround(pos, new Vector3(100, 0, 100)).setY(UnityEngine.Random.Range(500F, 1500F));
			foreach (ParticleSystem p in go.GetComponentsInChildren<ParticleSystem>())
				p.Play();
			GameObject item = ObjectUtil.createWorldObject(items.getRandomEntry());
			item.transform.SetParent(go.transform);
			item.transform.localPosition = Vector3.zero;
			go.GetComponent<FallingMaterialTag>().velocity = MathUtil.getRandomVectorAround(Vector3.zero, 20).setY(-24);
			SoundManager.playSoundAt(entrySound, go.transform.position, false, 9999);
			UnityEngine.Object.Destroy(currentSpawner.gameObject);
			currentSpawner = null;
			countdown.holder.SetActive(false);
		}
	    
	    internal void modifyScannableList(uGUI_MapRoomScanner gui) {
			if (hasFinderUpgrade(gui)) {
				gui.availableTechTypes.Clear();
				gui.availableTechTypes.Add(fallingMaterialSpawner.TechType);
			}
			else {
				gui.availableTechTypes.Remove(fallingMaterialSpawner.TechType);
			}
		}
		
		internal void tickMapRoom(MapRoomFunctionality map) {
			//SNUtil.writeToChat("Tick map room "+map+" @ "+map.transform.position+" > "+map.scanActive+" & "+map.typeToScan+" & "+hasFinderUpgrade(map)+" OF "+currentSpawner);
			if (map.scanActive && map.typeToScan == fallingMaterialSpawner.TechType && hasFinderUpgrade(map)) {
				if (currentSpawner) {
					signal.move(currentSpawner.transform.position);
					signal.attachToObject(currentSpawner.gameObject);
					countdown.holder.SetActive(true);
				}
			}
		}
		
		internal bool hasFinderUpgrade(uGUI_MapRoomScanner gui) {
			return hasFinderUpgrade(gui.mapRoom);
		}
		
		internal bool hasFinderUpgrade(MapRoomFunctionality map) {
			return map.storageContainer.container.GetCount(AuroresourceMod.meteorDetector.TechType) > 0;
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
			/*
			ResourceTracker rt = go.EnsureComponent<ResourceTracker>();
			rt.techType = TechType;
			rt.overrideTechType = TechType;
			rt.prefabIdentifier = pi;
			rt.pickupable = null;
			rt.rb = null;
			*/
			return go;
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(AuroresourceMod.modDLL, "Textures/falling-material");
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
	
	class FallingMaterialCountdownTag : MonoBehaviour {
		
		private int currentTime;
		
		internal GameObject holder;
		internal Text titleText;
		internal Text timerText;
		
		internal void setTime(float time) {
			int t = (int)time;
			if (t != currentTime) {
				currentTime = t;
				TimeSpan ts = TimeSpan.FromSeconds(currentTime);
				timerText.text = string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
			}
		}
		
	}
	
}
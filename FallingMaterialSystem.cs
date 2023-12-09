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
		private static readonly SoundManager.SoundData alertSound = SoundManager.registerSound(AuroresourceMod.modDLL, "debrisalert", "Sounds/debris-alert.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 9999);}, SoundSystem.masterBus);
		internal static readonly SoundManager.SoundData splashSound = SoundManager.registerSound(AuroresourceMod.modDLL, "debrissplash", "Sounds/debris-splash.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 9999);}, SoundSystem.masterBus);
		
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
		
		public static event Action<GameObject, float> timerBeginEvent;
		public static event Action<GameObject> entryEvent;
		public static event Action<GameObject, Pickupable> impactEvent;
		
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
			if (DIHooks.getWorldAge() < 1)
				return;
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
			
			if (countdown) {
				if (currentSpawner)
					countdown.setTime(currentSpawner.timeLeft);
				else
					countdown.holder.SetActive(false);
			}
			if (nextReEntry <= 0) {
				scheduleNextReEntry(time);
			}
			else if (time >= nextReEntry && !VanillaBiomes.VOID.isInBiome(Player.main.transform.position)) {
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
			if (timerBeginEvent != null)
				timerBeginEvent.Invoke(currentSpawner.gameObject, currentSpawner.timeLeft);
		}
		
		private Vector3 selectRandomPosition() {
			Vector3 sel = MathUtil.getRandomVectorAround(Player.main.transform.position.setY(0), new Vector3(1200, 0, 1200));
			while (VanillaBiomes.VOID.isInBiome(sel.setY(-5)) || isCloseToExclusion(sel)) {
				sel = MathUtil.getRandomVectorAround(Player.main.transform.position.setY(0), new Vector3(1200, 0, 1200));
			}
			return sel.setY(-2);
		}
		
		private bool isCloseToExclusion(Vector3 sel) {
			if (Vector3.Distance(sel, floatingIslandCenter) <= floatingIslandRadius)
				return true;
			if (MathUtil.getDistanceToLineSegment(sel, mountainIslandPoint1, mountainIslandPoint2) <= mountainIslandPointRadius)
				return true;
			if (WorldUtil.isInsideAurora2D(sel))
				return true;
			return false;
		}
		
		public float getTimeUntilNextEntry() {
			return nextReEntry-DayNightCycle.main.timePassedAsFloat;
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
			if (!go)
				return;
			go.transform.position = MathUtil.getRandomVectorAround(pos, new Vector3(100, 0, 100)).setY(UnityEngine.Random.Range(500F, 1500F));
			foreach (ParticleSystem p in go.GetComponentsInChildren<ParticleSystem>())
				p.Play();
			GameObject item = ObjectUtil.createWorldObject(items.getRandomEntry());
			if (!item)
				return;
			item.transform.SetParent(go.transform);
			item.transform.localPosition = Vector3.zero;
			FallingMaterialTag tag = go.EnsureComponent<FallingMaterialTag>();
			tag.velocity = MathUtil.getRandomVectorAround(Vector3.zero, 20).setY(-24);
			if (Player.main.transform.position.y >= -50)
				SoundManager.playSoundAt(entrySound, go.transform.position, false, 9999);
			signal.deactivate();
			if (currentSpawner)
				UnityEngine.Object.Destroy(currentSpawner.gameObject);
			currentSpawner = null;
			countdown.holder.SetActive(false);
			if (entryEvent != null)
				entryEvent.Invoke(go);
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
					signal.activate();
					if (!countdown.holder.activeSelf)
						SoundManager.playSoundAt(alertSound, currentSpawner.transform.position, false, 9999);
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
		
		internal void impact(FallingMaterialTag tag, Pickupable pp) {
			if (impactEvent != null)
				impactEvent.Invoke(tag.gameObject, pp);
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
					Pickupable pp = GetComponentInChildren<Pickupable>();
					pp.transform.SetParent(null);
					UnityEngine.Object.Destroy(gameObject, 3);
					if (Player.main.transform.position.y >= -100 || Vector3.Distance(Player.main.transform.position, transform.position) <= 200) {
						float vol = (float)Math.Min(MathUtil.linterpolate(-Player.main.transform.position.y, 50, 200, 1, 0, true), MathUtil.linterpolate(Vector3.Distance(Player.main.transform.position, transform.position), 200, 350, 1, 0, true));
						SoundManager.playSoundAt(FallingMaterialSystem.splashSound, transform.position, false, 9999, vol);
					}
					FallingMaterialSystem.instance.impact(this, pp);
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
					UnityEngine.Object.Destroy(gameObject);
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

using System;
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.IO;    //For data read/write methods
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using ReikaKalseki.DIAlterra;

using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.Auroresource {

	static class ARPatches {

		[HarmonyPatch(typeof(Drillable))]
		[HarmonyPatch("SpawnLoot")]
		public static class DrillableDropsHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Drillable", "ChooseRandomResource", true, new Type[0]);
					codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.Auroresource.ARHooks", "getDrillableDrop", false, typeof(Drillable));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(BreakableResource))]
		[HarmonyPatch("Awake")]
		public static class ReefbackGrowthIncrease {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.Auroresource.ARHooks", "onBreakableResourceSpawn", false, typeof(BreakableResource)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(MapRoomFunctionality))]
		[HarmonyPatch("Start")]
		public static class MapRoomModuleAddHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.Auroresource.ARHooks", "onMapRoomSpawn", false, typeof(MapRoomFunctionality)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Geyser))]
		[HarmonyPatch("Start")]
		public static class GeyserSpawn {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.Auroresource.ARHooks", "onGeyserSpawn", false, typeof(Geyser)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

	}
}

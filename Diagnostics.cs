using HarmonyLib; 
using Il2Cpp;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine;

namespace MindfulNeighborhood
{
   
    [HarmonyPatch(typeof(WoodStove), "Awake")]
    internal static class WoodStoveAwakeDiagnostic
    {
        [HarmonyPriority(Priority.First)]
        static void Prefix(WoodStove __instance)
        {
            GameObject gi = __instance.gameObject;
            if (!gi.name.Contains("INTERACTIVE_FirePlace"))
                return;

            Log.Msg($"[FFP-DIAG] >>> Awake ENTER '{gi.name}' id={gi.GetInstanceID()} scene='{gi.scene.name}' frame={Time.frameCount}");
            LogState(gi, "PRE-AWAKE");
        }

        [HarmonyPriority(Priority.Last)]
        static void Postfix(WoodStove __instance)
        {
            GameObject gi = __instance.gameObject;
            if (!gi.name.Contains("INTERACTIVE_FirePlace"))
                return;

            LogState(gi, "POST-AWAKE(all patches)");
            Log.Msg($"[FFP-DIAG] <<< Awake EXIT  '{gi.name}' id={gi.GetInstanceID()} frame={Time.frameCount}");
        }

        private static void LogState(GameObject gi, string tag)
        {
            try
            {
                CookingSlot[] slots = gi.GetComponentsInChildren<CookingSlot>(true);
                Log.Msg($"[FFP-DIAG] [{tag}] '{gi.name}' CookingSlot count = {slots.Length}");
                for (int i = 0; i < slots.Length; i++)
                {
                    Transform t = slots[i].transform;
                    Log.Msg($"[FFP-DIAG]   slot[{i}] '{t.name}' id={slots[i].GetInstanceID()} localPos={t.localPosition} parent='{t.parent?.name}'");
                }

                GearPlacePoint[] gpp = gi.GetComponentsInChildren<GearPlacePoint>(true);
                Log.Msg($"[FFP-DIAG] [{tag}] '{gi.name}' GearPlacePoint count = {gpp.Length}");
                for (int i = 0; i < gpp.Length; i++)
                {
                    Transform t = gpp[i].transform;
                    Log.Msg($"[FFP-DIAG]   gpp[{i}] '{t.name}' id={gpp[i].GetInstanceID()} localPos={t.localPosition} parent='{t.parent?.name}'");
                }

                Transform placePoints = gi.transform.Find("PlacePoints");
                if (placePoints != null)
                {
                    Log.Msg($"[FFP-DIAG] [{tag}] '{gi.name}' PlacePoints childCount = {placePoints.childCount}");
                    for (int i = 0; i < placePoints.childCount; i++)
                    {
                        Transform c = placePoints.GetChild(i);
                        Log.Msg($"[FFP-DIAG]     PlacePoints/child[{i}] = '{c.name}' localPos={c.localPosition}");
                    }
                }
                else
                {
                    Log.Warning($"[FFP-DIAG] [{tag}] '{gi.name}' has no 'PlacePoints' child.");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[FFP-DIAG] [{tag}] exception while logging state on '{gi.name}': {ex}");
            }
        }
    }
}
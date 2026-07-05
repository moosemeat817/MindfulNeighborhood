using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2Cpp;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine;

namespace MindfulNeighborhood
{
    public class Clones : MonoBehaviour
    {
       
        private const float FireplaceRotationCorrectionDegrees = 11f;

        // Cooking slot tuning - previously exposed as mod-settings sliders, now locked in at
        // the values dialed in visually in-game.
        private const float CookingSlotSpacingScale = 0.6994298f;
        private const float CookingSlotOffsetX = 0.15429084f;
        private const float CookingSlotOffsetZ = 0.20290215f;

        public static string[,] itemDataArray =
        {
            {"0_Scene", "1_Path"},

            {"AirfieldRegion", "INTERACTIVE_FirePlace"},
            {"AirfieldRegion", "TRN_Icicle_E_Prefab (2)"},
            {"AirfieldRegion", "OBJ_SmallHutA_Prefab"},
            {"AirfieldRegion_SANDBOX", "TRIGGER_FishingHolePlacementFresh_sphere"},
            {"AirfieldRegion", "STR_CampTrailerBBurnt_Collidable_Prefab"},
            {"AirfieldRegion", "SPWNR_SecurityHutA_Prefab"},
            {"AirfieldRegion", "STRSPAWN_HuntersBlind_Prefab"},
            {"AirfieldRegion", "OBJ_PicnicBenchC_Prefab"},
            {"AirfieldRegion", "OBJ_PicnicBenchB_Prefab"},
            {"AirfieldRegion", "OBJ_FenceWoodB1_Prefab (18)"},

        };

        public static void ChangeObjects()
        {
            for (int i = 1; i < itemDataArray.GetLength(0); i++)
            {
                GameObject findTargetGO = FindGameObjectByPath(itemDataArray[i, 1], itemDataArray[i, 0]);
                if (findTargetGO == null)
                    continue;

                string sceneName = itemDataArray[i, 0];
                string objectPath = itemDataArray[i, 1];

                switch (sceneName)
                {
                    case "AirfieldRegion":
                        HandleAirfieldRegion(findTargetGO, objectPath);
                        break;

                    case "AirfieldRegion_SANDBOX":
                        HandleAirfieldRegion_SANDBOX(findTargetGO, objectPath);
                        break;


                }
            }
        }

        /// <summary>
        /// Finds a GameObject by its hierarchical path or name within a specific scene
        /// </summary>
        /// <param name="path">Either a simple name or hierarchical path like "Parent/Child/Target"</param>
        /// <param name="sceneName">Name of the scene to search in</param>
        /// <returns>The found GameObject or null if not found</returns>
        private static GameObject FindGameObjectByPath(string path, string sceneName)
        {
            // If path doesn't contain '/', treat it as a simple name search
            if (!path.Contains("/"))
            {
                GameObject simpleFind = GameObject.Find(path);
                if (simpleFind != null && simpleFind.scene.name == sceneName)
                    return simpleFind;
                return null;
            }

            // Get the scene and validate it
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);

            // Check if the scene is valid and loaded
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogWarning($"Scene '{sceneName}' is not valid or not loaded yet.");
                return null;
            }

            // Split the path into segments
            string[] pathSegments = path.Split('/');

            // Get root objects from the validated scene
            GameObject[] rootObjects;
            try
            {
                rootObjects = scene.GetRootGameObjects();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to get root objects from scene '{sceneName}': {ex.Message}");
                return null;
            }

            foreach (GameObject rootObj in rootObjects)
            {
                GameObject result = SearchInHierarchy(rootObj, pathSegments, 0);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Recursively searches through the GameObject hierarchy to find the target path
        /// </summary>
        /// <param name="current">Current GameObject being examined</param>
        /// <param name="pathSegments">Array of path segments to match</param>
        /// <param name="currentIndex">Current index in the path segments</param>
        /// <returns>The found GameObject or null if not found</returns>
        private static GameObject SearchInHierarchy(GameObject current, string[] pathSegments, int currentIndex)
        {
            // If we've reached the end of the path and the current object matches, we found it
            if (currentIndex >= pathSegments.Length)
                return current;

            // Check if current GameObject name matches the current path segment
            if (current.name == pathSegments[currentIndex])
            {
                // If this is the last segment, we found our target
                if (currentIndex == pathSegments.Length - 1)
                    return current;

                // Otherwise, search children for the next segment
                for (int i = 0; i < current.transform.childCount; i++)
                {
                    GameObject child = current.transform.GetChild(i).gameObject;
                    GameObject result = SearchInHierarchy(child, pathSegments, currentIndex + 1);
                    if (result != null)
                        return result;
                }
            }
            else
            {
                // Current name doesn't match, but continue searching children
                // This allows for partial path matching (e.g., starting from any level)
                for (int i = 0; i < current.transform.childCount; i++)
                {
                    GameObject child = current.transform.GetChild(i).gameObject;
                    GameObject result = SearchInHierarchy(child, pathSegments, currentIndex);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Extracts the object name from a path (the last segment)
        /// </summary>
        /// <param name="path">Full path or simple name</param>
        /// <returns>The object name</returns>
        private static string GetObjectNameFromPath(string path)
        {
            if (!path.Contains("/"))
                return path;

            string[] segments = path.Split('/');
            return segments[segments.Length - 1];
        }

        private static void HandleAirfieldRegion(GameObject original, string objectPath)
        {
            if (!Settings.options.forsakenFireplaces)
                return;

            string objectName = GetObjectNameFromPath(objectPath);

            switch (objectName)
            {
                case "INTERACTIVE_FirePlace":
                    Log.Msg($"[FFP-DIAG][Clones] HandleAirfieldRegion: original '{original.name}' id={original.GetInstanceID()}");
                    LogFireplaceState(original, "ORIGINAL (before any clone made)");

                    CreateCloneIfNotExists(original, "INTERACTIVE_FirePlace(Clone)",
                        new Vector3(-737.7227f, 295.0449f, 1132.773f),
                        Quaternion.Euler(-0, 32.8549f + FireplaceRotationCorrectionDegrees, 0),
                        new Vector3(1f, 1f, 1f),
                        postSetup: clone =>
                        {
                            NormalizeCookingSlotLayout(original, clone);
                            LogFireplaceState(clone, "POST-NORMALIZE (clone INTERACTIVE_FirePlace(Clone))");
                        });
                    break;


                case "TRN_Icicle_E_Prefab (2)":
                    Log.Msg($"[FFP-DIAG][Clones] HandleAirfieldRegion: original '{original.name}' id={original.GetInstanceID()}");

                    // Straightforward clone - no cooking slots involved, so no postSetup needed.
                    CreateCloneIfNotExists(original, "TRN_Icicle_E_Prefab(Clone)",
                        new Vector3(-737.6559f, 300.8475f, 1133.36f),
                        Quaternion.Euler(339.9999f, 234.173f, 0),
                        new Vector3(3.2236f, 1.5144f, 5.5144f));
                    break;


                case "OBJ_SmallHutA_Prefab":
                    Log.Msg($"[FFP-DIAG][Clones] HandleAirfieldRegion: original '{original.name}' id={original.GetInstanceID()}");

                    // Straightforward clone - no cooking slots involved, so no postSetup needed.
                    CreateCloneIfNotExists(original, "OBJ_SmallHutA_Prefab(Clone)",
                        new Vector3(-807.8589f, 275.981f, 1167.855f),
                        Quaternion.Euler(-0, 218.8813f, 0),
                        new Vector3(1f, 1f, 1f));

                    break;



                case "STR_CampTrailerBBurnt_Collidable_Prefab":
                    Log.Msg($"[FFP-DIAG][Clones] HandleAirfieldRegion: original '{original.name}' id={original.GetInstanceID()}");

                    // Straightforward clone - no cooking slots involved, so no postSetup needed.
                    CreateCloneIfNotExists(original, "STR_CampTrailerBBurnt_Collidable_Prefab(Clone)",
                        new Vector3(1009.428f, 287.2994f, -952.2245f),
                        Quaternion.Euler(-0, 351.4053f, 0),
                        new Vector3(1f, 1f, 1f));
                    break;


                case "SPWNR_SecurityHutA_Prefab":
                    Log.Msg($"[FFP-DIAG][Clones] HandleAirfieldRegion: original '{original.name}' id={original.GetInstanceID()}");

                    // Straightforward clone - no cooking slots involved, so no postSetup needed.
                    CreateCloneIfNotExists(original, "SPWNR_SecurityHutA_Prefab(Clone)",
                        new Vector3(-733.6689f, 188.6262f, 912.7291f),
                        Quaternion.Euler(-0, 92.9053f, 0),
                        new Vector3(1.3f, 1f, 1.1f));
                    break;



                case "STRSPAWN_HuntersBlind_Prefab":
                    Log.Msg($"[FFP-DIAG][Clones] HandleAirfieldRegion: original '{original.name}' id={original.GetInstanceID()}");

                    // Straightforward clone - no cooking slots involved, so no postSetup needed.
                    CreateCloneIfNotExists(original, "STRSPAWN_HuntersBlind_Prefab(Clone)",
                        new Vector3(-1035.759f, 284.6987f, 1216.091f),
                        Quaternion.Euler(-0, 134.3691f, 0),
                        new Vector3(1.3f, 1f, 1.1f));
                    break;


                case "OBJ_PicnicBenchC_Prefab":
                    Log.Msg($"[FFP-DIAG][Clones] HandleAirfieldRegion: original '{original.name}' id={original.GetInstanceID()}");

                    // Straightforward clone - no cooking slots involved, so no postSetup needed.
                    CreateCloneIfNotExists(original, "OBJ_PicnicBenchC_Prefab(Clone)",
                        new Vector3(-842.0684f, 276.501f, 1210.307f),
                        Quaternion.Euler(-0, 61.2164f, 0),
                        new Vector3(1f, 1f, 1f));
                    break;



                case "OBJ_PicnicBenchB_Prefab":
                    Log.Msg($"[FFP-DIAG][Clones] HandleAirfieldRegion: original '{original.name}' id={original.GetInstanceID()}");

                    // Straightforward clone - no cooking slots involved, so no postSetup needed.
                    CreateCloneIfNotExists(original, "OBJ_PicnicBenchB_Prefab(Clone)",
                        new Vector3(-838.9617f, 276.4711f, 1208.13f),
                        Quaternion.Euler(-0, 184.8385f, 0),
                        new Vector3(1f, 1f, 1f));
                    break;




                case "OBJ_FenceWoodB1_Prefab (18)":
                    Log.Msg($"[FFP-DIAG][Clones] HandleAirfieldRegion: original '{original.name}' id={original.GetInstanceID()}");

                    // Straightforward clone - no cooking slots involved, so no postSetup needed.
                    CreateCloneIfNotExists(original, "OBJ_FenceWoodB1_Prefab (18)(Clone)",
                        new Vector3(-930.9295f, 284.6283f, 1276.9f),
                        Quaternion.Euler(-0, 154.33f, 0),
                        new Vector3(1f, 1f, 1.2f));


                    CreateCloneIfNotExists(original, "OBJ_FenceWoodB1_Prefab (18)2(Clone)",
                        new Vector3(-929.4294f, 284.6283f, 1273.4f),
                        Quaternion.Euler(12f, 154.33f, 6),
                        new Vector3(1f, 1f, 1.2f));


                    CreateCloneIfNotExists(original, "OBJ_FenceWoodB1_Prefab (18)3(Clone)",
                        new Vector3(-930.9295f, 284.6283f, 1276.9f),
                        Quaternion.Euler(5f, 241.33f, 0),
                        new Vector3(1f, 1f, 1.2f));


                    CreateCloneIfNotExists(original, "OBJ_FenceWoodB1_Prefab (18)4(Clone)",
                        new Vector3(-934.3294f, 284.2283f, 1275.4f),
                        Quaternion.Euler(10f, 241.3299f, 10),
                        new Vector3(1f, 1f, 1.2f));






                    CreateCloneIfNotExists(original, "OBJ_FenceWoodB1_Prefab (18)5(Clone)",
                        new Vector3(-937.7293f, 283.8283f, 1273.4f),
                        Quaternion.Euler(17, 241.3299f, 19),
                        new Vector3(1f, 1f, 1.2f));


                    CreateCloneIfNotExists(original, "OBJ_FenceWoodB1_Prefab (18)6(Clone)",
                        new Vector3(-944.711f, 284.2283f, 1267.4f),
                        Quaternion.Euler(21, 138.9661f, 347),
                        new Vector3(1f, 1f, 1.2f));


                    CreateCloneIfNotExists(original, "OBJ_FenceWoodB1_Prefab (18)7(Clone)",
                        new Vector3(-939.0109f, 283.4283f, 1260.2f),
                        Quaternion.Euler(13.0004f, 315.257f, 16.9999f),
                        new Vector3(1f, 1f, 1.2f));

                    break;
            }
        }


        private static void HandleAirfieldRegion_SANDBOX(GameObject original, string objectPath)
        {
            if (!Settings.options.forsakenFireplaces)
                return;

            string objectName = GetObjectNameFromPath(objectPath);

            switch (objectName)
            {


                case "TRIGGER_FishingHolePlacementFresh_sphere":
                    Log.Msg($"[FFP-DIAG][Clones] HandleAirfieldRegion: original '{original.name}' id={original.GetInstanceID()}");

                    // Straightforward clone - no cooking slots involved, so no postSetup needed.
                    CreateCloneIfNotExists(original, "TRIGGER_FishingHolePlacementFresh_sphere(Clone)",
                        new Vector3(-529.8676f, 296.215f, 1350.446f),
                        Quaternion.Euler(-0, 321.7562f, 0),
                        new Vector3(15f, 15f, 15f));
                    break;

            }
        }

       
        private static void CreateCloneIfNotExists(GameObject original, string cloneName, Vector3 position,
            Quaternion rotation, Vector3? scale = null, Action<GameObject> postSetup = null)
        {
            if (GameObject.Find(cloneName) != null)
            {
                Log.Msg($"[FFP-DIAG][Clones] '{cloneName}' already exists, skipping.");
                return;
            }

            Log.Msg($"[FFP-DIAG][Clones] PRE-INSTANTIATE state of source '{original.name}' id={original.GetInstanceID()} (about to become '{cloneName}')");

            GameObject clone = Instantiate(original, position, rotation);
            clone.name = cloneName;

           
            Log.Msg($"[FFP-DIAG][Clones] POST-INSTANTIATE state of clone '{clone.name}' id={clone.GetInstanceID()}");

            if (scale.HasValue)
                clone.transform.localScale = scale.Value;

            

            postSetup?.Invoke(clone);
        }

       

        private static void NormalizeCookingSlotLayout(GameObject original, GameObject clone)
        {
            CookingSlot[] origSlots = original.GetComponentsInChildren<CookingSlot>(true);
            CookingSlot[] cloneSlots = clone.GetComponentsInChildren<CookingSlot>(true);

            if (cloneSlots.Length <= origSlots.Length)
            {
                Log.Msg($"[FFP][Clones] NormalizeCookingSlotLayout: '{clone.name}' has {cloneSlots.Length} CookingSlot(s), not more than the original's {origSlots.Length}; leaving untouched.");
                return;
            }

            Dictionary<string, CookingSlot> origByName = new Dictionary<string, CookingSlot>();
            foreach (CookingSlot s in origSlots)
            {
                origByName[s.gameObject.name] = s;
            }

            Dictionary<string, List<CookingSlot>> cloneGroups = new Dictionary<string, List<CookingSlot>>();
            foreach (CookingSlot s in cloneSlots)
            {
                string n = s.gameObject.name;
                if (!cloneGroups.ContainsKey(n))
                    cloneGroups[n] = new List<CookingSlot>();
                cloneGroups[n].Add(s);
            }

            
            float rowSlotY = origSlots[0].transform.localPosition.y;
            float rowSlotZ = origSlots[0].transform.localPosition.z;
            GearPlacePoint sampleGpp = origSlots[0].m_GearPlacePoint;
            float rowGppY = sampleGpp != null ? sampleGpp.transform.localPosition.y : rowSlotY - 0.01f;
            float rowGppZ = sampleGpp != null ? sampleGpp.transform.localPosition.z : rowSlotZ - 0.01f;

            List<float> sortedOrigX = new List<float>();
            foreach (CookingSlot s in origSlots)
                sortedOrigX.Add(s.transform.localPosition.x);
            sortedOrigX.Sort();

            float avgSpacing = 0.3f; 
            if (sortedOrigX.Count > 1)
            {
                float totalGap = sortedOrigX[sortedOrigX.Count - 1] - sortedOrigX[0];
                avgSpacing = totalGap / (sortedOrigX.Count - 1);
            }

            
            float lowX = sortedOrigX[0];
            float highX = sortedOrigX[sortedOrigX.Count - 1];

            Log.Msg($"[FFP][Clones] Normalizing '{clone.name}': {cloneSlots.Length} clone slot(s) across {cloneGroups.Count} name group(s), vs {origSlots.Length} on original. rowY={rowSlotY:F3} rowZ={rowSlotZ:F3} spacing={avgSpacing:F3}");

            List<CookingSlot> finalSlots = new List<CookingSlot>();

            foreach (KeyValuePair<string, List<CookingSlot>> kvp in cloneGroups)
            {
                string name = kvp.Key;
                List<CookingSlot> group = kvp.Value;

                if (!origByName.TryGetValue(name, out CookingSlot origSlot))
                {
                    Log.Warning($"[FFP][Clones]   '{name}' has no matching slot on the original fireplace; leaving {group.Count} instance(s) untouched (unexpected name).");
                    finalSlots.AddRange(group);
                    continue;
                }

                Vector3 targetSlotPos = origSlot.transform.localPosition;
                GearPlacePoint origGpp = origSlot.m_GearPlacePoint;
                bool hasTargetGpp = origGpp != null;
                Vector3 targetGppPos = hasTargetGpp ? origGpp.transform.localPosition : Vector3.zero;

                
                group.Sort((a, b) =>
                    Vector3.Distance(a.transform.localPosition, targetSlotPos)
                        .CompareTo(Vector3.Distance(b.transform.localPosition, targetSlotPos)));

                CookingSlot keeper = group[0];
                Vector3 oldSlotPos = keeper.transform.localPosition;
                keeper.transform.localPosition = targetSlotPos;

                if (hasTargetGpp && keeper.m_GearPlacePoint != null)
                {
                    Vector3 oldGppPos = keeper.m_GearPlacePoint.transform.localPosition;
                    keeper.m_GearPlacePoint.transform.localPosition = targetGppPos;
                    Log.Msg($"[FFP][Clones]   '{name}' slot {oldSlotPos} -> {targetSlotPos}; point '{keeper.m_GearPlacePoint.gameObject.name}' {oldGppPos} -> {targetGppPos}");
                }
                else
                {
                    Log.Msg($"[FFP][Clones]   '{name}' slot {oldSlotPos} -> {targetSlotPos} (no linked GearPlacePoint to move).");
                }

                finalSlots.Add(keeper);

                for (int i = 1; i < group.Count; i++)
                {
                    CookingSlot extra = group[i];

                    
                    float currentX = extra.transform.localPosition.x;
                    bool extendLow = Mathf.Abs(currentX - lowX) <= Mathf.Abs(currentX - highX);
                    float newX = extendLow ? lowX - avgSpacing : highX + avgSpacing;
                    if (extendLow) lowX = newX; else highX = newX;

                    Vector3 oldExtraPos = extra.transform.localPosition;
                    Vector3 newExtraPos = new Vector3(newX, rowSlotY, rowSlotZ);
                    extra.transform.localPosition = newExtraPos;

                    GearPlacePoint extraGpp = extra.m_GearPlacePoint;
                    if (extraGpp != null)
                    {
                        Vector3 oldExtraGppPos = extraGpp.transform.localPosition;
                        Vector3 newExtraGppPos = new Vector3(newX, rowGppY, rowGppZ);
                        extraGpp.transform.localPosition = newExtraGppPos;
                        Log.Msg($"[FFP][Clones]   Extending row for extra duplicate '{extra.gameObject.name}' (id={extra.GetInstanceID()}): slot {oldExtraPos} -> {newExtraPos}; point '{extraGpp.gameObject.name}' {oldExtraGppPos} -> {newExtraGppPos}");
                    }
                    else
                    {
                        Log.Warning($"[FFP][Clones]   Extending row for extra duplicate '{extra.gameObject.name}' (id={extra.GetInstanceID()}): slot {oldExtraPos} -> {newExtraPos} (no linked GearPlacePoint to move).");
                    }

                    finalSlots.Add(extra);
                }
            }

  
            if (finalSlots.Count > 1)
            {
                float measuredWidth = -1f;
                foreach (CookingSlot s in finalSlots)
                {
                    measuredWidth = MeasureSlotWidthX(s);
                    if (measuredWidth > 0f)
                        break;
                }

                
                float rawSpacing = measuredWidth > 0f ? measuredWidth : avgSpacing;
                float uniformSpacing = rawSpacing * CookingSlotSpacingScale;
                if (measuredWidth > 0f)
                    Log.Msg($"[FFP][Clones]   Measured slot width = {rawSpacing:F3} (from renderer/collider/mesh bounds); scale = {CookingSlotSpacingScale:F3}; applied spacing = {uniformSpacing:F3}.");
                else
                    Log.Warning($"[FFP][Clones]   Could not measure slot mesh width on any of {finalSlots.Count} slot(s); falling back to the original's averaged gap ({rawSpacing:F3}) * scale ({CookingSlotSpacingScale:F3}) = {uniformSpacing:F3}.");

                finalSlots.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));

                float centerX = (finalSlots[0].transform.localPosition.x + finalSlots[finalSlots.Count - 1].transform.localPosition.x) / 2f;
                float startX = centerX - uniformSpacing * (finalSlots.Count - 1) / 2f;

                for (int i = 0; i < finalSlots.Count; i++)
                {
                    CookingSlot s = finalSlots[i];
                    float newX = startX + uniformSpacing * i;
                    float deltaX = newX - s.transform.localPosition.x;

                    Vector3 oldPos = s.transform.localPosition;
                    s.transform.localPosition = new Vector3(newX, rowSlotY, rowSlotZ);

                    GearPlacePoint linkedGpp = s.m_GearPlacePoint;
                    if (linkedGpp != null)
                    {
                        Vector3 oldGppPos = linkedGpp.transform.localPosition;
                        Vector3 newGppPos = oldGppPos + new Vector3(deltaX, 0f, 0f);
                        linkedGpp.transform.localPosition = newGppPos;
                        Log.Msg($"[FFP][Clones]   Re-leveled '{s.gameObject.name}' slot {oldPos} -> {s.transform.localPosition}; point '{linkedGpp.gameObject.name}' {oldGppPos} -> {newGppPos}");
                    }
                    else
                    {
                        Log.Msg($"[FFP][Clones]   Re-leveled '{s.gameObject.name}' slot {oldPos} -> {s.transform.localPosition} (no linked GearPlacePoint to move).");
                    }
                }

           
                float offsetX = CookingSlotOffsetX;
                float offsetZ = CookingSlotOffsetZ;
                if (offsetX != 0f || offsetZ != 0f)
                {
                    Log.Msg($"[FFP][Clones]   Shifting whole row by offsetX={offsetX:F3}, offsetZ={offsetZ:F3}.");
                    foreach (CookingSlot s in finalSlots)
                    {
                        Vector3 oldPos = s.transform.localPosition;
                        Vector3 newPos = oldPos + new Vector3(offsetX, 0f, offsetZ);
                        s.transform.localPosition = newPos;

                        GearPlacePoint linkedGpp = s.m_GearPlacePoint;
                        if (linkedGpp != null)
                        {
                            Vector3 oldGppPos = linkedGpp.transform.localPosition;
                            Vector3 newGppPos = oldGppPos + new Vector3(offsetX, 0f, offsetZ);
                            linkedGpp.transform.localPosition = newGppPos;
                            Log.Msg($"[FFP][Clones]     Shifted '{s.gameObject.name}' slot {oldPos} -> {newPos}; point '{linkedGpp.gameObject.name}' {oldGppPos} -> {newGppPos}");
                        }
                        else
                        {
                            Log.Msg($"[FFP][Clones]     Shifted '{s.gameObject.name}' slot {oldPos} -> {newPos} (no linked GearPlacePoint to move).");
                        }
                    }
                }
            }

            WoodStove stove = clone.GetComponent<WoodStove>();
            if (stove != null)
            {
                stove.m_CookingSlots = finalSlots.ToArray();
                Log.Msg($"[FFP][Clones] Refreshed '{clone.name}' WoodStove.m_CookingSlots to {finalSlots.Count} entr{(finalSlots.Count == 1 ? "y" : "ies")}.");
            }
            else
            {
                Log.Warning($"[FFP][Clones] '{clone.name}' has no WoodStove component; could not refresh m_CookingSlots.");
            }
        }

  
        private static float MeasureSlotWidthX(CookingSlot sample)
        {
            Renderer rend = sample.GetComponent<Renderer>();
            if (rend != null)
                return rend.bounds.size.x;

            Collider col = sample.GetComponent<Collider>();
            if (col != null)
                return col.bounds.size.x;

            MeshFilter mf = sample.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
                return mf.sharedMesh.bounds.size.x * sample.transform.lossyScale.x;

            return -1f;
        }

     
        private static void LogFireplaceState(GameObject go, string label)
        {
            try
            {
                CookingSlot[] slots = go.GetComponentsInChildren<CookingSlot>(true);
                Log.Msg($"[FFP-DIAG][Clones]   [{label}] CookingSlot count = {slots.Length}");
                for (int i = 0; i < slots.Length; i++)
                {
                    Transform t = slots[i].transform;
                    Log.Msg($"[FFP-DIAG][Clones]     slot[{i}] '{t.name}' id={slots[i].GetInstanceID()} localPos={t.localPosition} worldPos={t.position} parent='{t.parent?.name}'");
                }

                GearPlacePoint[] gpp = go.GetComponentsInChildren<GearPlacePoint>(true);
                Log.Msg($"[FFP-DIAG][Clones]   [{label}] GearPlacePoint count = {gpp.Length}");
                for (int i = 0; i < gpp.Length; i++)
                {
                    Transform t = gpp[i].transform;
                    Log.Msg($"[FFP-DIAG][Clones]     gpp[{i}] '{t.name}' id={gpp[i].GetInstanceID()} localPos={t.localPosition} worldPos={t.position} parent='{t.parent?.name}'");
                }

                Log.Msg($"[FFP-DIAG][Clones]   [{label}] Slot -> GearPlacePoint pairings:");
                for (int i = 0; i < slots.Length; i++)
                {
                    CookingSlot s = slots[i];
                    GearPlacePoint linked = s.m_GearPlacePoint;
                    if (linked == null)
                    {
                        Log.Msg($"[FFP-DIAG][Clones]     slot[{i}] '{s.gameObject.name}' id={s.GetInstanceID()} -> m_GearPlacePoint = NULL");
                        continue;
                    }
                    Vector3 delta = linked.transform.position - s.transform.position;
                    Log.Msg($"[FFP-DIAG][Clones]     slot[{i}] '{s.gameObject.name}' id={s.GetInstanceID()} worldPos={s.transform.position} -> point '{linked.gameObject.name}' id={linked.GetInstanceID()} worldPos={linked.transform.position} delta={delta}");
                }

                Transform placePoints = go.transform.Find("PlacePoints");
                if (placePoints != null)
                {
                    Log.Msg($"[FFP-DIAG][Clones]   [{label}] PlacePoints childCount = {placePoints.childCount}");
                    for (int i = 0; i < placePoints.childCount; i++)
                    {
                        Transform c = placePoints.GetChild(i);
                        Log.Msg($"[FFP-DIAG][Clones]       PlacePoints/child[{i}] = '{c.name}' localPos={c.localPosition}");
                    }
                }
                else
                {
                    Log.Warning($"[FFP-DIAG][Clones]   [{label}] no 'PlacePoints' child found under '{go.name}'.");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[FFP-DIAG][Clones]   [{label}] exception logging state on '{go.name}': {ex}");
            }
        }
    }
}
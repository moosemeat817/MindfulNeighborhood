using MelonLoader.Utils;
using UnityEngine.AddressableAssets;
using Il2CppSystem;
using UnityEngine.UIElements;
using UnityEngine;
using Il2Cpp;
using MelonLoader;
using System.Collections;

namespace MindfulNeighborhood
{
    public class MindfulNeighborhoodManager
    {
        public void PlaceTerrain()
        {
            string scene = GameManager.m_ActiveScene;


        }

        public IEnumerator PlaceAssetsAsync()
        {
            string scene = GameManager.m_ActiveScene;

            Log.Msg($"****************************** PlaceAssetsAsync");


            if (scene == "AirfieldRegion" && Settings.options.mindfulNeighborhood)
            {
                Log.Msg($"****************************** coastalfireplaces");


                yield return PlaceAssetAsync("STR_CoastalHouseCFirePlace",
                    new Vector3(-739.3165f, 294.795f, 1133.453f),
                    new Vector3(0f, 132.8001f, 0f),
                    new Vector3(1.5f, 1.1f, 1.1f));
            }
        }

        private IEnumerator PlaceAssetAsync(string prefabName, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject prefab = null;
            yield return AssetUtils.LoadPrefabAsync(prefabName, (go) => prefab = go);

            if (prefab != null)
            {
                SceneUtils.PlaceAssetsInScene(prefabName, pos, rot, scale);
            }
            else
            {
                Log.Warning($"Failed to load prefab: {prefabName}");
            }
        }
    }
}
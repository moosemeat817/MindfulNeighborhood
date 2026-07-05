using MelonLoader.Utils;
using UnityEngine.Rendering.PostProcessing;
using MelonLoader;
using UnityEngine;
using System.Collections;

namespace MindfulNeighborhood
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Settings.OnLoad();
        }



        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {


            if (sceneName == "AirfieldRegion")
            {
                Log.Msg($"****************************** OnSceneWasLoaded");
                MelonCoroutines.Start(new MindfulNeighborhoodManager().PlaceAssetsAsync());


                GameObject.Find("Art/Structures/STR_WoodCabinE_Prefab").transform.SetPositionAndRotation(new Vector3(-934.8487f, 284.7958f, 1267.208f), Quaternion.Euler(new Vector3(-0, 232.537f, 0)));

                Transform structures = GameObject.Find("Art/Structures").transform;
                structures.GetChild(5).SetPositionAndRotation(new Vector3(-531.3672f, 296.215f, 1350.146f), Quaternion.Euler(new Vector3(-0, 321.7562f, 0)));


                GameObject potBellyStove = GameObject.Find("Lit Art/STR_SteepCabinA_InteriorObjects_Prefab/Interior Objects/INTERACTIVE_PotBellyStove");
                if (potBellyStove != null)
                {
                    potBellyStove.SetActive(false);
                }
                else
                {
                    Log.Warning("[MindfulNeighborhood] Could not find 'INTERACTIVE_PotBellyStove' to disable (likely an existing save started before this mod was enabled).");
                }

                GameObject metalBucket = GameObject.Find("Lit Art/STR_SteepCabinA_InteriorObjects_Prefab/Interior Objects/OBJ_MetalBucketA_Prefab");
                if (metalBucket != null)
                {
                    metalBucket.SetActive(false);
                }
                else
                {
                    Log.Warning("[MindfulNeighborhood] Could not find 'OBJ_MetalBucketA_Prefab' to disable (likely an existing save started before this mod was enabled).");
                }

                GameObject cardboardBox = GameObject.Find("Lit Art/STR_SteepCabinA_InteriorObjects_Prefab/Interior Objects/OBJ_BoxCardboardOpenD_Prefab");
                if (cardboardBox != null)
                {
                    cardboardBox.SetActive(false);
                }
                else
                {
                    Log.Warning("[MindfulNeighborhood] Could not find 'OBJ_BoxCardboardOpenD_Prefab' to disable (likely an existing save started before this mod was enabled).");
                }
            }

            if (sceneName == "AirfieldRegion" || sceneName == "AirfieldRegion_SANDBOX")
            {

                Clones.ChangeObjects();
            }




        }
    }
}
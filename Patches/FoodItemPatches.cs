using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace FrozenFood.Patches
{
    internal class FoodItemPatches
    {


        [HarmonyPatch(typeof(FoodItem), nameof(FoodItem.Awake))]

        public class FoodItem_Awake
        {

            private static void Postfix(FoodItem __instance)
            {
                GameObject food = __instance.gameObject;
                if (food.name.ToLowerInvariant().Contains("cattail") || food.name.ToLowerInvariant().Contains("acorn") || food.name.ToLowerInvariant().Contains("coffee") || food.name.ToLowerInvariant().Contains("birch") || food.name.ToLowerInvariant().Contains("burdock") || food.name.ToLowerInvariant().Contains("rose") || food.name.ToLowerInvariant().Contains("energy")) return;

                if (!food.GetComponent<ObjectGuid>())
                {
                    Guid uniqueId = Guid.NewGuid();
                    ObjectGuid.MaybeAttachObjectGuidAndRegister(food, uniqueId.ToString());
                }

                food.AddComponent<FrozenFood>();
            }

        }

        [HarmonyPatch(typeof(FoodItem), nameof(FoodItem.Serialize))]

        public class FrozenFood_Save
        {
            private static void Postfix(FoodItem __instance)
            {
                FrozenFood ff = __instance.gameObject.GetComponent<FrozenFood>();

                if (ff != null && !GameManager.m_ActiveScene.Contains("menu"))
                {
                    ff.Serialize();
                }
            }

        }

        [HarmonyPatch(typeof(GearItem), "Degrade", new Type[] { typeof(float) })]
        internal class GearItem_Degrade
        {
            private static void Prefix(GearItem __instance, ref float hp)
            {

                if(__instance.m_FoodItem != null)
                {
                    FrozenFood ff = __instance.gameObject.GetComponent<FrozenFood>();
                    if(ff is not null)
                    {
                        hp *= ff.GetDecayMod();
                    }
                }

               
            }
        }
    }
}

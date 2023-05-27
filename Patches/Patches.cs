using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using Il2Cpp;
using HarmonyLib;
using Il2CppTLD.PDID;
using Il2CppTLD.Gear;

namespace FrozenFood.Patches
{
    internal class Patches
    {

      


        [HarmonyPatch(typeof(FoodItem), nameof(FoodItem.Awake))]

        public class FoodItem_Awake
        {

            private static void Postfix(FoodItem __instance)
            {
                GameObject food = __instance.gameObject;

                Guid uniqueId = Guid.NewGuid();
                ObjectGuid.MaybeAttachObjectGuidAndRegister(food, uniqueId.ToString());

                food.AddComponent<FrozenFood>();
               
            }

        }

        [HarmonyPatch(typeof(FoodItem), nameof(FoodItem.Serialize))]

        public class FrozenFood_Save
        {
            private static void Postfix(FoodItem __instance)
            {
                FrozenFood ff = __instance.gameObject.GetComponent<FrozenFood>();

                if(ff != null && !GameManager.m_ActiveScene.Contains("menu"))
                {
                    ff.Serialize();
                }
                else
                {
                    MelonLogger.Error("Cannot serialize because frozen food component can't be found");
                }
            }

        }


    }
}

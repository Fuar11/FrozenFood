﻿using System;
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
using Il2CppSystem;
using Guid = System.Guid;

namespace FrozenFood.Patches
{
    internal class Patches
    {

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.CanUseFoodInventoryItem))]

        public class EatingRestriction
        {

            private static bool Prefix(GearItem gi, ref bool __result)
            {

                FrozenFood ff = gi.gameObject.GetComponent<FrozenFood>();

                if (ff != null)
                {
                    if (ff.IsFrozen())
                    {
                        //if item is frozen, can't eat it
                        InterfaceManager.GetPanel<Panel_BodyHarvest>().DisplayErrorMessage("Food is too frozen too eat!");
                        GameAudioManager.PlayGUIError();
                        __result = false;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.UseFoodInventoryItem))]

        public class EatingDebuff
        {
            private static void Postfix(GearItem gi)
            {
                FrozenFood ff = gi.gameObject.GetComponent<FrozenFood>();

                if (ff != null)
                {
                    if (ff.GetPercentFrozen() > 25f && ff.GetPercentFrozen() < 50f)
                    {
                        //if item is still edible but somewhat frozen, make the character colder
                        GameManager.GetFreezingComponent().AddFreezing(15f);
                    }
                }

            }


        }


        [HarmonyPatch(typeof(FoodItem), nameof(FoodItem.Awake))]

        public class FoodItem_Awake
        {

            private static void Postfix(FoodItem __instance)
            {
                GameObject food = __instance.gameObject;
                if (food.name.ToLowerInvariant().Contains("cattail") || food.name.ToLowerInvariant().Contains("acorn") || food.name.ToLowerInvariant().Contains("coffee") || food.name.ToLowerInvariant().Contains("birch") || food.name.ToLowerInvariant().Contains("burdock") || food.name.ToLowerInvariant().Contains("rose")) return;

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

                if(ff != null && !GameManager.m_ActiveScene.Contains("menu"))
                {
                    ff.Serialize();
                }
            }

        }


    }
}

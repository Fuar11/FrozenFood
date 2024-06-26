using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using MelonLoader;
using Il2Cpp;
using FrozenFood.Utils;

namespace FrozenFood.Patches
{
    internal class MiscPatches
    {

        [HarmonyPatch(typeof(Cookable), nameof(Cookable.GetCookTimeMinutesForCaloriesRemaining))]

        public class CookTimeModifier
        {

            private static void Postfix(Cookable __instance, ref float __result)
            {

                if (__instance.gameObject.GetComponent<FrozenFood>())
                {
                    float percentFrozen = __instance.gameObject.GetComponent<FrozenFood>().GetPercentFrozen();
                    float multiplier = 1f;

                    multiplier = UtilityFunctions.MapPercentageToVariable(percentFrozen, 1f, 1.5f);

                    __result *= multiplier;
                }

            }


        }

        /*
        [HarmonyPatch(typeof(PlayerManager), "EnterInspectGearMode", new Type[] {typeof (GearItem), typeof(Container), typeof(IceFishingHole), typeof(Harvestable), typeof(CookingPotItem)})]
        public class CheckFirstInspectClass
        {

            private static void Postfix(PlayerManager __instance)
            {
                if (__instance.m_Gear.m_FoodItem)
                {
                    if(__instance.m_Gear.GetComponent<FrozenFood>() != null)
                    {
                        __instance.m_Gear.GetComponent<FrozenFood>().OnFirstInspect();
                    }
                }

            }

        } */

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.AddItemToPlayerInventory))]

        public class AddItemToInvClass
        {

            private static void Postfix(GearItem gi)
            {

                if (gi.m_FoodItem)
                {
                    if(gi.GetComponent<FrozenFood>() != null)
                    {
                        gi.GetComponent<FrozenFood>().PickedUp();
                    }
                }

            }

        }

        [HarmonyPatch(typeof(Inventory), "RemoveGear", new Type[] {typeof(GameObject), typeof(bool)})]
        public class CheckDroppedClass
        {

            private static void Postfix(GameObject go)
            {

                GearItem component = go.GetComponent<GearItem>();

                if (component.m_FoodItem)
                {

                    if (component.GetComponent<FrozenFood>() == null) return;

                    component.GetComponent<FrozenFood>().Dropped();
                }
            }

        }

        [HarmonyPatch(typeof(ItemDescriptionPage), nameof(ItemDescriptionPage.UpdateGearItemDescription))]


        public class GodHelpMe
        {

            private static void Postfix(ItemDescriptionPage __instance, GearItem gi)
            {
                if (gi.m_FoodItem || gi.m_ClothingItem)
                {

                    if (gi.m_FoodItem)
                    {
                        if (gi.m_FoodItem.gameObject.GetComponent<FrozenFood>())
                        {
                            if (gi.m_FoodItem.gameObject.GetComponent<FrozenFood>().GetPercentFrozen() < 1f) return;
                        }
                    }

                    Il2Cpp.Utils.SetActive(__instance.m_ClothingStatsObject, true);
                }
            }

        }

    }
}

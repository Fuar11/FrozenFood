using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using MelonLoader;
using Il2Cpp;

namespace FrozenFood.Patches
{
    internal class MiscPatches
    {
       

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
                    component.GetComponent<FrozenFood>().Dropped();
                }
            }

        }

        [HarmonyPatch(typeof(ItemDescriptionPage), nameof(ItemDescriptionPage.UpdateTopRightStatus))]

        public class AddToInventoryScreenClass
        {

            private static void Postfix(GearItem gi, ItemDescriptionPage __instance)
            {

                if (gi.m_FoodItem)
                {
                    if(gi.GetComponent<FrozenFood>() != null)
                    {
                        if(gi.GetComponent<FrozenFood>().GetFrozenNormalized() > 0f)
                        {
                            __instance.m_TopRightStatusLabel.text = gi.GetComponent<FrozenFood>().GetFrozenOnlyString();
                            if (!string.IsNullOrEmpty(__instance.m_TopRightStatusLabel.text))
                            {
                                __instance.m_TopRightStatusLabel.text = __instance.StripBraces(__instance.m_TopRightStatusLabel.text);
                                __instance.UpdateAndEnableTopRightStatus(__instance.m_TopRightStatusLabel.text, __instance.m_FrozenStatusIcon, __instance.m_FrozenStatusColor);

                                //I know how messy this is. Don't judge pls
                                __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(5).gameObject.SetActive(false);
                                __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(6).gameObject.SetActive(false);
                                __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(7).gameObject.SetActive(false);
                                __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(8).gameObject.SetActive(false);
                                __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(9).gameObject.SetActive(false);

                            }
                        }
                    }
                    else
                    {
                        MelonLogger.Error("Doesn't have frozen component");
                    }
                }
                else if (gi.m_ClothingItem)
                {
                    __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(5).gameObject.SetActive(true);
                    __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(6).gameObject.SetActive(true);
                    __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(7).gameObject.SetActive(true);
                    __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(8).gameObject.SetActive(true);
                    __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(9).gameObject.SetActive(true);
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
                            if (gi.m_FoodItem.gameObject.GetComponent<FrozenFood>().IsThawed()) return;
                        }
                    }

                    Il2Cpp.Utils.SetActive(__instance.m_ClothingStatsObject, true);
                }
            }

        }

    }
}

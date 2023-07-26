using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using System.Collections;
using static Il2Cpp.ak.wwise;

namespace FrozenFood.Patches
{
    internal class InventoryPatches : MonoBehaviour
    {


        [HarmonyPatch(typeof(Inventory), nameof(Inventory.ProcessItems))]

        public class UpdateItemInInventory 
        {

            private static void Postfix(Inventory __instance)
            {

                for (int i = 0; i < __instance.m_Items.Count; i++)
                {
                    GearItem gi = __instance.m_Items[i];

                    if (!gi.gameObject.activeInHierarchy)
                    {
                        if (gi.m_FoodItem)
                        {
                            FrozenFood ff = gi.gameObject.GetComponent<FrozenFood>();
                            if(ff != null) ff.Update();
                        }
                           
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Container), nameof(Container.UpdateContainer))]

        public class UpdateItemInContainer
        {
            private static void Postfix(Container __instance)
            {
                foreach(GearItem gi in __instance.m_Items)
                {
                    FrozenFood ff = gi.gameObject.GetComponent<FrozenFood>();

                    if (ff != null) ff.Update();
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
                    if (gi.GetComponent<FrozenFood>() != null)
                    {
                        if (gi.GetComponent<FrozenFood>().GetFrozenNormalized() > 0.01f)
                        {
                            __instance.m_TopRightStatusLabel.text = gi.GetComponent<FrozenFood>().GetFrozenOnlyString();
                            if (!string.IsNullOrEmpty(__instance.m_TopRightStatusLabel.text))
                            {
                                __instance.m_TopRightStatusLabel.text = __instance.StripBraces(__instance.m_TopRightStatusLabel.text);
                                __instance.UpdateAndEnableTopRightStatus(__instance.m_TopRightStatusLabel.text, __instance.m_FrozenStatusIcon, __instance.m_FrozenStatusColor);

                                //I know how messy this is. Don't judge pls
                                __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(10).gameObject.SetActive(false);
                                __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(5).gameObject.SetActive(false);
                                __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(6).gameObject.SetActive(false);
                                __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(7).gameObject.SetActive(false);
                                __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(8).gameObject.SetActive(false);
                                __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(9).gameObject.SetActive(false);

                            }
                        }
                        else return;
                    }
                    else
                    {
                        __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(10).gameObject.SetActive(false);
                        __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(5).gameObject.SetActive(false);
                        __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(6).gameObject.SetActive(false);
                        __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(7).gameObject.SetActive(false);
                        __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(8).gameObject.SetActive(false);
                        __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(9).gameObject.SetActive(false);
                    }
                }
                else if (gi.m_ClothingItem)
                {
                    __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(10).gameObject.SetActive(true);
                    __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(5).gameObject.SetActive(true);
                    __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(6).gameObject.SetActive(true);
                    __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(7).gameObject.SetActive(true);
                    __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(8).gameObject.SetActive(true);
                    __instance.gameObject.transform.GetChild(3).gameObject.transform.GetChild(9).gameObject.SetActive(true);
                }
            }

        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.InitLabelsForGear))]

        public class InspectModeUpdate2
        {

        
            private static void Postfix(PlayerManager __instance)
            {

                Panel_HUD hud = InterfaceManager.GetPanel<Panel_HUD>();

                if (__instance.m_Gear.m_FoodItem)
                {
                    if (__instance.m_Gear.m_FoodItem.IsHot()) return;
                    else
                    {
                        MelonCoroutines.Start(EnableColdSprite(hud));   

                        if(__instance.m_Gear.gameObject.GetComponent<FrozenFood>() == null) return;

                        if (__instance.m_Gear.gameObject.GetComponent<FrozenFood>().GetPercentFrozen() > 1f)
                        {
                            SetFrozenSprite();
                            string str = __instance.m_Gear.gameObject.GetComponent<FrozenFood>().GetFrozenOnlyString();
                            SetFrozenLabel(RemoveBrackets(str.ToUpper()));
                        }

                    }

                }

            }

            private static IEnumerator EnableColdSprite(Panel_HUD hud)
            {
                float waitSeconds = 1.2f;
                for (float t = 0f; t < waitSeconds; t += Time.deltaTime) yield return null;
                hud.m_InspectMode_FoodCold.gameObject.SetActive(true);
            }

            private static void SetFrozenSprite()
            {
                UISprite FrozenSprite = InterfaceManager.GetPanel<Panel_HUD>().m_InspectMode_FoodCold;
                InterfaceManager.GetPanel<Panel_HUD>().m_InspectMode_FoodCold.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                FrozenSprite.spriteName = "ico_HUD_cold";
                FrozenSprite.SetDimensions(34, 34);
                Color FrozenColour = new Color(0, 0.844f, 1, 1);
                FrozenSprite.color = FrozenColour;
            }

            private static void SetFrozenLabel(string percentage)
            {

                UILabel FrozenLabel = InterfaceManager.GetPanel<Panel_HUD>().m_InspectMode_FoodCold.gameObject.transform.GetChild(1).gameObject.GetComponent<UILabel>();
                FrozenLabel.mProcessedText = percentage;
                Color FrozenColour = new Color(0, 0.844f, 1, 1);
                FrozenLabel.color = FrozenColour;
                
            }

            private static string RemoveBrackets(string text)
            {
                if (text.Substring(0, 1) == "(" || text.Substring(0, 1) == "[")
                {
                    return text.Substring(1, text.Length - 2);
                }
                return text;
            }

        }

        [HarmonyPatch(typeof(Panel_Container), nameof(Panel_Container.Update))]

        public class ContainerItemViewUpdate
        {

            public static void Postfix(Panel_Container __instance)
            {

                ContainerUI ui = __instance.m_ContainerUIComponent;
                Color FrozenColour = new Color(0, 0.844f, 1, 1);

                //update container side
                for (var i = 0; i < __instance.m_FilteredContainerList.Count; i++)
                {
                    if (__instance.m_FilteredContainerList[i].m_FoodItem)
                    {
                        FrozenFood ff = __instance.m_FilteredContainerList[i].GetComponent<FrozenFood>();
                        if (ff != null && ff.IsFrozen())
                        {
                            ui.m_ContainerTableItems[i].m_NotificationFlag.SetActive(true);
                            ui.m_ContainerTableItems[i].m_NotificationFlag.GetComponent<UISprite>().spriteName = "ico_HUD_cold";
                            ui.m_ContainerTableItems[i].m_NotificationFlag.GetComponent<UISprite>().color = FrozenColour;
                        }
                        else
                        {
                            ui.m_ContainerTableItems[i].m_NotificationFlag.SetActive(false);
                        }
                    }
                    else
                    {
                        ui.m_ContainerTableItems[i].m_NotificationFlag.SetActive(false);
                    }
                }

                //update inventory side
                for (var i = 0; i < __instance.m_FilteredInventoryList.Count; i++)
                {
                    if (__instance.m_FilteredInventoryList[i].m_FoodItem)
                    {
                        FrozenFood ff = __instance.m_FilteredInventoryList[i].m_FoodItem.GetComponent<FrozenFood>();
                        if (ff != null && ff.IsFrozen())
                        {
                            ui.m_InventoryTableItems[i].m_NotificationFlag.SetActive(true);
                            ui.m_InventoryTableItems[i].m_NotificationFlag.GetComponent<UISprite>().spriteName = "ico_HUD_cold";
                            ui.m_InventoryTableItems[i].m_NotificationFlag.GetComponent<UISprite>().color = FrozenColour;
                        }
                        else
                        {
                            ui.m_InventoryTableItems[i].m_NotificationFlag.SetActive(false);
                        }
                    }
                    else
                    {
                        ui.m_InventoryTableItems[i].m_NotificationFlag.SetActive(false);
                    }
                }

            }

        }

    }
}

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
using FrozenFood.Utils;
using Il2CppTLD.SaveState;

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

                        if (__instance.m_Gear.gameObject.GetComponent<FrozenFood>().GetPercentFrozen() >= 1f)
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
                for (var i = 0; i < ui.m_ContainerTableItems.Count; i++)
                {
                    GearItem gi = ui.m_ContainerTableItems[i].m_GearItem;
                    if (gi != null && gi.m_FoodItem)
                    {
                        FrozenFood ff = gi.GetComponent<FrozenFood>();
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
                for (var i = 0; i < ui.m_InventoryTableItems.Count; i++)
                {

                    GearItem gi = ui.m_InventoryTableItems[i].m_GearItem;

                    if (gi != null && gi.m_FoodItem)
                    {
                        FrozenFood ff = gi.GetComponent<FrozenFood>();
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

        
        [HarmonyPatch(typeof(Panel_ActionsRadial), nameof(Panel_ActionsRadial.UpdateStackStatus))]

        public class RadialFoodFrozenIndicator
        {

            public static bool Prefix()
            {
                return false;
            }

            public static void Postfix(ref GearItem gi, Panel_ActionsRadial __instance)
            {

                __instance.m_StackStatusLabel.gameObject.SetActive(false);
               __instance.m_StackStatusIcon.gameObject.SetActive(false);

                MeasurementUnits u = new MeasurementUnits();

                try
                {
                    if (InterfaceManager.GetPanel<Panel_OptionsMenu>().m_UnitsPopupList.m_SelectedItem.ToLowerInvariant().Contains("metric")) u = MeasurementUnits.Metric;
                    else u = MeasurementUnits.Imperial;
                }
                catch (Exception e)
                {
                //if i put a log here it will spam the console
                }


                if (gi.m_StoneItem && gi.m_StackableItem)
                {
                    __instance.UpdateAndEnableStackStatus(GameManager.GetInventoryComponent().GetNumStones().ToString(), gi.m_StackableItem.m_StackSpriteName, __instance.m_StackStatusColor);
                }
                else if (gi.m_FoodItem)
                {

                    FrozenFood ff = gi.m_FoodItem.gameObject.GetComponent<FrozenFood>();
                    string sprite = InterfaceManager.GetPanel<Panel_HUD>().m_UnitsSprite_Calories;
                    Color colour = __instance.m_FoodColdStatusColor;

                    if (ff is not null)
                    {
                        if (ff.IsFrozen())
                        {
                            sprite = "ico_HUD_cold";
                            colour = new Color(0, 0.844f, 1, 1);
                        }
                    }

                    if (gi.m_FoodItem.IsHot() && gi.m_FoodItem.m_HeatedWhenCooked)
                    {
                        __instance.UpdateAndEnableStackStatus(gi.m_FoodItem.m_CaloriesRemaining.ToString("F0"), InterfaceManager.GetPanel<Panel_HUD>().m_UnitsSprite_Calories, __instance.m_FoodHotStatusColor);
                    }
                    else if (!gi.m_FoodItem.IsHot() && gi.m_FoodItem.m_HeatedWhenCooked)
                    {
                        __instance.UpdateAndEnableStackStatus(gi.m_FoodItem.m_CaloriesRemaining.ToString("F0"), sprite, colour);
                    }
                    else
                    {
                        __instance.UpdateAndEnableStackStatus(gi.m_FoodItem.m_CaloriesRemaining.ToString("F0"), sprite, colour);
                    }
                }
                else if (gi.m_GunItem)
                {
                    __instance.UpdateAndEnableStackStatus(gi.m_GunItem.NumRoundsInClip().ToString(), gi.m_GunItem.m_AmmoSpriteName, __instance.m_StackStatusColor);
                }
                else if (gi.m_FlareItem)
                {
                    int numFlares = GameManager.GetInventoryComponent().GetNumFlares(gi.m_FlareItem.m_Type);
                    if (numFlares > 1)
                    {
                        __instance.UpdateAndEnableStackStatus(numFlares.ToString(), gi.m_FlareItem.m_RadialSpriteName, __instance.m_StackStatusColor);
                    }
                }
                else if (gi.m_TorchItem)
                {
                    int numTorches = GameManager.GetInventoryComponent().GetNumTorches();
                    if (numTorches > 1)
                    {
                        __instance.UpdateAndEnableStackStatus(numTorches.ToString(), gi.m_TorchItem.m_RadialSpriteName, __instance.m_StackStatusColor);
                    }
                }
                else if (gi.m_StackableItem && gi.m_StackableItem.m_Units > 1)
                {
                    __instance.UpdateAndEnableStackStatus(gi.m_StackableItem.m_Units.ToString(), gi.m_StackableItem.m_StackSpriteName, __instance.m_StackStatusColor);
                }
                else if (gi.m_WaterSupply)
                {

                    string liquidQuantityStringWithUnitsNoOunces = IntBackedUnitExtensions.ToFormattedStringNoOunces(gi.m_WaterSupply.m_VolumeInLiters);

                    __instance.UpdateAndEnableStackStatus(liquidQuantityStringWithUnitsNoOunces, __instance.m_LiquidIcon, __instance.m_StackStatusColor);
                }
                else if (gi.m_KeroseneLampItem)
                {
                    string liquidQuantityStringWithUnitsNoOunces2 = IntBackedUnitExtensions.ToFormattedStringNoOunces(gi.m_KeroseneLampItem.m_CurrentFuelLiters);
                    __instance.UpdateAndEnableStackStatus(liquidQuantityStringWithUnitsNoOunces2, __instance.m_LampFuelIcon, __instance.m_StackStatusColor);
                }
                else if (gi.m_LiquidItem)
                {
                    string liquidQuantityStringWithUnitsNoOunces3 = IntBackedUnitExtensions.ToFormattedStringNoOunces(gi.m_LiquidItem.m_Liquid);
                    __instance.UpdateAndEnableStackStatus(liquidQuantityStringWithUnitsNoOunces3, __instance.m_LiquidIcon, __instance.m_StackStatusColor);
                }
                else if (gi.m_BowItem)
                {
                    __instance.UpdateAndEnableStackStatus(gi.m_BowItem.GetNumAllArrowsInInventory().ToString(), "ico_ammo_arrow", __instance.m_StackStatusColor);
                }
                else if (gi.m_NoiseMakerItem)
                {
                    __instance.UpdateAndEnableStackStatus(GameManager.GetInventoryComponent().GetNumNoiseMakers().ToString(), gi.m_NoiseMakerItem.m_RadialSpriteName, __instance.m_StackStatusColor);
                }
            }

        }
        
       

        [HarmonyPatch(typeof(Panel_HUD), nameof(Panel_HUD.SetHoverText))]

        public class FrozenHoverText
        {
            public static void Prefix(ref string hoverText, ref GameObject itemUnderCrosshairs, ref HoverTextState textState, Panel_HUD __instance)
            {

                if (GameManager.IsMainMenuActive()) return;
                if (hoverText == null || itemUnderCrosshairs == null) return;

                string textToConcat = "";
                Color frozenColour = new Color(0, 0.844f, 1, 1);

                FrozenFood ff = itemUnderCrosshairs.GetComponent<FrozenFood>();
                if (ff != null)
                {

                    textToConcat = ff.GetFrozenOnlyString();

                    if (ff.GetPercentFrozen() >= 1f)
                    {
                        __instance.m_Label_SubText.color = frozenColour;

                        if (hoverText.Contains("(Cold)"))
                        {
                            hoverText = hoverText.Replace("Cold", textToConcat);
                        }
                        else hoverText += ("\n" + textToConcat);
                    }
                }
            }
        }

       


    }
}

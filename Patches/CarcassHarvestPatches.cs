using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using Il2Cpp;
using UnityEngine;
using Il2CppTLD.Stats;
using Il2CppTLD.SaveState;
using Unity.VisualScripting;
using Il2CppTLD.IntBackedUnit;

namespace FrozenFood.Patches
{
    internal class CarcassHarvestPatches
    {

        [HarmonyPatch(typeof(Panel_BodyHarvest), nameof(Panel_BodyHarvest.TransferMeatFromCarcassToInventory))]

        public class ThawOrFreezeMeatFromCarcass
        {

            private static bool Prefix()
            {
                return false;
            }

            private static void Postfix(Panel_BodyHarvest __instance)
            {

                GameObject meatPrefab = __instance.m_BodyHarvest.m_MeatPrefab;
                GearItem component = meatPrefab.GetComponent<GearItem>();
                float num = __instance.m_MenuItem_Meat.HarvestAmount.m_Units;
                while ((double)num > 0.01)
                {

                    float num2 = num / component.GetSingleItemWeightKG().m_Units;

                    GearItem gearItem = GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(component, 1, 86f);

                    //ff stuff

                    if (gearItem.GetComponent<FrozenFood>())
                    {
                        gearItem.GetComponent<FrozenFood>().m_PercentFrozenFromHarvest = __instance.m_BodyHarvest.m_PercentFrozen;
                        gearItem.GetComponent<FrozenFood>().harvestCheck = true;
                    }

                    

                    num -= component.GetSingleItemWeightKG().m_Units;
                    num = Mathf.Clamp(num, 0f, float.PositiveInfinity);
                    if (gearItem)
                    {
                        if (num2 < 1f)
                        {
                            gearItem.m_FoodItem.m_CaloriesRemaining = gearItem.m_FoodItem.m_CaloriesTotal * num2;
                        }
                        if (!__instance.m_BodyHarvest.IsGearItem())
                        {
                            gearItem.CurrentHP = __instance.m_BodyHarvest.GetCondition() / 100f * 100;
                        }
                        else
                        {
                            gearItem.CurrentHP = __instance.m_BodyHarvest.GetGearItemCondition() * 100;
                        }
                        gearItem.MarkAsHarvested();
                        StatsManager.IncrementValue(StatID.MeatHarvested, gearItem.GetItemWeightKG());

                    }
                }

                __instance.m_BodyHarvest.m_MeatAvailableKG = new ItemWeight(__instance.m_BodyHarvest.m_MeatAvailableKG.m_Units - __instance.m_MenuItem_Meat.HarvestAmount.m_Units);
                __instance.m_BodyHarvest.MaybeRoundMeatAvailableToZero();
                if (__instance.m_MenuItem_Meat.HarvestAmount.m_Units > 0f)
                {
                    string weightOneDecimalPlaceWithUnitsString = IntBackedUnitExtensions.ToFormattedStringWithUnits(__instance.m_MenuItem_Meat.HarvestAmount, 1);
                    string message = component.name + " (" + weightOneDecimalPlaceWithUnitsString + ")";
                    GearMessage.AddMessage(component.name, Localization.Get("GAMEPLAY_Harvested"), message);
                }

            }
        }

        [HarmonyPatch(typeof(BodyHarvest), nameof(BodyHarvest.MaybeFreeze))]

        public class ThawOrFreezeCarcass
        {

            private static bool Prefix() { return false; }

            private static void Postfix(BodyHarvest __instance, ref float hours)
            {

                float Temp = GameManager.GetWeatherComponent().GetCurrentTemperature();
                float TemperatureMultiplier = 1f;

                if (Temp < 0 && Temp > -10)
                {
                    if (GameManager.GetWeatherComponent().IsIndoorEnvironment())
                    {
                        TemperatureMultiplier = 1.05f;
                    }
                    else
                    {
                        TemperatureMultiplier = 1.2f;
                    }
                }
                else if (Temp < -10 && Temp > -20) TemperatureMultiplier = 1.5f;
                else if (Temp < -20 && Temp > -30) TemperatureMultiplier = 1.7f;
                else if (Temp < -30) TemperatureMultiplier = 2f;
                else TemperatureMultiplier = 1f;

                if (Temp > 0 && Temp < 10) TemperatureMultiplier = 1.2f;
                else if (Temp > 10 && Temp < 20) TemperatureMultiplier = 1.5f;
                else if (Temp > 20 && Temp < 30) TemperatureMultiplier = 1.7f;
                else if (Temp > 30) TemperatureMultiplier = 2f;
                else if (Temp > 50) TemperatureMultiplier = 2.5f;
                else if (Temp > 80) TemperatureMultiplier = 2.85f;
                else if (Temp > 100) TemperatureMultiplier = 3f;
                else TemperatureMultiplier = 1f;

               if (GameManager.GetWeatherComponent().GetCurrentTemperature() > 0)
               {
                    __instance.m_PercentFrozen -= hours / (GameManager.GetBodyHarvestManagerComponent().m_NumHoursToThawFrozenCarcass * 100f) * TemperatureMultiplier;
               }
               else
               {
                    if (!__instance.m_AllowDecay) return;
                    __instance.m_PercentFrozen += hours / (GameManager.GetBodyHarvestManagerComponent().m_NumHoursToFreezeCarcass * 100f) * TemperatureMultiplier;
               }
                __instance.m_PercentFrozen = Mathf.Clamp(__instance.m_PercentFrozen, 0f, 100f);
                if (Il2Cpp.Utils.Approximately(__instance.m_PercentFrozen, 100f))
                {
                    __instance.m_Frozen = true;
                }
                else
                {
                    __instance.m_Frozen = false;
                }
            }
        }
    }
}

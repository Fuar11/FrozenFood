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
using Il2CppSystem;
using Guid = System.Guid;

namespace FrozenFood.Patches
{
    internal class PlayerPatches
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

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.AddItemToPlayerInventory))]

        public class AddItemToInvClass
        {

            private static void Postfix(GearItem gi)
            {

                if (gi.m_FoodItem)
                {
                    if (gi.GetComponent<FrozenFood>() != null)
                    {
                        gi.GetComponent<FrozenFood>().PickedUp();
                    }
                }

            }

        }
    }
}

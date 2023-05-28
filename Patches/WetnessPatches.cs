using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace FrozenFood.Patches
{
    internal class WetnessPatches
    {

        [HarmonyPatch(typeof(IceCrackingTrigger), nameof(IceCrackingTrigger.DidFallThroughIceFadeOut))]

        public class FreezeOnFallThroughIce
        {

            private static void Postfix(IceCrackingTrigger __instance)
            {

                if(__instance.m_ClothingRegionsToMakeWet.Count > 0)
                {

                    Random rand = new Random();
                    float randomFrozen = rand.Next(40, 70);
                    FreezeAllFood(randomFrozen);
                }
                else
                {
                    FreezeAllFood(100f);
                }
            }
        }

        [HarmonyPatch(typeof(WetZoneTrigger), nameof(WetZoneTrigger.IncreaseWetness))]

        public class FreezeOnGetWet
        {

            private static void Postfix()
            {
                FreezeAllFood(100f);
            }

        }

        [HarmonyPatch(typeof(WetZoneApplyOnlyTrigger), nameof(WetZoneApplyOnlyTrigger.IncreaseWetness))]

        public class FreezeOnGetWet2
        {

            private static void Postfix(WetZoneApplyOnlyTrigger __instance)
            {
                if(__instance.m_InstantWetOnContact) FreezeAllFood(100f);
            }

        }

        public static void FreezeAllFood(float amount)
        {

            for (int i = 0; i < GameManager.GetInventoryComponent().m_Items.Count; i++)
            {
                GearItem gi = GameManager.GetInventoryComponent().m_Items[i];

                if (!gi.gameObject.activeInHierarchy)
                {
                    if (gi.m_FoodItem)
                    {
                        FrozenFood ff = gi.gameObject.GetComponent<FrozenFood>();
                        if (ff != null) ff.ForceFreeze(amount);
                    }

                }
            }

        }

    }
}

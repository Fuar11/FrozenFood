using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace FrozenFood.Patches
{
    internal class InventoryPatches
    {


        [HarmonyPatch(typeof(Inventory), nameof(Inventory.ProcessItems))]

        public class test
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
                            gi.gameObject.GetComponent<FrozenFood>().Update();
                        }
                    }
                }
            }
        }

    }
}

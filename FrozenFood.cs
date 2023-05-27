﻿using FrozenFood.Utils;
using Il2Cpp;
using Il2CppTLD.Gear;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Text.Json;
using static Il2CppSystem.Globalization.TimeSpanParse;
using Random = System.Random;

namespace FrozenFood
{
    [RegisterTypeInIl2Cpp]
    internal class FrozenFood : MonoBehaviour
    {

        //This is our custom component

        public FrozenFood(IntPtr ptr) : base(ptr) { }

        private GearItem m_GearItem = new GearItem();

        public string m_GUID;

        private bool loadCheck;

        private bool m_DroppedIndoors;

        private float m_PercentFrozen; //temporary

        private float m_TempToLoadDataWith;

        public float m_ThawPercentPerHour;

        public float m_FreezePercentPerHour;

        public void Awake()
        {


            m_GearItem = this.GetComponent<GearItem>();
            loadCheck = false;

        }

        public void Start()
        {
        }

        public void LoadOrInitData()
        {

            SaveDataManager sdm = Implementation.sdm;

            m_GUID = this.gameObject.GetComponent<ObjectGuid>().PDID;

            /*
            MelonLogger.Msg("Item ID is: {0}", this.gameObject.name);
            MelonLogger.Msg("Instance ID is: {0}", this.gameObject.GetInstanceID());
            MelonLogger.Msg("GUID is: {0}", m_GUID); */

            //check to see if object already has component data on it

            string loadedData = sdm.LoadFrozenFoodData(m_GUID);
            FrozenFoodSaveDataProxy? ldp;

            if (loadedData != null)
            {
                ldp = JsonSerializer.Deserialize<FrozenFoodSaveDataProxy>(loadedData);

                if (ldp != null)
                {
                    float hoursSinceSaving = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() - ldp.m_HoursPlayedAtTimeOfSave;
                    m_PercentFrozen = ldp.m_PercentFrozen;

                    if (!Il2Cpp.Utils.IsZero(hoursSinceSaving))
                    {

                        if (ldp.m_HoursRemainingOnClosestFire > 0f) 
                        {
                            //this checks if the time remaining on fire or time passed since last save is greater than one another
                            float timeToModifyFrozenValueBy = Mathf.Min(ldp.m_HoursRemainingOnClosestFire, hoursSinceSaving);
                            //if this variable is m_HoursRemainingOnClosestFire then it means the fire is out. if it's hoursSinceSaving then the fire is still going

                            m_TempToLoadDataWith = ldp.m_MaxFireTemperature; //temperature used to calculate the thaw amount because the fire WAS going at some point, so this value is used to determine the temperature at that point
                            DoThawOrFreeze(timeToModifyFrozenValueBy, nearFireOverride: true); //override used to tell the method to thaw, even if the current temperature <0 and the fire is out
                            hoursSinceSaving -= timeToModifyFrozenValueBy; //this variable now becomes the amount of time passed since the fire has gone out
                        }

                        if (hoursSinceSaving > 0f) //if there is time left since fire has gone out, thaw or freeze according to the temperature from before the fire (in line with current temp)
                        {
                            m_TempToLoadDataWith = ldp.m_ActualTemperature;
                            DoThawOrFreeze(hoursSinceSaving); //if the temperature is <0. At this point if there was a fire and it thawed, it will start to freeze again

                            Serialize(); //this saves the data, it is used for going outside only
                        }
                    }

                    if (ldp.m_IsInBackpack) m_PercentFrozen = ldp.m_PercentFrozen; //if item was in the backpack the whole time, no need to do all that calculation, just pull the value over from previous scene
                }
            }
            else
            {

                if (GameManager.GetWeatherComponent().IsIndoorEnvironment() && GetCurrentAirTemp() < 0f)
                {

                    Random rand = new Random();

                    float randomFrozen = rand.Next(40, 100);
                    float randomFrozenInContainer = rand.Next(5, 50);

                    m_PercentFrozen = randomFrozen; //if food item is new (i.e. you entered the scene for the first time) and it's cold in there, set to a temp base half frozen value
                    if (IsInBackpack()) m_PercentFrozen = 0f; //if it's in your backpack, don't freeze it. this is if the player spawns indoors with food in their pack
                    else if (IsInContainer()) m_PercentFrozen = randomFrozenInContainer; //if it's in container, freeze a little bit
                }
                else if (!GameManager.GetWeatherComponent().IsIndoorEnvironment())
                {
                    if (IsInBackpack()) m_PercentFrozen = 0f; //if it's in your backpack, don't freeze it. player gets a starting chance >:)
                    m_PercentFrozen = 100f; //if it's not in your backpack, it's been outside for a while so it's frozen
                }
                else m_PercentFrozen = 0f;

                //I don't even know if we need to save here vvv

                FrozenFoodSaveDataProxy sdp = new FrozenFoodSaveDataProxy();
                sdp.m_PercentFrozen = m_PercentFrozen;

                string dataToSave = JsonSerializer.Serialize(sdp); //instance in json format to save with
                sdm.Save(dataToSave, m_GUID); //if SDM can't find any data, it means this item hasn't been saved yet, so save it.
            }
        }

        public void Update()
        {
            
                if (!GameManager.m_IsPaused)
                {
                    if (!loadCheck)
                    {
                        LoadOrInitData();
                        loadCheck = true;
                    }

                    float tODHours = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
                    DoThawOrFreeze(tODHours);

                    //texture stuff happens here
                }
        }

        public void Serialize()
        {
            SaveDataManager sdm = Implementation.sdm;

            FrozenFoodSaveDataProxy sdp = new FrozenFoodSaveDataProxy();
            sdp.m_PercentFrozen = m_PercentFrozen;
            sdp.m_HoursPlayedAtTimeOfSave = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();


            if (!IsInBackpack())
            {
                sdp.m_IsInBackpack = false;

                if (IsNearFire())
                {
                    Fire closestFire = GameManager.GetFireManagerComponent().GetClosestFire(this.gameObject.transform.position);

                    if (closestFire != null)
                    {
                        sdp.m_HoursRemainingOnClosestFire = closestFire.GetRemainingLifeTimeSeconds() / 3600f;
                        sdp.m_MaxFireTemperature = closestFire.m_HeatSource.m_TempIncrease;
                        sdp.m_ActualTemperature = GameManager.GetWeatherComponent().GetCurrentTemperatureWithoutHeatSources();
                    }
                }
                else
                {
                    sdp.m_ActualTemperature = GetCurrentAirTemp();
                }
            }
            else
            {
                sdp.m_IsInBackpack = true;
            }



            string dataToSave = JsonSerializer.Serialize(sdp); //instance in json format to save with
            sdm.Save(dataToSave, m_GUID);
        }

        private void DoThawOrFreeze(float numHoursDelta, bool nearFireOverride = false)
        {

            if (IsAirTempPositive() || nearFireOverride)
            {
                m_ThawPercentPerHour = CalculateTimeToThaw(nearFireOverride);
                Thaw(m_ThawPercentPerHour * numHoursDelta);
            }
            else
            {
                m_FreezePercentPerHour = CalculateTimeToFreeze();
                Freeze(m_FreezePercentPerHour * numHoursDelta);
            }

        }

        public void Thaw(float amountToThaw)
        {
            //if item has just been cooked, instantly thaw it
            if (m_GearItem.gameObject.GetComponent<FoodItem>().IsHot()) m_PercentFrozen = 0;

            if(m_PercentFrozen > 0f)
            {
                m_PercentFrozen -= amountToThaw;
            }

            m_PercentFrozen = Mathf.Clamp(m_PercentFrozen, 0f, 100f);
        }

        public void Freeze(float amountToFreeze)
        {

            //if item is hot, do not freeze
            if (m_GearItem.gameObject.GetComponent<FoodItem>().IsHot()) return;

            if(m_PercentFrozen < 100f)
            {
                m_PercentFrozen += amountToFreeze;
            }

            //Maple Syrup cannot freeze solid due to the sugars in it
            if(m_GearItem.name == "GEAR_MapleSyrup" && m_PercentFrozen >= 50)
            {
                m_PercentFrozen = 49f;
            }

            m_PercentFrozen = Mathf.Clamp(m_PercentFrozen, 0f, 100f);
        }
       
        public float CalculateTimeToFreeze()
        {

            //Factor 1: Temperature
            //Factor 2: Item 
            //Factor 3: Storage location

            float Temp;

            if (m_TempToLoadDataWith != null)
            {
               Temp = m_TempToLoadDataWith;
            }
            else
            {
                Temp = GetCurrentAirTemp();
            }

            string Item = m_GearItem.name;
            bool InPack = IsInBackpack();
            bool InContainer = IsInContainer();
            string foodType = GetFoodType(Item);

            float TTF = 0;
            float TemperatureMultiplier = 0;

            if (foodType == "Dry") TTF += 10;
            else if (foodType == "Mid") TTF += 20;
            else if (foodType == "Wet") TTF += 30;
            else TTF += 25;

            if (Temp < 0 && Temp > -10) TemperatureMultiplier = 1.2f;
            else if (Temp < -10 && Temp > -20) TemperatureMultiplier = 1.5f;
            else if (Temp < -20 && Temp > -30) TemperatureMultiplier = 1.7f;
            else if (Temp < -30) TemperatureMultiplier = 2f;
            else TemperatureMultiplier = 1f;

            TTF = TTF * TemperatureMultiplier;

            if (InPack)
            {
                TTF /= 1.5f;
            }
            else if (InContainer)
            {
                TTF /= 2f;
            }

            return TTF;

        }

        public float CalculateTimeToThaw(bool simulatePositiveTemp = false)
        {

            //Factor 1: Temperature
            //Factor 2: Item 
            //Factor 3: Storage location

            float Temp = GetCurrentAirTemp();
            string Item = m_GearItem.name;
            bool InPack = IsInBackpack();
            bool InContainer = IsInContainer();
            string foodType = GetFoodType(Item);

            if (simulatePositiveTemp) Temp = m_TempToLoadDataWith;

            float TTT = 0;
            float TemperatureMultiplier = 0;

            if (foodType == "Dry") TTT += 60;
            else if (foodType == "Mid") TTT += 50;
            else if (foodType == "Wet") TTT += 30;
            else TTT += 40;

            if (Temp > 0 && Temp < 10) TemperatureMultiplier = 1.2f;
            else if (Temp > 10 && Temp < 20) TemperatureMultiplier = 1.5f;
            else if (Temp > 20 && Temp < 30) TemperatureMultiplier = 1.7f;
            else if (Temp > 30) TemperatureMultiplier = 2f;
            else TemperatureMultiplier = 1f;

            TTT = TTT * TemperatureMultiplier;

            if (InPack)
            {
                TTT /= 1.5f;
            }
            else if (InContainer)
            {
                TTT /= 2f;
            }

            return TTT;

        }

        public string GetFoodType(string item)
        {
            if (item == "GEAR_Crackers" || item == "GEAR_KetchupChips" || item == "GEAR_BeefJerky" || item == "GEAR_GranolaBar" || item == "GEAR_CandyBar") return "Dry";
            else if (item.ToLowerInvariant().Contains("meat") || item.Contains("CohoSalmon") || item.Contains("LakeWhiteFish") || item.Contains("RainbowTrout") || item.Contains("SmallMouthBass") || item == "GEAR_PinnacleCanPeaches" || item == "GEAR_TomatoSoupCan") return "Wet";
            else return "Mid";
        }

        private bool IsNearFire()
        {
            return GameManager.GetFireManagerComponent().GetDistanceToClosestFire(this.transform.position) < GameManager.GetBodyHarvestManagerComponent().m_RadiusToThawFromFire;
        }
        public bool IsAirTempPositive()
        {
            return (GameManager.GetWeatherComponent().GetCurrentTemperature() > 0) ? true : false;
        }

        public float GetCurrentAirTemp()
        {
            return GameManager.GetWeatherComponent().GetCurrentTemperature();
        }

        public float GetPercentFrozen()
        {
            return m_PercentFrozen;
        }
        public bool IsThawed()
        {
            return m_PercentFrozen == 0f;
        }

        public bool IsFrozen()
        {
            return m_PercentFrozen >= 50f;
        }

        public bool IsInBackpack()
        {
            int id = m_GearItem.gameObject.GetInstanceID();

            foreach(GearItemObject gio in GameManager.GetInventoryComponent().m_Items)
            {
                if(gio.m_GearItem.gameObject.GetInstanceID() == id)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsInContainer()
        {

            if (IsInBackpack()) return false;

            if (!this.gameObject.transform.parent.gameObject.name.Contains("gear")) return true;

            return false;

        }
        public void Dropped()
        {
            m_DroppedIndoors = GameManager.GetWeatherComponent().IsIndoorEnvironment();
        }

        public void PickedUp()
        {
            m_DroppedIndoors = false;
        }

        /*
        public void OnFirstInspect()
        {
            m_DroppedIndoors = GameManager.GetWeatherComponent().IsIndoorEnvironment();
            if (m_DroppedIndoors)
            {
                m_PercentFrozen = 0f;
            }
        } */

        public string GetFrozenOnlyString()
        {
            string result = string.Empty;
            if (m_PercentFrozen > 99.9f)
            {
                result = Localization.Get("GAMEPLAY_FrozenPostfix");
            }
            else if (m_PercentFrozen > 1f)
            {
                result = Localization.Get("GAMEPLAY_FrozenPercentPostfix");
                result = result.Replace("{%-value}", Mathf.Clamp(Mathf.FloorToInt(m_PercentFrozen), 1, 99).ToString());
            }
            return result;
        }

        public float GetFrozenNormalized()
        {
            return m_PercentFrozen / 100f;
        }

      
    }
}

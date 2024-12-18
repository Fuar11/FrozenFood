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
using Il2CppXGamingRuntime.Interop;

namespace FrozenFood
{
    [RegisterTypeInIl2Cpp]
    internal class FrozenFood : MonoBehaviour
    {

        //This is our custom component

        public FrozenFood(IntPtr ptr) : base(ptr) { }

        private GearItem m_GearItem = new GearItem();

        private FoodItem m_FoodItem = new FoodItem();

        public ObjectGuid m_GUID;

        private bool loadCheck;

        public bool harvestCheck;

        private bool m_ForceFrozen;

        private bool m_DroppedIndoors;

        private float m_PercentFrozen;

        public float m_PercentFrozenFromHarvest;

        private Nullable <float> m_TimeSodaBeenFrozen;

        private float m_TempToLoadDataWith;

        public float m_ThawPercentPerHour;

        public float m_FreezePercentPerHour;

        public void Awake()
        {
            m_GearItem = this.GetComponent<GearItem>();
            m_FoodItem = this.GetComponent<FoodItem>();
            loadCheck = false;
        }

        public void Start()
        {
        }

        public void LoadOrInitData()
        {

            SaveDataManager sdm = Implementation.sdm;

            m_GUID = base.GetComponent<ObjectGuid>();

            /*
            MelonLogger.Msg("Item ID is: {0}", this.gameObject.name);
            MelonLogger.Msg("Instance ID is: {0}", this.gameObject.GetInstanceID());
            MelonLogger.Msg("GUID is: {0}", m_GUID); */

            //check to see if object already has component data on it

            string loadedData = sdm.LoadFrozenFoodData(m_GUID.PDID);
            FrozenFoodSaveDataProxy? ldp;

            if (loadedData != null)
            {

                ldp = JsonSerializer.Deserialize<FrozenFoodSaveDataProxy>(loadedData);

                if (ldp != null)
                {

                    m_PercentFrozen = ldp.m_PercentFrozen;
                    m_ForceFrozen = ldp.m_ForceFrozen;

                    if (m_GearItem.name.ToLowerInvariant().Contains("soda")) m_TimeSodaBeenFrozen = ldp.m_TimeSodaBeenFrozen;

                    float hoursSinceSaving = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() - ldp.m_HoursPlayedAtTimeOfSave;

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

                            //  Serialize(); //this saves the data, it is used for going outside only
                        }
                    }

                }
            }
            else
            {

                if (harvestCheck)
                {
                    m_PercentFrozen = m_PercentFrozenFromHarvest;
                    harvestCheck = false;
                    return;
                }

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
                    if (IsInBackpack()) m_PercentFrozen = 0f; //if it's in your backpack, don't freeze it. player gets a starting chance :)
                    else m_PercentFrozen = 100f; //if it's not in your backpack, it's been outside for a while so it's frozen
                }
                else m_PercentFrozen = 0f;
            }
        }

        public void Update()
        {

            if (!GameManager.m_IsPaused)
            {
                if (!loadCheck)
                {
                    loadCheck = true;
                    LoadOrInitData();
                }

                //Sodas explode after being frozen for some time
                if (m_GearItem.name.ToLowerInvariant().Contains("soda"))
                {

                    if (!m_GearItem.m_BeenInPlayerInventory) return;

                    if (m_PercentFrozen == 100f && GetCurrentAirTemp() < -10)
                    {
                        if (m_TimeSodaBeenFrozen == null) m_TimeSodaBeenFrozen = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                    }
                    else
                    {
                        m_TimeSodaBeenFrozen = null;
                    }
                }

                BlowUpSoda();

                float tODHours = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);

                DoThawOrFreeze(tODHours, IsNearFire(10) && GetNearestFireTemp(10) > 0);

                //texture stuff happens here

            }
        }

        public void Serialize()
        {

            SaveDataManager sdm = Implementation.sdm;

            FrozenFoodSaveDataProxy sdp = new FrozenFoodSaveDataProxy();
            sdp.m_PercentFrozen = m_PercentFrozen;
            sdp.m_HoursPlayedAtTimeOfSave = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
            sdp.m_ForceFrozen = m_ForceFrozen;

            if(m_GearItem.name.ToLowerInvariant().Contains("soda")) sdp.m_TimeSodaBeenFrozen = m_TimeSodaBeenFrozen;

           
                if (IsNearFire(10))
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

            string dataToSave = JsonSerializer.Serialize(sdp); //instance in json format to save with
            sdm.Save(dataToSave, m_GUID.PDID);
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
            if (m_FoodItem.IsHot()) m_PercentFrozen = 0;

            if (m_PercentFrozen > 0f)
            {
                m_PercentFrozen -= amountToThaw;
            }

            m_PercentFrozen = Mathf.Clamp(m_PercentFrozen, 0f, 100f);
        }

        public void Freeze(float amountToFreeze)
        {

            //if item is hot, do not freeze
            if (m_FoodItem.IsHot())
            {
                m_PercentFrozen = 0;
                return;
            }

            if (m_PercentFrozen < 100f)
            {
                m_PercentFrozen += amountToFreeze;
            }

            //Maple Syrup cannot freeze solid due to the sugars in it
            if (m_GearItem.name == "GEAR_MapleSyrup" && m_PercentFrozen >= 50)
            {
                if (!m_ForceFrozen) m_PercentFrozen = 49f;
            }
            else if ((GetFoodType(m_GearItem.name) == "Dry") && m_PercentFrozen >= 26)
            {
                if (!m_ForceFrozen) m_PercentFrozen = 25f;
            }

            m_PercentFrozen = Mathf.Clamp(m_PercentFrozen, 0f, 100f);
        }

        public void ForceFreeze(float amount)
        {

            m_PercentFrozen += amount;
            m_FoodItem.m_HeatPercent = 0f;
            m_ForceFrozen = true;

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
            string foodType = GetFoodType(Item);

            float TTF = 0;
            float TemperatureMultiplier = 0;

            if (foodType == "Dry") TTF += 10;
            else if (foodType == "Mid") TTF += 20;
            else if (foodType == "Wet") TTF += 30;
            else TTF += 25;

            TemperatureMultiplier = 1f + (Temp / (-60f)) * 2f;

            TTF *= TemperatureMultiplier;

            if (GameManager.GetWeatherComponent().IsIndoorEnvironment())
            {
                TTF /= 3f;
            }

            if (IsInBackpack())
            {
                TTF /= 1.5f;
            }
            else if (IsInContainer())
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
            string foodType = GetFoodType(Item);

            if (simulatePositiveTemp) Temp = m_TempToLoadDataWith;

            float TTT = 0;
            float TemperatureMultiplier = 0;

            if (foodType == "Dry") TTT += 60;
            else if (foodType == "Mid") TTT += 40;
            else if (foodType == "Wet") TTT += 30;
            else TTT += 40;

            bool thawBonus = false; 

            if (IsNearFire(10f) && !IsInBackpack() && !IsInContainer())
            {
                float fireTemp = GetNearestFireTemp(10f);

                if (IsNearFire(1.3f) && !m_GearItem.IsAttachedToPlacePoint())
                {
                    if (GameManager.GetWeatherComponent().GetCurrentTemperatureWithoutHeatSources() > -15f && fireTemp >= 5f)
                    {
                        Temp = fireTemp;
                        thawBonus = true;
                    }
                }
                else if (m_GearItem.IsAttachedToPlacePoint())
                {
                    if(fireTemp >= 10f)
                    {
                        Temp = fireTemp + 10f;
                        thawBonus = true;
                    }
                }
                else //if player isn't near the food item, the temperature is taken from the air temperature (since it doesn't vary much from fire distance to player distance) and the fire temperature
                {
                    Temp += fireTemp;
                }
            }
          

            TemperatureMultiplier = 1f + (Temp / (60f)) * 2f;

            TTT = TTT * TemperatureMultiplier;

            if(thawBonus) TTT *= 4f;

            if (IsInBackpack())
            {
                TTT /= 1.5f;
            }
            else if (IsInContainer())
            {
                TTT /= 2f;
            }

            return TTT;
        }

        public string GetFoodType(string item)
        {
            if (item == "GEAR_Crackers" || item.Contains("Cereal") || item == "GEAR_KetchupChips" || item == "GEAR_BeefJerky" || item == "GEAR_GranolaBar" || item == "GEAR_CandyBar" || item.Contains("Stew") || item.Contains("Pie") || item == "GEAR_CuredMeat" || item == "GEAR_CuredFish" || item == "GEAR_DriedApples" || item.ToLowerInvariant().Contains("pemmican")) return "Dry";
            else if (item.ToLowerInvariant().Contains("meat") || item.Contains("CohoSalmon") || item.Contains("LakeWhiteFish") || item.Contains("RainbowTrout") || item.Contains("SmallMouthBass") || item == "GEAR_PinnacleCanPeaches" || item == "GEAR_TomatoSoupCan" || item == "GEAR_CannedSardines" || item == "GEAR_PinnacleCanPeaches" || item == "GEAR_CondensedMilk" || item.Contains("Corn") || item.Contains("Broth") || item.Contains("Soup") || item.Contains("Pickles")) return "Wet";
            else return "Mid";
        }

        private bool IsNearFire(float distance)
        {
            return GameManager.GetFireManagerComponent().GetDistanceToClosestFire(this.transform.position) < distance;
        }

        private float GetNearestFireTemp(float distance)
        {
                Fire closestFire = GameManager.GetFireManagerComponent().GetClosestFire(this.gameObject.transform.position);
                return closestFire.m_HeatSource.m_TempIncrease;
        }

        public bool IsAirTempPositive()
        {
            return (GameManager.GetWeatherComponent().GetCurrentTemperature() > 0) ? true : false;
        }

        public float GetCurrentAirTemp()
        {
            return GameManager.GetWeatherComponent().GetCurrentTemperature();
        }

        public void BlowUpSoda()
        {
            if (GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() - m_TimeSodaBeenFrozen >= 6)
            {
                Destroy(m_GearItem.gameObject);
            }
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
            return m_GearItem.m_InPlayerInventory;
        }

        public bool IsInContainer()
        {
            return this.gameObject.GetComponent<GearItem>().IsInsideContainer();
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

        public float GetDecayMod()
        {
            return 1 + m_PercentFrozen / 100;
        }

    }
}

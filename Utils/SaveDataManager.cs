using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using ModData;
using MelonLoader;

namespace FrozenFood.Utils
{
    internal class SaveDataManager
    {

        ModDataManager dm = new ModDataManager("Frozen Food", false);

        Dictionary<string, string> dataDict = new Dictionary<string, string>();

        public void Save(string data, string suffix)
        {
           
            if (dataDict.ContainsKey(suffix))
            {
                dataDict[suffix] = data;
            }
            else
            {
                dataDict.Add(suffix, data);
            }

            string? dataDictString;

            dataDictString = JsonSerializer.Serialize<Dictionary<string, string>>(dataDict);

            dm.Save(dataDictString);
        }

        public string LoadFrozenFoodData(string suffix)
        {
            bool flag = LoadDataStruct();

            //if no data exists at all, don't try looking for individual identifiers/items OR if the individual identifier/item cannot be found
            if (!flag || !dataDict.ContainsKey(suffix)) return null;

            string? jsonData = null;
            try
            {
                jsonData = dataDict[suffix];
            }
            catch(Exception e)
            {
                MelonLogger.Error("Unable to fetch frozen food data from data dictionary, returning null");
            }

            return jsonData;
        }

       public bool LoadDataStruct()
        {
            string? jsonDataStruct = dm.Load();

            if(jsonDataStruct is null)
            {
                return false;
            }

            dataDict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonDataStruct);
            return true;
        }

    }
}

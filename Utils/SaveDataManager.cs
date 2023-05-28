using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModData;

namespace FrozenFood.Utils
{
    internal class SaveDataManager
    {

        ModDataManager dm = new ModDataManager("Frozen Food", false);
        public void Save(string data, string suffix)
        {
            dm.Save(data, suffix);
        }

        public string LoadFrozenFoodData(string suffix)
        {
            string? jsonData = dm.Load(suffix);

            return jsonData;
        }

    }
}

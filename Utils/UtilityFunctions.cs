using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrozenFood.Utils
{
    internal class UtilityFunctions
    {

        public static float MapPercentageToVariable(double percentage, float minVariableValue = 1.0f, float maxVariableValue = 2.0f)
        {

            double minPercentage = 0;
            double maxPercentage = 100;

            float variable = (float)((percentage - minPercentage) / (maxPercentage - minPercentage) * (maxVariableValue - minVariableValue) + minVariableValue);

            variable = Math.Max(minVariableValue, Math.Min(maxVariableValue, variable));

            return variable;
        }


    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrozenFood
{
    internal class FrozenFoodSaveDataProxy
    {

        public float m_PercentFrozen { get; set; }

        public bool m_IsInBackpack { get; set; }
        public float m_HoursRemainingOnClosestFire { get; set; }
        public float m_MaxFireTemperature { get; set; }
        public float m_ActualTemperature { get; set; }
        public float m_HoursPlayedAtTimeOfSave { get; set; }

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Vintagestory.API.Common;

namespace FromGoldenCombs.config
{
    class FromGoldenCombsConfig
    {
        public float SkepDaysToHarvestIn30DayMonths = 7;
        public float ClayPotDaysToHarvestIn30DayMonths = 7;
        public float LangstrothDaysToHarvestIn30DayMonths = 3.5f;
        public int MaxStackSize = 6;
        public int baseframedurability = 32;
        public int minFramePerCycle = 2;
        public int maxFramePerCycle = 3;
        public bool showcombpoptime = true;
        public int CeramicPotMinYield { get; set; } = 2;
        public int CeramicPotMaxYield { get; set; } = 5;
        public int FrameMinYield { get; set; } = 2;
        public int FrameMaxYield { get; set; } = 5;
        public int SkepMinYield { get; set; } = 1;
        public int SkepMaxYield { get; set; } = 3;

        //private ArrayList HiveSeasons = new();

        public FromGoldenCombsConfig()
        {}

        public static FromGoldenCombsConfig Current { get; set; }
       

        public static FromGoldenCombsConfig GetDefault()
        {
            FromGoldenCombsConfig defaultConfig = new();

            defaultConfig.SkepDaysToHarvestIn30DayMonths = 7;
            defaultConfig.ClayPotDaysToHarvestIn30DayMonths = 7;
            defaultConfig.LangstrothDaysToHarvestIn30DayMonths = 7f;
            defaultConfig.SkepMinYield = 1;
            defaultConfig.SkepMaxYield = 3;
            defaultConfig.CeramicPotMinYield= 2;
            defaultConfig.CeramicPotMaxYield= 4;
            defaultConfig.minFramePerCycle = 2;
            defaultConfig.maxFramePerCycle = 3;
            defaultConfig.FrameMinYield = 2;
            defaultConfig.FrameMaxYield = 4;
            defaultConfig.MaxStackSize = 6;
            defaultConfig.baseframedurability = 32;
            defaultConfig.minFramePerCycle = 2;
            defaultConfig.maxFramePerCycle = 4;
            defaultConfig.showcombpoptime = true;
            return defaultConfig;
        }

    }
}

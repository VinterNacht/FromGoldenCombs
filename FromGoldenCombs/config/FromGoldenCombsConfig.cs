using ProtoBuf;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace FromGoldenCombs.config
{
    [ProtoContract()]
    class FromGoldenCombsConfig
    {

        /// <summary>The number of days until a skep is ready to harvest when using 30 day months</summary>
        [ProtoMember(1)]
        public float SkepDaysToHarvestIn30DayMonths = 7;

        /// <summary>The number of days until a ceramic hive is ready to harvest when using 30 day months</summary>
        [ProtoMember(2)]
        public float ClayPotDaysToHarvestIn30DayMonths = 7;

        /// <summary>The number of days until a Langstroth Hive is ready to harvest when using 30 day months</summary>
        [ProtoMember(3)]
        public float LangstrothDaysToHarvestIn30DayMonths = 3.5f;

        /// <summary>Maximum number of langstroth blocks that can be put in a Langstroth Stack</summary>
        [ProtoMember(4)]
        public int MaxStackSize = 6;

        /// <summary>The default durability of beeframes</summary>
        [ProtoMember(5)]
        public int baseframedurability = 32;

        /// <summary>The minimum number of frames filled per cycle</summary>
        [ProtoMember(6)]
        public int minFramePerCycle = 2;

        /// <summary>The maximum number of frames filled per cycle</summary>
        [ProtoMember(7)]
        public int maxFramePerCycle = 3;

        /// <summary>Whether to show the time left until new comb/frames are produced</summary>
        
        [ProtoMember(8)]
        public bool showcombpoptime = true;

        /// <summary>Minimum number of honeycombs produced when a honeypot is harvested from a ceramic hive</summary>
        /// <value>The ceramic pot minimum yield.</value>
        [ProtoMember(9)]
        public int CeramicPotMinYield { get; set; } = 2;

        /// <summary>Gets or sets the maximum honey produced when a honeypot is harvest from a ceramic hive.</summary>
        /// <value>The ceramic pot maximum yield.</value>
        [ProtoMember(10)]
        public int CeramicPotMaxYield { get; set; } = 5;

        /// <summary>Minimum number of frames filled per cycle in a Langstroth Hive</summary>
        /// <value>The frame minimum yield.</value>
        [ProtoMember(11)]
        public int FrameMinYield { get; set; } = 2;

        /// <summary>Maximum number of frames filled per cycle in a Langstroth Hive</summary>
        /// <value>The frame maximum yield.</value>
        [ProtoMember(12)]
        public int FrameMaxYield { get; set; } = 5;

        /// <summary>Gets or sets the minimum honeycomb yield from a skep.</summary>
        /// <value>The skep minimum yield.</value>
        [ProtoMember(13)]
        public int SkepMinYield { get; set; } = 1;

        /// <summary>Gets or sets the maximum honeycomb yield from a skep.</summary>
        /// <value>The skep maximum yield.</value>
        [ProtoMember(14)]
        public int SkepMaxYield { get; set; } = 3;

        /// <summary>Set the minimum temperature at which Ceramic Hive Bees will fly.</summary>
        /// <value>The minimum temperature in C as a float</value>
        [ProtoMember(15)]
        public float SkepHiveMinTemp { get; set; } = 10f;
        [ProtoMember(16)]
        public float SkepHiveMaxTemp { get; set; } = 37f;
        [ProtoMember(17)]
        public float CeramicHiveMinTemp { get; set; } = 10f;
        [ProtoMember(18)]       
        public float CeramicHiveMaxTemp { get; set; } = 37f;

        /// <summary>Set the minimum temperature at which Langstroth Hive Bees will fly.</summary>
        /// <value>The minimum temperature in C as a float</value>
        [ProtoMember(19)]
        public float LangstrothHiveMinTemp { get; set; } = 10f;

        [ProtoMember(20)]
        public float LangstrothHiveMaxTemp { get; set; } = 37f;

        [ProtoMember(21)]
        public double skepBaseChargesPerDay; //Number of hours until the hive accumulates a new grow charge.
      
        [ProtoMember(22)]
        public int skepMaxCropCharges = 8;

        [ProtoMember(23)]
        public int skepCropRange = 8;

        [ProtoMember(24)]
        public double ceramicBaseChargesPerDay; //Number of hours until the hive accumulates a new grow charge.

        [ProtoMember(25)]
        public int ceramicMaxCropCharges = 8;

        [ProtoMember(26)]
        public int ceramicCropRange = 8;

        [ProtoMember(27)]
        public double langstrothBaseChargesPerDay; //Number of hours until the hive accumulates a new grow charge.

        [ProtoMember(28)]
        public int langstrothMaxCropCharges = 8;

        [ProtoMember(29)]
        public int langstrothCropRange = 8;

        [ProtoMember(30)]
        public bool showCurrentCropCharges = true;

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
            defaultConfig.SkepHiveMinTemp = 10f;
            defaultConfig.SkepHiveMaxTemp = 37f;
            defaultConfig.CeramicHiveMinTemp = 10f;
            defaultConfig.CeramicHiveMaxTemp = 37f;
            defaultConfig.LangstrothHiveMinTemp = 10f;
            defaultConfig.LangstrothHiveMaxTemp = 37f;
            defaultConfig.skepBaseChargesPerDay = 1; 
            defaultConfig.skepMaxCropCharges = 50;
            defaultConfig.skepCropRange = 10;
            defaultConfig.ceramicBaseChargesPerDay = 2;
            defaultConfig.ceramicMaxCropCharges = 90;
            defaultConfig.ceramicCropRange = 12;
            defaultConfig.langstrothBaseChargesPerDay = 3;
            defaultConfig.langstrothMaxCropCharges = 150;
            defaultConfig.langstrothCropRange = 17;
            defaultConfig.showCurrentCropCharges = true;

            return defaultConfig;
        }

        internal static void createConfig(ICoreAPI api)
        {
            try
            {
                var Config = api.LoadModConfig<FromGoldenCombsConfig>("fromgoldencombs.json");
                if (Config != null)
                {
                    api.Logger.Notification(Lang.Get("modconfigload"));
                    FromGoldenCombsConfig.Current = Config;
                }
                else
                {
                    api.Logger.Notification(Lang.Get("nomodconfig"));
                    FromGoldenCombsConfig.Current = FromGoldenCombsConfig.GetDefault();
                }
            }
            catch
            {
                FromGoldenCombsConfig.Current = FromGoldenCombsConfig.GetDefault();
                api.Logger.Error(Lang.Get("defaultloaded"));
            }
            finally
            {
                api.StoreModConfig(FromGoldenCombsConfig.Current, "fromgoldencombs.json");
            }
        }
    }
}

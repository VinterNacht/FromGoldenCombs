using ProtoBuf;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace FromGoldenCombs.Util.config
{
    [ProtoContract()]
    class FGCServerConfig
    {
        [ProtoMember(0)]
        public double ConfigVersion = 1.0;
        /// <summary>The number of days until a skep is ready to harvest when using 30 day months</summary>
        /// 
        public bool retainConfigOnVersionChange = false;

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
        public double skepBaseChargesPerDay = 1; //Number of hours until the hive accumulates a new grow charge.

        [ProtoMember(22)]
        public int skepMaxCropCharges = 50;

        [ProtoMember(23)]
        public int skepCropRange = 10;

        [ProtoMember(24)]
        public double ceramicBaseChargesPerDay = 2; //Number of hours until the hive accumulates a new grow charge.

        [ProtoMember(25)]
        public int ceramicMaxCropCharges = 90;

        [ProtoMember(26)]
        public int ceramicCropRange = 13;

        [ProtoMember(27)]
        public double langstrothBaseChargesPerDay = 3; //Number of hours until the hive accumulates a new grow charge.

        [ProtoMember(28)]
        public int langstrothMaxCropCharges = 150;

        [ProtoMember(29)]
        public int langstrothCropRange = 17;

        [ProtoMember(30)]
        public bool showCurrentCropCharges = true;

        public FGCServerConfig()
        { }

        public static FGCServerConfig Current { get; set; }


        public static FGCServerConfig GetServerDefault()
        {
            FGCServerConfig defaultServerConfig = new();

            defaultServerConfig.ConfigVersion = 1.1;
            defaultServerConfig.retainConfigOnVersionChange = false;
            defaultServerConfig.SkepDaysToHarvestIn30DayMonths = 7;
            defaultServerConfig.ClayPotDaysToHarvestIn30DayMonths = 7;
            defaultServerConfig.LangstrothDaysToHarvestIn30DayMonths = 7f;
            defaultServerConfig.SkepMinYield = 1;
            defaultServerConfig.SkepMaxYield = 3;
            defaultServerConfig.CeramicPotMinYield = 2;
            defaultServerConfig.CeramicPotMaxYield = 4;
            defaultServerConfig.minFramePerCycle = 2;
            defaultServerConfig.maxFramePerCycle = 3;
            defaultServerConfig.FrameMinYield = 2;
            defaultServerConfig.FrameMaxYield = 4;
            defaultServerConfig.MaxStackSize = 6;
            defaultServerConfig.baseframedurability = 32;
            defaultServerConfig.minFramePerCycle = 2;
            defaultServerConfig.maxFramePerCycle = 4;
            defaultServerConfig.showcombpoptime = true;
            defaultServerConfig.SkepHiveMinTemp = 10f;
            defaultServerConfig.SkepHiveMaxTemp = 37f;
            defaultServerConfig.CeramicHiveMinTemp = 10f;
            defaultServerConfig.CeramicHiveMaxTemp = 37f;
            defaultServerConfig.LangstrothHiveMinTemp = 10f;
            defaultServerConfig.LangstrothHiveMaxTemp = 37f;
            defaultServerConfig.skepBaseChargesPerDay = 1;
            defaultServerConfig.skepMaxCropCharges = 50;
            defaultServerConfig.skepCropRange = 5;
            defaultServerConfig.ceramicBaseChargesPerDay = 2;
            defaultServerConfig.ceramicMaxCropCharges = 90;
            defaultServerConfig.ceramicCropRange = 6;
            defaultServerConfig.langstrothBaseChargesPerDay = 3;
            defaultServerConfig.langstrothMaxCropCharges = 150;
            defaultServerConfig.langstrothCropRange = 8;
            defaultServerConfig.showCurrentCropCharges = true;

            return defaultServerConfig;
        }

        internal static void createServerConfig(ICoreAPI api)
        {
            double MasterServerConfigVersion = 1.1;
            try
            {
                var ServerConfig = api.LoadModConfig<FGCServerConfig>("fromgoldencombs/fromgoldencombsserver.json");
                if (ServerConfig != null && ServerConfig.ConfigVersion == MasterServerConfigVersion)
                {
                    api.Logger.Notification(Lang.Get("fromgoldencombs:modserverconfigload"));
                    Current = ServerConfig;
                }
                else
                {
                    if((ServerConfig?.ConfigVersion != MasterServerConfigVersion) && ServerConfig.retainConfigOnVersionChange)
                    {
                        api.Logger.Notification(Lang.Get("fromgoldencombs:wrongserverconfigversion"));
                    }
                    api.Logger.Notification(Lang.Get("fromgoldencombs:nomodserverconfig"));
                    Current = GetServerDefault();
                }
            }
            catch
            {
                Current = GetServerDefault();
                api.Logger.Error(Lang.Get("fromgoldencombs:defaultserverconfigloaded"));
            }
            finally
            {
                api.StoreModConfig(Current, "fromgoldencombs/fromgoldencombsserver.json");
            }
        }
    }
}

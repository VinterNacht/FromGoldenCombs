using ProtoBuf;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace FromGoldenCombs.Util.config
{
    [ProtoContract()]
    class FGCServerConfig
    {
        [ProtoMember(1)]
        public double ConfigVersion = 1.1;
        [ProtoMember(2)]
        public bool retainConfigOnVersionChange = false;
        [ProtoMember(3)]
        public float SkepDaysToHarvestIn30DayMonths = 7;
        [ProtoMember(4)]
        public float ClayPotDaysToHarvestIn30DayMonths = 7;
        [ProtoMember(5)]
        public float LangstrothDaysToHarvestIn30DayMonths = 3.5f;
        [ProtoMember(6)]
        public int MaxStackSize = 6;
        [ProtoMember(7)]
        public int baseframedurability = 32;
        [ProtoMember(8)]
        public int minFramePerCycle = 2;
        [ProtoMember(9)]
        public int maxFramePerCycle = 3;
        [ProtoMember(10)]
        public bool showcombpoptime = true;
        [ProtoMember(11)]
        public int CeramicPotMinYield { get; set; } = 2;
        [ProtoMember(12)]
        public int CeramicPotMaxYield { get; set; } = 5;
        [ProtoMember(13)]
        public int FrameMinYield { get; set; } = 2;
        [ProtoMember(14)]
        public int FrameMaxYield { get; set; } = 5;
        [ProtoMember(15)]
        public int SkepMinYield { get; set; } = 1;
        [ProtoMember(16)]
        public int SkepMaxYield { get; set; } = 3;
        [ProtoMember(17)]
        public float SkepHiveMinTemp { get; set; } = 10f;
        [ProtoMember(18)]
        public float SkepHiveMaxTemp { get; set; } = 37f;
        [ProtoMember(19)]
        public float CeramicHiveMinTemp { get; set; } = 10f;
        [ProtoMember(20)]
        public float CeramicHiveMaxTemp { get; set; } = 37f;
        [ProtoMember(21)]
        public float LangstrothHiveMinTemp { get; set; } = 10f;
        [ProtoMember(22)]
        public float LangstrothHiveMaxTemp { get; set; } = 37f;
        [ProtoMember(23)]
        public double skepBaseChargesPerDay = 1;
        [ProtoMember(24)]
        public int skepMaxCropCharges = 50;
        [ProtoMember(25)]
        public int skepCropRange = 5;
        [ProtoMember(26)]
        public double ceramicBaseChargesPerDay = 2;
        [ProtoMember(27)]
        public int ceramicMaxCropCharges = 100;
        [ProtoMember(28)]
        public int ceramicCropRange = 7;
        [ProtoMember(29)]
        public double langstrothBaseChargesPerDay = 3;
        [ProtoMember(30)]
        public int langstrothMaxCropCharges = 150;
        [ProtoMember(31)]
        public int langstrothCropRange = 8;
        [ProtoMember(32)]
        public bool showExtraBeehiveInfo = true;

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
            defaultServerConfig.ceramicMaxCropCharges = 100;
            defaultServerConfig.ceramicCropRange = 6;
            defaultServerConfig.langstrothBaseChargesPerDay = 3;
            defaultServerConfig.langstrothMaxCropCharges = 150;
            defaultServerConfig.langstrothCropRange = 8;
            defaultServerConfig.showExtraBeehiveInfo = true;

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
                    if (ServerConfig != null)
                    {
                        if ((ServerConfig?.ConfigVersion != MasterServerConfigVersion) && ServerConfig.retainConfigOnVersionChange)
                        {
                            api.Logger.Notification(Lang.Get("fromgoldencombs:wrongserverconfigversion"));
                        }
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

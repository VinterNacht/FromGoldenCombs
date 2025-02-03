using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace FromGoldenCombs.Util.config
{
    public class NetworkHandler
    {
        internal void RegisterMessages(ICoreAPI api)
        {
            api.Network
                .RegisterChannel("fromgoldencombschannel")
                .RegisterMessageType(typeof(FGCConfigFromServerMessage))
                .RegisterMessageType(typeof(OnPlayerLoginMessage));

            ;
        }

        #region Client
        IClientNetworkChannel clientChannel;
        ICoreClientAPI clientApi;
        public void InitializeClientSideNetworkHandler(ICoreClientAPI capi)
        {
            clientApi = capi;

            clientChannel = capi.Network.GetChannel("fromgoldencombschannel")
                .SetMessageHandler<FGCConfigFromServerMessage>(RecieveFGCConfigAction);
            ;

        }

        //SetToolConfigValues received from Server
        private void RecieveFGCConfigAction(FGCConfigFromServerMessage fgcConfig)
        {
            FGCServerConfig.Current.SkepDaysToHarvestIn30DayMonths = fgcConfig.SkepDaysToHarvestIn30DayMonths;
            FGCServerConfig.Current.ClayPotDaysToHarvestIn30DayMonths = fgcConfig.ClayPotDaysToHarvestIn30DayMonths;
            FGCServerConfig.Current.LangstrothDaysToHarvestIn30DayMonths = fgcConfig.LangstrothDaysToHarvestIn30DayMonths;
            FGCServerConfig.Current.SkepMinYield = fgcConfig.SkepMinYield;
            FGCServerConfig.Current.SkepMaxYield = fgcConfig.SkepMaxYield;
            FGCServerConfig.Current.CeramicPotMinYield = fgcConfig.CeramicPotMinYield;
            FGCServerConfig.Current.CeramicPotMaxYield = fgcConfig.CeramicPotMaxYield;
            FGCServerConfig.Current.minFramePerCycle = fgcConfig.minFramePerCycle;
            FGCServerConfig.Current.maxFramePerCycle = fgcConfig.maxFramePerCycle;
            FGCServerConfig.Current.FrameMinYield = fgcConfig.FrameMinYield;
            FGCServerConfig.Current.FrameMaxYield = fgcConfig.FrameMaxYield;
            FGCServerConfig.Current.MaxStackSize = fgcConfig.maxStackSize;
            FGCServerConfig.Current.baseframedurability = fgcConfig.baseFrameDurability;
            FGCServerConfig.Current.minFramePerCycle = fgcConfig.minFramePerCycle;
            FGCServerConfig.Current.maxFramePerCycle = fgcConfig.maxStackSize;
            FGCServerConfig.Current.showcombpoptime = fgcConfig.showcombpoptime;
            FGCServerConfig.Current.CeramicHiveMinTemp = fgcConfig.CeramicHiveMinTemp;
            FGCServerConfig.Current.LangstrothHiveMinTemp = fgcConfig.LangstrothHiveMinTemp;
            FGCServerConfig.Current.skepBaseChargesPerDay = fgcConfig.skepCropChargeHoursIn30DayMonths;
            FGCServerConfig.Current.skepMaxCropCharges = fgcConfig.skepMaxCropCharges;
            FGCServerConfig.Current.skepCropRange = fgcConfig.skepCropRange;
            FGCServerConfig.Current.ceramicBaseChargesPerDay = fgcConfig.ceramicCropChargeHoursIn30DayMonths;
            FGCServerConfig.Current.ceramicMaxCropCharges = fgcConfig.ceramicMaxCropCharges;
            FGCServerConfig.Current.ceramicCropRange = fgcConfig.ceramicCropRange;
            FGCServerConfig.Current.langstrothBaseChargesPerDay = fgcConfig.langstrothCropChargeHoursIn30DayMonths;
            FGCServerConfig.Current.langstrothMaxCropCharges = fgcConfig.langstrothMaxCropCharges;
            FGCServerConfig.Current.langstrothCropRange = fgcConfig.langstrothCropRange;
            FGCServerConfig.Current.showCurrentCropCharges = true;
        }

        #endregion

        #region server
        IServerNetworkChannel serverChannel;
        ICoreServerAPI serverApi;
        public void InitializeServerSideNetworkHandler(ICoreServerAPI api)
        {
            serverApi = api;

            //Listen for player join events
            api.Event.PlayerJoin += OnPlayerJoin;

            serverChannel = api.Network.GetChannel("fromgoldencombschannel")
                .SetMessageHandler<OnPlayerLoginMessage>(OnPlayerJoin);
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            OnPlayerJoin(player, new OnPlayerLoginMessage());
        }

        //Send a packet on the client channel containing a new instance of FGCConfigFromServerMessage
        //Which is pre-loaded on creation with all the values for Tool Config.
        private void OnPlayerJoin(IServerPlayer fromPlayer, OnPlayerLoginMessage packet)
        {
            serverChannel.SendPacket(new FGCConfigFromServerMessage(), fromPlayer);
        }


        #endregion


        [ProtoContract]
        class FGCConfigFromServerMessage
        {
            [ProtoMember(1)]
            public float SkepDaysToHarvestIn30DayMonths = FGCServerConfig.Current.SkepDaysToHarvestIn30DayMonths;
            [ProtoMember(2)]
            public float ClayPotDaysToHarvestIn30DayMonths = FGCServerConfig.Current.ClayPotDaysToHarvestIn30DayMonths;
            [ProtoMember(3)]
            public float LangstrothDaysToHarvestIn30DayMonths = FGCServerConfig.Current.LangstrothDaysToHarvestIn30DayMonths;
            [ProtoMember(4)]
            public int maxStackSize = FGCServerConfig.Current.MaxStackSize;
            [ProtoMember(5)]
            public int baseFrameDurability = FGCServerConfig.Current.baseframedurability;
            [ProtoMember(6)]
            public int minFramePerCycle = FGCServerConfig.Current.minFramePerCycle;
            [ProtoMember(7)]
            public int maxFramePerCycle = FGCServerConfig.Current.maxFramePerCycle;
            [ProtoMember(8)]
            public bool showcombpoptime = FGCServerConfig.Current.showcombpoptime;
            [ProtoMember(9)]
            public int CeramicPotMinYield = FGCServerConfig.Current.CeramicPotMinYield;
            [ProtoMember(10)]
            public int CeramicPotMaxYield = FGCServerConfig.Current.CeramicPotMaxYield;
            [ProtoMember(11)]
            public int FrameMinYield = FGCServerConfig.Current.FrameMinYield;
            [ProtoMember(12)]
            public int FrameMaxYield = FGCServerConfig.Current.FrameMaxYield;
            [ProtoMember(13)]
            public int SkepMinYield = FGCServerConfig.Current.SkepMinYield;
            [ProtoMember(14)]
            public int SkepMaxYield = FGCServerConfig.Current.SkepMaxYield;
            [ProtoMember(15)]
            public float SkepHiveMinTemp = FGCServerConfig.Current.SkepHiveMinTemp;
            [ProtoMember(16)]
            public float SkepHiveMaxTemp = FGCServerConfig.Current.SkepHiveMaxTemp;
            [ProtoMember(17)]
            public float CeramicHiveMinTemp = FGCServerConfig.Current.CeramicHiveMinTemp;
            [ProtoMember(18)]
            public float CeramicHiveMaxTemp = FGCServerConfig.Current.CeramicHiveMaxTemp;
            [ProtoMember(19)]
            public float LangstrothHiveMinTemp = FGCServerConfig.Current.LangstrothHiveMinTemp;
            [ProtoMember(20)]
            public float LangstrothHiveMaxTemp = FGCServerConfig.Current.LangstrothHiveMaxTemp;
            [ProtoMember(21)]
            public double skepCropChargeHoursIn30DayMonths = FGCServerConfig.Current.skepBaseChargesPerDay; //Number of hours until the hive accumulates a new grow charge.

            [ProtoMember(22)]
            public int skepMaxCropCharges = FGCServerConfig.Current.skepMaxCropCharges;

            [ProtoMember(23)]
            public int skepCropRange = FGCServerConfig.Current.skepCropRange;

            [ProtoMember(24)]
            public double ceramicCropChargeHoursIn30DayMonths = FGCServerConfig.Current.ceramicBaseChargesPerDay; //Number of hours until the hive accumulates a new grow charge.

            [ProtoMember(25)]
            public int ceramicMaxCropCharges = FGCServerConfig.Current.ceramicMaxCropCharges;

            [ProtoMember(26)]
            public int ceramicCropRange = FGCServerConfig.Current.ceramicCropRange;

            [ProtoMember(27)]
            public double langstrothCropChargeHoursIn30DayMonths = FGCServerConfig.Current.langstrothBaseChargesPerDay; //Number of hours until the hive accumulates a new grow charge.

            [ProtoMember(28)]
            public int langstrothMaxCropCharges = FGCServerConfig.Current.langstrothMaxCropCharges;

            [ProtoMember(29)]
            public int langstrothCropRange = FGCServerConfig.Current.langstrothCropRange;

            [ProtoMember(30)]
            public bool showCurrentCropCharges = FGCServerConfig.Current.showCurrentCropCharges;
        }

        [ProtoContract]
        class OnPlayerLoginMessage
        {
            [ProtoMember(1)]
            IPlayer[] fromPlayer;
        }
    }
}

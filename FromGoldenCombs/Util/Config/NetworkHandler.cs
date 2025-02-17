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

        }

        //SetToolConfigValues received from Server
        private void RecieveFGCConfigAction(FGCConfigFromServerMessage fgcConfig)
        {
            FGCServerConfig.Current.ConfigVersion = fgcConfig.ConfigVersion;
            FGCServerConfig.Current.retainConfigOnVersionChange = fgcConfig.retainConfigOnVersionChange;
            FGCServerConfig.Current.SkepDaysToHarvestIn30DayMonths = fgcConfig.SkepDaysToHarvestIn30DayMonths;
            FGCServerConfig.Current.ClayPotDaysToHarvestIn30DayMonths = fgcConfig.ClayPotDaysToHarvestIn30DayMonths;
            FGCServerConfig.Current.LangstrothDaysToHarvestIn30DayMonths = fgcConfig.LangstrothDaysToHarvestIn30DayMonths;
            FGCServerConfig.Current.MaxStackSize = fgcConfig.maxStackSize;
            FGCServerConfig.Current.baseframedurability = fgcConfig.baseFrameDurability;
            FGCServerConfig.Current.minFramePerCycle = fgcConfig.minFramePerCycle;
            FGCServerConfig.Current.maxFramePerCycle = fgcConfig.maxFramePerCycle;
            FGCServerConfig.Current.showcombpoptime = fgcConfig.showcombpoptime;
            FGCServerConfig.Current.CeramicPotMinYield = fgcConfig.CeramicPotMinYield;
            FGCServerConfig.Current.CeramicPotMaxYield = fgcConfig.CeramicPotMaxYield;
            FGCServerConfig.Current.FrameMinYield = fgcConfig.FrameMinYield;
            FGCServerConfig.Current.FrameMaxYield = fgcConfig.FrameMaxYield;
            FGCServerConfig.Current.SkepMinYield = fgcConfig.SkepMinYield;
            FGCServerConfig.Current.SkepMaxYield = fgcConfig.SkepMaxYield;
            FGCServerConfig.Current.SkepHiveMinTemp = fgcConfig.SkepHiveMinTemp;
            FGCServerConfig.Current.SkepHiveMaxTemp = fgcConfig.SkepHiveMaxTemp;
            FGCServerConfig.Current.CeramicHiveMinTemp = fgcConfig.CeramicHiveMinTemp;
            FGCServerConfig.Current.CeramicHiveMaxTemp = fgcConfig.CeramicHiveMaxTemp;
            FGCServerConfig.Current.LangstrothHiveMinTemp = fgcConfig.LangstrothHiveMinTemp;
            FGCServerConfig.Current.LangstrothHiveMaxTemp = fgcConfig.LangstrothHiveMaxTemp;
            FGCServerConfig.Current.skepBaseChargesPerDay = fgcConfig.skepBaseChargesPerDay;
            FGCServerConfig.Current.skepMaxCropCharges = fgcConfig.skepMaxCropCharges;
            FGCServerConfig.Current.skepCropRange = fgcConfig.skepCropRange;
            FGCServerConfig.Current.ceramicBaseChargesPerDay = fgcConfig.ceramicBaseChargesPerDay;
            FGCServerConfig.Current.ceramicMaxCropCharges = fgcConfig.ceramicMaxCropCharges;
            FGCServerConfig.Current.ceramicCropRange = fgcConfig.ceramicCropRange;
            FGCServerConfig.Current.langstrothBaseChargesPerDay = fgcConfig.langstrothBaseChargesPerDay;
            FGCServerConfig.Current.langstrothMaxCropCharges = fgcConfig.langstrothMaxCropCharges;
            FGCServerConfig.Current.langstrothCropRange = fgcConfig.langstrothCropRange;
            FGCServerConfig.Current.showExtraBeehiveInfo = fgcConfig.showCurrentCropCharges;
        }

        #endregion

        #region Server
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




        [ProtoContract]
        class FGCConfigFromServerMessage
        {

            [ProtoMember(1)]
            public double ConfigVersion = FGCServerConfig.Current.ConfigVersion;
            [ProtoMember(2)]
            public bool retainConfigOnVersionChange = FGCServerConfig.Current.retainConfigOnVersionChange;
            [ProtoMember(3)]
            public float SkepDaysToHarvestIn30DayMonths = FGCServerConfig.Current.SkepDaysToHarvestIn30DayMonths;
            [ProtoMember(4)]            
            public float ClayPotDaysToHarvestIn30DayMonths = FGCServerConfig.Current.ClayPotDaysToHarvestIn30DayMonths;
            [ProtoMember(5)]
            public float LangstrothDaysToHarvestIn30DayMonths = FGCServerConfig.Current.LangstrothDaysToHarvestIn30DayMonths;
            [ProtoMember(6)]
            public int maxStackSize = FGCServerConfig.Current.MaxStackSize;
            [ProtoMember(7)]
            public int baseFrameDurability = FGCServerConfig.Current.baseframedurability;
            [ProtoMember(8)]
            public int minFramePerCycle = FGCServerConfig.Current.minFramePerCycle;
            [ProtoMember(9)]
            public int maxFramePerCycle = FGCServerConfig.Current.maxFramePerCycle;
            [ProtoMember(10)]
            public bool showcombpoptime = FGCServerConfig.Current.showcombpoptime;
            [ProtoMember(11)]
            public int CeramicPotMinYield = FGCServerConfig.Current.CeramicPotMinYield;
            [ProtoMember(12)]
            public int CeramicPotMaxYield = FGCServerConfig.Current.CeramicPotMaxYield;
            [ProtoMember(13)]
            public int FrameMinYield = FGCServerConfig.Current.FrameMinYield;
            [ProtoMember(14)]
            public int FrameMaxYield = FGCServerConfig.Current.FrameMaxYield;
            [ProtoMember(15)]
            public int SkepMinYield = FGCServerConfig.Current.SkepMinYield;
            [ProtoMember(16)]
            public int SkepMaxYield = FGCServerConfig.Current.SkepMaxYield;
            [ProtoMember(17)]
            public float SkepHiveMinTemp = FGCServerConfig.Current.SkepHiveMinTemp;
            [ProtoMember(18)]
            public float SkepHiveMaxTemp = FGCServerConfig.Current.SkepHiveMaxTemp;
            [ProtoMember(19)]
            public float CeramicHiveMinTemp = FGCServerConfig.Current.CeramicHiveMinTemp;
            [ProtoMember(20)]
            public float CeramicHiveMaxTemp = FGCServerConfig.Current.CeramicHiveMaxTemp;
            [ProtoMember(21)]
            public float LangstrothHiveMinTemp = FGCServerConfig.Current.LangstrothHiveMinTemp;
            [ProtoMember(22)]
            public float LangstrothHiveMaxTemp = FGCServerConfig.Current.LangstrothHiveMaxTemp;
            [ProtoMember(23)]
            public double skepBaseChargesPerDay = FGCServerConfig.Current.skepBaseChargesPerDay;
            [ProtoMember(24)]
            public int skepMaxCropCharges = FGCServerConfig.Current.skepMaxCropCharges;
            [ProtoMember(25)]
            public int skepCropRange = FGCServerConfig.Current.skepCropRange;
            [ProtoMember(26)]
            public double ceramicBaseChargesPerDay = FGCServerConfig.Current.ceramicBaseChargesPerDay;
            [ProtoMember(27)]
            public int ceramicMaxCropCharges = FGCServerConfig.Current.ceramicMaxCropCharges;
            [ProtoMember(28)]
            public int ceramicCropRange = FGCServerConfig.Current.ceramicCropRange;
            [ProtoMember(29)]
            public double langstrothBaseChargesPerDay = FGCServerConfig.Current.langstrothBaseChargesPerDay;
            [ProtoMember(30)]
            public int langstrothMaxCropCharges = FGCServerConfig.Current.langstrothMaxCropCharges;
            [ProtoMember(31)]
            public int langstrothCropRange = FGCServerConfig.Current.langstrothCropRange;
            [ProtoMember(32)]
            public bool showCurrentCropCharges = FGCServerConfig.Current.showExtraBeehiveInfo;


        }

        [ProtoContract]
        class OnPlayerLoginMessage
        {
            [ProtoMember(1)]
            IPlayer[] fromPlayer;
        }

        #endregion
    }
}

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
            FGCServerConfig.Current.SkepHiveMinTemp = fgcConfig.SkepHiveMaxTemp;
            FGCServerConfig.Current.CeramicHiveMinTemp = fgcConfig.CeramicHiveMinTemp;
            FGCServerConfig.Current.CeramicHiveMaxTemp = fgcConfig.CeramicHiveMaxTemp;
            FGCServerConfig.Current.LangstrothHiveMinTemp = fgcConfig.LangstrothHiveMinTemp;
            FGCServerConfig.Current.LangstrothHiveMaxTemp = fgcConfig.LangstrothHiveMaxTemp;
            FGCServerConfig.Current.skepBaseChargesPerDay = fgcConfig.skepCropChargeHoursIn30DayMonths;
            FGCServerConfig.Current.skepMaxCropCharges = fgcConfig.skepMaxCropCharges;
            FGCServerConfig.Current.skepCropRange = fgcConfig.skepCropRange;
            FGCServerConfig.Current.ceramicBaseChargesPerDay = fgcConfig.ceramicCropChargeHoursIn30DayMonths;
            FGCServerConfig.Current.ceramicMaxCropCharges = fgcConfig.ceramicMaxCropCharges;
            FGCServerConfig.Current.ceramicCropRange = fgcConfig.ceramicCropRange;
            FGCServerConfig.Current.langstrothBaseChargesPerDay = fgcConfig.langstrothCropChargeHoursIn30DayMonths;
            FGCServerConfig.Current.langstrothMaxCropCharges = fgcConfig.langstrothMaxCropCharges;
            FGCServerConfig.Current.langstrothCropRange = fgcConfig.langstrothCropRange;
            FGCServerConfig.Current.showCurrentCropCharges = fgcConfig.showCurrentCropCharges;
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
            public double ConfigVersion;
            [ProtoMember(2)]
            public bool retainConfigOnVersionChange;
            [ProtoMember(3)]
            public float SkepDaysToHarvestIn30DayMonths;
            [ProtoMember(4)]            
            public float ClayPotDaysToHarvestIn30DayMonths;
            [ProtoMember(5)]
            public float LangstrothDaysToHarvestIn30DayMonths;
            [ProtoMember(6)]
            public int maxStackSize;
            [ProtoMember(7)]
            public int baseFrameDurability;
            [ProtoMember(8)]
            public int minFramePerCycle;
            [ProtoMember(9)]
            public int maxFramePerCycle;
            [ProtoMember(10)]
            public bool showcombpoptime;
            [ProtoMember(11)]
            public int CeramicPotMinYield;
            [ProtoMember(12)]
            public int CeramicPotMaxYield;
            [ProtoMember(13)]
            public int FrameMinYield;
            [ProtoMember(14)]
            public int FrameMaxYield;
            [ProtoMember(15)]
            public int SkepMinYield;
            [ProtoMember(16)]
            public int SkepMaxYield;
            [ProtoMember(17)]
            public float SkepHiveMinTemp;
            [ProtoMember(18)]
            public float SkepHiveMaxTemp;
            [ProtoMember(19)]
            public float CeramicHiveMinTemp;
            [ProtoMember(20)]
            public float CeramicHiveMaxTemp;
            [ProtoMember(21)]
            public float LangstrothHiveMinTemp;
            [ProtoMember(22)]
            public float LangstrothHiveMaxTemp;
            [ProtoMember(23)]
            public double skepCropChargeHoursIn30DayMonths;
            [ProtoMember(24)]
            public int skepMaxCropCharges;
            [ProtoMember(25)]
            public int skepCropRange;
            [ProtoMember(26)]
            public double ceramicCropChargeHoursIn30DayMonths;
            [ProtoMember(27)]
            public int ceramicMaxCropCharges;
            [ProtoMember(28)]
            public int ceramicCropRange;
            [ProtoMember(29)]
            public double langstrothCropChargeHoursIn30DayMonths;
            [ProtoMember(30)]
            public int langstrothMaxCropCharges;
            [ProtoMember(31)]
            public int langstrothCropRange;
            [ProtoMember(32)]
            public bool showCurrentCropCharges;


        }

        [ProtoContract]
        class OnPlayerLoginMessage
        {
            [ProtoMember(1)]
            IPlayer[] fromPlayer;
        }
    }
}

using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace FromGoldenCombs.config
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
            FromGoldenCombsConfig.Current.SkepDaysToHarvestIn30DayMonths = fgcConfig.SkepDaysToHarvestIn30DayMonths;
            FromGoldenCombsConfig.Current.ClayPotDaysToHarvestIn30DayMonths = fgcConfig.ClayPotDaysToHarvestIn30DayMonths;
            FromGoldenCombsConfig.Current.LangstrothDaysToHarvestIn30DayMonths = fgcConfig.LangstrothDaysToHarvestIn30DayMonths;
            FromGoldenCombsConfig.Current.SkepMinYield = fgcConfig.SkepMinYield;
            FromGoldenCombsConfig.Current.SkepMaxYield = fgcConfig.SkepMaxYield;
            FromGoldenCombsConfig.Current.CeramicPotMinYield = fgcConfig.CeramicPotMinYield;
            FromGoldenCombsConfig.Current.CeramicPotMaxYield = fgcConfig.CeramicPotMaxYield;
            FromGoldenCombsConfig.Current.minFramePerCycle = fgcConfig.minFramePerCycle;
            FromGoldenCombsConfig.Current.maxFramePerCycle = fgcConfig.maxFramePerCycle;
            FromGoldenCombsConfig.Current.FrameMinYield = fgcConfig.FrameMinYield;
            FromGoldenCombsConfig.Current.FrameMaxYield = fgcConfig.FrameMaxYield;
            FromGoldenCombsConfig.Current.MaxStackSize = fgcConfig.maxStackSize;
            FromGoldenCombsConfig.Current.baseframedurability = fgcConfig.baseFrameDurability;
            FromGoldenCombsConfig.Current.minFramePerCycle = fgcConfig.minFramePerCycle;
            FromGoldenCombsConfig.Current.maxFramePerCycle = fgcConfig.maxStackSize;
            FromGoldenCombsConfig.Current.showcombpoptime = fgcConfig.showcombpoptime;
            FromGoldenCombsConfig.Current.CeramicHiveMinTemp = fgcConfig.CeramicHiveMinTemp;
            FromGoldenCombsConfig.Current.LangstrothHiveMinTemp = fgcConfig.LangstrothHiveMinTemp;

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
            public float SkepDaysToHarvestIn30DayMonths = FromGoldenCombsConfig.Current.SkepDaysToHarvestIn30DayMonths;
            [ProtoMember(2)]
            public float ClayPotDaysToHarvestIn30DayMonths = FromGoldenCombsConfig.Current.ClayPotDaysToHarvestIn30DayMonths;
            [ProtoMember(3)]
            public float LangstrothDaysToHarvestIn30DayMonths = FromGoldenCombsConfig.Current.LangstrothDaysToHarvestIn30DayMonths;
            [ProtoMember(4)]
            public int maxStackSize = FromGoldenCombsConfig.Current.MaxStackSize;
            [ProtoMember(5)]
            public int baseFrameDurability = FromGoldenCombsConfig.Current.baseframedurability;
            [ProtoMember(6)]
            public int minFramePerCycle = FromGoldenCombsConfig.Current.minFramePerCycle;
            [ProtoMember(7)]
            public int maxFramePerCycle = FromGoldenCombsConfig.Current.maxFramePerCycle;
            [ProtoMember(8)]
            public bool showcombpoptime = FromGoldenCombsConfig.Current.showcombpoptime;
            [ProtoMember(9)]
            public int CeramicPotMinYield = FromGoldenCombsConfig.Current.CeramicPotMinYield;
            [ProtoMember(10)]
            public int CeramicPotMaxYield = FromGoldenCombsConfig.Current.CeramicPotMaxYield;
            [ProtoMember(11)]
            public int FrameMinYield = FromGoldenCombsConfig.Current.FrameMinYield;
            [ProtoMember(12)]
            public int FrameMaxYield = FromGoldenCombsConfig.Current.FrameMaxYield;
            [ProtoMember(13)]
            public int SkepMinYield = FromGoldenCombsConfig.Current.SkepMinYield;
            [ProtoMember(14)]
            public int SkepMaxYield = FromGoldenCombsConfig.Current.SkepMaxYield;
            [ProtoMember(15)]
            public float SkepHiveMinTemp = FromGoldenCombsConfig.Current.SkepHiveMinTemp;
            [ProtoMember(16)]
            public float SkepHiveMaxTemp = FromGoldenCombsConfig.Current.SkepHiveMaxTemp;
            [ProtoMember(17)]
            public float CeramicHiveMinTemp = FromGoldenCombsConfig.Current.CeramicHiveMinTemp;
            [ProtoMember(18)]
            public float CeramicHiveMaxTemp = FromGoldenCombsConfig.Current.CeramicHiveMaxTemp;
            [ProtoMember(19)]
            public float LangstrothHiveMinTemp = FromGoldenCombsConfig.Current.LangstrothHiveMinTemp;
            [ProtoMember(20)]
            public float LangstrothHiveMaxTemp = FromGoldenCombsConfig.Current.LangstrothHiveMaxTemp;
        }

        [ProtoContract]
        class OnPlayerLoginMessage
        {
            [ProtoMember(1)]
            IPlayer[] fromPlayer;
        }
    }
}

using Vintagestory.API.Common;
using FromGoldenCombs.Blocks;
using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.Blocks.Langstroth;
using FromGoldenCombs.Items;
using VFromGoldenCombs.Blocks.Langstroth;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using FromGoldenCombs.BlockBehaviors;
using FromGoldenCombs.Util.config;
using FromGoldenCombs.Util.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Server;

namespace FromGoldenCombs
{
    class FromGoldenCombs : ModSystem
    {
        NetworkHandler networkHandler;
        public delegate void PollinationEventHandler(string eventName, BlockPos cropPos, ref EnumHandling handled, IAttribute data);
        public event PollinationEventHandler OnPollination;

        private void HandlePollinationEvents(string eventName, ref EnumHandling handled, IAttribute data)
        {
            if (data is TreeAttribute tdata)
            {
                BlockPos cropPos = new(tdata.GetInt("x"), tdata.GetInt("y"), tdata.GetInt("z"));
                if (OnPollination != null)
                {
                    //TODO: On future iterations of the mod, make sure that all hives have a parent type that includes an abstract method that can be called to activate the pollination.  This way we can ensure that only the closest hive pollinates a crop.
                    foreach (PollinationEventHandler handler in OnPollination.GetInvocationList())
                    {
                        handler.Invoke(eventName, cropPos, ref handled, data);
                        
                        break;

                    }
                    
                }

            }
        }
        enum EnumHivePopSize
        {
            Poor = 0,
            Decent = 1,
            Large = 2
        }

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        #region Client
        public override void StartClientSide(ICoreClientAPI api)
        {
            networkHandler.InitializeClientSideNetworkHandler(api);

        }
        #endregion

        #region server
        public override void StartServerSide(ICoreServerAPI api)
        {
            networkHandler.InitializeServerSideNetworkHandler(api);
            api.Event.RegisterEventBusListener(HandlePollinationEvents, 0.5, "cropbreak");
            api.Event.RegisterEventBusListener(HandlePollinationEvents, 0.5, "berryharvest");
            api.Event.RegisterEventBusListener(HandlePollinationEvents, 0.5, "fruitharvest");
        }
        #endregion
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            networkHandler = new NetworkHandler();
            //BlockEntities
            api.RegisterBlockEntityClass("fgcbeehive", typeof(BEFGCBeehive));
            api.RegisterBlockEntityClass("beceramichive", typeof(BECeramicBroodPot));
            api.RegisterBlockEntityClass("belangstrothsuper", typeof(BELangstrothSuper));
            api.RegisterBlockEntityClass("belangstrothstack", typeof(BELangstrothStack));
            api.RegisterBlockEntityClass("beframerack", typeof(BEFrameRack));

            //Blocks
            api.RegisterBlockClass("ceramicbroodpot", typeof(CeramicBroodPot));
            api.RegisterBlockClass("hivetop", typeof(ClayHiveTop));
            api.RegisterBlockClass("rawceramichive", typeof(RawBroodPot));
            api.RegisterBlockClass("langstrothsuper", typeof(LangstrothSuper));
            api.RegisterBlockClass("langstrothbrood", typeof(LangstrothBrood));
            api.RegisterBlockClass("langstrothbase", typeof(LangstrothBase));
            api.RegisterBlockClass("langstrothstack", typeof(LangstrothStack));
            api.RegisterBlockClass("framerack", typeof(FrameRack));

            //Items
            api.RegisterItemClass("langstrothpartcore", typeof(LangstrothPartCore));

            //Events
            api.RegisterBlockBehaviorClass("PushEventOnCropBroken", typeof(PushEventOnCropBreakBehavior));
            api.RegisterBlockBehaviorClass("PushEventOnBlockHarvested", typeof(PushEventOnBlockHarvested));
            networkHandler.RegisterMessages(api);
            FGCClientConfig.createClientConfig(api);
            FGCServerConfig.createServerConfig(api);
        }
    }   
}

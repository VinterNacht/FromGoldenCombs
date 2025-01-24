using Vintagestory.API.Common;
using FromGoldenCombs.Blocks;
using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.config;
using FromGoldenCombs.Blocks.Langstroth;
using FromGoldenCombs.Items;
using VFromGoldenCombs.Blocks.Langstroth;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using FromGoldenCombs.BlockBehavior;

namespace FromGoldenCombs
{
    class FromGoldenCombs : ModSystem
    {
        NetworkHandler networkHandler;

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
        }
        #endregion
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            networkHandler = new NetworkHandler();
            //BlockEntities
            api.RegisterBlockEntityClass("fgcbeehive", typeof(FGCBeehive));
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

            //
            api.RegisterBlockBehaviorClass("PushEventOnCropBroken", typeof(PushEventOnCropBreakBehavior));

            networkHandler.RegisterMessages(api);
            FromGoldenCombsConfig.createConfig(api);
        }
    }
}

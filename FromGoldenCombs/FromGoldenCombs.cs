using Vintagestory.API.Common;
using FromGoldenCombs.Blocks;
using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.config;
using FromGoldenCombs.Blocks.Langstroth;
using FromGoldenCombs.Items;
using VFromGoldenCombs.Blocks.Langstroth;

namespace FromGoldenCombs
{
    class FromGoldenCombs : ModSystem
    {
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

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            //BlockEntities
            api.RegisterBlockEntityClass("fgcbeehive", typeof(FGCBeehive));
            api.RegisterBlockEntityClass("beclaypothive", typeof(BEClayBroodPot));
            api.RegisterBlockEntityClass("beceramichive", typeof(BECeramicBroodPot));
            api.RegisterBlockEntityClass("belangstrothsuper", typeof(BELangstrothSuper));
            api.RegisterBlockEntityClass("belangstrothstack", typeof(BELangstrothStack));
            api.RegisterBlockEntityClass("beframerack", typeof(BEFrameRack));

            //Blocks
            api.RegisterBlockClass("ceramicbroodpot", typeof(CeramicBroodPot));
            api.RegisterBlockClass("claypothive", typeof(ClayBroodPot));
            api.RegisterBlockClass("hivetop", typeof(ClayHiveTop));
            api.RegisterBlockClass("rawceramichive", typeof(RawBroodPot));
            api.RegisterBlockClass("langstrothsuper", typeof(LangstrothSuper));
            api.RegisterBlockClass("langstrothbrood", typeof(LangstrothBrood));
            api.RegisterBlockClass("langstrothbase", typeof(LangstrothBase));
            api.RegisterBlockClass("langstrothstack", typeof(LangstrothStack));
            api.RegisterBlockClass("framerack", typeof(FrameRack));

            //Items
            api.RegisterItemClass("langstrothpartcore", typeof(LangstrothPartCore));


            try
            {
                var Config = api.LoadModConfig<FromGoldenCombsConfig>("fromgoldencombs.json");
                if (Config != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                    FromGoldenCombsConfig.Current = Config;
                }
                else
                {
                    api.Logger.Notification("No Mod Config specified. Falling back to default settings");
                    FromGoldenCombsConfig.Current = FromGoldenCombsConfig.GetDefault();
                }
            }
            catch
            {
                FromGoldenCombsConfig.Current = FromGoldenCombsConfig.GetDefault();
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
            }
            finally
            {
                if (FromGoldenCombsConfig.Current.hiveHoursToHarvest <= 0)
                    FromGoldenCombsConfig.Current.hiveHoursToHarvest = 1488;
                if (FromGoldenCombsConfig.Current.clayPotHiveHoursToHarvest <= 0)
                    FromGoldenCombsConfig.Current.clayPotHiveHoursToHarvest = 1488;
                api.StoreModConfig(FromGoldenCombsConfig.Current, "fromgoldencombs.json");
            }
        }

            //TODO: Project List
            //Add Clay Honeypot For Pre-Bucket/Barrel storage
            //Add Wax Blocks For placeable, stackable storage of wax
            //Give Raccons the ability to knock the top off a hive, and then eat it.
            //Give hivetops growth mechanics for volume.
        }
}

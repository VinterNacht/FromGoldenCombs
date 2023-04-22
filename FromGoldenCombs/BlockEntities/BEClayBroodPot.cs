using FromGoldenCombs.Blocks;
using FromGoldenCombs.config;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FromGoldenCombs.BlockEntities
{

    class BEClayBroodPot : BlockEntity
    {
        MeshData plane;
        double harvestableAtTotalHours;
        double cooldownUntilTotalHours;
        public bool Harvestable;
        int quantityNearbyFlowers;
        int quantityNearbyHives;
        float actvitiyLevel;
        RoomRegistry roomreg;
        float roomness;
        public static SimpleParticleProperties Bees;
        int scanQuantityNearbyFlowers;
        int scanQuantityNearbyHives;
        int scanIteration;
        EnumHivePopSize hivePopSize;

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            mesher.AddMeshData(plane);
            return false;
        }

        static BEClayBroodPot()
        {
            Bees = new SimpleParticleProperties(
                1, 1,
                ColorUtil.ToRgba(255, 215, 156, 65),
                new Vec3d(), new Vec3d(),
                new Vec3f(0, 0, 0),
                new Vec3f(0, 0, 0),
                1f,
                0f,
                0.5f, 0.5f,
                EnumParticleModel.Cube
            );
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            RegisterGameTickListener(TestHarvestable, 6000);
            RegisterGameTickListener(OnScanForFlowers, api.World.Rand.Next(5000) + 30000);

            roomreg = Api.ModLoader.GetModSystem<RoomRegistry>();

            if (api.Side == EnumAppSide.Client)
            {
               ICoreClientAPI capi = api as ICoreClientAPI;
                Block ownBlock = Api.World.BlockAccessor.GetBlock(Pos, 0);
                Shape shape = capi.Assets.TryGet(new AssetLocation("fromgoldencombs", "shapes/block/hive/ceramic/claypothive-empty-none-notop.json")).ToObject<Shape>();
                capi.Tesselator.TesselateShape(ownBlock, shape, out plane);

                if (api.Side == EnumAppSide.Client)
                {
                    RegisterGameTickListener(SpawnBeeParticles, 300);
                }
            }
        }

        public void BlockInteract(IWorldAccessor world)
        {

            //TODO: Create simplified BlockInteract that converts it into the new Ceramic Hive.
            //This will require identifying the correct type of honeypot to give the player, if the Ceramic Hive has one,
            //Or, it will require converting the hive to a Ceramic Hive, and placing the new top.  Populated vs Not will have to be transferred as well

            Block hive = Api.World.BlockAccessor.GetBlock(Pos, 0);

            ItemStack stack = new(world.GetBlock(new AssetLocation("fromgoldencombs", "ceramicbroodpot-notop")));
            ItemStack hivetopStack = new(world.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-" + (hive.Variant["harvestable"]=="harvestable"?"harvestable":"empty"))));

            stack.Attributes.SetBool("populated", this.Block.Variant["populated"] == "populated");      

            if(hive.Variant["top"] == "notop"){
                Api.World.BlockAccessor.SetBlock(stack.Block.BlockId, Pos, stack);
            } 
            else
            {
                Api.World.BlockAccessor.SetBlock(stack.Block.BlockId, Pos, stack);
                BECeramicBroodPot beCBP = (BECeramicBroodPot)world.BlockAccessor.GetBlockEntity(Pos);
                beCBP.TryPutDirect(hivetopStack);
            }
        }

        private void TestHarvestable(float dt)
        {
            float harvestBase = (FromGoldenCombsConfig.Current.ClayPotDaysToHarvestIn30DayMonths) * Api.World.Calendar.DaysPerMonth / 30f;
            double worldTime = Api.World.Calendar.TotalHours;
            Block hive = Api.World.BlockAccessor.GetBlock(Pos, 0);
            ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);
            if (conds == null) return;

            float temp = conds.Temperature + (roomness > 0 ? 5 : 0);
            actvitiyLevel = GameMath.Clamp(temp / 5f, 0f, 1f);

            // Reset timers during winter
            if (temp <= -10)
            {
                //TODO: Readdress harvestAtTotalHours math to ensure it works for all ranges of growth time.
                harvestableAtTotalHours = worldTime + HarvestableTime(harvestBase);
                cooldownUntilTotalHours = worldTime + 4 / 2 * 24;
            }

            if(!Harvestable && harvestableAtTotalHours==0 && hivePopSize > EnumHivePopSize.Poor && hive.Variant["top"] == "withtop")
            {
                harvestableAtTotalHours = worldTime + HarvestableTime(harvestBase);
            }
            else if (!Harvestable && worldTime > harvestableAtTotalHours && hivePopSize > EnumHivePopSize.Poor && hive.Variant["top"] == "withtop")
            {
                Harvestable = true;
                Api.World.BlockAccessor.ExchangeBlock(Api.World.GetBlock(new AssetLocation("fromgoldencombs", "claypothive-populated-harvestable-withtop")).BlockId, Pos);
                MarkDirty(true);
            }
        }

        private double HarvestableTime(float i)
        {
            i = (i * Api.World.Calendar.DaysPerMonth / 30f) * Api.World.Calendar.HoursPerDay;
            Random rand = new();
            return (i * .75) + ((i * .5) * rand.NextDouble());
        }

        readonly Vec3d startPos = new();
        readonly Vec3d endPos = new();
        Vec3f minVelo = new();

        private bool isWildHive;

        private void SpawnBeeParticles(float dt)
        {
                float dayLightStrength = Api.World.Calendar.GetDayLightStrength(Pos.X, Pos.Z);
                if (Api.World.Rand.NextDouble() > 2 * dayLightStrength - 0.5) return;

                Random rand = Api.World.Rand;

                Bees.MinQuantity = actvitiyLevel;

                // Leave hive
                if (Api.World.Rand.NextDouble() > 0.5)
                {
                    startPos.Set(Pos.X + 0.5f, Pos.Y + 0.5f, Pos.Z + 0.5f);
                    minVelo.Set((float)rand.NextDouble() * 3 - 1.5f, (float)rand.NextDouble() * 1 - 0.5f, (float)rand.NextDouble() * 3 - 1.5f);

                    Bees.MinPos = startPos;
                    Bees.MinVelocity = minVelo;
                    Bees.LifeLength = 1f;
                    Bees.WithTerrainCollision = false;
                }

                // Go back to hive
                else
                {
                    startPos.Set(Pos.X + rand.NextDouble() * 5 - 2.5, Pos.Y + rand.NextDouble() * 2 - 1f, Pos.Z + rand.NextDouble() * 5 - 2.5f);
                    endPos.Set(Pos.X + 0.5f, Pos.Y + 0.5f, Pos.Z + 0.5f);

                    minVelo.Set((float)(endPos.X - startPos.X), (float)(endPos.Y - startPos.Y), (float)(endPos.Z - startPos.Z));
                    minVelo /= 2;

                    Bees.MinPos = startPos;
                    Bees.MinVelocity = minVelo;
                    Bees.WithTerrainCollision = true;
                Api.World.SpawnParticles(Bees);
            }
        }

        private void OnScanForFlowers(float dt)
        {
            //Scan to get number of nearby flowers and active hives
            Room room = roomreg?.GetRoomForPosition(Pos);
            roomness = (room != null && room.SkylightCount > room.NonSkylightCount && room.ExitCount == 0) ? 1 : 0;

            if (actvitiyLevel < 1) return;
            if (Api.Side == EnumAppSide.Client) return;
            if (Api.World.Calendar.TotalHours < cooldownUntilTotalHours) return;

            if (scanIteration == 0)
            {
                scanQuantityNearbyFlowers = 0;
                scanQuantityNearbyHives = 0;
            }

            int minX = -8 + 8 * (scanIteration / 2);
            int minZ = -8 + 8 * (scanIteration % 2);
            int size = 8;

            Block fullSkepN = Api.World.GetBlock(new AssetLocation("skep-populated-north"));
            Block fullSkepE = Api.World.GetBlock(new AssetLocation("skep-populated-east"));
            Block fullSkepS = Api.World.GetBlock(new AssetLocation("skep-populated-south"));
            Block fullSkepW = Api.World.GetBlock(new AssetLocation("skep-populated-west"));

            Block wildhive1 = Api.World.GetBlock(new AssetLocation("wildbeehive-medium"));
            Block wildhive2 = Api.World.GetBlock(new AssetLocation("wildbeehive-large"));

            Block claypothive = Api.World.GetBlock(new AssetLocation("claypothive-populated-empty-withtop"));
            Block claypothive2 = Api.World.GetBlock(new AssetLocation("claypothive-populated-empty-notop"));
            Block claypothive3 = Api.World.GetBlock(new AssetLocation("claypothive-populated-harvestable-notop"));
            Block claypothive4 = Api.World.GetBlock(new AssetLocation("claypothive-populated-harvestable-withtop"));

            Api.World.BlockAccessor.WalkBlocks(Pos.AddCopy(minX, -5, minZ), Pos.AddCopy(minX + size - 1, 5, minZ + size - 1), (block, posx, posy, posz) =>
            {
                if (block.Id == 0) return;

                if (block.Attributes?.IsTrue("beeFeed") == true) scanQuantityNearbyFlowers++;

                if (block == fullSkepN || block == fullSkepE || block == fullSkepS || block == fullSkepW || block == wildhive1 || block == wildhive2 || block == claypothive || block == claypothive2 || block == claypothive3 || block == claypothive4)
                {
                    scanQuantityNearbyHives++;
                }
            });

            scanIteration++;

            if (scanIteration == 4)
            {
                scanIteration = 0;
                OnScanComplete();
            }
        }
        private void OnScanComplete()
        {
            quantityNearbyFlowers = scanQuantityNearbyFlowers;
            quantityNearbyHives = scanQuantityNearbyHives;

            hivePopSize = (EnumHivePopSize)GameMath.Clamp(quantityNearbyFlowers - 3 * quantityNearbyHives, 0, 2);

            MarkDirty();

        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (Api.World.EntityDebugMode && forPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative)
            {
                dsc.AppendLine(
                    Lang.Get("Nearby flowers: {0}, Nearby Hives: {1}, Empty Hives: {2}, Pop after hours: {3}. harvest in {4}, repop cooldown: {5}",quantityNearbyFlowers, quantityNearbyHives, (harvestableAtTotalHours - Api.World.Calendar.TotalHours).ToString("#.##"), (cooldownUntilTotalHours - Api.World.Calendar.TotalHours).ToString("#.##"))
                    + "\n" + Lang.Get("Population Size: " + hivePopSize));
            }

            string str = Lang.Get("Nearby flowers: {0}\nPopulation Size: {1}", quantityNearbyFlowers, hivePopSize);
            if (Harvestable) str += "\n" + Lang.Get("Harvestable");

            dsc.AppendLine(str);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);


            tree.SetInt("scanIteration", scanIteration);

            tree.SetInt("quantityNearbyFlowers", quantityNearbyFlowers);
            tree.SetInt("quantityNearbyHives", quantityNearbyHives);


            tree.SetInt("scanQuantityNearbyFlowers", scanQuantityNearbyFlowers);
            tree.SetInt("scanQuantityNearbyHives", scanQuantityNearbyHives);

            tree.SetInt("isWildHive", isWildHive ? 1 : 0);
            tree.SetInt("harvestable", Harvestable ? 1 : 0);
            tree.SetDouble("cooldownUntilTotalHours", cooldownUntilTotalHours);
            tree.SetDouble("harvestableAtTotalHours", harvestableAtTotalHours);
            tree.SetInt("hiveHealth", (int)hivePopSize);
            tree.SetFloat("roomness", roomness);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            bool wasHarvestable = Harvestable;

            scanIteration = tree.GetInt("scanIteration");

            quantityNearbyFlowers = tree.GetInt("quantityNearbyFlowers");
            quantityNearbyHives = tree.GetInt("quantityNearbyHives");

            scanQuantityNearbyFlowers = tree.GetInt("scanQuantityNearbyFlowers");
            scanQuantityNearbyHives = tree.GetInt("scanQuantityNearbyHives");

            isWildHive = tree.GetInt("isWildHive") > 0;
            Harvestable = tree.GetInt("harvestable") > 0;

            cooldownUntilTotalHours = tree.GetDouble("cooldownUntilTotalHours");
            harvestableAtTotalHours = tree.GetDouble("harvestableAtTotalHours");
            hivePopSize = (EnumHivePopSize)tree.GetInt("hiveHealth");
            roomness = tree.GetFloat("roomness");

            if (Harvestable != wasHarvestable && Api != null)
            {
                MarkDirty(true);
            }
        }

    }
}

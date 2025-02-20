using FromGoldenCombs.BlockBehaviors;
using FromGoldenCombs.Blocks;
using FromGoldenCombs.Blocks.Langstroth;
using FromGoldenCombs.Util.config;
using FromGoldenCombs.Util.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FromGoldenCombs.BlockEntities
{
    class BEFGCBeehive : BlockEntityBeehive, IAnimalFoodSource
    {
            
        // Stored values
        int scanIteration;
        int quantityNearbyFlowers;
        int quantityNearbyHives;
        List<BlockPos> emptySkeps = new();
        bool isWildHive;
        BlockPos skepToPop;
        double beginPopStartTotalHours;
        float popHiveAfterHours;
        double cooldownUntilTotalHours;
        double harvestableAtTotalHours;
        float harvestBase;
        new bool Harvestable;
        float threeDayTemp;

        // Current scan values
        int scanQuantityNearbyFlowers;
        int scanQuantityNearbyHives;
        List<BlockPos> scanEmptySkeps = new();
        double cropChargeGrowthHours = 24;
        double chargesPerDay = FGCServerConfig.Current.skepBaseChargesPerDay; //Number of hours until the hive accumulates a new grow charge.
        double cropChargeAtTotalHours;
        double cooldownUntilCropCharge;
        int cropcharges;
        int maxCropCharges = FGCServerConfig.Current.skepMaxCropCharges;
        int cropChargeRange = FGCServerConfig.Current.skepCropRange;


        // Temporary values
        public EnumHivePopSize hivePopSize;
        bool wasPlaced = false;
        string orientation;
        private RoomRegistry roomreg;
        private Vec3d startPos = new Vec3d();
        private Vec3d endPos = new Vec3d();
        private Vec3f minVelo = new Vec3f();
        private Vec3f maxVelo = new Vec3f();
        public float actvitiyLevel;
        public float roomness;
        public Vec3d Position => Pos.ToVec3d().Add(0.5, 0.5, 0.5);
        public string Type => "food";

        static BEFGCBeehive()
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

                RegisterGameTickListener(TestHarvestable, 5000);
                RegisterGameTickListener(OnScanForEmptySkep, api.World.Rand.Next(5000) + 30000);
                cropChargeGrowthHours = api.World.Calendar.HoursPerDay;
                roomreg = Api.ModLoader.GetModSystem<RoomRegistry>();

                harvestBase = (FGCServerConfig.Current.SkepDaysToHarvestIn30DayMonths * (Api.World.Calendar.DaysPerMonth / 30f)) * Api.World.Calendar.HoursPerDay;

                if (api.Side == EnumAppSide.Client)
                {
                    RegisterGameTickListener(SpawnBeeParticles, 300);
                }

                if (wasPlaced)
                {
                harvestableAtTotalHours = Api.World.Calendar.TotalHours + GetHarvestTime();
                }

                orientation = Block.LastCodePart();
                isWildHive = Block.FirstCodePart() != "skep";

                if (!isWildHive && api.Side == EnumAppSide.Client)
                {
                    ICoreClientAPI capi = api as ICoreClientAPI;
                    Block fullSkep = api.World.GetBlock(new AssetLocation("skep-populated-east"));

                    MeshData mesh;
                    capi.Tesselator.TesselateShape(
                        fullSkep,
                        api.Assets.TryGet("shapes/block/beehive/skep-harvestable.json").ToObject<Shape>(),
                        out mesh,
                        new Vec3f(0, BlockFacing.FromCode(orientation).HorizontalAngleIndex * 90 - 90, 0)
                    );

                    api.ObjectCache["beehive-harvestablemesh-" + orientation] = mesh;
                }

                if (!isWildHive && api.Side == EnumAppSide.Server)
                {
                    api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this);
                }
                Api.Event.RegisterEventBusListener(managePollinationBoost, 0.5, "cropbreak");
                Api.Event.RegisterEventBusListener(managePollinationBoost, 0.5, "berryharvest");
                Api.Event.RegisterEventBusListener(managePollinationBoost, 0.5, "fruitharvest");
        }

        private void managePollinationBoost(string eventName, ref EnumHandling handling, IAttribute data)
        {
            TreeAttribute tdata = data as TreeAttribute;
            BlockPos cropPos = new(tdata.GetInt("x"), tdata.GetInt("y"), tdata.GetInt("z"));
            int deltaX = cropPos.X - Pos.X;
            int deltaY = cropPos.Y - Pos.Y;
            int deltaZ = cropPos.Z - Pos.Z;
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

            if (this.Block.Variant["type"].ToString() == "populated" && hivePopSize != EnumHivePopSize.Poor)
            {
                if (eventName == "cropbreak")
                {
                    manageCropBoost(cropPos, distance, ref handling);
                }
                else if (eventName == "berryharvest")
                {
                    manageBerryBoost(cropPos, distance, ref handling);
                }
                else if (eventName == "fruitharvest")
                {
                    manageFruitBoost(cropPos, distance, ref handling);
                }
            }
        }

        private void manageCropBoost(BlockPos cropPos, double distance, ref EnumHandling handling)
        {

            if (cropcharges >= 1 && Api.World.BlockAccessor.GetBlock(cropPos).HasBehavior<PushEventOnCropBreakBehavior>() && distance < cropChargeRange)
            {

                if (Api.World.BlockAccessor.GetBlock(cropPos) is BlockCrop crop && Api.World.BlockAccessor.GetBlockEntity(cropPos.DownCopy()) is BlockEntityFarmland farmland)
                {

                    if (Api.World.BlockAccessor.GetBlock(cropPos).GetBehavior<PushEventOnCropBreakBehavior>().validCropStages.Contains<int>(crop.CurrentCropStage))
                    {
                        Api.World.BlockAccessor.GetBlock(cropPos).GetBehavior<PushEventOnCropBreakBehavior>().setHandling(EnumHandling.PreventSubsequent);
                        cropcharges--;
                    }

                    handling = EnumHandling.PreventSubsequent;
                }

                MarkDirty();
            }
        }


        private void manageBerryBoost(BlockPos bushPos, double distance, ref EnumHandling handling)
        {
            if (cropcharges >= 1 && Api.World.BlockAccessor.GetBlock(bushPos).HasBehavior<PushEventOnBlockHarvested>() && distance < cropChargeRange)
            {
                PushEventOnBlockHarvested eventBehavior = Api.World.BlockAccessor.GetBlock(bushPos).GetBehavior<PushEventOnBlockHarvested>();
                eventBehavior.useBeeBoost = true;
                cropcharges--;

                handling = EnumHandling.PreventSubsequent;
                MarkDirty();
            }
        }

        private void manageFruitBoost(BlockPos fruitFoliagePos, double distance, ref EnumHandling handling)
        {

            if (cropcharges >= 1 && Api.World.BlockAccessor.GetBlockEntity(fruitFoliagePos) is BlockEntityFruitTreePart beFTP && distance < cropChargeRange)
            {
                AssetLocation loc = AssetLocation.Create(beFTP.Block.Attributes["branchBlock"].AsString(null), beFTP.Block.Code.Domain);
                foreach (BlockDropItemStack drop in (beFTP.Api.World.GetBlock(loc) as BlockFruitTreeBranch).TypeProps[beFTP.TreeType].FruitStacks)
                {
                    ItemStack stack = drop.GetNextItemStack(0.25f);
                    if (stack != null)
                    {

                        beFTP.Api.World.SpawnItemEntity(stack, beFTP.Pos.Add(0.0f, 0.5f, 0.0f), null);
                    }
                    if (drop.LastDrop)
                    {
                        break;
                    }
                }
                cropcharges--;
            }
        }

        private double GetHarvestTime()
        {
            Random rand = new();
            double newHarvestTime = (harvestBase * .75f) + ((harvestBase * .5f) * rand.NextDouble());
            return (float)newHarvestTime;
        }

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
            }

            Api.World.SpawnParticles(Bees);
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            wasPlaced = true;
            if (Api?.World != null)
            {
                harvestableAtTotalHours = Api.World.Calendar.TotalHours + GetHarvestTime();
            }
        }

        private void TestHarvestable(float dt)
        {
            float minTemp = FGCServerConfig.Current.CeramicHiveMinTemp;
            float maxTemp = FGCServerConfig.Current.CeramicHiveMaxTemp == 0 ? 37f : FGCServerConfig.Current.CeramicHiveMaxTemp;
            double worldTime = Api.World.Calendar.TotalHours;
            ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);
            float todayNoonTemp = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, (Double)((int)(Api.World.Calendar.TotalDays)) + 0.66f).Temperature;
            float yesterdayNoonTemp = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, (Double)((int)(Api.World.Calendar.TotalDays - 1)) + 0.66f).Temperature;
            float twoDayAgoNoonTemp = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, (Double)((int)(Api.World.Calendar.TotalDays - 2)) + 0.66f).Temperature;
            if (conds == null) return;

            float threeDayTemp = (todayNoonTemp * 2 + yesterdayNoonTemp + twoDayAgoNoonTemp) / 4 + (roomness > 0 ? 5 : 0);
            float optimalTemp = (maxTemp + minTemp) / 2;
            double distance = Math.Abs((conds.Temperature + (roomness > 0 ? 5 : 0)) - optimalTemp); //The roomness is added to account for the presence of a greenhouse
            double range = Math.Max(maxTemp - optimalTemp, optimalTemp - minTemp);
            float beeParticleModifier = 1f - (float)(distance / range);
            actvitiyLevel = GameMath.Clamp(beeParticleModifier, 0f, 1f);
            //Reset timers during winter - Vanilla Settings
            //if (temp <= -10)
            //Reset timers when temp drops below 15c - FGC Settings
            bool tempOutOfRange = false;

            if (threeDayTemp < minTemp || threeDayTemp > maxTemp && quantityNearbyFlowers != 0)
            {
                harvestableAtTotalHours = worldTime + GetHarvestTime();
                cooldownUntilTotalHours = worldTime + 4 / 2 * 24;
                tempOutOfRange = true;
            }

            if(hivePopSize > 0 && !tempOutOfRange) handleCropCharges(tempOutOfRange, worldTime);

            // Reset timers during winter
            if (threeDayTemp <= minTemp || threeDayTemp >= maxTemp)
            {
                harvestableAtTotalHours = Api.World.Calendar.TotalHours + harvestBase;
                cooldownUntilTotalHours = Api.World.Calendar.TotalHours + 48.0;
            }

            if (!Harvestable && !isWildHive && Api.World.Calendar.TotalHours > harvestableAtTotalHours && hivePopSize > EnumHivePopSize.Poor)
            {
                Harvestable = true;
            }
            MarkDirty(redrawOnClient: true);
        }

        private void handleCropCharges(bool tempOutOfRange, double worldTime)
        {
            if (worldTime > cropChargeAtTotalHours && cropcharges < maxCropCharges && hivePopSize != EnumHivePopSize.Poor && quantityNearbyFlowers > 0)
            {
                if (cropChargeAtTotalHours != 0)
                {
                    int cropchargebase = (int)Math.Max(1, (Math.Round((worldTime - cropChargeAtTotalHours) / (float)(Api.World.Calendar.HoursPerDay * (float)(Api.World.Calendar.DaysPerMonth / 30f)))));
                    int cropchargegrowth = (int)Math.Max(1, ((cropchargebase * chargesPerDay) * (int)hivePopSize));
                    cropcharges = (int)Math.Min(cropchargegrowth + cropcharges, maxCropCharges);
                }
                cropChargeAtTotalHours = worldTime + cropChargeGrowthHours;
            }
        }

        private void OnScanForEmptySkep(float dt)
        {
            Room room = roomreg?.GetRoomForPosition(Pos);
            roomness = (room != null && room.SkylightCount > room.NonSkylightCount && room.ExitCount == 0) ? 1 : 0;
            MarkDirty();
            if (actvitiyLevel <= 0) return;
            if (Api.Side == EnumAppSide.Client) return;
            if (Api.World.Calendar.TotalHours < cooldownUntilTotalHours) return;

            if (scanIteration == 0)
            {
                scanQuantityNearbyFlowers = 0;
                scanQuantityNearbyHives = 0;
                scanEmptySkeps.Clear();
            }

            int minX = -8 + 8 * (scanIteration / 2);
            int minZ = -8 + 8 * (scanIteration % 2);
            int size = 8;

            Api.World.BlockAccessor.WalkBlocks(Pos.AddCopy(minX, -5, minZ), Pos.AddCopy(minX + size - 1, 5, minZ + size - 1), delegate (Block block, int x, int y, int z)
            {
                if (block.Id != 0)
                {
                    if (block.BlockMaterial == EnumBlockMaterial.Plant || block is BlockPlantContainer)
                    {
                        JsonObject attributes = block.Attributes;
                        if (attributes != null && attributes.IsTrue("beeFeed"))
                        {
                            scanQuantityNearbyFlowers++;
                        }
                    }
                    else if (block.BlockMaterial == EnumBlockMaterial.Other)
                    {
                        string path = block.Code.Path;
                        BlockPos pos = new BlockPos(x, y, z, 0);
                        if (path.StartsWithOrdinal("skep-empty"))
                        {
                            scanEmptySkeps.Add(pos);
                        }
                        else if (path.StartsWithOrdinal("skep-populated") || path.StartsWithOrdinal("wildbeehive"))
                        {
                            scanQuantityNearbyHives++;
                        } 
                        else if (block is LangstrothStack hive && Api.World.BlockAccessor.GetBlockEntity<BELangstrothStack>(pos).isHiveActive())
                        {
                            scanQuantityNearbyHives++;
                        }
                        else if (block is CeramicBroodPot hive2 && Api.World.BlockAccessor.GetBlockEntity<BECeramicBroodPot>(pos).isActiveHive)
                        {
                            scanQuantityNearbyHives++;
                        }
                    } 
                }
            });

            scanIteration++;
            if (scanIteration == 4)
            {
                scanIteration = 0;
                OnScanComplete();
            }
            MarkDirty();
        }

        private void OnScanComplete()
        {
            quantityNearbyFlowers = scanQuantityNearbyFlowers;
            quantityNearbyHives = scanQuantityNearbyHives;
            emptySkeps = new List<BlockPos>(scanEmptySkeps);

            if (emptySkeps.Count == 0)
            {
                skepToPop = null;
            }

                hivePopSize = (EnumHivePopSize)GameMath.Clamp(quantityNearbyFlowers - 3 * quantityNearbyHives, 0, 2);

                System.Diagnostics.Debug.WriteLine("hivePopSize at line 288 is " + hivePopSize.ToString() + " on " + Api.Side.ToString());

            if (3 * quantityNearbyHives + 3 > quantityNearbyFlowers)
            {
                skepToPop = null;
                MarkDirty();
                return;
            }

            if (skepToPop != null && Api.World.Calendar.TotalHours > beginPopStartTotalHours + popHiveAfterHours)
            {
                TryPopCurrentSkep();
                //TODO: Implement variable length swarm lengths based on DaysPerMonth
                cooldownUntilTotalHours = Api.World.Calendar.TotalHours + 48.0;
                MarkDirty();
                return;
            }

            // Default Spread speed: Once every 4 in game days * factor
            // Don't spread at all if 3 * livinghives + 3 > flowers

            // factor = Clamped(livinghives / Math.Sqrt(flowers - 3 * livinghives - 3), 1, 1000)
            // After spreading: 4 extra days cooldown

            float swarmability = GameMath.Clamp(quantityNearbyFlowers - 3 - 3 * quantityNearbyHives, 0, 20) / 5f;
            // We want to translate the swarmability value 0..4
            // into swarm days 12..0
            float swarmInDays = (4f - swarmability) * 2.5f;
            
            if (swarmability <= 0) skepToPop = null;


            //if (skepToPop != null && Api.World.Calendar.GetSeason(Pos)==EnumSeason.Spring)
            if (skepToPop != null)
            {
                float newPopHours = 24 * swarmInDays;
                this.popHiveAfterHours = (float)(0.75 * popHiveAfterHours + 0.25 * newPopHours);

                if (!emptySkeps.Contains(skepToPop))
                {
                    skepToPop = null;
                    MarkDirty();
                }

                return;
            }

            popHiveAfterHours = 24f * swarmInDays;
            beginPopStartTotalHours = Api.World.Calendar.TotalHours;

            float mindistance = 999f;
            BlockPos closestEmptySkep = new(0);
            foreach (BlockPos emptySkep in emptySkeps)
            {
                float dist = emptySkep.DistanceTo(this.Pos);
                if (dist < mindistance)
                {
                    mindistance = dist;
                    closestEmptySkep = emptySkep;
                }
            }

            skepToPop = closestEmptySkep;
        }


        private void TryPopCurrentSkep()
        {
            Block skepToPopBlock = Api.World.BlockAccessor.GetBlock(skepToPop, 0);
            if (skepToPopBlock == null || !(skepToPopBlock is BlockSkep))
            {
                // Skep must have changed since last time we checked, so lets restart 
                this.skepToPop = null;
                return;
            }

            string orient = skepToPopBlock.LastCodePart();
            string blockcode = "skep-populated-" + orient;
            Block fullSkep = Api.World.GetBlock(new AssetLocation(blockcode));

            if (fullSkep == null)
            {
                Api.World.Logger.Warning("BEBeehive.TryPopSkep() - block with code {0} does not exist?", blockcode);
            }
            else
            {
                Api.World.BlockAccessor.SetBlock(fullSkep.BlockId, skepToPop);
                hivePopSize = EnumHivePopSize.Poor;
                this.skepToPop = null;
            }
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (Harvestable)
            {
                mesher.AddMeshData(Api.ObjectCache["beehive-harvestablemesh-" + orientation] as MeshData);
                return true;
            }

            return false;
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            
            
            //Debug Information
            if (Api.World.EntityDebugMode && forPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative)
            {
                dsc.AppendLine(
                        Lang.Get("Nearby flowers: {0}, Nearby Hives: {1}, Empty Hives: {2}, Pop after hours: {3}. harvest in {4}, repop cooldown: {5}",
                        quantityNearbyFlowers,
                        quantityNearbyHives,
                        emptySkeps.Count,
                        (beginPopStartTotalHours + popHiveAfterHours - Api.World.Calendar.TotalHours).ToString("#.##"),
                        (harvestableAtTotalHours - Api.World.Calendar.TotalHours) / Api.World.Calendar.HoursPerDay,
                        (cooldownUntilTotalHours - Api.World.Calendar.TotalHours).ToString("#.##"))
                        + "\n" + Lang.Get("Population Size: ") + hivePopSize);
            }

            //General Information
            ClimateCondition conds2 = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);
            float todayNoonTemp = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, (Double)((int)(Api.World.Calendar.TotalDays)) + 0.66f).Temperature;
            float yesterdayNoonTemp = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, (Double)((int)(Api.World.Calendar.TotalDays - 1)) + 0.66f).Temperature;
            float twoDayAgoNoonTemp = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, (Double)((int)(Api.World.Calendar.TotalDays - 2)) + 0.66f).Temperature;
            float threeDayTemp = (todayNoonTemp * 2 + yesterdayNoonTemp + twoDayAgoNoonTemp) / 4 + (roomness > 0 ? 5 : 0);
            float minTemp = FGCServerConfig.Current.SkepHiveMinTemp;
            float maxTemp = FGCServerConfig.Current.SkepHiveMaxTemp;

            string tempReport = Lang.Get("fromgoldencombs:3DayTemp") + " " + (threeDayTemp > maxTemp ? Lang.Get("fromgoldencombs:3DayTooHot") : threeDayTemp < minTemp ? Lang.Get("fromgoldencombs:3DayTooCold") : Lang.Get("fromgoldencombs:3DayPerfect"));

            double worldTime = Api.World.Calendar.TotalHours;
            float hoursPerDay = Api.World.Calendar.HoursPerDay;
            int daysTillHarvest = (int)Math.Round((harvestableAtTotalHours - worldTime) / hoursPerDay);
            daysTillHarvest = daysTillHarvest <= 0 ? 0 : daysTillHarvest;
            string hiveState = Lang.Get("fromgoldencombs:nearbyflowers", quantityNearbyFlowers, Lang.Get(hivePopSize.ToString()));

            bool outOfTemp = (threeDayTemp <= minTemp || threeDayTemp >= maxTemp);
            if (threeDayTemp < minTemp)
            {
                hiveState += "\n" + Lang.Get("fromgoldencombs:toocold");
            }
            if (threeDayTemp > maxTemp)
            {
                hiveState += "\n" + Lang.Get("fromgoldencombs:toohot");
            }
            else if ((harvestableAtTotalHours - worldTime / 24 > 0) && !Harvestable && hivePopSize > EnumHivePopSize.Poor && !outOfTemp)
            {
                if (FGCServerConfig.Current.showcombpoptime)
                {
                    hiveState += "\n" + Lang.Get("fromgoldencombs:timetillpop", daysTillHarvest < 1 ? Lang.Get("fromgoldencombs:lessthanday") : (daysTillHarvest + " days"));
                }
            }
            else if (quantityNearbyFlowers > 0 && !outOfTemp && !Harvestable)
            {
                hiveState += "\n" + "The bees are out gathering.";
            }
            else if (!outOfTemp && !Harvestable) 
            {
                hiveState += "\n" + "The bees are scouting for flowers.";
            }

            if (skepToPop != null && Api.World.Calendar.TotalHours > cooldownUntilTotalHours)
            {
                double inhours = beginPopStartTotalHours + popHiveAfterHours - Api.World.Calendar.TotalHours;
                double days = inhours / Api.World.Calendar.HoursPerDay;

                if (days > 1.5)
                {
                    hiveState += "\n" + Lang.Get("Will swarm in approx. {0} days", Math.Round(days));
                }
                else if (days > 0.5)
                {
                    hiveState += "\n" + Lang.Get("Will swarm in approx. one day");
                }
                else
                {
                    hiveState += "\n" + Lang.Get("Will swarm in less than a day");
                }
            }
            dsc.AppendLine(hiveState);
            if (this.roomness > 0f)
            {
                dsc.AppendLine("\n" + Lang.Get("greenhousetempbonus", Array.Empty<object>()));
                
            }
            if (FGCServerConfig.Current.showExtraBeehiveInfo && (forPlayer.Entity.Controls.ShiftKey || FGCClientConfig.Current.alwaysShowHiveInfo == true))
            {
                dsc.AppendLine(tempReport);
                dsc.AppendLine(Lang.Get("fromgoldencombs:cropcharges") + " " + cropcharges);
            }
        }


        #region Attributes Methods
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("scanIteration", scanIteration);
            tree.SetInt("quantityNearbyFlowers", quantityNearbyFlowers);
            tree.SetInt("quantityNearbyHives", quantityNearbyHives);
            TreeAttribute treeAttribute = new TreeAttribute();
            for (int i = 0; i < emptySkeps.Count; i++)
            {
                treeAttribute.SetInt("posX-" + i, emptySkeps[i].X);
                treeAttribute.SetInt("posY-" + i, emptySkeps[i].Y);
                treeAttribute.SetInt("posZ-" + i, emptySkeps[i].Z);
            }

            tree["emptyskeps"] = treeAttribute;
            tree.SetInt("scanQuantityNearbyFlowers", scanQuantityNearbyFlowers);
            tree.SetInt("scanQuantityNearbyHives", scanQuantityNearbyHives);
            TreeAttribute treeAttribute2 = new TreeAttribute();
            for (int j = 0; j < scanEmptySkeps.Count; j++)
            {
                treeAttribute2.SetInt("posX-" + j, scanEmptySkeps[j].X);
                treeAttribute2.SetInt("posY-" + j, scanEmptySkeps[j].Y);
                treeAttribute2.SetInt("posZ-" + j, scanEmptySkeps[j].Z);
            }

            tree["scanEmptySkeps"] = treeAttribute2;
            tree.SetInt("isWildHive", isWildHive ? 1 : 0);
            tree.SetInt("harvestable", Harvestable ? 1 : 0);
            tree.SetInt("skepToPopX", (!(skepToPop == null)) ? skepToPop.X : 0);
            tree.SetInt("skepToPopY", (!(skepToPop == null)) ? skepToPop.Y : 0);
            tree.SetInt("skepToPopZ", (!(skepToPop == null)) ? skepToPop.Z : 0);
            tree.SetDouble("beginPopStartTotalHours", beginPopStartTotalHours);
            tree.SetFloat("popHiveAfterHours", popHiveAfterHours);
            tree.SetDouble("cooldownUntilTotalHours", cooldownUntilTotalHours);
            tree.SetDouble("harvestableAtTotalHours", harvestableAtTotalHours);
            tree.SetInt("hiveHealth", (int)hivePopSize);
            tree.SetFloat("roomness", roomness);
            tree.SetDouble("cropChargeAtTotalHours", cropChargeAtTotalHours);
            tree.SetInt("cropcharges", cropcharges);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            bool harvestable = Harvestable;
            scanIteration = tree.GetInt("scanIteration");
            quantityNearbyFlowers = tree.GetInt("quantityNearbyFlowers");
            quantityNearbyHives = tree.GetInt("quantityNearbyHives");
            emptySkeps.Clear();
            TreeAttribute treeAttribute = tree["emptyskeps"] as TreeAttribute;
            for (int i = 0; i < treeAttribute.Count / 3; i++)
            {
                emptySkeps.Add(new BlockPos(treeAttribute.GetInt("posX-" + i), treeAttribute.GetInt("posY-" + i), treeAttribute.GetInt("posZ-" + i)));
            }

            scanQuantityNearbyFlowers = tree.GetInt("scanQuantityNearbyFlowers");
            scanQuantityNearbyHives = tree.GetInt("scanQuantityNearbyHives");
            scanEmptySkeps.Clear();
            TreeAttribute treeAttribute2 = tree["scanEmptySkeps"] as TreeAttribute;
            int num = 0;
            while (treeAttribute2 != null && num < treeAttribute2.Count / 3)
            {
                scanEmptySkeps.Add(new BlockPos(treeAttribute2.GetInt("posX-" + num), treeAttribute2.GetInt("posY-" + num), treeAttribute2.GetInt("posZ-" + num)));
                num++;
            }

            isWildHive = tree.GetInt("isWildHive") > 0;
            Harvestable = tree.GetInt("harvestable") > 0;
            int @int = tree.GetInt("skepToPopX");
            int int2 = tree.GetInt("skepToPopY");
            int int3 = tree.GetInt("skepToPopZ");
            if (@int != 0 || int2 != 0 || int3 != 0)
            {
                skepToPop = new BlockPos(@int, int2, int3);
            }
            else
            {
                skepToPop = null;
            }

            beginPopStartTotalHours = tree.GetDouble("beginPopStartTotalHours");
            popHiveAfterHours = tree.GetFloat("popHiveAfterHours");
            cooldownUntilTotalHours = tree.GetDouble("cooldownUntilTotalHours");
            harvestableAtTotalHours = tree.GetDouble("harvestableAtTotalHours");
            hivePopSize = (EnumHivePopSize)tree.GetInt("hiveHealth");
            roomness = tree.GetFloat("roomness");
            cropChargeAtTotalHours = tree.GetDouble("cropChargeAtTotalHours");
            cropcharges = tree.GetInt("cropcharges");
            if (Harvestable != harvestable && Api != null)
            {
                MarkDirty(redrawOnClient: true);
            }
        }


        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            if (!isWildHive && Api.Side == EnumAppSide.Server)
            {
                Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this);
            }
        }
        #endregion

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            ICoreAPI api = Api;
            if (api?.Side == EnumAppSide.Server)
            {
                api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this);
            }
        }
    }
}

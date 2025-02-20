using FromGoldenCombs.BlockBehaviors;
using FromGoldenCombs.Util.config;
using FromGoldenCombs.Util.Config;
using FromGoldenCombs.Util.HarmonyPatches;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static OpenTK.Graphics.OpenGL.GL;

namespace FromGoldenCombs.BlockEntities
{
    class BECeramicBroodPot : BlockEntityDisplay
    {
        double harvestableAtTotalHours;
        double cooldownUntilTotalHours;
        
        int quantityNearbyFlowers;
        int quantityNearbyHives;
        float _activityLevel;
        RoomRegistry roomreg;
        float roomness;
        public static SimpleParticleProperties Bees;
        int scanQuantityNearbyFlowers;
        int scanQuantityNearbyHives;
        int scanIteration;
        public bool isActiveHive = false;
        double cropChargeGrowthHours = 24;
        double chargesPerDay = FGCServerConfig.Current.ceramicBaseChargesPerDay; //Number of hours until the hive accumulates a new grow charge.
        double cropChargeAtTotalHours;
        double cooldownUntilCropCharge;
        int cropcharges;
        int maxCropCharges = FGCServerConfig.Current.ceramicMaxCropCharges;
        int cropChargeRange = FGCServerConfig.Current.ceramicCropRange;
        float harvestBase;
        EnumHivePopSize _hivePopSize;
        //TODO: Implement Config Option To Set AllowUndergroundApiculture.
        bool AllowUndergroundApiculture = false;

        public readonly InventoryGeneric inv;
        public override InventoryBase Inventory => inv;

        public EnumHivePopSize HivePopSize { 
            get { return _hivePopSize; } 
        }

        public float ActivityLevel
        {
            get { return _activityLevel; }
        }
        public override string InventoryClassName => "ceramicbroodpot";

        public BECeramicBroodPot()
        {
            inv = new InventoryGeneric(1, "hivepot-slot", null, null);
        }

        static BECeramicBroodPot()
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
            RegisterGameTickListener(OnScanForFlowers, api.World.Rand.Next(5000) + 30000);
            //TODO: Implement Config Option To Set This Value.
            this.AllowUndergroundApiculture = this.AllowUndergroundApiculture;
            //PushEventOnBlockBroken
            roomreg = Api.ModLoader.GetModSystem<RoomRegistry>();

            if (api.Side == EnumAppSide.Client)
            {
                ICoreClientAPI capi = api as ICoreClientAPI;
                Shape shape = capi.Assets.TryGet(new AssetLocation("fromgoldencombs", "shapes/block/hive/ceramic/ceramicbroodpot.json")).ToObject<Shape>();

                if (api.Side == EnumAppSide.Client)
                {
                    RegisterGameTickListener(SpawnBeeParticles, 300);
                }
            }
            Api.Event.RegisterEventBusListener(managePollinationBoost, 0.5, "cropbreak");
            Api.Event.RegisterEventBusListener(managePollinationBoost, 0.5, "berryharvest");
            Api.Event.RegisterEventBusListener(managePollinationBoost, 0.5, "fruitharvest");
            harvestBase = (FGCServerConfig.Current.ClayPotDaysToHarvestIn30DayMonths * (Api.World.Calendar.DaysPerMonth/ 30f)) * api.World.Calendar.HoursPerDay;
        }

        private void managePollinationBoost(string eventName, ref EnumHandling handling, IAttribute data)
        {
            TreeAttribute tdata = data as TreeAttribute;
            BlockPos cropPos = new(tdata.GetInt("x"), tdata.GetInt("y"), tdata.GetInt("z"));
            int deltaX = cropPos.X - Pos.X;
            int deltaY = cropPos.Y - Pos.Y;
            int deltaZ = cropPos.Z - Pos.Z;

            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

            if (isActiveHive && _hivePopSize != EnumHivePopSize.Poor)
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

        private void manageCropBoost(BlockPos cropPos, double distance, ref EnumHandling handling) { 
            
            if (cropcharges >= 1 && Api.World.BlockAccessor.GetBlock(cropPos).HasBehavior<PushEventOnCropBreakBehavior>() && distance < FGCServerConfig.Current.ceramicCropRange)
            {

                if (Api.World.BlockAccessor.GetBlock(cropPos) is BlockCrop crop && Api.World.BlockAccessor.GetBlockEntity(cropPos.DownCopy()) is BlockEntityFarmland farmland)
                {

                    if(Api.World.BlockAccessor.GetBlock(cropPos).GetBehavior<PushEventOnCropBreakBehavior>().validCropStages.Contains<int>(crop.CurrentCropStage)) {
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

            if (cropcharges >= 1 && Api.World.BlockAccessor.GetBlockEntity(fruitFoliagePos) is BlockEntityFruitTreePart beFTP && distance < FGCServerConfig.Current.ceramicCropRange)
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



        public void SetHiveSize(int size)
        {
            _hivePopSize = (EnumHivePopSize)size;
        }

        public bool OnInteract(IPlayer byPlayer)
        {
            Block hive = Api.World.BlockAccessor.GetBlock(Pos,0);
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Empty)
            {
                if (TryTake(byPlayer))
                {
                    MarkDirty(true);
                    updateMeshes();
                    return true;
                }
            } else if (slot.Itemstack.Collectible.WildCardMatch(new AssetLocation("game", "skep-populated-*")) && !isActiveHive)
            {
                byPlayer.InventoryManager.ActiveHotbarSlot.TakeOutWhole();

                isActiveHive = true;
                updateMeshes();
                return true;
            }
            else if (TryPut(slot) ) {
                {
                    Api.World.BlockAccessor.ExchangeBlock(Api.World.BlockAccessor.GetBlock(hive.CodeWithVariant("top", "withtop")).BlockId, Pos);
                    MarkDirty(true);
                }
                updateMeshes();
                return true; //This prevents TryPlaceBlock from passing if TryPut fails.
            }
            return false;

        }

        private bool TryTake(IPlayer player)
        {
                ItemSlot activeHotbarSlot = player.InventoryManager.ActiveHotbarSlot;
                BlockContainer blockContainer = this.Api.World.BlockAccessor.GetBlock(Pos, 0) as BlockContainer;
                int index = 0;
            if (!inv[index].Empty)
            {

                ItemStack stack = inv[0].TakeOut(1);
                player.InventoryManager.TryGiveItemstack(stack, false);
                if (stack.StackSize > 0)
                {
                    this.Api.World.SpawnItemEntity(stack, this.Pos, null);
                }
                Api.World.BlockAccessor.ExchangeBlock(Api.World.BlockAccessor.GetBlock(this.Block.CodeWithVariant("top", "notop")).BlockId, Pos);
                return true;
            }
            else if (activeHotbarSlot.Empty && activeHotbarSlot.StorageType == EnumItemStorageFlags.Backpack)
            {
                ItemStack stack = blockContainer.OnPickBlock(this.Api.World, Pos);
                SetAttributes(stack);

                if (player.InventoryManager.TryGiveItemstack(stack.Clone(), true))
                {
                    Api.World.BlockAccessor.SetBlock(0, Pos);
                    return true;
                }
            }
            return false;
        }

        public void TryPutDirect(ItemStack stack)
        {
            int index = 0;
            if (inv[index].Empty
               && stack.Block.FirstCodePart() == "hivetop" && stack.Block.Variant["type"] != "raw")
            {
                inv[index].Itemstack = stack;
            }

        }

        private bool TryPut(ItemSlot slot)
        {
            int index = 0;
            if (inv[index].Empty
                && slot.Itemstack.Collectible.FirstCodePart() == "hivetop" && slot.Itemstack.Collectible.Variant["type"] != "raw")
            {
                slot.TryPutInto(Api.World, inv[index]);
                if(inv[index].Itemstack.Block.Variant["type"] != "raw" && isActiveHive)
                {
                    cooldownUntilTotalHours = 0;
                }
                return true;
            }
            return false;
        }

        public virtual void SetAttributes(ItemStack hiveStack)
        {
            hiveStack.Attributes.SetInt("scanIteration", scanIteration);
            hiveStack.Attributes.SetInt("quantityNearbyFlowers", 0);
            hiveStack.Attributes.SetInt("quantityNearbyHives", 0);
            hiveStack.Attributes.SetInt("scanQuantityNearbyFlowers", 0);
            hiveStack.Attributes.SetInt("scanQuantityNearbyHives", 0);
            hiveStack.Attributes.SetBool("isactivehive", isActiveHive);
            hiveStack.Attributes.SetDouble("cooldownUntilTotalHours", 0);
            hiveStack.Attributes.SetDouble("harvestableAtTotalHours", 0);
            hiveStack.Attributes.SetInt("hiveHealth", (int)_hivePopSize);
            hiveStack.Attributes.SetFloat("roomness", 0.0f);
            hiveStack.Attributes.SetInt("cropCharges", 0);
        }
        //Rendering Processes
        readonly Matrixf mat = new();

        public override void updateMeshes()
        {
            mat.Identity();
            mat.RotateYDeg(this.Block.Shape.rotateY);

            base.updateMeshes();
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            mat.Identity();
            return base.OnTesselation(mesher, tessThreadTesselator);

        }

        public void TestHarvestable(float dt)
        {

            bool hasEmptyHivetop = !inv[0].Empty && inv[0]?.Itemstack?.Block.Variant["type"] == "empty";
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
            double distance = Math.Abs(conds.Temperature + (roomness > 0 ? 5 : 0) - optimalTemp); //The roomness is added to account for the presence of a greenhouse
            double range = Math.Max(maxTemp - optimalTemp, optimalTemp - minTemp);
            float beeParticleModifier = 1f - (float)(distance / range);
            _activityLevel = GameMath.Clamp(beeParticleModifier, 0f, 1f);

            if (!isActiveHive) { cropcharges = 0; return; }
            //Reset timers during winter - Vanilla Settings
            //if (temp <= -10)
            //Reset timers when temp drops below 15c - FGC Settings


            bool tempOutOfRange = false;

            if ((threeDayTemp < minTemp || threeDayTemp > maxTemp) && quantityNearbyFlowers != 0)
            {
                harvestableAtTotalHours = worldTime + HarvestableTime(harvestBase);
                cooldownUntilTotalHours = worldTime + 2 * 24;
                tempOutOfRange = true;
            }

            if (HivePopSize > 0 && !tempOutOfRange)
            {
                handleCropCharges(worldTime);
            }

            //If not cooling down
            if (worldTime > cooldownUntilTotalHours && hasEmptyHivetop && quantityNearbyFlowers>0)
            {
                //If harvestableAtHours is not currently set, but the hivesize is greater than Poor
                if (harvestableAtTotalHours == 0 && _hivePopSize > EnumHivePopSize.Poor)
                {
                    harvestableAtTotalHours = worldTime + HarvestableTime(harvestBase);
                }

                //If harvestableAthours is reached, update inv[0] to harvestable honey pot,
                //then reset harvestableAtHours, and begin cooldown stage
                else if (worldTime > harvestableAtTotalHours && _hivePopSize > EnumHivePopSize.Poor)
                {
                    inv[0].Itemstack = new ItemStack(Api.World.GetBlock(inv[0]?.Itemstack?.Collectible.CodeWithVariant("type", "harvestable")), 1);
                    harvestableAtTotalHours = 0;
                    cooldownUntilTotalHours = worldTime + 4 / 2 * 24;
                    updateMeshes();
                }
                //If not cooling down, but also does not have a valid honey pot, restart cooldown stage
                else if (cooldownUntilTotalHours <= 0 && !hasEmptyHivetop)
                {
                    harvestableAtTotalHours = 0;
                    cooldownUntilTotalHours = worldTime + 4 / 2 * 24;
                }
            }
            MarkDirty(true);
        }

        private void handleCropCharges(double worldTime)
        {
            if (worldTime > cropChargeAtTotalHours && cropcharges < maxCropCharges && _hivePopSize != EnumHivePopSize.Poor && quantityNearbyFlowers > 0)
            {
                if (cropChargeAtTotalHours != 0)
                {
                    int cropchargebase = (int)Math.Max(1,(Math.Round((worldTime - cropChargeAtTotalHours) / (float)(Api.World.Calendar.HoursPerDay * (float)(Api.World.Calendar.DaysPerMonth / 30f))))); ;
                    int cropchargegrowth = (int)Math.Max(1, ((cropchargebase * chargesPerDay) * (int)_hivePopSize));
                    cropcharges = (int)Math.Min(cropchargegrowth + cropcharges, maxCropCharges);
                }
                cropChargeAtTotalHours = worldTime + cropChargeGrowthHours;
            }
        }

        private double HarvestableTime(float harvestbase)
        {
            Random rand = new();
            return (harvestBase * .75) + ((harvestBase * .5) * rand.NextDouble());
        }

        readonly Vec3d startPos = new();
        readonly Vec3d endPos = new();
        Vec3f minVelo = new();
                             
        private void SpawnBeeParticles(float dt)
        {
            if (isActiveHive)
            {
                float dayLightStrength = Api.World.Calendar.GetDayLightStrength(Pos.X, Pos.Z);
                if (Api.World.Rand.NextDouble() > (2 * dayLightStrength - 0.5))                     
                    return;

                Random rand = Api.World.Rand;

                Bees.MinQuantity = _activityLevel;

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
        }

        private void OnScanForFlowers(float dt)
        {
            double worldTime = Api.World.Calendar.TotalHours;

            if (isActiveHive)
            {

                Room room = roomreg?.GetRoomForPosition(Pos);
                roomness = (room != null && room.SkylightCount > room.NonSkylightCount && room.ExitCount == 0) ? 1 : 0;
                
                if (_activityLevel <= 0) return;
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

                Block langstrothstacke = Api.World.GetBlock(new AssetLocation("langstrothstack-one-east"));
                Block langstrothstackn = Api.World.GetBlock(new AssetLocation("langstrothstack-one-north"));
                Block langstrothstacks = Api.World.GetBlock(new AssetLocation("langstrothstack-one-south"));
                Block langstrothstackw = Api.World.GetBlock(new AssetLocation("langstrothstack-one-west"));

                Block langstrothstack2e = Api.World.GetBlock(new AssetLocation("langstrothstack-two-east"));
                Block langstrothstack2n = Api.World.GetBlock(new AssetLocation("langstrothstack-two-north"));
                Block langstrothstack2s = Api.World.GetBlock(new AssetLocation("langstrothstack-two-south"));
                Block langstrothstack2w = Api.World.GetBlock(new AssetLocation("langstrothstack-two-west"));

                Block langstrothstack3e = Api.World.GetBlock(new AssetLocation("langstrothstack-three-east"));
                Block langstrothstack3n = Api.World.GetBlock(new AssetLocation("langstrothstack-three-north"));
                Block langstrothstack3s = Api.World.GetBlock(new AssetLocation("langstrothstack-three-south"));
                Block langstrothstack3w = Api.World.GetBlock(new AssetLocation("langstrothstack-three-west"));

                Api.World.BlockAccessor.WalkBlocks(Pos.AddCopy(minX, -5, minZ), Pos.AddCopy(minX + size - 1, 5, minZ + size - 1), (block, posx, posy, posz) =>
                {
                    
                    if (block.Id == 0 || (roomness > 0 && !room.Contains(new BlockPos(posx, posy, posz)))) return;
                    

                    if (block.Attributes != null && block.Attributes.IsTrue("beeFeed"))
                    {
                        scanQuantityNearbyFlowers++;
                    };

                    if (block == fullSkepN || block == fullSkepE || block == fullSkepS || block == fullSkepW
                    || block == wildhive1 || block == wildhive2
                    || block == claypothive || block == claypothive2 || block == claypothive3 || block == claypothive4
                    || block == langstrothstacke || block == langstrothstackn || block == langstrothstacks || block == langstrothstackw
                    || block == langstrothstack2e || block == langstrothstack2n || block == langstrothstack2s || block == langstrothstack2w
                    || block == langstrothstack3e || block == langstrothstack3n || block == langstrothstack3s || block == langstrothstack3w)
                    {
                        scanQuantityNearbyHives++;
                    }
                });
                scanIteration++;
                System.Diagnostics.Debug.WriteLine("Scan Iteration is " + scanIteration);
                if (scanIteration == 4)
                {
                    scanIteration = 0;
                    OnScanComplete();
                }
                MarkDirty(true);

            }
        }

        private void OnScanComplete()
        {
            quantityNearbyFlowers = scanQuantityNearbyFlowers;
            quantityNearbyHives = scanQuantityNearbyHives;
            _hivePopSize = (EnumHivePopSize)GameMath.Clamp(quantityNearbyFlowers - 3 * quantityNearbyHives, 0, 2);
        }



        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetInt("scanIteration", scanIteration);

            tree.SetInt("quantityNearbyFlowers", quantityNearbyFlowers);
            tree.SetInt("quantityNearbyHives", quantityNearbyHives);


            tree.SetInt("scanQuantityNearbyFlowers", scanQuantityNearbyFlowers);
            tree.SetInt("scanQuantityNearbyHives", scanQuantityNearbyHives);

            tree.SetBool("isactivehive", isActiveHive);
            tree.SetDouble("cooldownUntilTotalHours", cooldownUntilTotalHours);
            tree.SetDouble("harvestableAtTotalHours", harvestableAtTotalHours);
            tree.SetInt("hiveHealth", (int)_hivePopSize);
            tree.SetFloat("roomness", roomness);

            tree.SetDouble("cropChargeAtTotalHours", cropChargeAtTotalHours);
            tree.SetInt("cropCharges", cropcharges);
            tree.SetInt("maxCropCharges", maxCropCharges);
            tree.SetInt("cropcharges", cropcharges);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            scanIteration = tree.GetInt("scanIteration");

            quantityNearbyFlowers = tree.GetInt("quantityNearbyFlowers");
            quantityNearbyHives = tree.GetInt("quantityNearbyHives");

            scanQuantityNearbyFlowers = tree.GetInt("scanQuantityNearbyFlowers");
            scanQuantityNearbyHives = tree.GetInt("scanQuantityNearbyHives");

            isActiveHive = tree.GetBool("isactivehive");
            cooldownUntilTotalHours = tree.GetDouble("cooldownUntilTotalHours");
            harvestableAtTotalHours = tree.GetDouble("harvestableAtTotalHours");
            _hivePopSize = (EnumHivePopSize)tree.GetInt("hiveHealth");
            roomness = tree.GetFloat("roomness");
            
            cropChargeAtTotalHours = tree.GetDouble("cropChargeAtTotalHours");
            cropcharges = tree.GetInt("cropCharges");
            maxCropCharges = tree.GetInt("maxCropCharges");
            cropcharges = tree.GetInt("cropcharges");

            updateMeshes();

        }

        protected ModelTransform genTransform(ItemStack stack, int index)
        {

            ModelTransform transform = new();
            //Vec3f offset = new Vec3f(0, .1f, 0);
            //transform.Origin = new Vec3f(0.5f, 0.0f, 0.5f);
            //transform.WithRotation(new Vec3f(0f, this.Block.Shape.rotateY * GameMath.DEG2RAD, 0f));
            //transform.Translation = offset;
            return transform;
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            float minTemp = FGCServerConfig.Current.CeramicHiveMinTemp;
            float maxTemp = FGCServerConfig.Current.CeramicHiveMaxTemp == 0?37f:FGCServerConfig.Current.CeramicHiveMaxTemp;
            float temp = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues).Temperature + (roomness > 0 ? 5 : 0);
            ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);
            float todayNoonTemp = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, (Double)((int)(Api.World.Calendar.TotalDays)) + 0.66f).Temperature;
            float yesterdayNoonTemp = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, (Double)((int)(Api.World.Calendar.TotalDays - 1)) + 0.66f).Temperature;
            float twoDayAgoNoonTemp = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, (Double)((int)(Api.World.Calendar.TotalDays - 2)) + 0.66f).Temperature;
            if (conds == null) return;
            float threeDayTemp = (todayNoonTemp * 2 + yesterdayNoonTemp + twoDayAgoNoonTemp) / 4 + (roomness > 0 ? 5f : 0f);
            string tempReport = Lang.Get("fromgoldencombs:3DayTemp") + " " + (threeDayTemp > maxTemp ? Lang.Get("fromgoldencombs:3DayTooHot") : threeDayTemp < minTemp ? Lang.Get("fromgoldencombs:3DayTooCold") : Lang.Get("fromgoldencombs:3DayPerfect"));
            bool isOutTemp = (temp <= minTemp || temp >= maxTemp);

            if (isActiveHive)
            {
                double worldTime = Api.World.Calendar.TotalHours;
                int daysTillHarvest = (int)Math.Round((harvestableAtTotalHours - worldTime) / Api.World.Calendar.HoursPerDay);
                daysTillHarvest = daysTillHarvest <= 0 ? 0 : daysTillHarvest;
                string hiveState = Lang.Get("fromgoldencombs:nearbyflowers", quantityNearbyFlowers, Lang.Get(_hivePopSize.ToString()));

                dsc.AppendLine(hiveState);
                if (temp < minTemp)
                {
                    dsc.AppendLine(Lang.Get("fromgoldencombs:toocold"));
                }
                if (temp > maxTemp)
                {
                    dsc.AppendLine(Lang.Get("fromgoldencombs:toohot"));
                }
                if ((harvestableAtTotalHours - worldTime / 24 > 0) && this.Block.Variant["top"] == "withtop" && !isOutTemp)
                {
                    if (FGCServerConfig.Current.showcombpoptime)
                    {
                        dsc.AppendLine(Lang.Get("fromgoldencombs:timetillpop", daysTillHarvest < 1 ? Lang.Get("fromgoldencombs:lessthanday") : (daysTillHarvest + " " + Lang.Get("fromgoldencombs:days"))));
                    }
                }
                else if (isActiveHive && (this.Block.Variant["top"] == "notop"))
                {
                    dsc.AppendLine(Lang.Get("fromgoldencombs:nopot"));

                }
                else if (inv[0]?.Itemstack?.Collectible.Variant["type"] == "harvestable")
                {

                    dsc.AppendLine(Lang.Get("fromgoldencombs:fullpot"));
                }
                else if (quantityNearbyFlowers <= 0 && !isOutTemp)
                {
                    dsc.AppendLine(Lang.Get("fromgoldencombs:outgathering"));
                }
                if (this.roomness > 0f)
                {
                    dsc.AppendLine(Lang.Get("greenhousetempbonus", Array.Empty<object>()));

                }
                if (FGCServerConfig.Current.showExtraBeehiveInfo && (forPlayer.Entity.Controls.ShiftKey || FGCClientConfig.Current.alwaysShowHiveInfo == true))
                {
                    dsc.AppendLine(tempReport);
                    dsc.AppendLine(Lang.Get("fromgoldencombs:cropcharges") + " " + cropcharges);
                }
            }            
        }

        protected override float[][] genTransformationMatrices()
        {
            float[][] tfMatrices = new float[1][];
            for (int index = 0; index < 1; index++)
            {
                ItemStack itemstack = this.Inventory[index].Itemstack;
                if (itemstack != null)
                {
                    tfMatrices[index] = new Matrixf().Translate(0, 1.018f, 1).RotateXDeg(180f).Values;
                }
            }
            return tfMatrices;
        }
    }
}

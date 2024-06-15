using FromGoldenCombs.config;
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
    
    class BEFrameRack : BlockEntityDisplay
    {

        readonly InventoryGeneric inv;
        public override InventoryBase Inventory => inv;

        public override string InventoryClassName => "framerack";

        Block block;

        public BEFrameRack()
        {
            inv = new InventoryGeneric(10, "frameslot-0", null, null);
        }

        public override void Initialize(ICoreAPI api)
        {
         
            block = api.World.BlockAccessor.GetBlock(Pos, 0);
            base.Initialize(api);
        }

        public override void OnBlockBroken(IPlayer byPlayer)
        {
            // Don't drop inventory contents
        }
        
        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            CollectibleObject colObj = slot.Itemstack?.Collectible;
            bool isBeeframe = colObj?.FirstCodePart() == "beeframe";
            BlockContainer block = Api.World.BlockAccessor.GetBlock(blockSel.Position, 0) as BlockContainer;
            int index = blockSel.SelectionBoxIndex;
            block.SetContents(new(block), this.GetContentStacks());
            if (slot.Empty && index < 10)
            {
                if (TryTake(byPlayer, blockSel))
                {
                    MarkDirty(true);
                    return true;
                }
            }
            else if (slot.Itemstack?.Item?.Tool != null && slot.Itemstack?.Item?.Tool == EnumTool.Knife && index < 10 && !inv[index].Empty && inv[index].Itemstack.Collectible.Variant["harvestable"] == "harvestable")
            {
                if (TryHarvest(Api.World, byPlayer, inv[index]))
                {

                    slot.Itemstack.Item.DamageItem(Api.World, byPlayer.Entity, slot, 1);
                    MarkDirty(true);
                    return true;
                }
                MarkDirty(true);
            }
            else if (slot.Itemstack?.Item?.FirstCodePart() == "waxedflaxtwine" && index < 10 && !inv[index].Empty && inv[index].Itemstack.Collectible.Variant["harvestable"] == "lined")
            {
                ItemStack rackSlot = inv[index].Itemstack;
                if (TryRepair(slot, rackSlot, index))
                {

                    MarkDirty(true);
                    return true;
                }
                MarkDirty(true);
            }
            else if (slot.Itemstack?.Item?.FirstCodePart() == "frameliner" && index < 10 && !inv[index].Empty && inv[index].Itemstack.Collectible.Variant["harvestable"] == "empty")
            {
                inv[index].Itemstack = new ItemStack(Api.World.GetItem(inv[index].Itemstack.Item.CodeWithVariant("harvestable", "lined")));
                inv[index].Itemstack.Attributes.SetInt("durability", 32);
                slot.TakeOut(1);
                MarkDirty(true);
                return true;
            }
            else if (isBeeframe && index < 10)
            {
                MarkDirty(true);
                if (TryPut(slot, blockSel))
                {
                    return true;
                }

            }
            else if (slot.Empty
                     && (int)slot.StorageType == 2
                     && byPlayer.InventoryManager.TryGiveItemstack(block.OnPickBlock(Api.World, blockSel.Position)))
            {
                Api.World.BlockAccessor.SetBlock(0, blockSel.Position);
                MarkDirty(true);
                return true;
            }
            return false;
        }

        private bool TryPut(ItemSlot slot, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;

            for (int i = 0; i < inv.Count; i++)
            {
                int slotnum = (index + i) % inv.Count;
                    if (inv[slotnum].Empty)
                    {
                        int moved = slot.TryPutInto(Api.World, inv[slotnum]);
                        updateMeshes();
                        return moved > 0;
                    }
            }
            return false;
        }

        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;
            
            if (!inv[index].Empty)
            {
                ItemStack stack = inv[index].TakeOut(1);
                if (byPlayer.InventoryManager.TryGiveItemstack(stack))
                {
                    AssetLocation sound = stack.Block?.Sounds?.Place;
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                }

                if (stack.StackSize > 0)
                {
                    Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                updateMeshes();
                return true;
            }

            return false;
        }

        private bool TryHarvest(IWorldAccessor world, IPlayer player, ItemSlot rackStack)
        {
            ThreadSafeRandom rnd = new();
            
            ItemStack stackHandler;
            int durability;

            stackHandler = rackStack.Itemstack;

            //Check to see if harvestable rack will break when harvested
            if (rackStack.Itemstack.Attributes.GetInt("durability") == 1)
            {
                //Next use will destroy frame, swap it for an empty frame instead
                rackStack.Itemstack = new ItemStack(Api.World.GetItem(stackHandler.Item.CodeWithVariant("harvestable", "empty")));
            } else {
                rackStack.Itemstack.Collectible.DamageItem(Api.World, player.Entity, rackStack, 1);
                durability = rackStack.Itemstack.Attributes.GetInt("durability");
                rackStack.Itemstack = new ItemStack(Api.World.GetItem(stackHandler.Item.CodeWithVariant("harvestable", "lined")));
                rackStack.Itemstack.Attributes.SetInt("durability", durability);

            }
            Api.World.SpawnItemEntity(new ItemStack(Api.World.GetItem(new AssetLocation("game", "honeycomb")), rnd.Next(FromGoldenCombsConfig.Current.FrameMinYield, FromGoldenCombsConfig.Current.FrameMaxYield)), Pos.ToVec3d());
            return true;
        }

        private bool TryRepair(ItemSlot slot, ItemStack rackStack, int index)
        {
            int durability = rackStack.Attributes.GetInt("durability");
            int maxDurability = FromGoldenCombsConfig.Current.baseframedurability;

            if (durability == maxDurability)
            return false;

            rackStack.Attributes.SetInt("durability", (maxDurability - durability) < 16 ? maxDurability : durability + 16);
            slot.TakeOut(1);
            inv[index].Itemstack = rackStack;
            return true;
        }

        readonly Matrixf mat = new();

        public Vec3f getTranslation(Block block, int index)
        {
            float x = 0f;
            //float y = 0.069f;
            //float z = 0f;
            Vec3f translation = new(0f, 0f, 0f);
            if (block.Variant["side"] == "north")
            {
                translation.X = .7253f + .0625f * index - 1;
            }
            else if (block.Variant["side"] == "south")
            {
                translation.X = x = 0.2747f - .0625f * index;
            }
            else if (block.Variant["side"] == "west")
            {
                translation.Z = 0.2747f - .0625f * index;
            }
            else if (block.Variant["side"] == "east")
            {
                translation.Z = 0.7253f + .0625f * index - 1;
            }
            return translation;
        }
        protected override MeshData getOrCreateMesh(ItemStack stack, int index)
        {
            MeshData mesh = this.getMesh(stack);
            if (mesh != null)
            {
                return mesh;
            }
            IContainedMeshSource meshSource = stack.Collectible as IContainedMeshSource;
            if (meshSource != null)
            {
                mesh = meshSource.GenMesh(stack, this.capi.BlockTextureAtlas, this.Pos);
                mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0f, base.Block.Shape.rotateY * 0.017453292f, 0f);
            }
            else
            {
                ICoreClientAPI capi = this.Api as ICoreClientAPI;
                if (stack.Class == EnumItemClass.Block)
                {
                    mesh = capi.TesselatorManager.GetDefaultBlockMesh(stack.Block).Clone();
                }
                else
                {
                    this.nowTesselatingObj = stack.Collectible;
                    this.nowTesselatingShape = null;
                    CompositeShape shape = stack.Item.Shape;
                    if (((shape != null) ? shape.Base : null) != null)
                    {
                        this.nowTesselatingShape = capi.TesselatorManager.GetCachedShape(stack.Item.Shape.Base);
                    }
                    capi.Tesselator.TesselateItem(stack.Item, out mesh, this);
                    mesh.RenderPassesAndExtraBits.Fill((short)2);
                }
            }
            JsonObject attributes = stack.Collectible.Attributes;
            if (attributes != null && attributes[this.AttributeTransformCode].Exists)
            {
                JsonObject attributes2 = stack.Collectible.Attributes;
                ModelTransform transform = (attributes2 != null) ? attributes2[this.AttributeTransformCode].AsObject<ModelTransform>(null) : null;
                transform.EnsureDefaultValues();
                mesh.ModelTransform(transform);
            }
            if (stack.Class == EnumItemClass.Item && (stack.Item.Shape == null || stack.Item.Shape.VoxelizeTexture))
            {
                mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 1.5707964f, 0f, 0f);
                mesh.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.33f, 0.33f, 0.33f);
                mesh.Translate(getTranslation(block, index));
            }
            string key = this.getMeshCacheKey(stack);
            this.MeshCache[key] = mesh;
            return mesh;
        }


        protected override float[][] genTransformationMatrices()
        {
            //float x = 0f;
            //float y = 0.069f;
            //float z = 0f;
            float[][] tfMatrices = new float[10][];
            for (int index = 0; index < 10; index++)
            {

                Vec3f translation = new(0f, 0.069f, 0f);

                if (block.Variant["side"] == "north")
                {
                    translation.X = .7253f + .0625f * index - 1;
                    tfMatrices[index] = new Matrixf().Translate(translation.X, translation.Y, translation.Z).Values;
                }
                else if (block.Variant["side"] == "south")
                {
                    translation.X = 0.2747f - .0625f * index;
                    tfMatrices[index] = new Matrixf().Translate(translation.X, translation.Y, translation.Z).Values;
                }
                else if (block.Variant["side"] == "west")
                {
                    translation.Z = 0.2747f - .0625f * index + 1;
                    tfMatrices[index] = new Matrixf().Translate(translation.X, translation.Y, translation.Z).RotateYDeg(90).Values;
                }
                else if (block.Variant["side"] == "east")
                {

                    translation.Z = 0.7253f + .0625f * index;
                    tfMatrices[index] = new Matrixf().Translate(translation.X, translation.Y, translation.Z).RotateYDeg(90).Values;
                }

            }
            return tfMatrices;
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if (forPlayer.CurrentBlockSelection == null)
            {
                base.GetBlockInfo(forPlayer, sb);
            }
            else { 
                sb.AppendLine();
                for (int i = 0; i < 10; i++)
                {
                    ItemSlot slot = inv[i];
                    if (slot.Empty)
                    {
                        sb.AppendLine(Lang.Get("Empty"));
                    }
                    else
                    {
                        sb.AppendLine(slot.Itemstack.GetName());
                    }
                }
            }
        }

    }
}


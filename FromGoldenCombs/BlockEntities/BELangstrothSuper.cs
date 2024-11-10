using FromGoldenCombs.Items;
using System;
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
    //TODO: Consider adding a lid object, or adding an animation showing the lid being slid off (This sounds neat). 
    //TODO: Find out how to get animation functioning
    //TODO: Fix selection box issue
    
    class BELangstrothSuper : BlockEntityDisplay
    {

        readonly InventoryGeneric inv;
        public override InventoryBase Inventory => inv;

        public override string InventoryClassName => "langstrothsuper";

        Block block;

        public BELangstrothSuper()
        {
            inv = new InventoryGeneric(10, "frameslot-0", null, null);
        }

        public override void Initialize(ICoreAPI api)
        {
            block = api.World.BlockAccessor.GetBlock(Pos, 0);
            base.Initialize(api);
        }
                
        public override void OnBlockBroken(IPlayer player)
        {
            // Don't drop inventory contents
        }

        //TODO: Add animations to Langstroth Super
        //BlockEntityAnimationUtil AnimUtil
        //{
        //    get { return GetBehavior<BEBehaviorAnimatable>()?.animUtil; }
        //}

        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot activeHotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            ItemStack itemstack = activeHotbarSlot.Itemstack;
            bool flag = (itemstack != null ? itemstack.Collectible.FirstCodePart() == "beeframe" : false);
                //((itemstack != null) ? itemstack.Collectible : null) is LangstrothFrame;
            BlockContainer blockContainer = this.Api.World.BlockAccessor.GetBlock(blockSel.Position, 0) as BlockContainer;
            blockContainer.SetContents(new ItemStack(blockContainer, 1), base.GetContentStacks(true));
            if (!byPlayer.Entity.Controls.ShiftKey && !activeHotbarSlot.Empty && activeHotbarSlot.Itemstack.Collectible.FirstCodePart(0) == "langstrothbroodtop" && activeHotbarSlot.Itemstack.Collectible.Variant["primary"] == base.Block.Variant["primary"] && activeHotbarSlot.Itemstack.Collectible.Variant["accent"] == base.Block.Variant["accent"])
            {
                if (this.inv.Empty)
                {
                    this.Api.World.BlockAccessor.SetBlock(this.Api.World.BlockAccessor.GetBlock(new AssetLocation("fromgoldencombs", string.Concat(new string[]
                    {
                        "langstrothbrood-empty-",
                        base.Block.Variant["primary"],
                        "-",
                        base.Block.Variant["accent"],
                        "-",
                        this.block.Variant["side"]
                    }))).BlockId, this.Pos);
                    activeHotbarSlot.TakeOut(1);
                    base.MarkDirty(true, null);
                    return true;
                }
                ICoreClientAPI coreClientAPI = byPlayer.Entity.World.Api as ICoreClientAPI;
                if (coreClientAPI != null)
                {
                    coreClientAPI.TriggerIngameError(this, "nonemptysuper", Lang.Get("fromgoldencombs:nonemptysuper", Array.Empty<object>()));
                }
            }
            else if ((activeHotbarSlot.Empty || !flag) && blockSel.SelectionBoxIndex < 10 && base.Block.Variant["open"] == "open")
            {
                if (this.TryTake(byPlayer, blockSel))
                {
                    base.MarkDirty(true, null);
                    return true;
                }
            }
            else if (flag && blockSel.SelectionBoxIndex < 10 && base.Block.Variant["open"] == "open")
            {
                base.MarkDirty(true, null);
                if (this.TryPut(activeHotbarSlot, blockSel))
                {
                    return true;
                }
            }
            else
            {
                if (byPlayer.Entity.Controls.ShiftKey && activeHotbarSlot.Itemstack == null /*&& activeHotbarSlot.StorageType == EnumItemStorageFlags.Backpack*/ && this.Api.World.BlockAccessor.GetBlock(blockSel.Position).Variant["open"] == "closed" && byPlayer.InventoryManager.TryGiveItemstack(blockContainer.OnPickBlock(this.Api.World, blockSel.Position), false))
                {
                    this.Api.World.BlockAccessor.SetBlock(0, blockSel.Position);
                    base.MarkDirty(true, null);
                    return true;
                }
                if (base.Block.Variant["open"] == "open" && !byPlayer.Entity.Controls.Sneak)
                {
                    this.Api.World.BlockAccessor.ExchangeBlock(this.Api.World.GetBlock(blockContainer.CodeWithVariant("open", "closed")).BlockId, blockSel.Position);
                    updateMeshes();
                    base.MarkDirty(true, null);
                    return true;
                }
                if (base.Block.Variant["open"] == "closed" && !byPlayer.Entity.Controls.Sneak)
                {
                    this.Api.World.BlockAccessor.ExchangeBlock(this.Api.World.GetBlock(blockContainer.CodeWithVariant("open", "open")).BlockId, blockSel.Position);
                    updateMeshes();
                    base.MarkDirty(true, null);
                    return true;
                }
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

                //updateMeshes();
                return true;
            }

            return false;
        }

        public Vec3f getTranslation(Block block,int index)
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
                mesh.Translate(getTranslation(block,index));
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
                    translation.Z = 0.2747f - .0625f * index +1;
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
            int index = forPlayer.CurrentBlockSelection.SelectionBoxIndex;
            if (forPlayer.CurrentBlockSelection == null)
            {
                base.GetBlockInfo(forPlayer, sb);
            }
            else if (this.Block.Variant["open"] == "closed")
            {

                return;
            }
            else if (index == 10)
            {
                
                sb.AppendLine("");
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
            else if (index < 10)
            {
                ItemSlot slot = inv[index];
                sb.AppendLine("");
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


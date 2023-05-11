using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace FromGoldenCombs.Blocks.Langstroth
{
    class LangstrothBrood : LangstrothCore
    {
        /// <summary>Call when the player right clicks the block.</summary>
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            
            if (slot.Empty)
            {
                ItemStack stack = new(api.World.BlockAccessor.GetBlock(blockSel.Position, 0));
                if (byPlayer.InventoryManager.TryGiveItemstack(stack))
                {
                    api.World.BlockAccessor.SetBlock(0, blockSel.Position);
                    return true;
                }

            } else if (slot.Itemstack?.Collectible.FirstCodePart() == "skep" && slot.Itemstack.Collectible.Variant["type"] == "populated"  && this.Variant["populated"] == "empty")
            {
                api.World.BlockAccessor.SetBlock(api.World.BlockAccessor.GetBlock(this.CodeWithVariant("populated","populated")).BlockId, blockSel.Position);
                byPlayer.InventoryManager.ActiveHotbarSlot.TakeOutWhole();
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            WorldInteraction[] wi;

            wi = ObjectCacheUtil.GetOrCreate(api, "superInteractions1", () =>
            {

                return new WorldInteraction[] {
                            new WorldInteraction() {
                                    ActionLangCode = "fromgoldencombs:blockhelp-langstrothbrood",
                                    MouseButton = EnumMouseButton.Right,
                                                               }
                    };

            });

            return wi;
        }

        public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos) {
            StringBuilder sb = new();
                        
            return Variant["populated"].UcFirst() + " " + sb.ToString() + base.GetPlacedBlockName(world, pos);
        }
    }
}

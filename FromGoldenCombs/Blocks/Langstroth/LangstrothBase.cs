using FromGoldenCombs.Blocks.Langstroth;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace VFromGoldenCombs.Blocks.Langstroth
{
    class LangstrothBase : LangstrothCore
    {
        //Core block for all Langstroth Blocks
        //TODO: Implement PickUpOnRightClick Behavior
        /// <summary>Called when player right clicks this block.</summary>
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Empty && (int)slot.StorageType == 2)
            {
                ItemStack stack = new(api.World.BlockAccessor.GetBlock(blockSel.Position, 0));
                api.World.BlockAccessor.SetBlock(0, blockSel.Position);
                return byPlayer.InventoryManager.TryGiveItemstack(stack);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}

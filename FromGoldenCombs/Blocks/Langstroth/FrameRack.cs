using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.Blocks.Langstroth;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace FromGoldenCombs.Blocks
{
    class FrameRack : LangstrothCore
    {
        
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            // Todo: Add interaction help
        }
        
        
        //Picks up block while retaining its contents
        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            ItemStack stack = base.OnPickBlock(world, pos);

            BEFrameRack bed = world.BlockAccessor.GetBlockEntity(pos) as BEFrameRack;
            if (bed is BEFrameRack)
            {
                SetContents(stack, bed.GetContentStacks());
            }

            return stack;
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnNeighbourBlockChange(world, blockPos, blockPos);
            base.OnBlockPlaced(world,blockPos,byItemStack);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BEFrameRack beFrameRack = (BEFrameRack)world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEFrameRack;

            if (beFrameRack is BEFrameRack)
            {
                beFrameRack.updateMeshes();
                return beFrameRack.OnInteract(byPlayer, blockSel);
            }
            return false;
        }
    }
}

using FromGoldenCombs.BlockEntities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Common;

namespace FromGoldenCombs.Blocks.Langstroth
{
    class LangstrothStack : LangstrothCore
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            // Todo: Add interaction help
        }
        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            if (world.BlockAccessor.GetBlock(pos.DownCopy(), 0).BlockMaterial == EnumBlockMaterial.Air)
            {
                this.OnBlockBroken(world, pos, null);
                if (world.BlockAccessor.GetBlock(pos.UpCopy(), 0) is LangstrothCore)
                {
                    world.BlockAccessor.GetBlock(pos.UpCopy(), 0).OnNeighbourBlockChange(world, pos.UpCopy(), neibpos);
                }

            }
            base.OnNeighbourBlockChange(world, pos, neibpos);
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (world.Side == EnumAppSide.Server && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative))
            {
                ItemStack[] array = new ItemStack[]{};
                for (int i = 0; i < array.Length; i++)
                {
                    world.SpawnItemEntity(array[i], new Vec3d((double)pos.X + 0.5, (double)pos.Y + 0.5, (double)pos.Z + 0.5), null);
                }
                world.PlaySoundAt(this.Sounds.GetBreakSound(byPlayer), (double)pos.X, (double)pos.Y, (double)pos.Z, byPlayer, true, 32f, 1f);
            }
            if (this.EntityClass != null)
            {
                BlockEntity blockEntity = world.BlockAccessor.GetBlockEntity(pos);
                if (blockEntity != null)
                {
                    blockEntity.OnBlockBroken();
                }
            }
            world.BlockAccessor.SetBlock(0, pos);
        }

        public override float GetAmbientSoundStrength(IWorldAccessor world, BlockPos pos)
        {
            if(world.BlockAccessor.GetBlockEntity(pos) is BELangstrothStack pot && pot.isHiveActive())
            {
                float soundVolume = 0f;
                switch ((int)pot.HivePopSize)
                {
                    case 0: soundVolume = 0.33f; break;
                    case 1: soundVolume = 0.66f; break;
                    default: soundVolume = 1f; break;
                }
                return Math.Min(soundVolume * pot.ActivityLevel, 0.25f);
            }
            return 0f;
            
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {

            BELangstrothStack belangstrothstack = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELangstrothStack;
            if (belangstrothstack is BELangstrothStack) return belangstrothstack.OnInteract(byPlayer);
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            List<Cuboidf> curSelectionBoxes = new List<Cuboidf>();
            BELangstrothStack curBE = blockAccessor.GetBlockEntity<BELangstrothStack>(pos);
            curSelectionBoxes.Add(SelectionBoxes[0]);

            if (curBE != null)
            {
                for (int i = 1; i < curBE.Inventory.Count-1; i++)
                {
                    if (!curBE.Inventory[i].Empty)
                    {
                        curSelectionBoxes.Add(SelectionBoxes[i]);
                    }
                }

            }
            return curSelectionBoxes.ToArray();
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            WorldInteraction[] wi;

            wi = ObjectCacheUtil.GetOrCreate(api, "stackInteractions1", () =>
            {

                return new WorldInteraction[] {
                            new WorldInteraction() {
                                    ActionLangCode = "fromgoldencombs:blockhelp-langstrothstack",
                                    MouseButton = EnumMouseButton.Right,
                            }
                    };

            });

            return wi;
        }
    }
}

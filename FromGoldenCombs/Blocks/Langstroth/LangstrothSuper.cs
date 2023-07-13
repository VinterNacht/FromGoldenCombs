using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.Blocks.Langstroth;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace FromGoldenCombs.Blocks
{
    class LangstrothSuper : LangstrothCore
    {

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!(byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Block is LangstrothCore))
            {
                BELangstrothSuper belangstrothsuper = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELangstrothSuper;
                if (belangstrothsuper is BELangstrothSuper) return belangstrothsuper.OnInteract(byPlayer, blockSel);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
           
            WorldInteraction[] wi;
            WorldInteraction[] wi2 = null;
            WorldInteraction[] wi3 = null;
            WorldInteraction[] wx = null;

            wi = ObjectCacheUtil.GetOrCreate(api, "superInteractions1", () =>
            {

                return new WorldInteraction[] {
                    new WorldInteraction() {
                            ActionLangCode = "fromgoldencombs:blockhelp-langstrothsuper",
                            MouseButton = EnumMouseButton.Right

                        }
                    };
            });

            if (Variant["open"] == "open")
            {
                wi2 = ObjectCacheUtil.GetOrCreate(api, "superInteractions2", () =>
                {

                    return new WorldInteraction[] {
                            new WorldInteraction() {
                                    ActionLangCode = "fromgoldencombs:blockhelp-langstrothsuper-open",
                                    MouseButton = EnumMouseButton.Right
                            }
                    };

                });
            }

            if (Variant["open"] == "closed")
            {
                wi2 = ObjectCacheUtil.GetOrCreate(api, "superInteractions3", () =>
                {

                    return new WorldInteraction[] {
                            new WorldInteraction() {
                                    ActionLangCode = "fromgoldencombs:blockhelp-langstrothsuper-closed",
                                    MouseButton = EnumMouseButton.Right,
                                    HotKeyCode = "sneak"
                            }
                    };

                });

                wi3 = ObjectCacheUtil.GetOrCreate(api, "superInteractions4", () =>
                {
                    List<ItemStack> toplist = new();

                    foreach (Item item in api.World.Items)
                    {
                        if (item.Code == null) continue;

                        if (item.FirstCodePart() == "langstrothbroodtop")
                        {
                            toplist.Add(new ItemStack(item));
                        }
                    }

                    return new WorldInteraction[] {
                            new WorldInteraction() {
                                    ActionLangCode = "fromgoldencombs:blockhelp-langstrothsuper-broodtop",
                                    MouseButton = EnumMouseButton.Right,
                                    Itemstacks = toplist.ToArray()
                            }
                    };
                });
            }
            if (wi2 == null || wi3 == null)
            {
                return wi;
            } else if (wi3 == null)
            {
                return wi.Append(wi2);
            }
            return wi.Append(wi2).Append(wi3).Append(wx);
        }
    }
}

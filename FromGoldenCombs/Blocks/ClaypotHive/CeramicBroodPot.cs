using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using FromGoldenCombs.BlockEntities;
using Vintagestory.GameContent;
using Vintagestory.API.Util;
using System.Collections.Generic;
using Vintagestory.API.Datastructures;

namespace FromGoldenCombs.Blocks
{

    class CeramicBroodPot : BlockContainer
    {

        public object ActionLangCode { get; private set; }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            ItemStack stack = base.OnPickBlock(world, pos);
            if (world.BlockAccessor.GetBlockEntity(pos) is BECeramicBroodPot)
            {
                BECeramicBroodPot bec = world.BlockAccessor.GetBlockEntity(pos) as BECeramicBroodPot;
                stack.Attributes.SetBool("populated", bec.isActiveHive);
            };
            return stack;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {                      
            BECeramicBroodPot beCeramicBroodPot = (BECeramicBroodPot)world.BlockAccessor.GetBlockEntity(blockSel.Position);         
            if (beCeramicBroodPot is BECeramicBroodPot) return beCeramicBroodPot.OnInteract(byPlayer);
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack stack)
        {
            if (stack != null)
            {
                bool isHiveActive = stack.Attributes.GetBool("populated", false);
                base.OnBlockPlaced(world, blockPos);
                BECeramicBroodPot beCeramicBroodPot = (BECeramicBroodPot)world.BlockAccessor.GetBlockEntity(blockPos);
                if (beCeramicBroodPot == null) return;
                beCeramicBroodPot.isActiveHive = isHiveActive;
            }
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            BECeramicBroodPot beCeramicBroodPot = (BECeramicBroodPot)world.BlockAccessor.GetBlockEntity(pos);
            //If the hive is broken, and is populated, potentially spawn bee mob.
            if (beCeramicBroodPot.isActiveHive && world.Rand.NextDouble() < 0.4)
            {
                EntityProperties type = world.GetEntityType(new AssetLocation("beemob"));
                Entity entity = world.ClassRegistry.CreateEntity(type);

                if (entity != null)
                {
                    entity.ServerPos.X = pos.X + 0.5f;
                    entity.ServerPos.Y = pos.Y + 0.5f;
                    entity.ServerPos.Z = pos.Z + 0.5f;
                    entity.ServerPos.Yaw = (float)world.Rand.NextDouble() * 2 * GameMath.PI;
                    entity.Pos.SetFrom(entity.ServerPos);

                    entity.Attributes.SetString("origin", "brokenbeehive");
                    world.SpawnEntity(entity);
                }
            }
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            WorldInteraction[] wi = null;
            WorldInteraction[] wi2 = base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
            WorldInteraction[] wi3 = base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);

            
            if (world.BlockAccessor.GetBlockEntity(selection.Position) is BECeramicBroodPot pot)
            {
                if (pot != null)

                {

                    List<ItemStack> topList = new();
                    topList.Add(new ItemStack(api.World.BlockAccessor.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-empty"))));
                    topList.Add(new ItemStack(api.World.BlockAccessor.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-harvestable"))));

                    //Information about world interaction
                    if (!pot.isActiveHive)
                    {
                        wi = ObjectCacheUtil.GetOrCreate(api, "broodPotInteractions", () =>
                        {
                            List<ItemStack> skepList = new();
                            skepList.Add(new ItemStack(api.World.BlockAccessor.GetBlock(new AssetLocation("game", "skep-populated-east")), 1));

                            return new WorldInteraction[]
                            {
                            new WorldInteraction(){
                                ActionLangCode = "fromgoldencombs:blockhelp-ceramichive-empty-notop",
                                MouseButton = EnumMouseButton.Right,
                                Itemstacks = skepList.ToArray()
                            }
                            };
                        });
                    }

                    wi2 = ObjectCacheUtil.GetOrCreate(api, "broodPotInteractions2", () =>
                    {
                        return new WorldInteraction[]
                        {
                          new WorldInteraction(){
                                ActionLangCode = "Place or Remove honey pot",
                                MouseButton = EnumMouseButton.Right,
                                Itemstacks = topList.ToArray()
                       }
                     };
                    });

                    if (Variant["top"] == "notop")
                    {
                        wi3 = ObjectCacheUtil.GetOrCreate(api, "broodPotInteractions3", () =>
                            {

                                return new WorldInteraction[]
                                {
                            new WorldInteraction(){
                                ActionLangCode = "Pick up in empty bag slot",
                                MouseButton = EnumMouseButton.Right,
                                Itemstacks = null
                            }
                                };
                            });

                    }
                }

                if (wi != null)
                {
                    return wi.Append(wi2).Append(wi3);
                }
                return wi2.Append(wi3);
            }
            return wi;
        }

        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            return GetHandbookDropsFromBreakDrops(handbookStack, forPlayer);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return null;
        }
    }
}

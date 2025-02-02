using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FromGoldenCombs.BlockBehaviors
{
    internal class PushEventOnCropBreakBehavior : BlockBehaviorPushEventOnBlockBroken
    {
        public int[] validCropStages { get
            {
                return _validCropStages;
            }
        }

        public PushEventOnCropBreakBehavior(Block block) : base(block)
        {
        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);
            this._eventName = ((properties["eventName"] != null) ? properties["eventName"].AsString(null) : null);
            this._validCropStages = properties["validCropStages"].AsArray<int>();
            this._beeChanceMultiplier = 1 + properties["beeChanceMultiplier"].AsFloat();
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
        {
            if (byPlayer != null)
            {
                TreeAttribute tree = new TreeAttribute();
                tree.SetInt("x", pos.X);
                tree.SetInt("y", pos.Y);
                tree.SetInt("z", pos.Z);
                world.Api.Event.PushEvent(this._eventName, tree);
                handling = _bHandling;
            }

            if (handling == EnumHandling.PreventSubsequent)
            {
                if (world.Side == EnumAppSide.Server && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative))
                {
                    ItemStack[] drops = block.GetDrops(world, pos, byPlayer, _beeChanceMultiplier);
                    if (drops != null)
                    {
                        for (int i = 0; i < drops.Length; i++)
                        {
                            if (block.SplitDropStacks)
                            {
                                for (int j = 0; j < drops[i].StackSize; j++)
                                {
                                    ItemStack stack = drops[i].Clone();
                                    stack.StackSize = 1;
                                    world.SpawnItemEntity(stack, new Vec3d((double)pos.X + 0.5, (double)pos.Y + 0.5, (double)pos.Z + 0.5), null);
                                }
                            }
                            else
                            {
                                world.SpawnItemEntity(drops[i].Clone(), new Vec3d((double)pos.X + 0.5, (double)pos.Y + 0.5, (double)pos.Z + 0.5), null);
                            }
                        }
                    }
                    BlockSounds sounds = block.Sounds;
                    world.PlaySoundAt((sounds != null) ? sounds.GetBreakSound(byPlayer) : null, (double)pos.X, (double)pos.Y, (double)pos.Z, byPlayer, true, 32f, 1f);
                }
                block.SpawnBlockBrokenParticles(pos);
                world.BlockAccessor.SetBlock(0, pos);
            }

        }


        public void setHandling(EnumHandling handling)
        {
            _bHandling = handling;
        }
        private string _eventName;
        private EnumHandling _bHandling = EnumHandling.PassThrough;
        private float _beeChanceMultiplier;
        private int[] _validCropStages;
    }
}

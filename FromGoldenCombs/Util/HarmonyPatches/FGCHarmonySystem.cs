using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.MathTools;
using HarmonyLib;
using System.Text;
using System;
using Vintagestory.GameContent;
using System.Reflection;
using Vintagestory.API.Datastructures;
using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.Util.Config;


namespace FromGoldenCombs.Util.HarmonyPatches
{
    public class FGCHarmonySystem : ModSystem
    {
        private Harmony harmony;
        private readonly string harmonyId = "vinternacht.FGCPatches";

        private static ICoreServerAPI sapi;
        private static IWorldGenBlockAccessor thisBlockAccessor;

        public override void Start(ICoreAPI api)
        {
            PatchGame();
            base.Start(api);
        }
        private void PatchGame()
        {
            harmony = new Harmony(harmonyId);
            harmony.Patch(typeof(BlockEntityFruitTreePart).GetMethod("OnBlockInteractStop", BindingFlags.Instance | BindingFlags.Public),
                prefix: new HarmonyMethod(typeof(FGCHarmonySystem).GetMethod("OnBlockInteractStopPrefix", BindingFlags.Static | BindingFlags.Public))
            );
            harmony.Patch(typeof(Block).GetMethod("GetAmbientSoundStrength", BindingFlags.Instance | BindingFlags.Public),
            prefix: new HarmonyMethod(typeof(FGCHarmonySystem).GetMethod("GetAmbientSoundStrengthPrefix", BindingFlags.Static | BindingFlags.Public))
            );
            harmony.PatchAll();
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(BlockEntityFruitTreePart), "OnBlockInteractStop")]
        public static bool OnBlockInteractStopPrefix(BlockEntityFruitTreePart __instance, float secondsUsed, IPlayer byPlayer, BlockSelection blockSel)
        {

            if (byPlayer.Entity.Api.Side.IsServer()) { 
                if ((double)secondsUsed > 1.1 && __instance.FoliageState == EnumFoliageState.Ripe)
                {
                    counter++;
                    if (counter > 1) { 
                        //This is some jury-rigged bullshit until I can find out why this is getting called twice. 
                        counter = 0; return true; 
                    }
                    if (byPlayer != null)
                    {
                        TreeAttribute tree = new TreeAttribute();
                        tree.SetInt("x", __instance.Pos.X);
                        tree.SetInt("y", __instance.Pos.Y);
                        tree.SetInt("z", __instance.Pos.Z);
                        byPlayer.Entity.World.Api.Event.PushEvent("fruitharvest", tree);
                    }
                }
            }
            return true;
        }

        public static bool GetAmbientSoundStrengthPrefix(Block __instance, IWorldAccessor world, BlockPos pos, ref float __result)
        {
            if(__instance is not BlockSkep hive || __instance is not BlockBeehive wildHive)
                return true;

            float soundVolume = 0f;
            if (wildHive != null)
            {
                switch (FGCClientConfig.Current.wildHiveSoundVolume)
                {
                    case "normal": soundVolume = 1f; break;
                    case "high": soundVolume *= 2f; break;
                    case "loud": soundVolume *= 4f; break;
                    default: soundVolume = 1f; break;
                }
                __result = soundVolume;
                return false;
            }
                
            if (world.BlockAccessor.GetBlockEntity(pos) is BEFGCBeehive skep)
            {
                
                switch ((int)skep.hivePopSize)
                {
                    case 0: soundVolume = 0.44f; break;
                    case 1: soundVolume = 0.88f; break;
                    default: soundVolume = 1f; break;
                }
                switch (FGCClientConfig.Current.hiveSoundVolume)
                {
                    case "off": soundVolume = 0f; break;
                    case "soft": soundVolume *= 0.5f; break;
                    case "normal": soundVolume = 1f; break;
                    case "high": soundVolume *= 2f; break;
                    case "loud": soundVolume *= 4f; break;
                    default: soundVolume = 1f; break;
                }
                soundVolume = Math.Max(soundVolume * skep.actvitiyLevel, 0.4f);
                __result = soundVolume;
            }
            return false;
        }


        static int counter;

        public override void Dispose()
        {
            harmony?.UnpatchAll();
            harmony = null;
            sapi = null;
            thisBlockAccessor = null;

        }
    }
}

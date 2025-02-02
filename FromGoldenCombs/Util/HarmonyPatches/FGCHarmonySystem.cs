using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.MathTools;
using HarmonyLib;
using System.Text;
using System;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Client;
using System.Reflection;
using Vintagestory.ServerMods;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;


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

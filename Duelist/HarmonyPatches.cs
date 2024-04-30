using Assets.Code;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duelist
{
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        /// <summary>
        /// Initialises variables in this class that are required to perform patches, then executes harmony patches.
        /// </summary>
        /// <param name="core"></param>
        public static void PatchingInit()
        {
            Patching();
        }

        private static void Patching()
        {
            Harmony.DEBUG = false;
            string harmonyID = "ILikeGoodFood.SOFG.Duelist";
            Harmony harmony = new Harmony(harmonyID);

            if (Harmony.HasAnyPatches(harmonyID))
            {
                return;
            }

            // Battle end postfix
            harmony.Patch(original: AccessTools.Method(typeof(BattleAgents), "retreatOrFlee", new Type[] { typeof(UA), typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(BattleAgents_retreatOrFlee_Postfix)));

            // Template Patch
            // harmony.Patch(original: AccessTools.Method(typeof(), nameof(), new Type[] { typeof() }), postfix: new HarmonyMethod(patchType, nameof()));
        }

        private static void BattleAgents_retreatOrFlee_Postfix(BattleAgents __instance, UA defeated, UA victor)
        {
            if (__instance is BattleAgents_Duel)
            {
                if (defeated == null || defeated.isDead)
                {
                    return;
                }

                if (defeated.location != victor.location)
                {
                    defeated.map.adjacentMoveTo(defeated, victor.location);
                }
            }
        }
    }
}

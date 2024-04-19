using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duelist
{
    public class ModCore : ModKernel
    {
        private static ModCore modCore;

        private CommunityLib.ModCore comLib;

        public static ModCore Get() => modCore;

        public override void beforeMapGen(Map map)
        {
            modCore = this;

            getModKernels(map);
        }

        public override void afterLoading(Map map)
        {
            modCore = this;

            getModKernels(map);
        }

        private void getModKernels(Map map)
        {
            foreach (ModKernel kernel in map.mods)
            {
                if (kernel is CommunityLib.ModCore core)
                {
                    comLib = core;
                }
            }
        }
    }
}

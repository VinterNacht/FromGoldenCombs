using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace FromGoldenCombs.Util.Config
{
    class FGCClientConfig
    {
        public double configVersion = 1.1;
        public bool retainConfigOnVersionChange = false;
        public string hiveSoundVolume = "normal";


        public FGCClientConfig()
        { }

        public static FGCClientConfig Current { get; set; }


        public static FGCClientConfig GetClientDefault()
        {
            FGCClientConfig defaultClientConfig = new();
            defaultClientConfig.configVersion = 1.1;
            defaultClientConfig.retainConfigOnVersionChange = false;
            defaultClientConfig.hiveSoundVolume = "on";

            return defaultClientConfig;
        }

        internal static void createClientConfig(ICoreAPI api)
        {
            String[] validHiveVolumes = { (Lang.Get("fromgoldencombs:off")), (Lang.Get("fromgoldencombs:soft")), (Lang.Get("fromgoldencombs:normal")), (Lang.Get("fromgoldencombs:high")), (Lang.Get("fromgoldencombs:loud")) };
            double MasterClientConfigVersion = 1.0;
            try
            {
                FGCClientConfig ClientConfig = api.LoadModConfig<FGCClientConfig>("fromgoldencombs/fromgoldencombsclient.json");
                if (ClientConfig != null && ClientConfig.configVersion == MasterClientConfigVersion)
                {
                    //bottomStack.quantityNearbyFlowers, Lang.Get(bottomStack._hivePopSize.ToString()));
                    api.Logger.Notification(Lang.Get("fromgoldencombs:modclientconfigload"));
                    if (!validHiveVolumes.Contains<string>(ClientConfig.hiveSoundVolume))
                    {
                        api.Logger.Notification(Lang.Get("fromgoldencombs:validhivevolumenotfound", Lang.Get("fromgoldencombs:off"), Lang.Get("fromgoldencombs:soft"), Lang.Get("fromgoldencombs:normal"), Lang.Get("fromgoldencombs:high"), Lang.Get("fromgoldencombs:loud")));
                          ClientConfig.hiveSoundVolume = (Lang.Get("fromgoldencombs:normal"));
                    }

                    Current = ClientConfig;
                }
                else
                {
                    if ((ClientConfig?.configVersion != MasterClientConfigVersion) && !ClientConfig.retainConfigOnVersionChange)
                    {
                        api.Logger.Notification(Lang.Get("fromgoldencombs:wrongclientconfigversion"));
                    }
                    api.Logger.Notification(Lang.Get("fromgoldencombs:nomodclientconfig"));
                    Current = GetClientDefault();
                }
            }
            catch
            {
                Current = GetClientDefault();
                api.Logger.Error(Lang.Get("fromgoldencombs:defaultclientloaded"));
            }
            finally
            {
                api.StoreModConfig(Current, "fromgoldencombs/fromgoldencombsclient.json");
            }
        }
    }
}

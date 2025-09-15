using System.Collections.Generic;

namespace ET.Server
{
    public static partial class GateRealmAddressHelper
    {
        public static StartSceneConfig GetRealm(int zone)
        {
            foreach (var config in StartSceneConfigCategory.Instance.GetAll().Values)
            {
                if (config.Zone == zone && config.Type == SceneType.Realm)
                {
                    return config;
                }
            }
            return null;
        }
    }
}
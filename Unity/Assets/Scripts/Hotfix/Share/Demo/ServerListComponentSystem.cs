using System;
using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(ServerListComponent))]
    [FriendOf(typeof(ServerListComponent))]
    public static partial class ServerListComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ServerListComponent self)
        {
            
        }

        [EntitySystem]
        private static void Destroy(this ServerListComponent self)
        {
            self.ZoneList?.Clear();
            self.CurrentServer = null;
            self.LastLoginServer = null;
        }

        public static ServerInfo GetLatestServer(this ServerListComponent self)
        {
            ServerInfo latest = null;
            DateTime latestTime = DateTime.MinValue;
            
            foreach (var zone in self.ZoneList)
            {
                foreach (var server in zone.ServerList)
                {
                    if (server.Status != ServerStatus.Maintenance && server.OpenTime > latestTime)
                    {
                        latest = server;
                        latestTime = server.OpenTime;
                    }
                }
            }
            
            return latest;
        }
        
        public static ServerInfo GetRecommendServer(this ServerListComponent self)
        {
            foreach (var zone in self.ZoneList)
            {
                foreach (var server in zone.ServerList)
                {
                    if (server.IsRecommend && server.Status != ServerStatus.Maintenance)
                    {
                        return server;
                    }
                }
            }
            
            return self.GetLatestServer();
        }
    }
}
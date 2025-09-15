using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
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
            self.ZoneList.Clear();
        }

        /// <summary>
        /// 从服务器加载服务器列表
        /// </summary>
        public static async ETTask<bool> LoadServerListFromServer(this ServerListComponent self)
        {
            try
            {
                Scene scene = self.Scene();
                
                // 获取服务器列表需要连接到Realm服务器
                R2C_GetServerList response = await self.GetServerListFromRealm();
                if (response == null)
                {
                    Log.Error("LoadServerListFromServer failed: cannot connect to Realm");
                    return false;
                }

                if (response.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"LoadServerListFromServer failed: {response.Error}, {response.Message}");
                    return false;
                }

                // 清空现有数据 - 先销毁所有子实体，再清空列表
                foreach (var existingZone in self.ZoneList.ToList())
                {
                    existingZone.Dispose();
                }
                self.ZoneList.Clear();

                foreach (var protoZone in response.ZoneList)
                {
                    if (protoZone.ZoneId <= 0 || self.GetChild<ServerZone>(protoZone.ZoneId) != null)
                        continue;
                    
                    var zone = self.AddChildWithId<ServerZone>(protoZone.ZoneId);
                    zone.ZoneId = protoZone.ZoneId;
                    zone.ZoneName = string.IsNullOrEmpty(protoZone.ZoneName) ? $"Zone {protoZone.ZoneId}" : protoZone.ZoneName;

                    foreach (var protoServer in protoZone.ServerList)
                    {
                        if (protoServer.ServerId <= 0 || zone.GetChild<ServerInfo>(protoServer.ServerId) != null)
                            continue;
                        
                        var server = zone.AddChildWithId<ServerInfo>(protoServer.ServerId);
                        server.ServerId = protoServer.ServerId;
                        server.ServerName = string.IsNullOrEmpty(protoServer.ServerName) ? $"Server {protoServer.ServerId}" : protoServer.ServerName;
                        server.ServerIP = protoServer.ServerIP;
                        server.ServerPort = protoServer.ServerPort;
                        server.Status = (ServerStatus)protoServer.Status;
                        server.OnlineCount = protoServer.OnlineCount;
                        server.MaxCount = protoServer.MaxCount;
                        server.IsNew = protoServer.IsNew;
                        server.IsRecommend = protoServer.IsRecommend;
                        server.OpenTime = new DateTime(protoServer.OpenTime);
                        server.ZoneId = protoServer.ZoneId;
                        
                        zone.ServerList.Add(server);
                    }
                    
                    if (zone.ServerList.Count > 0)
                        self.ZoneList.Add(zone);
                    else
                        zone.Dispose();
                }

                if (response.LastLoginServerId > 0)
                    self.LastLoginServer = self.GetServerById(response.LastLoginServerId);
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"LoadServerListFromServer failed: {e}");
                return false;
            }
        }

        /// <summary>
        /// 选择服务器
        /// </summary>
        public static async ETTask<(bool success, string address, long key, long gateId)> SelectServer(this ServerListComponent self, int serverId)
        {
            try
            {
                Scene scene = self.Scene();
                ClientSenderComponent clientSenderComponent = scene.GetComponent<ClientSenderComponent>();
                if (clientSenderComponent == null)
                    return (false, "", 0, 0);
                
                var request = C2R_SelectServer.Create();
                request.ServerId = serverId;
                var response = await clientSenderComponent.Call(request) as R2C_SelectServer;

                if (response?.Error != ErrorCode.ERR_Success)
                    return (false, "", 0, 0);

                self.CurrentServer = self.GetServerById(serverId);
                self.LastLoginServer = self.CurrentServer;
                return (true, response.ServerAddress, response.Key, response.GateId);
            }
            catch (Exception e)
            {
                Log.Error($"SelectServer failed: {e}");
                return (false, "", 0, 0);
            }
        }

        /// <summary>
        /// 根据服务器ID获取服务器信息
        /// </summary>
        public static ServerInfo GetServerById(this ServerListComponent self, int serverId)
        {
            foreach (var zone in self.ZoneList)
            {
                foreach (var server in zone.ServerList)
                {
                    if (server.ServerId == serverId)
                        return server;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取推荐服务器
        /// </summary>
        public static ServerInfo GetRecommendServer(this ServerListComponent self, int zoneId = -1)
        {
            var targetZones = zoneId == -1 ? self.ZoneList : self.ZoneList.FindAll(z => z.ZoneId == zoneId);
            
            foreach (var zone in targetZones)
            {
                foreach (var server in zone.ServerList)
                {
                    if (server.IsRecommend && server.Status != ServerStatus.Maintenance && server.Status != ServerStatus.Full)
                        return server;
                }
            }
            return null;
        }

        /// <summary>
        /// 使用现有的Realm连接获取服务器列表
        /// </summary>
        private static async ETTask<R2C_GetServerList> GetServerListFromRealm(this ServerListComponent self)
        {
            try
            {
                ClientSenderComponent clientSenderComponent = self.Scene().GetComponent<ClientSenderComponent>();
                if (clientSenderComponent == null)
                    return null;
                
                var request = C2R_GetServerList.Create();
                return await clientSenderComponent.Call(request) as R2C_GetServerList;
            }
            catch (System.Exception e)
            {
                Log.Error($"GetServerListFromRealm exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// 获取新服
        /// </summary>
        public static ServerInfo GetNewServer(this ServerListComponent self, int zoneId = -1)
        {
            var targetZones = zoneId == -1 ? self.ZoneList : self.ZoneList.FindAll(z => z.ZoneId == zoneId);
            
            foreach (var zone in targetZones)
            {
                foreach (var server in zone.ServerList)
                {
                    if (server.IsNew && server.Status != ServerStatus.Maintenance && server.Status != ServerStatus.Full)
                        return server;
                }
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;

namespace ET.Server
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
        /// 从配置加载服务器列表
        /// </summary>
        public static async ETTask LoadServerConfig(this ServerListComponent self)
        {
            try
            {
                self.ZoneList.Clear();

                // 从ServerGroupConfig配置表加载区组信息
                var groupConfigs = ServerGroupConfigCategory.Instance?.GetAll();
                if (groupConfigs == null || groupConfigs.Count == 0)
                {
                    Log.Warning("ServerGroupConfigCategory为空或没有配置数据，创建默认区组");
                    await self.CreateDefaultZone();
                }
                else
                {
                    Log.Info($"从ServerGroupConfig加载 {groupConfigs.Count} 个区组配置");
                    
                    foreach (var (groupId, groupConfig) in groupConfigs)
                    {
                        Log.Info($"处理区组配置: GroupId={groupId}, GroupName='{groupConfig.GroupName}'");
                        
                        // 检查该区组是否有服务器
                        if (!self.HasServersInGroup(groupId))
                        {
                            Log.Info($"跳过没有服务器的区组: {groupId}");
                            continue;
                        }

                        var zone = self.AddChildWithId<ServerZone>(groupId);
                        zone.ZoneId = groupId;
                        zone.ZoneName = groupConfig.GroupName;

                        // 从ServerConfig配置表加载该区组下的服务器列表
                        await self.LoadServersForZone(zone, groupConfig);
                        
                        if (zone.ServerList.Count > 0)
                        {
                            self.ZoneList.Add(zone);
                            Log.Info($"区组 {zone.ZoneName} 加载完成，包含 {zone.ServerList.Count} 个服务器");
                        }
                        else
                        {
                            Log.Warning($"区组 {zone.ZoneName} 没有服务器，销毁");
                            zone.Dispose();
                        }
                    }
                }

                Log.Info($"Loaded {self.ZoneList.Count} zones with servers");
            }
            catch (Exception e)
            {
                Log.Error($"LoadServerConfig failed: {e}");
                throw;
            }

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 创建默认区组（当配置文件不存在时的后备方案）
        /// </summary>
        private static async ETTask CreateDefaultZone(this ServerListComponent self)
        {
            Log.Info("创建默认区组和服务器");
            
            var zone = self.AddChildWithId<ServerZone>(1);
            zone.ZoneId = 1;
            zone.ZoneName = "Default Zone";

            // 创建默认服务器
            var server = zone.AddChildWithId<ServerInfo>(1001);
            server.ServerId = 1001;
            server.ServerName = "Default Server";
            server.ServerIP = "127.0.0.1";
            server.ServerPort = 10002;
            server.Status = ServerStatus.Normal;
            server.OnlineCount = RandomGenerator.RandomNumber(10, 100);
            server.MaxCount = 1000;
            server.IsNew = true;
            server.IsRecommend = true;
            server.OpenTime = DateTime.Now.AddDays(-1);
            server.ZoneId = zone.ZoneId;
            zone.ServerList.Add(server);

            self.ZoneList.Add(zone);
            Log.Info($"创建默认区组完成: {zone.ZoneName}");
            
            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 检查指定区组是否有服务器配置
        /// </summary>
        private static bool HasServersInGroup(this ServerListComponent self, int groupId)
        {
            var serverConfigs = ServerConfigCategory.Instance?.GetAll();
            if (serverConfigs == null) return false;
            
            foreach (var (serverId, serverConfig) in serverConfigs)
            {
                if (serverConfig.GroupId == groupId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 为指定区组加载服务器列表
        /// </summary>
        private static async ETTask LoadServersForZone(this ServerListComponent self, ServerZone zone, ServerGroupConfig groupConfig)
        {
            // 从ServerConfig配置表加载该区组下的服务器信息
            var serverConfigs = ServerConfigCategory.Instance?.GetAll();
            if (serverConfigs == null)
            {
                Log.Warning($"ServerConfigCategory为空，无法加载区组 {zone.ZoneId} 的服务器");
                return;
            }

            foreach (var (serverId, serverConfig) in serverConfigs)
            {
                // 只加载属于当前区组的服务器
                if (serverConfig.GroupId != zone.ZoneId)
                    continue;

                var server = zone.AddChildWithId<ServerInfo>(serverConfig.Id);
                server.ServerId = serverConfig.Id;
                server.ServerName = serverConfig.ServerName;
                server.ServerIP = serverConfig.Host;
                server.ServerPort = serverConfig.Port;
                server.MaxCount = serverConfig.MaxPlayers;
                server.ZoneId = zone.ZoneId;
                
                // 根据配置的状态设置服务器状态
                server.Status = (ServerStatus)serverConfig.Status;
                
                // 模拟在线人数（实际应该从游戏服务器获取）
                server.OnlineCount = RandomGenerator.RandomNumber(0, serverConfig.MaxPlayers / 2);
                
                // 根据权重和开服时间设置推荐和新服状态
                server.IsRecommend = serverConfig.Weight >= 150; // 权重大于等于150的服务器推荐
                var openTime = new DateTime(serverConfig.OpenTime);
                server.OpenTime = openTime;
                server.IsNew = (DateTime.Now - openTime).TotalDays <= 7; // 7天内开服的为新服

                zone.ServerList.Add(server);
                Log.Info($"加载服务器: {server.ServerName} (ID:{serverConfig.Id}) IP:{server.ServerIP}:{server.ServerPort}");
            }

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 更新服务器状态（定期调用）
        /// </summary>
        public static void UpdateServerStatus(this ServerListComponent self)
        {
            foreach (var zone in self.ZoneList)
            {
                foreach (var server in zone.ServerList)
                {
                    // 这里可以从实际的游戏服务器获取状态信息
                    // 模拟状态更新
                    if (server.OnlineCount >= server.MaxCount * 0.9f)
                    {
                        server.Status = ServerStatus.Full;
                    }
                    else if (server.OnlineCount >= server.MaxCount * 0.7f)
                    {
                        server.Status = ServerStatus.Crowded;
                    }
                    else if (server.OnlineCount >= server.MaxCount * 0.3f)
                    {
                        server.Status = ServerStatus.Normal;
                    }
                    else
                    {
                        server.Status = ServerStatus.Smooth;
                    }
                }
            }
        }
    }
}
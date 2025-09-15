using System;
using System.Collections.Generic;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    public class C2R_GetServerListHandler : MessageSessionHandler<C2R_GetServerList, R2C_GetServerList>
    {
        protected override async ETTask Run(Session session, C2R_GetServerList request, R2C_GetServerList response)
        {
            Scene scene = session.Scene();
            
            try
            {
                // 从配置表获取区服信息
                var serverListComponent = scene.GetComponent<ServerListComponent>();
                if (serverListComponent == null)
                {
                    serverListComponent = scene.AddComponent<ServerListComponent>();
                    await serverListComponent.LoadServerConfig();
                }

                // 构建返回数据
                foreach (var zoneEntity in serverListComponent.ZoneList)
                {
                    var zone = ServerZoneProto.Create();
                    zone.ZoneId = zoneEntity.ZoneId;
                    zone.ZoneName = zoneEntity.ZoneName;
                    
                    foreach (var serverEntity in zoneEntity.ServerList)
                    {
                        var server = ServerInfoProto.Create();
                        server.ServerId = serverEntity.ServerId;
                        server.ServerName = serverEntity.ServerName;
                        server.ServerIP = serverEntity.ServerIP;
                        server.ServerPort = serverEntity.ServerPort;
                        server.Status = (int)serverEntity.Status;
                        server.OnlineCount = serverEntity.OnlineCount;
                        server.MaxCount = serverEntity.MaxCount;
                        server.IsNew = serverEntity.IsNew;
                        server.IsRecommend = serverEntity.IsRecommend;
                        server.OpenTime = serverEntity.OpenTime.Ticks;
                        server.ZoneId = serverEntity.ZoneId;
                        
                        zone.ServerList.Add(server);
                    }
                    
                    response.ZoneList.Add(zone);
                }

                // 获取最后登录的服务器ID（可从玩家数据或缓存中获取）
                response.LastLoginServerId = serverListComponent.LastLoginServer?.ServerId ?? 0;
            }
            catch (Exception e)
            {
                Log.Error($"GetServerList failed: {e}");
                response.Error = ErrorCode.ERR_ServerListLoadFailed;
                response.Message = "获取服务器列表失败";
            }

            await ETTask.CompletedTask;
        }
    }
}
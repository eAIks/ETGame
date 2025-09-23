using System.Linq;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    public class C2R_SelectServerHandler : MessageSessionHandler<C2R_SelectServer, R2C_SelectServer>
    {
        protected override async ETTask Run(Session session, C2R_SelectServer request, R2C_SelectServer response)
        {
            Scene scene = session.Scene();
            
            try
            {
                var serverListComponent = scene.GetComponent<ServerListComponent>();
                if (serverListComponent == null)
                {
                    response.Error = ErrorCode.ERR_ServerListNotLoaded;
                    response.Message = "服务器列表未加载";
                    return;
                }

                // 查找指定的服务器
                ServerInfo selectedServer = null;
                foreach (var zone in serverListComponent.ZoneList)
                {
                    selectedServer = zone.ServerList.FirstOrDefault(s => s.ServerId == request.ServerId);
                    if (selectedServer != null)
                        break;
                }

                if (selectedServer == null)
                {
                    response.Error = ErrorCode.ERR_ServerNotFound;
                    response.Message = "服务器不存在";
                    return;
                }

                // 检查服务器状态
                if (selectedServer.Status == ServerStatus.Maintenance)
                {
                    response.Error = ErrorCode.ERR_ServerMaintenance;
                    response.Message = "服务器维护中";
                    return;
                }

                if (selectedServer.Status == ServerStatus.Full)
                {
                    response.Error = ErrorCode.ERR_ServerFull;
                    response.Message = "服务器已满";
                    return;
                }

                // 记录最后登录的服务器
                serverListComponent.LastLoginServer = selectedServer;
                
                // 获取账号信息和UUID
                SessionPlayerComponent playerComponent = session.GetComponent<SessionPlayerComponent>();
                string account = playerComponent?.Account ?? "";
                string accountUUID = playerComponent?.AccountUUID ?? "";
                
                if (string.IsNullOrEmpty(account))
                {
                    response.Error = ErrorCode.ERR_AccountNameFormError;
                    response.Message = "账号信息异常";
                    return;
                }
                
                if (string.IsNullOrEmpty(accountUUID))
                {
                    response.Error = ErrorCode.ERR_SystemError;
                    response.Message = "账号UUID信息缺失";
                    return;
                }
                
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    response.Error = ErrorCode.ERR_SystemError;
                    response.Message = "数据库组件未找到";
                    return;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                // 使用协程锁防止并发操作同一记录
                long playerID;
                using (await scene.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.DB, 
                    (account + request.ServerId.ToString()).GetHashCode()))
                {
                    var existingRecords = await dbComponent.Query<AccountServer>(
                        record => record.Account == account && record.ServerId == request.ServerId, 
                        "ET.Server.AccountServerInfo");
                    
                    if (existingRecords.Count > 0)
                    {
                        // 更新现有记录的登录时间，使用已有的PlayerID
                        var accountServer = existingRecords[0];
                        accountServer.LoginTime = TimeInfo.Instance.ServerNow();
                        
                        // 更新AccountUUID（可能发生变化）
                        accountServer.AccountUUID = accountUUID;
                        
                        // 如果PlayerID为0，说明是旧数据，需要生成PlayerID
                        if (accountServer.PlayerID == 0)
                        {
                            accountServer.PlayerID = IdGenerater.Instance.GenerateId();
                            Log.Info($"为已有账号生成PlayerID: Account={account}, ServerId={request.ServerId}, PlayerID={accountServer.PlayerID}");
                        }
                        
                        playerID = accountServer.PlayerID;
                        await dbComponent.Save(accountServer, "ET.Server.AccountServerInfo");
                        Log.Info($"更新AccountServer: Account={account}, UUID={accountUUID}, ServerId={request.ServerId}, PlayerID={playerID}");
                    }
                    else
                    {
                        // 创建新的AccountServer记录，生成新的PlayerID并与ServerId绑定
                        long entityId = IdGenerater.Instance.GenerateId();
                        playerID = IdGenerater.Instance.GenerateId();
                        
                        AccountServer newAccountServer = scene.AddChildWithId<AccountServer, string, int>(
                            entityId, account, request.ServerId);
                        newAccountServer.PlayerID = playerID;
                        newAccountServer.AccountUUID = accountUUID;
                        
                        await dbComponent.Save(newAccountServer, "ET.Server.AccountServerInfo");
                        Log.Info($"创建新AccountServer: Account={account}, UUID={accountUUID}, ServerId={request.ServerId}, PlayerID={playerID}");
                    }
                }

                // 随机分配一个Gate
                StartSceneConfig config = RealmGateAddressHelper.GetGate(session.Zone(), "");
                Log.Debug($"gate address: {config}");
                
                // 向gate请求一个key,客户端可以拿着这个key连接gate
                R2G_GetLoginKey r2GGetLoginKey = R2G_GetLoginKey.Create();
                r2GGetLoginKey.Account = account;
                r2GGetLoginKey.AccountUUID = accountUUID;  // 传递UUID给Gate
                r2GGetLoginKey.PlayerID = playerID;  // 传递PlayerID给Gate
                r2GGetLoginKey.ServerId = request.ServerId;  // 传递配表中的服务器ID给Gate
                G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await session.Fiber().Root.GetComponent<MessageSender>().Call(
                    config.ActorId, r2GGetLoginKey);

                // 检查Gate响应是否成功
                if (g2RGetLoginKey.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"Gate请求失败: {g2RGetLoginKey.Error}, {g2RGetLoginKey.Message}");
                    response.Error = g2RGetLoginKey.Error;
                    response.Message = g2RGetLoginKey.Message ?? "获取登录凭证失败";
                    return;
                }

                response.ServerAddress = config.InnerIPPort.ToString();
                response.Key = g2RGetLoginKey.Key;
                response.GateId = g2RGetLoginKey.GateId;
                response.AccountUUID = accountUUID;  // 返回UUID给客户端
                response.PlayerID = g2RGetLoginKey.PlayerID;  // 返回PlayerID给客户端

                Log.Info($"Player selected server: {selectedServer.ServerName} (ID: {selectedServer.ServerId})");
                
                // 选择区服成功后，延迟关闭连接让客户端有时间处理响应
                CloseSession(session).Coroutine();
            }
            catch (System.Exception e)
            {
                Log.Error($"SelectServer failed: {e}");
                response.Error = ErrorCode.ERR_ServerSelectFailed;
                response.Message = "选择服务器失败";
            }

            await ETTask.CompletedTask;
        }
        
        
        private async ETTask CloseSession(Session session)
        {
            await session.Root().GetComponent<TimerComponent>().WaitAsync(1000);
            session.Dispose();
        }
    }
}
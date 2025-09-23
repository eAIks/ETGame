using System.Collections.Generic;

namespace ET.Server
{
    public static class PlayerDataService
    {
        public static async ETTask<PlayerData> QueryOrCreatePlayerByAccountAndServerId(Scene scene, string account, int serverId)
        {
            Log.Info($"PlayerDataService: 开始根据Account和ServerId查询或创建角色数据 Account={account}, ServerId={serverId}");
            
            DBManagerComponent dbManagerComponent = scene.Root().GetComponent<DBManagerComponent>();
            if (dbManagerComponent == null)
            {
                Log.Error("数据库管理组件未找到");
                return null;
            }

            DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
            Log.Info($"PlayerDataService: 获得数据库组件 Zone={scene.Zone()}");

            // 使用协程锁防止并发操作同一账号记录
            using (await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.DB, account.GetHashCode() % DBComponent.TaskCount))
            {
                // 第一步：从AccountServerInfo表获取PlayerId
                Log.Info($"PlayerDataService: 查询AccountServerInfo表获取PlayerId - Account={account}, ServerId={serverId}");
                var accountServerRecords = await dbComponent.Query<AccountServer>(
                    record => record.Account == account && record.ServerId == serverId, 
                    "ET.Server.AccountServerInfo");
                
                if (accountServerRecords.Count == 0)
                {
                    Log.Error($"PlayerDataService: AccountServerInfo中未找到对应记录 Account={account}, ServerId={serverId}");
                    return null;
                }
                
                long playerId = accountServerRecords[0].PlayerID;
                if (playerId == 0)
                {
                    Log.Error($"PlayerDataService: AccountServerInfo中PlayerId为0 Account={account}, ServerId={serverId}");
                    return null;
                }
                
                Log.Info($"PlayerDataService: 从AccountServerInfo获取到PlayerId={playerId}");
                
                // 第二步：根据PlayerId查询区服用户表
                string tableName = $"ET.Server.User.{serverId}";
                Log.Info($"PlayerDataService: 使用数据库表名 {tableName}");
                
                List<PlayerData> existingPlayers = await dbComponent.Query<PlayerData>(
                    player => player.PlayerId == playerId, tableName);

                Log.Info($"PlayerDataService: 查询区服表结果，找到 {existingPlayers.Count} 条记录");

                if (existingPlayers.Count > 0)
                {
                    // 找到已存在的玩家记录，更新最后登录时间并返回
                    PlayerData existingPlayer = existingPlayers[0];
                    existingPlayer.UpdateLastLoginTime();
                    await dbComponent.Save(existingPlayer, tableName);
                    
                    Log.Info($"PlayerDataService: 查询到已有角色，PlayerId={existingPlayer.PlayerId}, Account={account}, ServerId={serverId}");
                    return existingPlayer;
                }
                else
                {
                    Log.Info($"PlayerDataService: 未找到角色记录，开始创建新角色，使用PlayerId={playerId}");
                    
                    // 没有找到记录，使用AccountServerInfo中的PlayerId创建角色
                    PlayerData newPlayerData = await CreateNewPlayerData(scene, account, playerId, serverId);
                    
                    if (newPlayerData != null)
                    {
                        Log.Info($"PlayerDataService: 开始保存新角色到数据库，表名={tableName}");
                        await dbComponent.Save(newPlayerData, tableName);
                        Log.Info($"PlayerDataService: 成功创建并保存新角色: Account={account}, PlayerId={playerId}, ServerId={serverId}");
                    }
                    else
                    {
                        Log.Error($"PlayerDataService: 创建新角色失败");
                    }
                    
                    return newPlayerData;
                }
            }
        }

        /// <summary>
        /// 根据Account和ServerId获取PlayerID，然后查询用户数据
        /// 这是推荐的方法，符合新的架构设计
        /// </summary>
        public static async ETTask<PlayerData> GetPlayerDataByAccountAndServerId(Scene scene, string account, int serverId)
        {
            return await QueryOrCreatePlayerByAccountAndServerId(scene, account, serverId);
        }

        /// <summary>
        /// 根据Account和ServerId从AccountServerInfo表获取PlayerID
        /// </summary>
        public static async ETTask<long> GetPlayerIdByAccountAndServerId(Scene scene, string account, int serverId)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.Root().GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("GetPlayerIdByAccountAndServerId: 数据库管理组件未找到");
                    return 0;
                }

                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                // 从AccountServerInfo表获取PlayerID
                var accountServerRecords = await dbComponent.Query<AccountServer>(
                    record => record.Account == account && record.ServerId == serverId, 
                    "ET.Server.AccountServerInfo");
                
                if (accountServerRecords.Count == 0)
                {
                    Log.Error($"GetPlayerIdByAccountAndServerId: AccountServerInfo中未找到对应记录 Account={account}, ServerId={serverId}");
                    return 0;
                }
                
                long playerId = accountServerRecords[0].PlayerID;
                if (playerId == 0)
                {
                    Log.Error($"GetPlayerIdByAccountAndServerId: AccountServerInfo中PlayerId为0 Account={account}, ServerId={serverId}");
                    return 0;
                }
                
                Log.Info($"GetPlayerIdByAccountAndServerId: 获取到PlayerId={playerId}, Account={account}, ServerId={serverId}");
                return playerId;
            }
            catch (System.Exception e)
            {
                Log.Error($"GetPlayerIdByAccountAndServerId failed: {e}");
                return 0;
            }
        }

        private static async ETTask<PlayerData> CreateNewPlayerData(Scene scene, string account, long playerId, int serverId)
        {
            Log.Info($"PlayerDataService: 开始创建新PlayerData, Account={account}, PlayerId={playerId}");
            
            // 获取UserBase初始配置（假设使用ID=1作为默认配置）
            UserBaseConfig userBaseConfig = UserBaseConfigCategory.Instance.Get(1);
            if (userBaseConfig == null)
            {
                Log.Error("UserBase配置未找到，无法创建角色");
                return null;
            }

            Log.Info($"PlayerDataService: 找到UserBase配置，Health={userBaseConfig.Health}, Attack={userBaseConfig.Attack}");

            // 创建PlayerData实体 - 这个实体仅用于数据库操作，不需要长期保存在Scene中
            PlayerData playerData = scene.AddChildWithId<PlayerData, long, string>(
                IdGenerater.Instance.GenerateId(), playerId, account);

            // 从配置初始化角色属性
            playerData.InitFromConfig(userBaseConfig, account, serverId);

            Log.Info($"PlayerDataService: 成功创建PlayerData实体，Health={playerData.Health}, Attack={playerData.Attack}, Defense={playerData.Defense}");
            
            await ETTask.CompletedTask;
            return playerData;
        }
    }
}
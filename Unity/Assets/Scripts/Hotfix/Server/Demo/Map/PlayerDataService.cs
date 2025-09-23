using System.Collections.Generic;

namespace ET.Server
{
    public static class PlayerDataService
    {
        public static async ETTask<PlayerData> QueryOrCreatePlayerData(Scene scene, string account, long playerId, int serverId)
        {
            Log.Info($"PlayerDataService: 开始查询或创建角色数据 Account={account}, PlayerId={playerId}, ServerId={serverId}");
            
            DBManagerComponent dbManagerComponent = scene.Root().GetComponent<DBManagerComponent>();
            if (dbManagerComponent == null)
            {
                Log.Error("数据库管理组件未找到");
                return null;
            }

            // 根据服务器ID构建数据库表名：game_server_<serverId>
            string tableName = $"ET.Server.User.{serverId}";
            Log.Info($"PlayerDataService: 使用数据库表名 {tableName}");
            
            DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
            Log.Info($"PlayerDataService: 获得数据库组件 Zone={scene.Zone()}");

            // 使用协程锁防止并发操作同一玩家记录
            using (await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.DB, playerId % DBComponent.TaskCount))
            {
                Log.Info($"PlayerDataService: 开始查询数据库，PlayerId={playerId}");
                
                // 查询玩家表中是否存在该角色
                List<PlayerData> existingPlayers = await dbComponent.Query<PlayerData>(
                    player => player.UserId == playerId, tableName);

                Log.Info($"PlayerDataService: 查询结果，找到 {existingPlayers.Count} 条记录");

                if (existingPlayers.Count > 0)
                {
                    // 角色已存在，更新最后登录时间
                    PlayerData existingPlayer = existingPlayers[0];
                    existingPlayer.UpdateLastLoginTime();
                    await dbComponent.Save(existingPlayer, tableName);
                    
                    Log.Info($"PlayerDataService: 查询到已有角色并更新登录时间: Account={account}, PlayerId={playerId}, ServerId={serverId}");
                    return existingPlayer;
                }
                else
                {
                    Log.Info($"PlayerDataService: 角色不存在，开始创建新角色");
                    
                    // 角色不存在，从UserBase配置初始化新角色
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
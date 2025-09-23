namespace ET.Server
{
    /// <summary>
    /// UserInfo相关的系统方法，用于数据库操作
    /// </summary>
    [FriendOf(typeof(UserInfo))]
    public static class UserInfoDBSystem
    {
        /// <summary>
        /// 根据Account和ServerId创建或查询用户角色信息
        /// </summary>
        public static async ETTask CreateUserInfoIfNotExists(Scene scene, string account, int serverId)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("CreateUserInfoIfNotExists: DBManagerComponent not found");
                    return;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                // 第一步：从AccountServerInfo表获取PlayerId
                var accountServerRecords = await dbComponent.Query<AccountServer>(
                    record => record.Account == account && record.ServerId == serverId, 
                    "ET.Server.AccountServerInfo");
                
                if (accountServerRecords.Count == 0)
                {
                    Log.Error($"CreateUserInfoIfNotExists: AccountServerInfo中未找到对应记录 Account={account}, ServerId={serverId}");
                    return;
                }
                
                long playerID = accountServerRecords[0].PlayerID;
                if (playerID == 0)
                {
                    Log.Error($"CreateUserInfoIfNotExists: AccountServerInfo中PlayerId为0 Account={account}, ServerId={serverId}");
                    return;
                }
                
                Log.Info($"CreateUserInfoIfNotExists: 从AccountServerInfo获取到PlayerId={playerID}");
                
                // 第二步：查询是否已存在角色（使用区服表）
                string tableName = $"ET.Server.User.{serverId}";
                var existingUsers = await dbComponent.Query<UserInfo>(
                    user => user.PlayerID == playerID, 
                    tableName);
                    
                if (existingUsers.Count == 0)
                {
                    // 创建新角色，使用默认配置ID = 1
                    long entityId = IdGenerater.Instance.GenerateId();
                    UserInfo newUser = scene.AddChildWithId<UserInfo, long, int>(entityId, playerID, 1);
                    newUser.SetName($"Player_{playerID}"); // 默认角色名
                    
                    await dbComponent.Save(newUser, tableName);
                    Log.Info($"创建新角色: PlayerID={playerID}, Account={account}, ServerId={serverId}, Name={newUser.Name}");
                }
                else
                {
                    Log.Info($"角色已存在: PlayerID={playerID}, Account={account}, ServerId={serverId}");
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"CreateUserInfoIfNotExists failed: {e}");
            }
        }
        
        /// <summary>
        /// 更新用户最后登录时间
        /// </summary>
        public static async ETTask UpdateUserInfoLastLoginTime(Scene scene, string account, int serverId)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    return;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                // 第一步：从AccountServerInfo表获取PlayerId
                var accountServerRecords = await dbComponent.Query<AccountServer>(
                    record => record.Account == account && record.ServerId == serverId, 
                    "ET.Server.AccountServerInfo");
                
                if (accountServerRecords.Count == 0)
                {
                    Log.Error($"UpdateUserInfoLastLoginTime: AccountServerInfo中未找到对应记录 Account={account}, ServerId={serverId}");
                    return;
                }
                
                long playerID = accountServerRecords[0].PlayerID;
                if (playerID == 0)
                {
                    Log.Error($"UpdateUserInfoLastLoginTime: AccountServerInfo中PlayerId为0 Account={account}, ServerId={serverId}");
                    return;
                }
                
                // 第二步：更新区服表中的用户信息
                string tableName = $"ET.Server.User.{serverId}";
                var existingUsers = await dbComponent.Query<UserInfo>(
                    user => user.PlayerID == playerID, 
                    tableName);
                    
                if (existingUsers.Count > 0)
                {
                    var userInfo = existingUsers[0];
                    userInfo.UpdateLastLoginTime();
                    await dbComponent.Save(userInfo, tableName);
                    Log.Info($"更新用户最后登录时间: PlayerID={playerID}, Account={account}, ServerId={serverId}");
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"UpdateUserInfoLastLoginTime failed: {e}");
            }
        }
        
        /// <summary>
        /// 根据Account和ServerId获取用户信息
        /// </summary>
        public static async ETTask<UserInfo> GetUserInfoByAccountAndServerId(Scene scene, string account, int serverId)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("GetUserInfoByAccountAndServerId: DBManagerComponent not found");
                    return null;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                // 第一步：从AccountServerInfo表获取PlayerId
                var accountServerRecords = await dbComponent.Query<AccountServer>(
                    record => record.Account == account && record.ServerId == serverId, 
                    "ET.Server.AccountServerInfo");
                
                if (accountServerRecords.Count == 0)
                {
                    Log.Error($"GetUserInfoByAccountAndServerId: AccountServerInfo中未找到对应记录 Account={account}, ServerId={serverId}");
                    return null;
                }
                
                long playerID = accountServerRecords[0].PlayerID;
                if (playerID == 0)
                {
                    Log.Error($"GetUserInfoByAccountAndServerId: AccountServerInfo中PlayerId为0 Account={account}, ServerId={serverId}");
                    return null;
                }
                
                // 第二步：从区服表查询用户信息
                string tableName = $"ET.Server.User.{serverId}";
                var existingUsers = await dbComponent.Query<UserInfo>(
                    user => user.PlayerID == playerID, 
                    tableName);
                    
                return existingUsers.Count > 0 ? existingUsers[0] : null;
            }
            catch (System.Exception e)
            {
                Log.Error($"GetUserInfoByAccountAndServerId failed: {e}");
                return null;
            }
        }
    }
}
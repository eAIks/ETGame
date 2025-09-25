namespace ET.Server
{
    /// <summary>
    /// RoleInfo相关的数据库操作系统
    /// </summary>
    [FriendOf(typeof(RoleInfo))]
    [FriendOf(typeof(DBComponent))]
    public static class RoleInfoDBSystem
    {
        /// <summary>
        /// 使用AccountServerInfo中的PlayerId创建角色
        /// 保存到ET.Server.User.RoleInfo表
        /// </summary>
        public static async ETTask<RoleInfo> CreateRoleFromAccountServerInfo(Scene scene, string account, int serverId)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("CreateRoleFromAccountServerInfo: DBManagerComponent not found");
                    return null;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                // 第一步：从AccountServerInfo表获取PlayerId
                var accountServerRecords = await dbComponent.Query<AccountServer>(
                    record => record.Account == account && record.ServerId == serverId, 
                    "ET.Server.AccountServerInfo");
                
                if (accountServerRecords.Count == 0)
                {
                    Log.Error($"CreateRoleFromAccountServerInfo: AccountServerInfo中未找到对应记录 Account={account}, ServerId={serverId}");
                    return null;
                }
                
                long playerId = accountServerRecords[0].PlayerID;
                if (playerId == 0)
                {
                    Log.Error($"CreateRoleFromAccountServerInfo: AccountServerInfo中PlayerId为0 Account={account}, ServerId={serverId}");
                    return null;
                }
                
                Log.Info($"CreateRoleFromAccountServerInfo: 从AccountServerInfo获取到PlayerId={playerId}");
                
                // 第二步：检查是否已存在角色信息
                var existingRoles = await dbComponent.Query<RoleInfo>(
                    role => role.PlayerId == playerId, 
                    "ET.Server.User.RoleInfo");
                    
                if (existingRoles.Count > 0)
                {
                    Log.Info($"角色信息已存在: PlayerId={playerId}, 昵称={existingRoles[0].NickName}");
                    return existingRoles[0];
                }
                
                // 第三步：创建新的角色信息
                long entityId = IdGenerater.Instance.GenerateId();
                RoleInfo newRole = scene.AddChildWithId<RoleInfo, long>(entityId, playerId);
                
                // 保存到数据库
                await dbComponent.Save(newRole, "ET.Server.User.RoleInfo");
                Log.Info($"成功创建角色信息: PlayerId={playerId}, 昵称={newRole.NickName}, Account={account}, ServerId={serverId}");
                
                return newRole;
            }
            catch (System.Exception e)
            {
                Log.Error($"CreateRoleFromAccountServerInfo failed: {e}");
                return null;
            }
        }
        
        /// <summary>
        /// 根据PlayerId获取角色信息
        /// </summary>
        public static async ETTask<RoleInfo> GetRoleInfoByPlayerId(Scene scene, long playerId)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("GetRoleInfoByPlayerId: DBManagerComponent not found");
                    return null;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                var roleRecords = await dbComponent.Query<RoleInfo>(
                    role => role.PlayerId == playerId, 
                    "ET.Server.User.RoleInfo");
                
                return roleRecords.Count > 0 ? roleRecords[0] : null;
            }
            catch (System.Exception e)
            {
                Log.Error($"GetRoleInfoByPlayerId failed: {e}");
                return null;
            }
        }
        
        /// <summary>
        /// 根据Account和ServerId获取角色信息
        /// </summary>
        public static async ETTask<RoleInfo> GetRoleInfoByAccountAndServerId(Scene scene, string account, int serverId)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("GetRoleInfoByAccountAndServerId: DBManagerComponent not found");
                    return null;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                // 第一步：从AccountServerInfo表获取PlayerId
                var accountServerRecords = await dbComponent.Query<AccountServer>(
                    record => record.Account == account && record.ServerId == serverId, 
                    "ET.Server.AccountServerInfo");
                
                if (accountServerRecords.Count == 0)
                {
                    Log.Error($"GetRoleInfoByAccountAndServerId: AccountServerInfo中未找到对应记录 Account={account}, ServerId={serverId}");
                    return null;
                }
                
                long playerId = accountServerRecords[0].PlayerID;
                if (playerId == 0)
                {
                    Log.Error($"GetRoleInfoByAccountAndServerId: AccountServerInfo中PlayerId为0 Account={account}, ServerId={serverId}");
                    return null;
                }
                
                // 第二步：根据PlayerId查询角色信息
                return await GetRoleInfoByPlayerId(scene, playerId);
            }
            catch (System.Exception e)
            {
                Log.Error($"GetRoleInfoByAccountAndServerId failed: {e}");
                return null;
            }
        }
        
        /// <summary>
        /// 更新角色信息到数据库
        /// </summary>
        public static async ETTask<bool> UpdateRoleInfo(Scene scene, RoleInfo roleInfo)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("UpdateRoleInfo: DBManagerComponent not found");
                    return false;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                roleInfo.UpdateLastTime();
                await dbComponent.Save(roleInfo, "ET.Server.User.RoleInfo");
                Log.Info($"成功更新角色信息: PlayerId={roleInfo.PlayerId}");
                
                return true;
            }
            catch (System.Exception e)
            {
                Log.Error($"UpdateRoleInfo failed: {e}");
                return false;
            }
        }
        
        /// <summary>
        /// 删除角色信息
        /// </summary>
        public static async ETTask<bool> DeleteRoleInfo(Scene scene, long playerId)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("DeleteRoleInfo: DBManagerComponent not found");
                    return false;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                
                await dbComponent.Remove<RoleInfo>(playerId, "ET.Server.User.RoleInfo");
                Log.Info($"成功删除角色信息: PlayerId={playerId}");
                
                return true;
            }
            catch (System.Exception e)
            {
                Log.Error($"DeleteRoleInfo failed: {e}");
                return false;
            }
        }
        
    }
}
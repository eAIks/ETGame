namespace ET.Server
{
    /// <summary>
    /// RoleInfo功能测试示例
    /// 演示如何使用AccountServerInfo中的PlayerId创建角色
    /// </summary>
    public static class RoleInfoTestExample
    {
        /// <summary>
        /// 测试角色创建功能的示例方法
        /// 使用示例：在需要创建角色的地方调用此方法
        /// </summary>
        public static async ETTask TestCreateRole(Scene scene, string testAccount, int testServerId)
        {
            Log.Info($"开始测试角色创建功能: Account={testAccount}, ServerId={testServerId}");
            
            try
            {
                // 方法1: 使用Account和ServerId创建角色
                RoleInfo roleInfo = await RoleInfoDBSystem.CreateRoleFromAccountServerInfo(scene, testAccount, testServerId);
                
                if (roleInfo != null)
                {
                    Log.Info($"角色创建成功!");
                    Log.Info($"  PlayerId: {roleInfo.PlayerId}");
                    Log.Info($"  昵称: {roleInfo.NickName}");
                    Log.Info($"  大境界: {roleInfo.MajorRealm}");
                    Log.Info($"  小境界: {roleInfo.MinorRealm}");
                    Log.Info($"  等级: {roleInfo.Level}");
                    Log.Info($"  鼎炉等级: {roleInfo.CauldronLevel}");
                    Log.Info($"  经验值: {roleInfo.CurrentExp}");
                    Log.Info($"  灵石: {roleInfo.SpiritStone}");
                    Log.Info($"  创建时间: {roleInfo.CreateTime}");
                    Log.Info($"  更新时间: {roleInfo.LastUpdateTime}");
                    
                    // 测试角色属性修改
                    await TestRoleModification(scene, roleInfo);
                }
                else
                {
                    Log.Error("角色创建失败!");
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"测试角色创建功能时发生异常: {e}");
            }
        }
        
        /// <summary>
        /// 测试角色属性修改
        /// </summary>
        private static async ETTask TestRoleModification(Scene scene, RoleInfo roleInfo)
        {
            Log.Info("开始测试角色属性修改...");
            
            try
            {
                // 测试修改昵称
                roleInfo.SetNickName("测试修真者");
                Log.Info($"修改昵称为: {roleInfo.NickName}");
                
                // 测试增加经验值
                roleInfo.AddExp(1000);
                Log.Info($"增加经验值1000，当前经验: {roleInfo.CurrentExp}");
                
                // 测试增加灵石
                roleInfo.AddSpiritStone(500);
                Log.Info($"增加灵石500，当前灵石: {roleInfo.SpiritStone}");
                
                // 测试设置境界
                roleInfo.SetRealm(1, 2);
                Log.Info($"设置境界为 大境界1，小境界2");
                
                // 测试升级
                roleInfo.LevelUp();
                Log.Info($"角色升级，当前等级: {roleInfo.Level}");
                
                // 测试鼎炉升级
                roleInfo.UpgradeCauldron();
                Log.Info($"鼎炉升级，当前鼎炉等级: {roleInfo.CauldronLevel}");
                
                // 保存修改到数据库
                bool saveResult = await RoleInfoDBSystem.UpdateRoleInfo(scene, roleInfo);
                if (saveResult)
                {
                    Log.Info("角色信息修改已保存到数据库");
                }
                else
                {
                    Log.Error("角色信息保存失败");
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"测试角色属性修改时发生异常: {e}");
            }
        }
        
        /// <summary>
        /// 测试查询角色功能
        /// </summary>
        public static async ETTask TestQueryRole(Scene scene, string testAccount, int testServerId)
        {
            Log.Info($"开始测试角色查询功能: Account={testAccount}, ServerId={testServerId}");
            
            try
            {
                // 方法1: 根据Account和ServerId查询
                RoleInfo roleInfo1 = await RoleInfoDBSystem.GetRoleInfoByAccountAndServerId(scene, testAccount, testServerId);
                if (roleInfo1 != null)
                {
                    Log.Info($"通过Account和ServerId查询成功: 昵称={roleInfo1.NickName}, PlayerId={roleInfo1.PlayerId}");
                }
                else
                {
                    Log.Info("通过Account和ServerId未找到角色信息");
                }
                
                // 如果找到了角色，再用PlayerId查询
                if (roleInfo1 != null)
                {
                    // 方法2: 根据PlayerId查询
                    RoleInfo roleInfo2 = await RoleInfoDBSystem.GetRoleInfoByPlayerId(scene, roleInfo1.PlayerId);
                    if (roleInfo2 != null)
                    {
                        Log.Info($"通过PlayerId查询成功: 昵称={roleInfo2.NickName}, 等级={roleInfo2.Level}");
                    }
                    else
                    {
                        Log.Info("通过PlayerId未找到角色信息");
                    }
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"测试角色查询功能时发生异常: {e}");
            }
        }
        
        /// <summary>
        /// 完整的角色创建和管理测试流程
        /// </summary>
        public static async ETTask RunFullRoleTest(Scene scene, string testAccount, int testServerId)
        {
            Log.Info("=== 开始完整的角色功能测试 ===");
            
            // 1. 测试角色创建
            await TestCreateRole(scene, testAccount, testServerId);
            
            // 2. 测试角色查询
            await TestQueryRole(scene, testAccount, testServerId);
            
            Log.Info("=== 角色功能测试完成 ===");
        }
    }
}
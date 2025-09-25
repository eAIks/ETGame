namespace ET.Server
{
    /// <summary>
    /// 境界等级测试工具类
    /// </summary>
    public static class RealmLevelTest
    {
        /// <summary>
        /// 根据等级查找境界配置（用于测试）
        /// </summary>
        public static RealmLevelConfig FindRealmConfigByLevel(int level)
        {
            try
            {
                // 获取所有境界配置
                var allConfigs = RealmLevelConfigCategory.Instance.GetAll();
                
                // 遍历所有配置，查找等级范围匹配的配置
                foreach (var kvp in allConfigs)
                {
                    var config = kvp.Value;
                    if (level >= config.LevelMin && level <= config.LevelMax)
                    {
                        return config;
                    }
                }
                
                Log.Warning($"未找到等级{level}对应的境界配置");
                return null;
            }
            catch (System.Exception e)
            {
                Log.Error($"查找境界配置时发生异常: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 测试不同等级的境界名称转换
        /// </summary>
        public static void TestRealmNames()
        {
            int[] testLevels = { 1, 5, 15, 25, 35, 45, 55, 65, 75, 85, 95, 105, 115, 125 };
            
            Log.Info("=== 境界名称测试开始 ===");
            
            foreach (int level in testLevels)
            {
                var config = FindRealmConfigByLevel(level);
                if (config != null)
                {
                    Log.Info($"等级{level}: {config.StageName} {config.SubStageName} (境界ID:{config.StageId}-{config.SubStage})");
                }
                else
                {
                    Log.Warning($"等级{level}: 未找到对应境界配置");
                }
            }
            
            Log.Info("=== 境界名称测试结束 ===");
        }
    }
}
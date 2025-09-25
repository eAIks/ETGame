using System;
using System.Threading.Tasks;

namespace ET.Server
{
    /// <summary>
    /// 装备生成器测试类
    /// </summary>
    public static class EquipmentGeneratorTest
    {
        /// <summary>
        /// 测试装备生成功能
        /// </summary>
        public static async ETTask TestEquipmentGeneration(Scene scene)
        {
            Log.Info("开始测试装备生成功能...");
            
            await ETTask.CompletedTask;
            
            try
            {
                // 创建装备生成器组件
                var equipmentGenerator = scene.GetComponent<EquipmentGeneratorComponent>();
                if (equipmentGenerator == null)
                {
                    equipmentGenerator = scene.AddComponent<EquipmentGeneratorComponent>();
                }
                
                // 测试不同鼎炉等级的装备生成
                int[] testCauldronLevels = { 1, 5, 10, 15, 20, 25, 30 };
                
                foreach (int cauldronLevel in testCauldronLevels)
                {
                    Log.Info($"测试鼎炉等级: {cauldronLevel}");
                    
                    // 为每个装备槽位生成装备
                    for (int slotType = 0; slotType <= 8; slotType++)
                    {
                        var equipment = equipmentGenerator.GenerateEquipment(cauldronLevel, slotType);
                        
                        // 获取品质配置
                        var qualityConfig = EquipQualityConfigCategory.Instance.Get(equipment.Quality);
                        
                        Log.Info($"  槽位{slotType}: {equipment.Name}, 品质:{qualityConfig.QualityName}, 颜色:{equipment.Color}, 攻击:{equipment.Attack}, 防御:{equipment.Defense}, 血量:{equipment.Health}");
                    }
                }
                
                Log.Info("装备生成测试完成！");
            }
            catch (Exception e)
            {
                Log.Error($"装备生成测试失败: {e}");
            }
        }
        
        /// <summary>
        /// 测试概率分布
        /// </summary>
        public static void TestProbabilityDistribution(Scene scene, int cauldronLevel, int testCount = 1000)
        {
            Log.Info($"开始测试鼎炉等级{cauldronLevel}的概率分布（测试{testCount}次）...");
            
            try
            {
                var equipmentGenerator = scene.GetComponent<EquipmentGeneratorComponent>();
                if (equipmentGenerator == null)
                {
                    equipmentGenerator = scene.AddComponent<EquipmentGeneratorComponent>();
                }
                
                // 统计各品质出现次数
                var qualityCount = new int[16]; // 0-15品质
                var slotTypeCount = new int[9]; // 0-8装备槽位
                
                for (int i = 0; i < testCount; i++)
                {
                    // 随机选择装备槽位进行测试
                    int randomSlotType = equipmentGenerator.SelectRandomSlotType();
                    var equipment = equipmentGenerator.GenerateEquipment(cauldronLevel, randomSlotType);
                    qualityCount[equipment.Quality]++;
                    slotTypeCount[equipment.SlotType]++;
                }
                
                Log.Info($"鼎炉等级{cauldronLevel}的概率分布结果:");
                for (int i = 1; i <= 15; i++)
                {
                    if (qualityCount[i] > 0)
                    {
                        var qualityConfig = EquipQualityConfigCategory.Instance.Get(i);
                        double percentage = (double)qualityCount[i] / testCount * 100;
                        Log.Info($"  {qualityConfig.QualityName}: {qualityCount[i]}次 ({percentage:F2}%)");
                    }
                }
                
                Log.Info("装备槽位分布结果:");
                string[] slotNames = { "武器", "头盔", "护甲", "腰带", "靴子", "项链", "戒指", "护腕", "护符" };
                for (int i = 0; i < 9; i++)
                {
                    double percentage = (double)slotTypeCount[i] / testCount * 100;
                    Log.Info($"  {slotNames[i]}: {slotTypeCount[i]}次 ({percentage:F2}%)");
                }
            }
            catch (Exception e)
            {
                Log.Error($"概率分布测试失败: {e}");
            }
        }
        
        /// <summary>
        /// 测试随机装备生成（包含随机槽位）
        /// </summary>
        public static async ETTask TestRandomEquipmentGeneration(Scene scene, long testPlayerId, int testCount = 10)
        {
            Log.Info($"开始测试随机装备生成（测试{testCount}次）...");
            
            try
            {
                var equipmentGenerator = scene.GetComponent<EquipmentGeneratorComponent>();
                if (equipmentGenerator == null)
                {
                    equipmentGenerator = scene.AddComponent<EquipmentGeneratorComponent>();
                }
                
                // 创建临时缓存组件
                var equipmentTempCache = scene.GetComponent<EquipmentTempCacheComponent>();
                if (equipmentTempCache == null)
                {
                    equipmentTempCache = scene.AddComponent<EquipmentTempCacheComponent>();
                }
                
                for (int i = 0; i < testCount; i++)
                {
                    Log.Info($"第{i+1}次生成:");
                    
                    bool success = await equipmentGenerator.GenerateEquipmentForPlayer(testPlayerId);
                    if (success)
                    {
                        var equipment = equipmentTempCache.GetTempEquipment(testPlayerId);
                        var slotIndex = equipmentTempCache.GetTempSlotIndex(testPlayerId);
                        
                        if (equipment != null)
                        {
                            var qualityConfig = EquipQualityConfigCategory.Instance.Get(equipment.Quality);
                            Log.Info($"  生成成功: {equipment.Name}, 槽位:{slotIndex}, 品质:{qualityConfig.QualityName}, 颜色:{equipment.Color}, 攻击:{equipment.Attack}, 防御:{equipment.Defense}, 血量:{equipment.Health}");
                        }
                        
                        // 清除缓存，准备下一次生成
                        equipmentTempCache.ClearTempEquipment(testPlayerId);
                    }
                    else
                    {
                        Log.Error($"  生成失败");
                    }
                }
                
                Log.Info("随机装备生成测试完成！");
            }
            catch (Exception e)
            {
                Log.Error($"随机装备生成测试失败: {e}");
            }
        }
        
        /// <summary>
        /// 客户端测试现有的C2G_GenerateEquipment消息
        /// 注意：这个方法展示如何使用现有的装备生成消息
        /// </summary>
        public static void ClientUsageExample()
        {
            Log.Info("客户端使用现有C2G_GenerateEquipment消息示例:");
            Log.Info("var request = C2G_GenerateEquipment.Create();");
            Log.Info("var response = await session.Call(request) as G2C_GenerateEquipment;");
            Log.Info("// 现在服务端会根据玩家鼎炉等级生成随机槽位和品质的装备");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
    [EntitySystemOf(typeof(EquipmentGeneratorComponent))]
    [FriendOf(typeof(EquipmentGeneratorComponent))]
    public static partial class EquipmentGeneratorComponentSystem
    {
        [EntitySystem]
        private static void Awake(this EquipmentGeneratorComponent self)
        {
            self.Random = new Random();
        }

        /// <summary>
        /// 根据鼎炉等级生成装备
        /// </summary>
        /// <param name="self"></param>
        /// <param name="cauldronLevel">鼎炉等级</param>
        /// <param name="slotType">装备槽位类型 (0-8)</param>
        /// <param name="playerLevel">玩家等级</param>
        /// <returns>生成的装备</returns>
        public static Equipment GenerateEquipment(this EquipmentGeneratorComponent self, int cauldronLevel, int slotType, int playerLevel = 1)
        {
            // 获取鼎炉等级配置
            var furnaceLevelConfigCategory = FurnaceLevelConfigCategory.Instance;
            if (!furnaceLevelConfigCategory.Contain(cauldronLevel))
            {
                Log.Error($"鼎炉等级配置不存在: {cauldronLevel}");
                cauldronLevel = 1; // 默认使用1级
            }
            
            var furnaceConfig = furnaceLevelConfigCategory.Get(cauldronLevel);
            
            // 根据概率选择品质
            int qualityId = self.SelectQualityByProbability(furnaceConfig);
            
            // 获取品质配置
            var qualityConfig = EquipQualityConfigCategory.Instance.Get(qualityId);
            
            // 生成装备等级：玩家等级上下浮动5，最小为1
            int equipmentLevel = self.GenerateEquipmentLevel(playerLevel);
            
            // 生成装备
            var equipment = new Equipment
            {
                Id = IdGenerater.Instance.GenerateId(),
                Name = self.GenerateEquipmentName(qualityConfig, slotType),
                SlotType = slotType,
                Quality = qualityId,
                Level = equipmentLevel,
                Icon = $"equipment_icon_{slotType}_{qualityConfig.Quality.ToLower()}",
                Description = $"品质: {qualityConfig.QualityName}",
                Color = qualityConfig.Color // 设置装备品质颜色
            };
            
            // 生成装备属性
            self.GenerateEquipmentAttributes(equipment, qualityConfig);
            
            Log.Info($"生成装备: Id={equipment.Id}, Name={equipment.Name}, Quality={qualityConfig.QualityName}, Color={equipment.Color}, Attack={equipment.Attack}, Defense={equipment.Defense}, Health={equipment.Health}");
            
            return equipment;
        }

        /// <summary>
        /// 根据概率选择装备品质
        /// </summary>
        private static int SelectQualityByProbability(this EquipmentGeneratorComponent self, FurnaceLevelConfig furnaceConfig)
        {
            // 构建概率表
            var probabilities = new List<(int QualityId, double Probability)>
            {
                (1, furnaceConfig.Common),
                (2, furnaceConfig.Basic),
                (3, furnaceConfig.Fine),
                (4, furnaceConfig.Superior),
                (5, furnaceConfig.Exquisite),
                (6, furnaceConfig.Rare),
                (7, furnaceConfig.Epic),
                (8, furnaceConfig.Legendary),
                (9, furnaceConfig.Ancient),
                (10, furnaceConfig.Mythic),
                (11, furnaceConfig.Celestial),
                (12, furnaceConfig.Holy),
                (13, furnaceConfig.Primordial),
                (14, furnaceConfig.Chaotic),
                (15, furnaceConfig.Eternal)
            };
            
            // 过滤掉概率为0的品质
            probabilities = probabilities.Where(p => p.Probability > 0).ToList();
            
            // 计算总概率
            double totalProbability = probabilities.Sum(p => p.Probability);
            
            // 生成随机数
            double randomValue = self.Random.NextDouble() * totalProbability;
            
            // 选择品质
            double accumulated = 0;
            foreach (var (qualityId, probability) in probabilities)
            {
                accumulated += probability;
                if (randomValue <= accumulated)
                {
                    return qualityId;
                }
            }
            
            // 默认返回第一个可用的品质
            return probabilities.First().QualityId;
        }

        /// <summary>
        /// 生成装备名称
        /// </summary>
        private static string GenerateEquipmentName(this EquipmentGeneratorComponent self, EquipQualityConfig qualityConfig, int slotType)
        {
            string[] equipmentNames = { "武器", "头盔", "护甲", "腰带", "靴子", "项链", "戒指", "护腕", "护符" };
            
            if (slotType < 0 || slotType >= equipmentNames.Length)
            {
                slotType = 0; // 默认武器
            }
            
            return $"{qualityConfig.QualityName}{equipmentNames[slotType]}";
        }

        /// <summary>
        /// 生成装备属性
        /// </summary>
        private static void GenerateEquipmentAttributes(this EquipmentGeneratorComponent self, Equipment equipment, EquipQualityConfig qualityConfig)
        {
            int baseAttr = qualityConfig.BaseAttr;
            double difference = qualityConfig.Difference;
            
            // 计算浮动范围
            int minAttr = (int)(baseAttr * (1 - difference));
            int maxAttr = (int)(baseAttr * (1 + difference));
            
            // 根据装备槽位类型分配属性
            switch (equipment.SlotType)
            {
                case 0: // 武器 - 主要攻击力
                    equipment.Attack = self.Random.Next(minAttr, maxAttr + 1);
                    equipment.Defense = self.Random.Next(minAttr / 4, maxAttr / 4 + 1);
                    equipment.Health = self.Random.Next(minAttr / 2, maxAttr / 2 + 1);
                    break;
                    
                case 1: // 头盔 - 主要防御力
                case 2: // 护甲 - 主要防御力
                    equipment.Defense = self.Random.Next(minAttr, maxAttr + 1);
                    equipment.Attack = self.Random.Next(minAttr / 4, maxAttr / 4 + 1);
                    equipment.Health = self.Random.Next(minAttr / 2, maxAttr / 2 + 1);
                    break;
                    
                case 3: // 腰带 - 平衡属性
                case 4: // 靴子 - 平衡属性
                    equipment.Attack = self.Random.Next(minAttr / 2, maxAttr / 2 + 1);
                    equipment.Defense = self.Random.Next(minAttr / 2, maxAttr / 2 + 1);
                    equipment.Health = self.Random.Next(minAttr, maxAttr + 1);
                    break;
                    
                case 5: // 项链 - 主要血量
                case 6: // 戒指 - 主要血量
                case 7: // 护腕 - 主要血量
                case 8: // 护符 - 主要血量
                    equipment.Health = self.Random.Next(minAttr, maxAttr + 1);
                    equipment.Attack = self.Random.Next(minAttr / 3, maxAttr / 3 + 1);
                    equipment.Defense = self.Random.Next(minAttr / 3, maxAttr / 3 + 1);
                    break;
                    
                default:
                    // 默认平衡分配
                    equipment.Attack = self.Random.Next(minAttr / 3, maxAttr / 3 + 1);
                    equipment.Defense = self.Random.Next(minAttr / 3, maxAttr / 3 + 1);
                    equipment.Health = self.Random.Next(minAttr / 3, maxAttr / 3 + 1);
                    break;
            }
            
            // 确保属性不为负数
            equipment.Attack = Math.Max(1, equipment.Attack);
            equipment.Defense = Math.Max(1, equipment.Defense);
            equipment.Health = Math.Max(1, equipment.Health);
        }

        /// <summary>
        /// 生成装备等级：玩家等级上下浮动5，最小为1
        /// </summary>
        private static int GenerateEquipmentLevel(this EquipmentGeneratorComponent self, int playerLevel)
        {
            // 计算浮动范围
            int minLevel = Math.Max(1, playerLevel - 5); // 最小为1
            int maxLevel = playerLevel + 5;
            
            // 生成随机等级
            return self.Random.Next(minLevel, maxLevel + 1);
        }

        /// <summary>
        /// 随机选择装备槽位类型
        /// </summary>
        public static int SelectRandomSlotType(this EquipmentGeneratorComponent self)
        {
            // 装备槽位类型：0-武器, 1-头盔, 2-护甲, 3-腰带, 4-靴子, 5-项链, 6-戒指, 7-护腕, 8-护符
            return self.Random.Next(0, 9); // 0-8 共9种装备槽位
        }

        /// <summary>
        /// 为玩家生成装备到临时缓存（随机装备槽位）
        /// </summary>
        /// <param name="self"></param>
        /// <param name="playerId">玩家ID</param>
        /// <returns>是否生成成功</returns>
        public static async ETTask<bool> GenerateEquipmentForPlayer(this EquipmentGeneratorComponent self, long playerId)
        {
            try
            {
                // 获取玩家角色信息
                var scene = self.Scene();
                var roleInfo = await GetPlayerRoleInfo(scene, playerId);
                if (roleInfo == null)
                {
                    Log.Error($"玩家角色信息不存在: {playerId}");
                    return false;
                }
                
                // 随机选择装备槽位类型
                int slotType = self.SelectRandomSlotType();
                
                // 生成装备
                var equipment = self.GenerateEquipment(roleInfo.CauldronLevel, slotType, roleInfo.Level);
                
                // 缓存到临时缓存组件
                var equipmentTempCache = scene.GetComponent<EquipmentTempCacheComponent>();
                if (equipmentTempCache == null)
                {
                    equipmentTempCache = scene.AddComponent<EquipmentTempCacheComponent>();
                }
                
                equipmentTempCache.CacheEquipment(playerId, equipment, slotType);
                
                Log.Info($"为玩家生成装备: PlayerId={playerId}, CauldronLevel={roleInfo.CauldronLevel}, SlotType={slotType}, Equipment={equipment.Name}");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"生成装备失败: PlayerId={playerId}, Error={e}");
                return false;
            }
        }

        /// <summary>
        /// 从数据库获取玩家RoleInfo
        /// </summary>
        private static async ETTask<RoleInfo> GetPlayerRoleInfo(Scene scene, long playerId)
        {
            try
            {
                DBManagerComponent dbManagerComponent = scene.GetComponent<DBManagerComponent>();
                if (dbManagerComponent == null)
                {
                    Log.Error("GetPlayerRoleInfo: DBManagerComponent not found");
                    return null;
                }
                
                DBComponent dbComponent = dbManagerComponent.GetZoneDB(scene.Zone());
                string tableName = $"ET.Server.User.RoleInfo";
                
                var roleInfoRecords = await dbComponent.Query<RoleInfo>(
                    roleInfo => roleInfo.PlayerId == playerId, 
                    tableName);
                
                return roleInfoRecords.Count > 0 ? roleInfoRecords[0] : null;
            }
            catch (Exception e)
            {
                Log.Error($"GetPlayerRoleInfo failed: PlayerId={playerId}, Error={e}");
                return null;
            }
        }
    }
}
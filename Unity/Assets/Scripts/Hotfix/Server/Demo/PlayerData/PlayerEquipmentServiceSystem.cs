using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(PlayerEquipmentService))]
    [FriendOf(typeof(PlayerEquipmentService))]
    public static partial class PlayerEquipmentServiceSystem
    {
        [EntitySystem]
        private static void Awake(this PlayerEquipmentService self)
        {
            self.PlayerEquipmentSlots.Clear();
        }
        
        public static bool IsPlayerExist(this PlayerEquipmentService self, long playerId)
        {
            return self.PlayerEquipmentSlots.ContainsKey(playerId);
        }
        
        public static void EnsurePlayerData(this PlayerEquipmentService self, long playerId)
        {
            if (!self.PlayerEquipmentSlots.ContainsKey(playerId))
            {
                self.PlayerEquipmentSlots[playerId] = new Dictionary<int, Equipment>();
                
                // 初始化9个装备槽
                for (int i = 0; i < 9; i++)
                {
                    self.PlayerEquipmentSlots[playerId][i] = null;
                }
            }
        }
        
        public static Dictionary<int, Equipment> GetPlayerEquipments(this PlayerEquipmentService self, long playerId)
        {
            self.EnsurePlayerData(playerId);
            return self.PlayerEquipmentSlots[playerId];
        }
        
        // 直接创建新装备的方法，不再需要从背包中查找
        public static Equipment CreateEquipment(long equipmentId, string name, int slotType, int attack, int defense, int health, int quality, int level, string icon, string description)
        {
            return new Equipment
            {
                Id = equipmentId,
                Name = name,
                SlotType = slotType,
                Attack = attack,
                Defense = defense,
                Health = health,
                Quality = quality,
                Level = level,
                Icon = icon,
                Description = description
            };
        }
        
        public static Equipment GetEquippedEquipment(this PlayerEquipmentService self, long playerId, int slotIndex)
        {
            self.EnsurePlayerData(playerId);
            
            if (slotIndex >= 0 && slotIndex < 9)
            {
                self.PlayerEquipmentSlots[playerId].TryGetValue(slotIndex, out Equipment equipment);
                return equipment;
            }
            
            return null;
        }
        
        public static Equipment SetEquippedEquipment(this PlayerEquipmentService self, long playerId, int slotIndex, Equipment equipment)
        {
            self.EnsurePlayerData(playerId);
            
            if (slotIndex >= 0 && slotIndex < 9)
            {
                // 获取旧装备（用于替换时的奖励计算）
                Equipment oldEquipment = self.PlayerEquipmentSlots[playerId][slotIndex];
                
                // 如果新装备不为空，标记为已穿戴
                if (equipment != null)
                {
                    equipment.HasBeenEquipped = true;
                }
                
                // 替换装备
                self.PlayerEquipmentSlots[playerId][slotIndex] = equipment;
                
                return oldEquipment; // 返回旧装备
            }
            
            return null;
        }
        
        /// <summary>
        /// 出售装备获得灵石和经验值
        /// </summary>
        /// <param name="equipment">要出售的装备</param>
        /// <param name="playerId">玩家ID</param>
        /// <param name="scene">场景</param>
        /// <returns>是否成功出售和角色信息</returns>
        public static async ETTask<(bool success, long spiritStone, long exp, bool levelChanged, int newLevel, int newMajorRealm, int newMinorRealm, long newCurrentExp, long newSpiritStone)> SellEquipment(Equipment equipment, long playerId, Scene scene)
        {
            if (equipment == null)
            {
                return (false, 0, 0, false, 0, 0, 0, 0, 0);
            }
            
            // 计算获得的灵石和经验值（所有装备都可以获得奖励）
            long spiritStoneGained = EquipmentValueCalculator.CalculateSpiritStone(equipment);
            long expGained = EquipmentValueCalculator.CalculateExperience(equipment);
            
            try
            {
                // 获取角色信息
                RoleInfo roleInfo = await scene.GetComponent<DBManagerComponent>()
                    .GetZoneDB(scene.Zone())
                    .Query<RoleInfo>(playerId, "ET.Server.User.RoleInfo");
                
                if (roleInfo == null)
                {
                    Log.Error($"未找到玩家角色信息: PlayerId={playerId}");
                    return (false, 0, 0, false, 0, 0, 0, 0, 0);
                }
                
                // 增加灵石和经验值
                roleInfo.AddSpiritStone(spiritStoneGained);
                bool levelChanged = roleInfo.AddExp(expGained);
                
                Log.Info($"[服务端调试] 装备出售后状态: PlayerId={playerId}, levelChanged={levelChanged}, Level={roleInfo.Level}, MajorRealm={roleInfo.MajorRealm}, MinorRealm={roleInfo.MinorRealm}, CurrentExp={roleInfo.CurrentExp}, SpiritStone={roleInfo.SpiritStone}");
                
                // 保存到数据库
                await scene.GetComponent<DBManagerComponent>()
                    .GetZoneDB(scene.Zone())
                    .Save(roleInfo, "ET.Server.User.RoleInfo");
                
                Log.Info($"装备出售成功: PlayerId={playerId}, 装备={equipment.Name}, 灵石+{spiritStoneGained}, 经验+{expGained}");
                return (true, spiritStoneGained, expGained, levelChanged, roleInfo.Level, roleInfo.MajorRealm, roleInfo.MinorRealm, roleInfo.CurrentExp, roleInfo.SpiritStone);
            }
            catch (System.Exception e)
            {
                Log.Error($"装备出售失败: {e.Message}");
                return (false, 0, 0, false, 0, 0, 0, 0, 0);
            }
        }
        
    }
}
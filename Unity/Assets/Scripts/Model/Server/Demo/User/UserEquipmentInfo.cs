namespace ET.Server
{
    /// <summary>
    /// 用户装备信息，存储到数据库中
    /// 表名：ET.Server.User.Equipment.info
    /// 主键：playerId + slotIndex
    /// </summary>
    [ChildOf(typeof(Scene))]
    public sealed class UserEquipmentInfo : Entity, IAwake<long, int>
    {
        /// <summary>
        /// 玩家唯一ID
        /// </summary>
        public long PlayerId { get; set; }
        
        /// <summary>
        /// 装备槽位索引 (0-8)
        /// </summary>
        public int SlotIndex { get; set; }
        
        /// <summary>
        /// 装备ID
        /// </summary>
        public long EquipmentId { get; set; }
        
        /// <summary>
        /// 装备名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 装备类型/槽位类型
        /// </summary>
        public int SlotType { get; set; }
        
        /// <summary>
        /// 攻击力
        /// </summary>
        public int Attack { get; set; }
        
        /// <summary>
        /// 防御力
        /// </summary>
        public int Defense { get; set; }
        
        /// <summary>
        /// 生命值
        /// </summary>
        public int Health { get; set; }
        
        /// <summary>
        /// 品质 (0:白 1:绿 2:蓝 3:紫 4:橙)
        /// </summary>
        public int Quality { get; set; }
        
        /// <summary>
        /// 装备等级
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }
        
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 装备时间
        /// </summary>
        public long EquipTime { get; set; }
    }
    
    /// <summary>
    /// UserEquipmentInfo的Awake系统
    /// </summary>
    [EntitySystemOf(typeof(UserEquipmentInfo))]
    [FriendOf(typeof(UserEquipmentInfo))]
    public static partial class UserEquipmentInfoSystem
    {
        [EntitySystem]
        private static void Awake(this UserEquipmentInfo self, long playerId, int slotIndex)
        {
            self.PlayerId = playerId;
            self.SlotIndex = slotIndex;
            self.EquipTime = TimeInfo.Instance.ServerNow();
        }
        
        /// <summary>
        /// 从Equipment对象设置装备信息
        /// </summary>
        public static void SetFromEquipment(this UserEquipmentInfo self, Equipment equipment)
        {
            self.EquipmentId = equipment.Id;
            self.Name = equipment.Name;
            self.SlotType = equipment.SlotType;
            self.Attack = equipment.Attack;
            self.Defense = equipment.Defense;
            self.Health = equipment.Health;
            self.Quality = equipment.Quality;
            self.Level = equipment.Level;
            self.Icon = equipment.Icon;
            self.Description = equipment.Description;
            self.EquipTime = TimeInfo.Instance.ServerNow();
        }
        
        /// <summary>
        /// 转换为Equipment对象
        /// </summary>
        public static Equipment ToEquipment(this UserEquipmentInfo self)
        {
            return new Equipment
            {
                Id = self.EquipmentId,
                Name = self.Name,
                SlotType = self.SlotType,
                Attack = self.Attack,
                Defense = self.Defense,
                Health = self.Health,
                Quality = self.Quality,
                Level = self.Level,
                Icon = self.Icon,
                Description = self.Description
            };
        }
    }
}
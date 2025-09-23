namespace ET.Server
{
    /// <summary>
    /// 用户角色信息，存储到数据库中
    /// </summary>
    [ChildOf(typeof(Scene))]
    public sealed class UserInfo : Entity, IAwake<long, int>
    {
        /// <summary>
        /// 玩家唯一ID
        /// </summary>
        public long PlayerID { get; set; }
        
        /// <summary>
        /// 用户基础配置ID，对应UserBaseConfig表
        /// </summary>
        public int UserBaseConfigId { get; set; }
        
        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 角色等级
        /// </summary>
        public int Level { get; set; } = 1;
        
        /// <summary>
        /// 当前气血
        /// </summary>
        public int CurrentHealth { get; set; }
        
        /// <summary>
        /// 最大气血（基础值+装备加成等）
        /// </summary>
        public int MaxHealth { get; set; }
        
        /// <summary>
        /// 攻击力（基础值+装备加成等）
        /// </summary>
        public int Attack { get; set; }
        
        /// <summary>
        /// 防御力（基础值+装备加成等）
        /// </summary>
        public int Defense { get; set; }
        
        /// <summary>
        /// 连击率%
        /// </summary>
        public int ComboRate { get; set; }
        
        /// <summary>
        /// 反击率%
        /// </summary>
        public int CounterRate { get; set; }
        
        /// <summary>
        /// 暴击率%
        /// </summary>
        public int CriticalRate { get; set; }
        
        /// <summary>
        /// 吸血率%
        /// </summary>
        public int LifeStealRate { get; set; }
        
        /// <summary>
        /// 击晕率%
        /// </summary>
        public int StunRate { get; set; }
        
        /// <summary>
        /// 抗连击率%
        /// </summary>
        public int AntiComboRate { get; set; }
        
        /// <summary>
        /// 抗反击率%
        /// </summary>
        public int AntiCounterRate { get; set; }
        
        /// <summary>
        /// 抗暴击率%
        /// </summary>
        public int AntiCriticalRate { get; set; }
        
        /// <summary>
        /// 抗吸血率%
        /// </summary>
        public int AntiLifeStealRate { get; set; }
        
        /// <summary>
        /// 抗击晕率%
        /// </summary>
        public int AntiStunRate { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public long CreateTime { get; set; }
        
        /// <summary>
        /// 最后登录时间
        /// </summary>
        public long LastLoginTime { get; set; }
    }
    
    /// <summary>
    /// UserInfo的Awake系统
    /// </summary>
    [EntitySystemOf(typeof(UserInfo))]
    [FriendOf(typeof(UserInfo))]
    public static partial class UserInfoSystem
    {
        [EntitySystem]
        private static void Awake(this UserInfo self, long playerId, int userBaseConfigId)
        {
            self.PlayerID = playerId;
            self.UserBaseConfigId = userBaseConfigId;
            
            // 从配置表获取基础属性
            UserBaseConfig config = UserBaseConfigCategory.Instance.Get(userBaseConfigId);
            self.MaxHealth = config.Health;
            self.CurrentHealth = config.Health; // 初始时满血
            self.Attack = config.Attack;
            self.Defense = config.Defense;
            self.ComboRate = config.ComboRate;
            self.CounterRate = config.CounterRate;
            self.CriticalRate = config.CriticalRate;
            self.LifeStealRate = config.LifeStealRate;
            self.StunRate = config.StunRate;
            self.AntiComboRate = config.AntiComboRate;
            self.AntiCounterRate = config.AntiCounterRate;
            self.AntiCriticalRate = config.AntiCriticalRate;
            self.AntiLifeStealRate = config.AntiLifeStealRate;
            self.AntiStunRate = config.AntiStunRate;
            
            self.CreateTime = TimeInfo.Instance.ServerNow();
            self.LastLoginTime = TimeInfo.Instance.ServerNow();
            
            Log.Info($"创建角色: PlayerID={playerId}, ConfigId={userBaseConfigId}, Health={self.MaxHealth}, Attack={self.Attack}");
        }
        
        /// <summary>
        /// 更新最后登录时间
        /// </summary>
        public static void UpdateLastLoginTime(this UserInfo self)
        {
            self.LastLoginTime = TimeInfo.Instance.ServerNow();
        }
        
        /// <summary>
        /// 设置角色名称
        /// </summary>
        public static void SetName(this UserInfo self, string name)
        {
            self.Name = name;
        }
        
        /// <summary>
        /// 获取角色基础信息
        /// </summary>
        public static (int health, int attack, int defense) GetBaseAttributes(this UserInfo self)
        {
            return (self.MaxHealth, self.Attack, self.Defense);
        }
    }
}
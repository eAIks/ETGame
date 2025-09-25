namespace ET.Server
{
    /// <summary>
    /// 角色信息，存储到ET.Server.User.RoleInfo表中
    /// 主键为PlayerID
    /// </summary>
    [ChildOf(typeof(Scene))]
    public sealed class RoleInfo : Entity, IAwake<long>
    {
        /// <summary>
        /// 玩家唯一ID，作为主键
        /// </summary>
        public long PlayerId { get; set; }

        /// <summary>
        /// 角色昵称，默认值为"凡人+PlayerId"
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 大境界等级，初始值为1
        /// </summary>
        public int MajorRealm { get; set; } = 1;

        /// <summary>
        /// 小境界等级，初始值为1
        /// </summary>
        public int MinorRealm { get; set; } = 1;

        /// <summary>
        /// 角色等级，初始值为0
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// 鼎炉等级，初始值为0
        /// </summary>
        public int CauldronLevel { get; set; } = 1;

        /// <summary>
        /// 当前经验值，初始值为0
        /// </summary>
        public long CurrentExp { get; set; } = 0;

        /// <summary>
        /// 灵石数量，初始值为0
        /// </summary>
        public long SpiritStone { get; set; } = 0;

        /// <summary>
        /// 角色创建时间
        /// </summary>
        public long CreateTime { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public long LastUpdateTime { get; set; }
    }

    /// <summary>
    /// RoleInfo的Awake系统
    /// </summary>
    [EntitySystemOf(typeof(RoleInfo))]
    [FriendOf(typeof(RoleInfo))]
    public static partial class RoleInfoSystem
    {
        [EntitySystem]
        private static void Awake(this RoleInfo self, long playerId)
        {
            self.PlayerId = playerId;
            self.NickName = $"凡人{playerId}"; // 默认昵称

            // 设置时间戳
            long currentTime = TimeInfo.Instance.ServerNow();
            self.CreateTime = currentTime;
            self.LastUpdateTime = currentTime;

            Log.Info($"创建角色信息: PlayerId={playerId}, 昵称={self.NickName}");
        }

        /// <summary>
        /// 更新最后修改时间
        /// </summary>
        public static void UpdateLastTime(this RoleInfo self)
        {
            self.LastUpdateTime = TimeInfo.Instance.ServerNow();
        }

        /// <summary>
        /// 设置角色昵称
        /// </summary>
        public static void SetNickName(this RoleInfo self, string nickName)
        {
            self.NickName = nickName;
            self.UpdateLastTime();
        }

        /// <summary>
        /// 增加经验值并自动升级
        /// </summary>
        public static bool AddExp(this RoleInfo self, long exp)
        {
            if (exp <= 0) return false;

            long oldExp = self.CurrentExp;
            int oldLevel = self.Level;
            int oldMajorRealm = self.MajorRealm;
            int oldMinorRealm = self.MinorRealm;

            self.CurrentExp += exp;

            // 检查是否需要升级
            bool levelChanged = self.CheckAndUpgradeLevel();

            self.UpdateLastTime();

            if (levelChanged)
            {
                Log.Info($"角色获得经验并升级: PlayerId={self.PlayerId}, 经验+{exp} ({oldExp}->{self.CurrentExp}), " +
                    $"等级{oldLevel}->{self.Level}, 大境界{oldMajorRealm}->{self.MajorRealm}，小境界 {oldMinorRealm}->{self.MinorRealm}");
            }
            else
            {
                Log.Info($"角色获得经验: PlayerId={self.PlayerId}, 经验+{exp} ({oldExp}->{self.CurrentExp})");
            }

            return levelChanged;
        }

        /// <summary>
        /// 检查并升级等级和境界
        /// </summary>
        private static bool CheckAndUpgradeLevel(this RoleInfo self)
        {
            bool changed = false;

            // 持续检查升级直到经验不足
            while (true)
            {
                // 根据当前等级查找对应的境界配置
                RealmLevelConfig currentConfig = self.GetCurrentRealmConfig();
                if (currentConfig == null) break;

                // 检查是否达到升级所需经验
                long expForNextLevel = currentConfig.ExpPerLevel;
                if (self.CurrentExp < expForNextLevel) break;

                // 消耗经验并升级
                self.CurrentExp -= expForNextLevel;
                self.Level++;
                changed = true;

                // 检查是否需要突破境界
                self.CheckAndUpgradeRealm();
            }

            return changed;
        }

        /// <summary>
        /// 检查并升级境界
        /// </summary>
        private static void CheckAndUpgradeRealm(this RoleInfo self)
        {
            // 查找当前等级对应的境界配置
            var allConfigs = RealmLevelConfigCategory.Instance.GetAll();
            foreach (var config in allConfigs.Values)
            {
                if (self.Level >= config.LevelMin && self.Level <= config.LevelMax)
                {
                    // 如果境界发生变化，更新境界
                    if (self.MajorRealm != config.StageId || self.MinorRealm != config.SubStage)
                    {
                        int oldMajorRealm = self.MajorRealm;
                        int oldMinorRealm = self.MinorRealm;

                        self.MajorRealm = config.StageId;
                        self.MinorRealm = config.SubStage;

                        Log.Info($"角色境界突破: PlayerId={self.PlayerId}, " +
                            $"从 {RealmLevelConfigCategory.Instance.GetRealmName(oldMajorRealm, oldMinorRealm)} " +
                            $"提升到 {config.StageName} {config.SubStageName}");
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// 获取当前等级对应的境界配置
        /// </summary>
        private static RealmLevelConfig GetCurrentRealmConfig(this RoleInfo self)
        {
            var allConfigs = RealmLevelConfigCategory.Instance.GetAll();
            foreach (var config in allConfigs.Values)
            {
                if (self.Level >= config.LevelMin && self.Level <= config.LevelMax)
                {
                    return config;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取升到下一级所需的总经验值（用于UI显示 当前经验/总经验）
        /// </summary>
        public static long GetExpForNextLevel(this RoleInfo self)
        {
            RealmLevelConfig currentConfig = self.GetCurrentRealmConfig();
            if (currentConfig == null)
            {
                return 100; // 默认值
            }

            // 返回配置中的总升级经验值，用于UI显示格式：当前经验/总升级经验
            return currentConfig.ExpPerLevel;
        }
        
        /// <summary>
        /// 获取还需要多少经验才能升级
        /// </summary>
        public static long GetExpNeededForUpgrade(this RoleInfo self)
        {
            RealmLevelConfig currentConfig = self.GetCurrentRealmConfig();
            if (currentConfig == null)
            {
                return 100; // 默认值
            }

            long expNeeded = currentConfig.ExpPerLevel - self.CurrentExp;
            return expNeeded > 0 ? expNeeded : 0;
        }

        /// <summary>
        /// 创建RealmInfoProto对象
        /// </summary>
        public static RealmInfoProto ToRealmInfoProto(this RoleInfo self)
        {
            RealmInfoProto realmInfo = RealmInfoProto.Create();
            realmInfo.MajorRealm = self.MajorRealm;
            realmInfo.MinorRealm = self.MinorRealm;
            realmInfo.Level = self.Level;
            realmInfo.CurrentExp = self.CurrentExp;
            realmInfo.SpiritStone = self.SpiritStone;
            realmInfo.ExpForNextLevel = self.GetExpForNextLevel();

            // 获取境界名称
            var currentConfig = self.GetCurrentRealmConfig();
            if (currentConfig != null)
            {
                realmInfo.MajorRealmName = currentConfig.StageName;
                realmInfo.MinorRealmName = currentConfig.SubStageName;
            }
            else
            {
                realmInfo.MajorRealmName = "未知境界";
                realmInfo.MinorRealmName = "未知";
            }

            return realmInfo;
        }

        /// <summary>
        /// 增加灵石
        /// </summary>
        public static void AddSpiritStone(this RoleInfo self, long amount)
        {
            self.SpiritStone += amount;
            self.UpdateLastTime();
        }

        /// <summary>
        /// 设置境界等级
        /// </summary>
        public static void SetRealm(this RoleInfo self, int majorRealm, int minorRealm)
        {
            self.MajorRealm = majorRealm;
            self.MinorRealm = minorRealm;
            self.UpdateLastTime();
        }

        /// <summary>
        /// 升级
        /// </summary>
        public static void LevelUp(this RoleInfo self)
        {
            self.Level++;
            self.UpdateLastTime();
            Log.Info($"角色升级: PlayerId={self.PlayerId}, 新等级={self.Level}");
        }

        /// <summary>
        /// 升级鼎炉
        /// </summary>
        public static void UpgradeCauldron(this RoleInfo self)
        {
            self.CauldronLevel++;
            self.UpdateLastTime();
            Log.Info($"鼎炉升级: PlayerId={self.PlayerId}, 新等级={self.CauldronLevel}");
        }
    }
}
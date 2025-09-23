namespace ET.Server
{
    [EntitySystemOf(typeof(PlayerData))]
    [FriendOf(typeof(PlayerData))]
    public static partial class PlayerDataSystem
    {
        [EntitySystem]
        private static void Awake(this PlayerData self, long playerId, string account)
        {
            self.PlayerId = playerId;
            self.Account = account;
            self.CreateTime = TimeInfo.Instance.ServerNow();
            self.LastLoginTime = TimeInfo.Instance.ServerNow();
        }

        public static void InitFromConfig(this PlayerData self, UserBaseConfig config, string name, int serverId)
        {
            self.Name = name;
            self.Health = config.Health;
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
            self.ServerId = serverId;
        }

        public static void UpdateLastLoginTime(this PlayerData self)
        {
            self.LastLoginTime = TimeInfo.Instance.ServerNow();
        }
    }
}
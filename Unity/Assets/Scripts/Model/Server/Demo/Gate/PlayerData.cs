namespace ET.Server
{
    [ChildOf(typeof(Scene))]
    public sealed class PlayerData : Entity, IAwake<long, string>
    {
        public long UserId { get; set; }
        public string Account { get; set; }
        public string Name { get; set; }
        public int Health { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int ComboRate { get; set; }
        public int CounterRate { get; set; }
        public int CriticalRate { get; set; }
        public int LifeStealRate { get; set; }
        public int StunRate { get; set; }
        public int AntiComboRate { get; set; }
        public int AntiCounterRate { get; set; }
        public int AntiCriticalRate { get; set; }
        public int AntiLifeStealRate { get; set; }
        public int AntiStunRate { get; set; }
        public long CreateTime { get; set; }
        public long LastLoginTime { get; set; }
        public int ServerId { get; set; }
    }
}
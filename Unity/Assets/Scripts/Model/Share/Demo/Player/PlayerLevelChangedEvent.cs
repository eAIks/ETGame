namespace ET
{
    public struct PlayerLevelChangedEvent
    {
        public int NewLevel;
        public int NewMajorRealm;
        public int NewMinorRealm;
        public long NewCurrentExp;
        public long NewSpiritStone;
        public long SpiritStoneGained;
        public long ExpGained;
    }
}
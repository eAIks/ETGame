namespace ET.Server
{
    [FriendOf(typeof(GateSessionKeyComponent))]
    public static partial class GateSessionKeyComponentSystem
    {
        public static void Add(this GateSessionKeyComponent self, long key, string account)
        {
            self.sessionKey.Add(key, account);
            self.TimeoutRemoveKey(key).Coroutine();
        }

        public static void Add(this GateSessionKeyComponent self, long key, string account, long playerID, string accountUUID, int serverId)
        {
            self.sessionKey.Add(key, account);
            self.sessionPlayerID.Add(key, playerID);
            self.sessionServerId.Add(key, serverId);
            if (!string.IsNullOrEmpty(accountUUID))
            {
                self.sessionAccountUUID.Add(key, accountUUID);
            }
            self.TimeoutRemoveKey(key).Coroutine();
        }

        public static string Get(this GateSessionKeyComponent self, long key)
        {
            string account = null;
            self.sessionKey.TryGetValue(key, out account);
            return account;
        }

        public static long GetPlayerID(this GateSessionKeyComponent self, long key)
        {
            self.sessionPlayerID.TryGetValue(key, out long playerID);
            return playerID;
        }

        public static string GetAccountUUID(this GateSessionKeyComponent self, long key)
        {
            self.sessionAccountUUID.TryGetValue(key, out string accountUUID);
            return accountUUID;
        }

        public static int GetServerId(this GateSessionKeyComponent self, long key)
        {
            self.sessionServerId.TryGetValue(key, out int serverId);
            return serverId;
        }

        public static void Remove(this GateSessionKeyComponent self, long key)
        {
            self.sessionKey.Remove(key);
            self.sessionPlayerID.Remove(key);
            self.sessionAccountUUID.Remove(key);
            self.sessionServerId.Remove(key);
        }

        private static async ETTask TimeoutRemoveKey(this GateSessionKeyComponent self, long key)
        {
            await self.Root().GetComponent<TimerComponent>().WaitAsync(20000);
            self.sessionKey.Remove(key);
            self.sessionPlayerID.Remove(key);
            self.sessionAccountUUID.Remove(key);
            self.sessionServerId.Remove(key);
        }
    }
}
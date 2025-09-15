using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    [EntitySystemOf(typeof(RouterAddressComponent))]
    [FriendOf(typeof(RouterAddressComponent))]
    public static partial class RouterAddressComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RouterAddressComponent self, string address, int port)
        {
            self.RouterManagerHost = address;
            self.RouterManagerPort = port;
        }
        
        public static async ETTask Init(this RouterAddressComponent self)
        {
            self.RouterManagerIPAddress = NetworkHelper.GetHostAddress(self.RouterManagerHost);
            await self.GetAllRouter();
        }

        private static async ETTask GetAllRouter(this RouterAddressComponent self)
        {
            string url = $"http://{self.RouterManagerHost}:{self.RouterManagerPort}/get_router?v={RandomGenerator.RandUInt32()}";
            Log.Debug($"start get router info: {url}");
            string routerInfo = await HttpClientHelper.Get(url);
            Log.Debug($"recv router info: {routerInfo}");
            HttpGetRouterResponse httpGetRouterResponse = MongoHelper.FromJson<HttpGetRouterResponse>(routerInfo);
            self.Info = httpGetRouterResponse;
            Log.Debug($"start get router info finish: {MongoHelper.ToJson(httpGetRouterResponse)}");
            
            // 打乱顺序
            RandomGenerator.BreakRank(self.Info.Routers);
            
            self.WaitTenMinGetAllRouter().Coroutine();
        }
        
        // 等10分钟再获取一次
        public static async ETTask WaitTenMinGetAllRouter(this RouterAddressComponent self)
        {
            await self.Root().GetComponent<TimerComponent>().WaitAsync(5 * 60 * 1000);
            if (self.IsDisposed)
            {
                return;
            }
            await self.GetAllRouter();
        }

        public static IPEndPoint GetAddress(this RouterAddressComponent self)
        {
            // 防御性检查：确保Info不为空且Routers列表不为空
            if (self.Info == null)
            {
                Log.Warning("RouterAddressComponent.GetAddress: Info is null, router may not be initialized yet");
                return null;
            }
            
            if (self.Info.Routers == null || self.Info.Routers.Count == 0)
            {
                Log.Warning("RouterAddressComponent.GetAddress: Routers list is null or empty");
                return null;
            }

            string address = self.Info.Routers[self.RouterIndex++ % self.Info.Routers.Count];
            Log.Info($"get router address: {self.RouterIndex - 1} {address}");
            string[] ss = address.Split(':');
            IPAddress ipAddress = IPAddress.Parse(ss[0]);
            if (self.RouterManagerIPAddress.AddressFamily == AddressFamily.InterNetworkV6)
            { 
                ipAddress = ipAddress.MapToIPv6();
            }
            return new IPEndPoint(ipAddress, int.Parse(ss[1]));
        }
        
        public static IPEndPoint GetRealmAddress(this RouterAddressComponent self, string account)
        {
            // 防御性检查：确保Info不为空且Realms列表不为空
            if (self.Info == null)
            {
                Log.Warning("RouterAddressComponent.GetRealmAddress: Info is null, router may not be initialized yet");
                return null;
            }
            
            if (self.Info.Realms == null || self.Info.Realms.Count == 0)
            {
                Log.Warning("RouterAddressComponent.GetRealmAddress: Realms list is null or empty");
                return null;
            }
            
            int v = account.Mode(self.Info.Realms.Count);
            string address = self.Info.Realms[v];
            string[] ss = address.Split(':');
            IPAddress ipAddress = IPAddress.Parse(ss[0]);
            //if (self.IPAddress.AddressFamily == AddressFamily.InterNetworkV6)
            //{ 
            //    ipAddress = ipAddress.MapToIPv6();
            //}
            return new IPEndPoint(ipAddress, int.Parse(ss[1]));
        }
    }
}
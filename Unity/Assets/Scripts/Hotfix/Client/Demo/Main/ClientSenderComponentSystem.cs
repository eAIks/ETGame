using System.Threading.Tasks;

namespace ET.Client
{
    [EntitySystemOf(typeof(ClientSenderComponent))]
    [FriendOf(typeof(ClientSenderComponent))]
    public static partial class ClientSenderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ClientSenderComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this ClientSenderComponent self)
        {
            self.RemoveFiberAsync().Coroutine();
        }

        private static async ETTask RemoveFiberAsync(this ClientSenderComponent self)
        {
            if (self.fiberId == 0)
            {
                return;
            }

            int fiberId = self.fiberId;
            self.fiberId = 0;
            await FiberManager.Instance.Remove(fiberId);
        }

        public static async ETTask DisposeAsync(this ClientSenderComponent self)
        {
            await self.RemoveFiberAsync();
            self.Dispose();
        }

        public static async ETTask<long> LoginAsync(this ClientSenderComponent self, string account, string password)
        {
            // 如果之前有 NetClient Fiber，先清理
            if (self.fiberId != 0)
            {
                Log.Info($"清理之前的 NetClient Fiber: {self.fiberId}");
                await FiberManager.Instance.Remove(self.fiberId);
                self.fiberId = 0;
            }
            
            self.fiberId = await FiberManager.Instance.Create(SchedulerType.ThreadPool, 0, SceneType.NetClient, "");
            self.netClientActorId = new ActorId(self.Fiber().Process, self.fiberId);

            Main2NetClient_Login main2NetClientLogin = Main2NetClient_Login.Create();
            main2NetClientLogin.OwnerFiberId = self.Fiber().Id;
            main2NetClientLogin.Account = account;
            main2NetClientLogin.Password = password;
            NetClient2Main_Login response = await self.Root().GetComponent<ProcessInnerSender>().Call(self.netClientActorId, main2NetClientLogin) as NetClient2Main_Login;
            return response.PlayerId;
        }

        public static async ETTask<int> RegisterAsync(this ClientSenderComponent self, string account, string password)
        {
            // 如果之前有 NetClient Fiber，先清理
            if (self.fiberId != 0)
            {
                Log.Info($"清理之前的 NetClient Fiber: {self.fiberId}");
                await FiberManager.Instance.Remove(self.fiberId);
                self.fiberId = 0;
            }
            
            self.fiberId = await FiberManager.Instance.Create(SchedulerType.ThreadPool, 0, SceneType.NetClient, "");
            self.netClientActorId = new ActorId(self.Fiber().Process, self.fiberId);

            // 等待 NetClient Fiber 完全初始化
            await self.Root().GetComponent<TimerComponent>().WaitAsync(300);
            
            Main2NetClient_Register main2NetClientRegister = Main2NetClient_Register.Create();
            main2NetClientRegister.OwnerFiberId = self.Fiber().Id;
            main2NetClientRegister.Account = account;
            main2NetClientRegister.Password = password;
            try
            {
                IResponse rawResponse = await self.Root().GetComponent<ProcessInnerSender>().Call(self.netClientActorId, main2NetClientRegister);
                Log.Info($"注册原始响应: {rawResponse?.GetType()?.Name}, {rawResponse}");
                
                NetClient2Main_Register response = rawResponse as NetClient2Main_Register;
                if (response == null)
                {
                    Log.Error($"响应类型转换失败: 期望 NetClient2Main_Register, 实际 {rawResponse?.GetType()?.Name}");
                    return ErrorCode.ERR_PasswordError; // 返回一个通用错误码
                }
                
                Log.Info($"注册响应: Error={response.Error}, Message={response.Message}");
                return response.Error;
            }
            catch (System.Exception ex)
            {
                Log.Error($"注册调用异常: {ex}");
                throw;
            }
        }

        public static void Send(this ClientSenderComponent self, IMessage message)
        {
            A2NetClient_Message a2NetClientMessage = A2NetClient_Message.Create();
            a2NetClientMessage.MessageObject = message;
            self.Root().GetComponent<ProcessInnerSender>().Send(self.netClientActorId, a2NetClientMessage);
        }

        public static async ETTask<IResponse> Call(this ClientSenderComponent self, IRequest request, bool needException = true)
        {
            A2NetClient_Request a2NetClientRequest = A2NetClient_Request.Create();
            a2NetClientRequest.MessageObject = request;
            using A2NetClient_Response a2NetClientResponse = await self.Root().GetComponent<ProcessInnerSender>().Call(self.netClientActorId, a2NetClientRequest) as A2NetClient_Response;
            IResponse response = a2NetClientResponse.MessageObject;
                        
            if (response.Error == ErrorCore.ERR_MessageTimeout)
            {
                throw new RpcException(response.Error, $"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: {request}, response: {response}");
            }

            if (needException && ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                throw new RpcException(response.Error, $"Rpc error: {request}, response: {response}");
            }
            return response;
        }

    }
}
using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    [MessageHandler(SceneType.NetClient)]
    public class Main2NetClient_RegisterHandler: MessageHandler<Scene, Main2NetClient_Register, NetClient2Main_Register>
    {
        protected override async ETTask Run(Scene root, Main2NetClient_Register request, NetClient2Main_Register response)
        {
            Log.Info($"Main2NetClient_RegisterHandler 开始处理: Account={request.Account}");
            string account = request.Account;
            string password = request.Password;
            
            // 创建一个ETModel层的Session
            root.RemoveComponent<RouterAddressComponent>();
            // 获取路由跟realmDispatcher地址
            RouterAddressComponent routerAddressComponent =
                    root.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
            await routerAddressComponent.Init();
            root.AddComponent<NetComponent, AddressFamily, NetworkProtocol>(routerAddressComponent.RouterManagerIPAddress.AddressFamily, NetworkProtocol.UDP);
            root.GetComponent<FiberParentComponent>().ParentFiberId = request.OwnerFiberId;

            NetComponent netComponent = root.GetComponent<NetComponent>();
            
            IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);

            R2C_Register r2CRegister;
            using (Session session = await netComponent.CreateRouterSession(realmAddress, account, password))
            {
                C2R_Register c2RRegister = C2R_Register.Create();
                c2RRegister.Account = account;
                c2RRegister.Password = password;
                r2CRegister = (R2C_Register)await session.Call(c2RRegister);
            }

            response.Error = r2CRegister.Error;
            response.Message = r2CRegister.Message;
            
            Log.Info($"Main2NetClient_RegisterHandler 完成处理: Account={account}, Error={r2CRegister.Error}, Message={r2CRegister.Message}");
            
            if (r2CRegister.Error == ErrorCode.ERR_Success)
            {
                Log.Debug($"注册成功! 账号: {account}");
            }
            else
            {
                Log.Warning($"注册失败: {r2CRegister.Message}");
            }
        }
    }
}
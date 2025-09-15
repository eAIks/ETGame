using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    [MessageHandler(SceneType.NetClient)]
    public class Main2NetClient_LoginHandler: MessageHandler<Scene, Main2NetClient_Login, NetClient2Main_Login>
    {
        protected override async ETTask Run(Scene root, Main2NetClient_Login request, NetClient2Main_Login response)
        {
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
            if (realmAddress == null)
            {
                Log.Error("Main2NetClient_LoginHandler: Failed to get realm address, router may not be initialized");
                response.Error = ErrorCore.ERR_PacketParserError;
                response.Message = "无法获取Realm服务器地址";
                return;
            }

            // 创建与Realm服务器的连接，登录成功后不关闭
            Session realmSession = await netComponent.CreateRouterSession(realmAddress, account, password);
            C2R_Login c2RLogin = C2R_Login.Create();
            c2RLogin.Account = account;
            c2RLogin.Password = password;
            R2C_Login r2CLogin = (R2C_Login)await realmSession.Call(c2RLogin);

            // 登录成功后保持与Realm服务器的连接，等待选择区服
            // 不再立即连接Gate服务器，而是保存Realm连接用于后续的区服选择
            root.AddComponent<SessionComponent>().Session = realmSession;
            
            Log.Debug("登陆Realm成功，等待选择区服!");

            response.PlayerId = 0; // 暂时设置为0，真正的PlayerId在连接Gate后获得
        }
    }
}
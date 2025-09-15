namespace ET.Client
{
    [MessageHandler(SceneType.NetClient)]
    public class Main2NetClient_ConnectGateHandler: MessageHandler<Scene, Main2NetClient_ConnectGate, NetClient2Main_ConnectGate>
    {
        protected override async ETTask Run(Scene root, Main2NetClient_ConnectGate request, NetClient2Main_ConnectGate response)
        {
            try
            {
                Log.Info($"Main2NetClient_ConnectGateHandler: 开始连接Gate服务器: {request.Address}");
                
                // 获取网络组件
                NetComponent netComponent = root.GetComponent<NetComponent>();
                if (netComponent == null)
                {
                    Log.Error("Main2NetClient_ConnectGateHandler: NetComponent为空，无法连接Gate");
                    response.Error = ErrorCode.ERR_ServerSelectFailed;
                    response.Message = "网络组件未初始化";
                    return;
                }
                
                // 创建与Gate的连接
                Session gateSession = await netComponent.CreateRouterSession(NetworkHelper.ToIPEndPoint(request.Address), "", "");
                gateSession.AddComponent<ClientSessionErrorComponent>();
                
                // 向Gate发送登录请求（在替换连接之前先验证Gate连接是否正常）
                C2G_LoginGate c2GLoginGate = C2G_LoginGate.Create();
                c2GLoginGate.Key = request.Key;
                c2GLoginGate.GateId = request.GateId;
                
                G2C_LoginGate g2CLoginGate;
                try
                {
                    g2CLoginGate = (G2C_LoginGate)await gateSession.Call(c2GLoginGate);
                }
                catch (System.Exception e)
                {
                    Log.Error($"Main2NetClient_ConnectGateHandler: Gate登录调用失败: {e.Message}");
                    gateSession.Dispose(); // 清理失败的Gate连接
                    response.Error = ErrorCode.ERR_ServerSelectFailed;
                    response.Message = "连接Gate服务器失败";
                    return;
                }
                
                if (g2CLoginGate.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"Main2NetClient_ConnectGateHandler: Gate登录失败: {g2CLoginGate.Error}, {g2CLoginGate.Message}");
                    gateSession.Dispose(); // 清理失败的Gate连接
                    response.Error = g2CLoginGate.Error;
                    response.Message = g2CLoginGate.Message;
                    return;
                }
                
                Log.Info($"Main2NetClient_ConnectGateHandler: Gate登录成功，现在替换Session连接");
                
                // 只有Gate连接和登录都成功后，才替换SessionComponent中的连接
                SessionComponent sessionComponent = root.GetComponent<SessionComponent>();
                Session oldSession = null;
                
                if (sessionComponent != null)
                {
                    oldSession = sessionComponent.Session; // 保存旧连接，稍后关闭
                    sessionComponent.Session = gateSession;
                }
                else
                {
                    root.AddComponent<SessionComponent>().Session = gateSession;
                }
                
                // 在新连接设置完成后，才安全地关闭旧连接
                oldSession?.Dispose();
                
                response.PlayerId = g2CLoginGate.PlayerId;
                
                Log.Info($"Main2NetClient_ConnectGateHandler: Gate连接成功！玩家ID: {g2CLoginGate.PlayerId}");
            }
            catch (System.Exception e)
            {
                Log.Error($"Main2NetClient_ConnectGateHandler: 连接Gate失败: {e.Message}");
                response.Error = ErrorCode.ERR_ServerSelectFailed;
                response.Message = "连接Gate服务器失败";
            }
        }
    }
}
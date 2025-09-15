namespace ET.Client
{
    [MessageHandler(SceneType.NetClient)]
    public class Main2NetClient_SelectServerHandler: MessageHandler<Scene, C2R_SelectServer, R2C_SelectServer>
    {
        protected override async ETTask Run(Scene root, C2R_SelectServer request, R2C_SelectServer response)
        {
            try
            {
                Log.Info($"Main2NetClient_SelectServerHandler: 处理选择服务器请求 {request.ServerId}");
                
                // 获取保存的Realm连接
                SessionComponent sessionComponent = root.GetComponent<SessionComponent>();
                if (sessionComponent?.Session == null)
                {
                    Log.Error("Main2NetClient_SelectServerHandler: 没有可用的Realm连接");
                    response.Error = ErrorCode.ERR_ServerSelectFailed;
                    response.Message = "没有可用的Realm连接";
                    return;
                }
                
                Session realmSession = sessionComponent.Session;
                
                Log.Info($"Main2NetClient_SelectServerHandler: 通过Realm连接发送C2R_SelectServer请求");
                R2C_SelectServer realmResponse = (R2C_SelectServer)await realmSession.Call(request);
                
                if (realmResponse == null)
                {
                    Log.Error("Main2NetClient_SelectServerHandler: Realm响应为空");
                    response.Error = ErrorCode.ERR_ServerSelectFailed;
                    response.Message = "服务器无响应";
                    return;
                }
                
                // 复制响应数据
                response.Error = realmResponse.Error;
                response.Message = realmResponse.Message;
                response.ServerAddress = realmResponse.ServerAddress;
                response.Key = realmResponse.Key;
                response.GateId = realmResponse.GateId;
                
                Log.Info($"Main2NetClient_SelectServerHandler: 选择服务器完成，错误码: {response.Error}");
                if (response.Error == ErrorCode.ERR_Success)
                {
                    Log.Info($"Main2NetClient_SelectServerHandler: Gate地址: {response.ServerAddress}, Key: {response.Key}");
                }
                
            }
            catch (System.Exception e)
            {
                Log.Error($"Main2NetClient_SelectServerHandler: 异常: {e}");
                response.Error = ErrorCode.ERR_ServerSelectFailed;
                response.Message = "选择服务器失败";
            }
        }
    }
}
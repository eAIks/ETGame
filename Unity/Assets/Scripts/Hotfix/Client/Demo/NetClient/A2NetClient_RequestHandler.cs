namespace ET.Client
{
    [MessageHandler(SceneType.NetClient)]
    public class A2NetClient_RequestHandler: MessageHandler<Scene, A2NetClient_Request, A2NetClient_Response>
    {
        protected override async ETTask Run(Scene root, A2NetClient_Request request, A2NetClient_Response response)
        {
            int rpcId = request.RpcId;
            
            // 防御性检查：确保SessionComponent和Session都不为空
            SessionComponent sessionComponent = root.GetComponent<SessionComponent>();
            if (sessionComponent == null)
            {
                Log.Error("A2NetClient_RequestHandler: SessionComponent is null");
                response.Error = ErrorCore.ERR_PacketParserError;
                response.Message = "SessionComponent not found";
                return;
            }
            
            if (sessionComponent.Session == null)
            {
                Log.Error("A2NetClient_RequestHandler: Session is null, connection may have been lost");
                response.Error = ErrorCore.ERR_PacketParserError; 
                response.Message = "Session connection is null";
                return;
            }
            
            try
            {
                IResponse res = await sessionComponent.Session.Call(request.MessageObject);
                res.RpcId = rpcId;
                response.MessageObject = res;
            }
            catch (System.Exception e)
            {
                Log.Error($"A2NetClient_RequestHandler: Call failed: {e.Message}");
                response.Error = ErrorCore.ERR_RpcFail;
                response.Message = e.Message;
            }
        }
    }
}
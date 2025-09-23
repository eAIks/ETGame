using System;


namespace ET.Client
{
    public static partial class EnterMapHelper
    {
        public static async ETTask EnterMapAsync(Scene root)
        {
            try
            {
                Log.Info("EnterMapHelper: 开始发送C2G_EnterMap请求");
                G2C_EnterMap g2CEnterMap = await root.GetComponent<ClientSenderComponent>().Call(C2G_EnterMap.Create()) as G2C_EnterMap;
                Log.Info($"EnterMapHelper: 收到服务器响应, Error={g2CEnterMap?.Error}, MyId={g2CEnterMap?.MyId}");
                
                if (g2CEnterMap.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"EnterMapHelper: 服务器返回错误: {g2CEnterMap.Error}, {g2CEnterMap.Message}");
                    return;
                }
                
                Log.Info("EnterMapHelper: 角色数据处理成功，等待场景切换完成");
                
                // 等待场景切换完成
                await root.GetComponent<ObjectWait>().Wait<Wait_SceneChangeFinish>();
                
                Log.Info("EnterMapHelper: 场景切换完成");
                EventSystem.Instance.Publish(root, new EnterMapFinish());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }	
        }
        
        public static async ETTask Match(Fiber fiber)
        {
            try
            {
                G2C_Match g2CEnterMap = await fiber.Root.GetComponent<ClientSenderComponent>().Call(C2G_Match.Create()) as G2C_Match;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }	
        }
    }
}
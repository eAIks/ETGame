namespace ET.Client
{
    [Event(SceneType.Demo)]
    public class EnterMapFinish_CreateMainUI: AEvent<Scene, EnterMapFinish>
    {
        protected override async ETTask Run(Scene scene, EnterMapFinish args)
        {
            Log.Info("EnterMapFinish_CreateMainUI: 进入地图完成，开始创建主界面");
            
            try
            {
                await UIHelper.Create(scene, UIType.UIMain, UILayer.Mid);
                
                UI ui = scene.GetComponent<UIComponent>()?.Get(UIType.UIMain);
                UIMainComponent mainComponent = ui?.GetComponent<UIMainComponent>();
                
                if (mainComponent != null)
                {
                    Log.Info("EnterMapFinish_CreateMainUI: UIMainComponent创建成功，开始请求数据");
                    
                    // 请求玩家装备数据并刷新UI
                    await mainComponent.RequestPlayerEquipments();
                    
                    Log.Info("EnterMapFinish_CreateMainUI: 装备数据请求完成，开始请求境界数据");
                    
                    // 请求玩家境界数据并刷新UI
                    await mainComponent.RequestPlayerRealm();
                    
                    Log.Info("EnterMapFinish_CreateMainUI: 境界数据请求完成");
                }
                else
                {
                    Log.Error("EnterMapFinish_CreateMainUI: UIMainComponent为空，无法请求数据");
                }
                
                Log.Info("EnterMapFinish_CreateMainUI: 主界面创建完成");
            }
            catch (System.Exception e)
            {
                Log.Error($"EnterMapFinish_CreateMainUI: 主界面创建失败: {e.Message}");
                Log.Error($"EnterMapFinish_CreateMainUI: 堆栈: {e.StackTrace}");
            }
        }
    }
}
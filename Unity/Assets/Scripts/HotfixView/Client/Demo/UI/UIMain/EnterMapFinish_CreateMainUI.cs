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
                    // 请求玩家装备数据并刷新UI
                    await mainComponent.RequestPlayerEquipments();
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
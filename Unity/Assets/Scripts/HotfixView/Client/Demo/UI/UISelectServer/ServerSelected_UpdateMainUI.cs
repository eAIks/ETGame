namespace ET.Client
{
    [Event(SceneType.Demo)]
    public class ServerSelected_UpdateMainUI : AEvent<Scene, ServerSelectedEvent>
    {
        protected override async ETTask Run(Scene scene, ServerSelectedEvent args)
        {
            Log.Info($"ServerSelected_UpdateMainUI: 收到服务器选择事件: {args.SelectedServer?.ServerName}");
            
            // 查找UISelectServer界面并更新显示
            UIComponent uiComponent = scene.GetComponent<UIComponent>();
            if (uiComponent != null)
            {
                UI selectServerUI = uiComponent.Get(UIType.UISelectServer);
                if (selectServerUI != null)
                {
                    UISelectServerComponent selectServerComp = selectServerUI.GetComponent<UISelectServerComponent>();
                    if (selectServerComp != null)
                    {
                        selectServerComp.OnServerSelected(args.SelectedServer);
                    }
                }
            }
            
            await ETTask.CompletedTask;
        }
    }
}
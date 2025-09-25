namespace ET.Client
{
    [Event(SceneType.Demo)]
    [FriendOf(typeof(UIMainComponent))]
    [FriendOf(typeof(UIRealmLevel))]
    public class PlayerResourceChangedEvent_RefreshUI : AEvent<Scene, PlayerResourceChangedEvent>
    {
        protected override async ETTask Run(Scene scene, PlayerResourceChangedEvent args)
        {
            Log.Info($"[事件调试] PlayerResourceChangedEvent_RefreshUI 被触发！");
            
            // 显示资源变化提示
            Log.Info($"[UI事件] 💰 资源获得! 灵石 +{args.SpiritStoneGained}, 经验 +{args.ExpGained}");
            Log.Info($"[当前状态] 经验: {args.NewCurrentExp}, 灵石: {args.NewSpiritStone}");
            
            // 尝试更新UI，需要获取当前的等级和境界信息
            try
            {
                UI uiMain = scene.GetComponent<UIComponent>()?.Get(UIType.UIMain);
                if (uiMain != null)
                {
                    UIMainComponent uiMainComponent = uiMain.GetComponent<UIMainComponent>();
                    if (uiMainComponent != null)
                    {
                        UIRealmLevel uiRealmLevel = uiMainComponent.UIRealmLevel;
                        Log.Info($"[资源事件调试] UIRealmLevel是否为空: {uiRealmLevel == null}");
                        
                        if (uiRealmLevel != null)
                        {
                            Log.Info($"[资源事件调试] GameObject是否为空: {uiRealmLevel.GameObject == null}");
                            // 更新当前的RealmInfo中的经验和灵石
                            if (uiRealmLevel.CurrentRealmInfo != null)
                            {
                                // 更新经验和灵石
                                uiRealmLevel.CurrentRealmInfo.CurrentExp = args.NewCurrentExp;
                                uiRealmLevel.CurrentRealmInfo.SpiritStone = args.NewSpiritStone;
                                
                                // 重新计算升级所需经验（从配置获取）
                                UIRealmLevelSystem.UpdateFromRoleData(uiRealmLevel, 
                                    uiRealmLevel.CurrentRealmInfo.Level,
                                    uiRealmLevel.CurrentRealmInfo.MajorRealm,
                                    uiRealmLevel.CurrentRealmInfo.MinorRealm,
                                    args.NewCurrentExp,
                                    args.NewSpiritStone);
                                
                                Log.Info("[UI更新] 成功更新经验和灵石显示");
                            }
                            else
                            {
                                // 如果当前RealmInfo为空，使用默认值创建基本的RealmInfo
                                Log.Info("[UI更新] CurrentRealmInfo为空，使用默认值创建基本RealmInfo");
                                UIRealmLevelSystem.UpdateFromRoleData(uiRealmLevel, 
                                    1, // 默认等级
                                    1, // 默认大境界
                                    1, // 默认小境界
                                    args.NewCurrentExp, 
                                    args.NewSpiritStone);
                                
                                Log.Info("[UI更新] 成功使用默认RealmInfo更新显示");
                            }
                        }
                        else
                        {
                            Log.Warning("[UI更新] UIRealmLevel为空，无法更新UI");
                        }
                    }
                    else
                    {
                        Log.Warning("[UI更新] UIMainComponent或UIRealmLevel为空，无法更新UI");
                    }
                }
                else
                {
                    Log.Warning("[UI更新] UIMain不存在，无法更新UI");
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"[UI更新] 更新UI时发生异常: {e.Message}");
            }
            
            await ETTask.CompletedTask;
        }
    }
}
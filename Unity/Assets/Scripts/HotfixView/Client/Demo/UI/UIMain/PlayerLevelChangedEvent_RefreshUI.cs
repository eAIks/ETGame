namespace ET.Client
{
    [Event(SceneType.Demo)]
    [FriendOf(typeof(UIMainComponent))]
    [FriendOf(typeof(UIRealmLevel))]
    public class PlayerLevelChangedEvent_RefreshUI : AEvent<Scene, PlayerLevelChangedEvent>
    {
        protected override async ETTask Run(Scene scene, PlayerLevelChangedEvent args)
        {
            Log.Info($"[事件调试] PlayerLevelChangedEvent_RefreshUI 被触发！");
            Log.Info($"[事件调试] Scene类型: {scene?.GetType()?.Name}, Scene是否为空: {scene == null}");
            
            // 显示升级提示
            Log.Info($"[UI事件] 🎉 角色升级! 新等级: {args.NewLevel}, 新境界: {args.NewMajorRealm}.{args.NewMinorRealm}");
            Log.Info($"[奖励获得] 灵石 +{args.SpiritStoneGained}, 经验 +{args.ExpGained}");
            Log.Info($"[当前状态] 等级: {args.NewLevel}, 经验: {args.NewCurrentExp}, 灵石: {args.NewSpiritStone}");
            
            // 尝试更新UI
            try
            {
                Log.Info($"[UI调试] 开始查找UIComponent...");
                UIComponent uiComponent = scene.GetComponent<UIComponent>();
                Log.Info($"[UI调试] UIComponent是否为空: {uiComponent == null}");
                
                if (uiComponent == null)
                {
                    Log.Error("[UI调试] UIComponent为空，无法获取UI");
                    return;
                }
                
                UI uiMain = uiComponent.Get(UIType.UIMain);
                Log.Info($"[UI调试] UIMain是否为空: {uiMain == null}");
                Log.Info($"[UI调试] UIType.UIMain值: {UIType.UIMain}");
                
                if (uiMain != null)
                {
                    Log.Info($"[UI调试] 找到UIMain，开始查找UIMainComponent...");
                    UIMainComponent uiMainComponent = uiMain.GetComponent<UIMainComponent>();
                    Log.Info($"[UI调试] UIMainComponent是否为空: {uiMainComponent == null}");
                    
                    if (uiMainComponent != null)
                    {
                        // 正确访问EntityRef<UIRealmLevel>
                        UIRealmLevel realmLevel = uiMainComponent.UIRealmLevel;
                        Log.Info($"[UI调试] UIRealmLevel是否为空: {realmLevel == null}");
                        
                        if (realmLevel != null)
                        {
                            Log.Info($"[UI调试] 开始更新UIRealmLevel, GameObject是否为空: {realmLevel.GameObject == null}");
                            
                            // 检查UI组件是否已正确初始化
                            if (realmLevel.GameObject == null)
                            {
                                Log.Warning("[UI调试] UIRealmLevel的GameObject为空，尝试重新初始化");
                                // 如果GameObject为空，说明初始化可能失败，尝试重新获取
                                return;
                            }
                            
                            // 检查Text组件状态
                            Log.Info($"[UI组件状态] MajorRealmText={realmLevel.MajorRealmText != null}, " +
                                    $"MinorRealmText={realmLevel.MinorRealmText != null}, " +
                                    $"LevelText={realmLevel.LevelText != null}, " +
                                    $"ExpText={realmLevel.ExpText != null}, " +
                                    $"SpiritStoneText={realmLevel.SpiritStoneText != null}");
                            
                            // 使用UpdateFromRoleData，它会自动从配置计算升级所需经验
                            UIRealmLevelSystem.UpdateFromRoleData(realmLevel, 
                                args.NewLevel, 
                                args.NewMajorRealm, 
                                args.NewMinorRealm, 
                                args.NewCurrentExp, 
                                args.NewSpiritStone);
                        
                            Log.Info("[UI更新] 成功调用UpdateFromRoleData方法");
                        }
                        else
                        {
                            Log.Warning("[UI更新] UIRealmLevel为空，无法更新UI");
                        }
                    }
                    else
                    {
                        Log.Warning("[UI更新] UIMainComponent为空，无法更新UI");
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
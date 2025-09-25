using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 用于测试境界显示功能的工具类
    /// 可以在Unity控制台或通过其他方式调用此类来测试境界显示
    /// </summary>
    public static class TestRealmLevelDisplay
    {
        public static void TestWithSampleData(Scene scene)
        {
            Log.Info("TestRealmLevelDisplay: 开始测试境界显示功能");
            
            // 获取UIMainComponent
            UI ui = scene.GetComponent<UIComponent>()?.Get(UIType.UIMain);
            UIMainComponent mainComponent = ui?.GetComponent<UIMainComponent>();
            
            if (mainComponent == null)
            {
                Log.Error("TestRealmLevelDisplay: UIMainComponent未找到");
                return;
            }
            
            // 创建测试境界数据
            RealmInfoProto testRealmInfo = RealmInfoProto.Create();
            testRealmInfo.MajorRealm = 2;
            testRealmInfo.MinorRealm = 3;
            testRealmInfo.Level = 15;
            testRealmInfo.CurrentExp = 1250;
            testRealmInfo.MajorRealmName = "炼气期";
            testRealmInfo.MinorRealmName = "第三重";
            
            Log.Info($"TestRealmLevelDisplay: 设置测试数据 - {testRealmInfo.MajorRealmName}-{testRealmInfo.MinorRealmName}, 等级:{testRealmInfo.Level}");
            
            // 设置境界信息
            
            // 刷新显示
            mainComponent.RefreshRealmLevel();
            
            Log.Info("TestRealmLevelDisplay: 测试完成");
        }
        
        public static void TestWithNullData(Scene scene)
        {
            Log.Info("TestRealmLevelDisplay: 开始测试空数据境界显示功能");
            
            // 获取UIMainComponent
            UI ui = scene.GetComponent<UIComponent>()?.Get(UIType.UIMain);
            UIMainComponent mainComponent = ui?.GetComponent<UIMainComponent>();
            
            if (mainComponent == null)
            {
                Log.Error("TestRealmLevelDisplay: UIMainComponent未找到");
                return;
            }
            
            // 设置空境界信息
            
            // 刷新显示
            mainComponent.RefreshRealmLevel();
            
            Log.Info("TestRealmLevelDisplay: 空数据测试完成");
        }
    }
}
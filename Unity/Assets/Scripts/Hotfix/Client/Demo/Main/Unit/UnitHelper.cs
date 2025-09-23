namespace ET.Client
{
    public static partial class UnitHelper
    {
        public static Unit GetMyUnitFromClientScene(Scene root)
        {
            // UnitComponent暂时未使用，直接返回null
            Log.Warning("UnitHelper.GetMyUnitFromClientScene: UnitComponent暂时未使用");
            return null;
        }
        
        public static Unit GetMyUnitFromCurrentScene(Scene currentScene)
        {
            // UnitComponent暂时未使用，直接返回null
            Log.Warning("UnitHelper.GetMyUnitFromCurrentScene: UnitComponent暂时未使用");
            return null;
        }
    }
}
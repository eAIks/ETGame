using Unity.Mathematics;

namespace ET.Client
{
    public static partial class UnitFactory
    {
        public static Unit Create(Scene currentScene, UnitInfo unitInfo)
        {
	        // UnitComponent暂时未使用，直接返回null
	        Log.Warning("UnitFactory.Create: UnitComponent暂时未使用，无法创建Unit");
	        return null;
        }
    }
}

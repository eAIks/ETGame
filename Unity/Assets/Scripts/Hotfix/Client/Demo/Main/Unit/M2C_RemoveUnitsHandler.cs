namespace ET.Client
{
	// 暂时禁用UnitComponent相关功能
	// [MessageHandler(SceneType.Demo)]
	public class M2C_RemoveUnitsHandler: MessageHandler<Scene, M2C_RemoveUnits>
	{
		protected override async ETTask Run(Scene root, M2C_RemoveUnits message)
		{	
			// UnitComponent暂时未使用，忽略删除单位消息
			Log.Warning("M2C_RemoveUnitsHandler: UnitComponent暂时未使用，忽略消息");
			await ETTask.CompletedTask;
		}
	}
}

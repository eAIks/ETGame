namespace ET.Client
{
	// 暂时禁用UnitComponent相关功能
	// [MessageHandler(SceneType.Demo)]
	public class M2C_CreateUnitsHandler: MessageHandler<Scene, M2C_CreateUnits>
	{
		protected override async ETTask Run(Scene root, M2C_CreateUnits message)
		{
			// UnitComponent暂时未使用，忽略创建单位消息
			Log.Warning("M2C_CreateUnitsHandler: UnitComponent暂时未使用，忽略消息");
			await ETTask.CompletedTask;
		}
	}
}

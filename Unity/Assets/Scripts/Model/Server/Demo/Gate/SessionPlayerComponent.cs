namespace ET.Server
{
	[ComponentOf(typeof(Session))]
	public class SessionPlayerComponent : Entity, IAwake, IDestroy
	{
		private EntityRef<Player> player;

		public Player Player
		{
			get
			{
				return this.player;
			}
			set
			{
				this.player = value;
			}
		}

		public string Account { get; set; }
		public string Password { get; set; }
		public string AccountUUID { get; set; }
		public long SessionKey { get; set; }
	}
}
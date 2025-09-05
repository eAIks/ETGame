namespace ET.Server
{
    [ChildOf(typeof(Scene))]
    public sealed class Account : Entity, IAwake<string, string>
    {
        public string AccountName { get; set; }
        public string Password { get; set; }
        public long CreateTime { get; set; }
    }
}
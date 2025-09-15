namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class UnitySceneLoadedComponent : Entity, IAwake, IDestroy, IUpdate
    {
        public string lastSceneName = "";
        public bool hasProcessedLogin = false;
    }
}
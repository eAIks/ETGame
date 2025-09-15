using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UISelectServerComponent : Entity, IAwake, IDestroy
    {
        public GameObject serverNameText;
        public GameObject serverStatusText;
        public GameObject onlineCountText;
        public GameObject selectServerBtn;
        public GameObject enterGameBtn;
        public GameObject serverIcon;
        public GameObject recommendTag;
        public GameObject newTag;
        
        public EntityRef<ServerInfo> currentSelectedServer;
        
        // 初始化状态标记
        public bool isInitialized = false;
    }
}
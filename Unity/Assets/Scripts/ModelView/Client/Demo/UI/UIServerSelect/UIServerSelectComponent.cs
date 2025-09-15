using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UIServerSelectComponent : Entity, IAwake, IDestroy
    {
        public GameObject zoneListParent;
        public GameObject serverListParent;
        public GameObject zoneItemTemplate;
        public GameObject serverItemTemplate;
        public GameObject closeBtn;
        
        public List<GameObject> zoneItems = new List<GameObject>();
        public List<GameObject> serverItems = new List<GameObject>();
        
        public EntityRef<ServerZone> currentZone;
        public EntityRef<ServerInfo> selectedServer;
        public int currentZoneId = -1;
    }
}
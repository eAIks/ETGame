using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ChildOf(typeof(UIMainComponent))]
    public class UIRealmLevel : Entity, IAwake
    {
        public GameObject GameObject;
        
        // UI组件
        public Text MajorRealmText;     // 大境界名称显示
        public Text MinorRealmText;     // 小境界名称显示
        public Text LevelText;          // 等级显示
        public Text ExpText;            // 经验值显示
        public Text SpiritStoneText;    // 灵石显示
        
        // 当前境界信息
        public RealmInfoProto CurrentRealmInfo;
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIRealmLevel))]
    [FriendOf(typeof(UIRealmLevel))]
    public static partial class UIRealmLevelSystem
    {
        [EntitySystem]
        private static void Awake(this UIRealmLevel self)
        {
            // GameObject在Awake时可能还未被赋值，延迟初始化
            if (self.GameObject == null)
            {
                return;
            }
            
            self.InitializeComponents();
        }
        
        public static void InitializeComponents(this UIRealmLevel self)
        {
            if (self.GameObject == null)
            {
                return;
            }
            
            Transform transform = self.GameObject.transform;
            Log.Info($"UIRealmLevel: 开始初始化，GameObject名称: {self.GameObject.name}");
            
            // 打印所有子对象的名称用于调试
            Log.Info($"UIRealmLevel子对象列表:");
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Log.Info($"  子对象{i}: {child.name}");
            }
            
            // 尝试多种可能的子对象名称
            self.MajorRealmText = FindTextComponent(transform, new[] { "MajorRealm", "大境界", "Realm", "RealmMajor" });
            self.MinorRealmText = FindTextComponent(transform, new[] { "MinorRealm", "小境界", "SubRealm", "RealmMinor" });
            self.LevelText = FindTextComponent(transform, new[] { "Level", "等级", "Lv", "PlayerLevel" });
            self.ExpText = FindTextComponent(transform, new[] { "Exp", "经验", "Experience", "CurrentExp" });
            
            self.SpiritStoneText = FindTextComponent(transform, new[] { "SpiritStone", "灵石", "Spirit", "Stone" });
            
            // 如果找不到指定名称的子对象，尝试在所有子对象中查找Text组件
            if (self.MajorRealmText == null)
            {
                self.MajorRealmText = self.GameObject.GetComponent<Text>();
                if (self.MajorRealmText == null)
                {
                    Text[] allTexts = self.GameObject.GetComponentsInChildren<Text>();
                    if (allTexts.Length > 0)
                    {
                        Log.Info($"在UIRealmLevel中找到{allTexts.Length}个Text组件，按顺序分配");
                        // 按顺序分配Text组件
                        if (allTexts.Length >= 1) self.MajorRealmText = allTexts[0];
                        if (allTexts.Length >= 2) self.MinorRealmText = allTexts[1];
                        if (allTexts.Length >= 3) self.LevelText = allTexts[2];
                        if (allTexts.Length >= 4) self.ExpText = allTexts[3];
                        if (allTexts.Length >= 5) self.SpiritStoneText = allTexts[4];
                        
                        Log.Info($"按顺序分配Text组件完成");
                    }
                }
                else
                {
                    Log.Info("在UIRealmLevel根对象上查找Text组件作为MajorRealmText");
                }
            }
            
            Log.Info($"UIRealmLevel组件初始化完成: MajorRealmText={self.MajorRealmText != null}, MinorRealmText={self.MinorRealmText != null}, LevelText={self.LevelText != null}, ExpText={self.ExpText != null}, SpiritStoneText={self.SpiritStoneText != null}");
        }
        
        private static Text FindTextComponent(Transform parent, string[] possibleNames)
        {
            foreach (string name in possibleNames)
            {
                Transform child = parent.Find(name);
                if (child != null)
                {
                    Text text = child.GetComponent<Text>();
                    if (text != null)
                    {
                        Log.Info($"UIRealmLevel找到Text组件: {name}");
                        return text;
                    }
                    
                    // 如果直接没有Text组件，尝试在子对象中查找
                    text = child.GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        Log.Info($"UIRealmLevel在{name}的子对象中找到Text组件");
                        return text;
                    }
                }
            }
            
            Log.Warning($"UIRealmLevel未找到Text组件，尝试的名称: [{string.Join(", ", possibleNames)}]");
            return null;
        }
        
        public static void SetRealmInfo(this UIRealmLevel self, RealmInfoProto realmInfo)
        {
            self.CurrentRealmInfo = realmInfo;
            
            Log.Info($"SetRealmInfo: 收到境界信息，是否为空: {realmInfo == null}");
            
            // 确保组件已初始化
            if (self.MajorRealmText == null || self.MinorRealmText == null || self.LevelText == null || self.ExpText == null || self.SpiritStoneText == null)
            {
                Log.Warning("UIRealmLevel组件未完全初始化，重新初始化组件");
                self.InitializeComponents();
                
                // 如果重新初始化后仍有组件为空，记录详细错误信息
                if (self.MajorRealmText == null || self.MinorRealmText == null || self.LevelText == null || self.ExpText == null || self.SpiritStoneText == null)
                {
                    Log.Error($"重新初始化后仍有组件为空: MajorRealmText={self.MajorRealmText != null}, " +
                             $"MinorRealmText={self.MinorRealmText != null}, LevelText={self.LevelText != null}, " +
                             $"ExpText={self.ExpText != null}, SpiritStoneText={self.SpiritStoneText != null}");
                    Log.Error($"UIRealmLevel GameObject信息: 名称={self.GameObject?.name}, 是否活跃={self.GameObject?.activeInHierarchy}");
                }
            }
            
            // 验证组件是否初始化成功
            Log.Info($"SetRealmInfo: 组件状态 - MajorRealmText: {self.MajorRealmText != null}, MinorRealmText: {self.MinorRealmText != null}, LevelText: {self.LevelText != null}, ExpText: {self.ExpText != null}, SpiritStoneText: {self.SpiritStoneText != null}");
            
            if (realmInfo != null)
            {
                Log.Info($"设置境界信息: {realmInfo.MajorRealmName}-{realmInfo.MinorRealmName}, 等级:{realmInfo.Level}, 经验:{realmInfo.CurrentExp}");
                
                // 设置大境界名称
                if (self.MajorRealmText != null)
                {
                    string majorRealmName = realmInfo.MajorRealmName ?? "未知境界";
                    self.MajorRealmText.text = majorRealmName;
                    Log.Info($"设置大境界名称: {majorRealmName}");
                }
                else
                {
                    Log.Warning("MajorRealmText组件为空，无法设置大境界名称");
                }
                
                // 设置小境界名称
                if (self.MinorRealmText != null)
                {
                    string minorRealmName = realmInfo.MinorRealmName ?? "未知";
                    self.MinorRealmText.text = minorRealmName;
                    Log.Info($"设置小境界名称: {minorRealmName}");
                }
                else
                {
                    Log.Warning("MinorRealmText组件为空，无法设置小境界名称");
                }
                
                // 设置等级
                if (self.LevelText != null)
                {
                    string levelText = $"等级 {realmInfo.Level}";
                    self.LevelText.text = levelText;
                    Log.Info($"设置等级: {levelText}");
                }
                else
                {
                    Log.Warning("LevelText组件为空，无法设置等级");
                }
                
                // 设置经验值 (当前经验/升级所需经验)
                if (self.ExpText != null)
                {
                    string expText = $"经验 {realmInfo.CurrentExp}/{realmInfo.ExpForNextLevel}";
                    self.ExpText.text = expText;
                    Log.Info($"设置经验: {expText}");
                }
                else
                {
                    Log.Warning("ExpText组件为空，无法设置经验");
                }
                
                // 设置灵石
                if (self.SpiritStoneText != null)
                {
                    string spiritStoneText = $"灵石 {realmInfo.SpiritStone}";
                    self.SpiritStoneText.text = spiritStoneText;
                    Log.Info($"设置灵石: {spiritStoneText}");
                }
                else
                {
                    Log.Warning("SpiritStoneText组件为空，无法设置灵石");
                }
            }
            else
            {
                Log.Info("境界信息为空，设置默认显示");
                
                // 设置默认显示
                if (self.MajorRealmText != null)
                {
                    self.MajorRealmText.text = "凡人";
                    Log.Info("设置默认大境界: 凡人");
                }
                
                if (self.MinorRealmText != null)
                {
                    self.MinorRealmText.text = "初入修仙";
                    Log.Info("设置默认小境界: 初入修仙");
                }
                
                if (self.LevelText != null)
                {
                    self.LevelText.text = "等级 1";
                    Log.Info("设置默认等级: 等级 1");
                }
                
                if (self.ExpText != null)
                {
                    self.ExpText.text = "经验 0/100";
                    Log.Info("设置默认经验: 经验 0/100");
                }
                
                if (self.SpiritStoneText != null)
                {
                    self.SpiritStoneText.text = "灵石 0";
                    Log.Info("设置默认灵石: 灵石 0");
                }
            }
        }
        
        /// <summary>
        /// 更新境界显示，对外公开的方法
        /// </summary>
        public static void UpdateRealmDisplay(this UIRealmLevel self, RealmInfoProto realmInfo)
        {
            self.SetRealmInfo(realmInfo);
        }
        
        /// <summary>
        /// 直接通过角色信息更新显示
        /// </summary>
        public static void UpdateFromRoleData(this UIRealmLevel self, int level, int majorRealm, int minorRealm, long currentExp, long spiritStone, long expForNextLevel = 0)
        {
            RealmInfoProto realmInfo = RealmInfoProto.Create();
            realmInfo.Level = level;
            realmInfo.MajorRealm = majorRealm;
            realmInfo.MinorRealm = minorRealm;
            realmInfo.CurrentExp = currentExp;
            realmInfo.SpiritStone = spiritStone;
            
            // 如果没有传入expForNextLevel，根据当前境界从配置计算
            if (expForNextLevel <= 0)
            {
                expForNextLevel = GetExpForNextLevelFromConfig(level, majorRealm, minorRealm);
            }
            realmInfo.ExpForNextLevel = expForNextLevel;
            
            // 根据境界ID获取名称和配置信息
            var allConfigs = RealmLevelConfigCategory.Instance.GetAll();
            foreach (var config in allConfigs.Values)
            {
                if (config.StageId == majorRealm && config.SubStage == minorRealm)
                {
                    realmInfo.MajorRealmName = config.StageName;
                    realmInfo.MinorRealmName = config.SubStageName;
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(realmInfo.MajorRealmName))
            {
                realmInfo.MajorRealmName = "未知境界";
                realmInfo.MinorRealmName = "未知";
            }
            
            self.SetRealmInfo(realmInfo);
        }
        
        /// <summary>
        /// 根据玩家当前等级和境界从配置获取升级所需经验
        /// </summary>
        private static long GetExpForNextLevelFromConfig(int level, int majorRealm, int minorRealm)
        {
            var allConfigs = RealmLevelConfigCategory.Instance.GetAll();
            
            // 查找当前境界的配置
            foreach (var config in allConfigs.Values)
            {
                if (config.StageId == majorRealm && config.SubStage == minorRealm)
                {
                    // 检查当前等级是否在这个境界的范围内
                    if (level >= config.LevelMin && level <= config.LevelMax)
                    {
                        return config.ExpPerLevel;
                    }
                }
            }
            
            // 如果没找到配置，返回默认值
            Log.Warning($"未找到等级{level}境界{majorRealm}.{minorRealm}的经验配置，使用默认值100");
            return 100;
        }
    }
}
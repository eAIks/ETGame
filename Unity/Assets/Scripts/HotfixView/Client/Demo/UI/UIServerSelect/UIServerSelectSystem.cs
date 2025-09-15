using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIServerSelectComponent))]
    [FriendOf(typeof(UIServerSelectComponent))]
    [FriendOf(typeof(UISelectServerComponent))]
    public static partial class UIServerSelectSystem
    {
        [EntitySystem]
        private static void Awake(this UIServerSelectComponent self)
        {
            // 获取UI元素引用
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            if (rc != null)
            {
                self.zoneListParent = rc.Get<GameObject>("ZoneListParent");
                self.serverListParent = rc.Get<GameObject>("ServerListParent");
                self.zoneItemTemplate = rc.Get<GameObject>("ZoneItemTemplate");
                self.serverItemTemplate = rc.Get<GameObject>("ServerItemTemplate");
                self.closeBtn = rc.Get<GameObject>("CloseBtn");
                
                // 检测并修复可能的数据绑定错误
                self.DetectAndFixDataBinding();
                
                // 隐藏模板并配置
                if (self.zoneItemTemplate != null)
                {
                    self.zoneItemTemplate.SetActive(false);
                    self.ConfigureTemplate(self.zoneItemTemplate);
                }
                if (self.serverItemTemplate != null)
                {
                    self.serverItemTemplate.SetActive(false);
                    self.ConfigureTemplate(self.serverItemTemplate);
                }
                
                // 绑定按钮事件
                if (self.closeBtn != null)
                {
                    self.closeBtn.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        self.OnCloseBtnClick();
                    });
                }
            }
            else
            {
                Log.Error("UIServerSelectSystem: 没有找到ReferenceCollector组件");
            }
            
            // 如果模板没有找到，尝试搜索
            if (self.zoneItemTemplate == null || self.serverItemTemplate == null)
            {
                self.SearchForMissingTemplates();
            }
            
            // 配置布局组件
            self.ConfigureLayoutComponents();
            
            // 初始化界面
            self.InitializeUI().Coroutine();
        }

        [EntitySystem]
        private static void Destroy(this UIServerSelectComponent self)
        {
            self.ClearItems();
        }
        
        /// <summary>
        /// 搜索缺失的模板
        /// </summary>
        private static void SearchForMissingTemplates(this UIServerSelectComponent self)
        {
            GameObject rootObj = self.GetParent<UI>().GameObject;
            
            if (self.zoneItemTemplate == null)
            {
                GameObject foundZone = self.FindChildByName(rootObj.transform, "ZoneItemTemplate");
                if (foundZone != null)
                {
                    self.zoneItemTemplate = foundZone;
                }
                else
                {
                    Log.Warning("UIServerSelectSystem: 未找到ZoneItemTemplate");
                }
            }
            
            if (self.serverItemTemplate == null)
            {
                GameObject foundServer = self.FindChildByName(rootObj.transform, "ServerItemTemplate");
                if (foundServer != null)
                {
                    self.serverItemTemplate = foundServer;
                }
                else
                {
                    Log.Warning("UIServerSelectSystem: 未找到ServerItemTemplate");
                }
            }
            
            // 如果父容器也没找到，尝试搜索
            if (self.zoneListParent == null)
            {
                GameObject foundZoneParent = self.FindChildByName(rootObj.transform, "ZoneListParent");
                if (foundZoneParent == null)
                    foundZoneParent = self.FindChildByName(rootObj.transform, "ZoneList");
                if (foundZoneParent == null)
                    foundZoneParent = self.FindChildByName(rootObj.transform, "Content"); // 可能在ScrollView的Content中
                
                if (foundZoneParent != null)
                {
                    self.zoneListParent = foundZoneParent;
                }
            }
            
            if (self.serverListParent == null)
            {
                GameObject foundServerParent = self.FindChildByName(rootObj.transform, "ServerListParent");
                if (foundServerParent == null)
                    foundServerParent = self.FindChildByName(rootObj.transform, "ServerList");
                if (foundServerParent == null)
                {
                    // 尝试找第二个Content（可能有两个ScrollView）
                    Transform[] allTransforms = rootObj.GetComponentsInChildren<Transform>();
                    List<Transform> contents = new List<Transform>();
                    foreach (var t in allTransforms)
                    {
                        if (t.name == "Content")
                            contents.Add(t);
                    }
                    if (contents.Count > 1)
                        foundServerParent = contents[1].gameObject;
                }
                
                if (foundServerParent != null)
                {
                    self.serverListParent = foundServerParent;
                }
            }
        }
        
        /// <summary>
        /// 配置布局组件
        /// </summary>
        private static void ConfigureLayoutComponents(this UIServerSelectComponent self)
        {
            // 尝试配置主容器的水平布局
            self.ConfigureMainLayout();
            
            // 配置区组列表布局（左侧）
            if (self.zoneListParent != null)
            {
                self.ConfigureVerticalLayout(self.zoneListParent, "区组列表");
            }
            
            // 配置服务器列表布局（右侧）
            if (self.serverListParent != null)
            {
                self.ConfigureVerticalLayout(self.serverListParent, "服务器列表");
            }
        }
        
        /// <summary>
        /// 配置主布局容器
        /// </summary>
        private static void ConfigureMainLayout(this UIServerSelectComponent self)
        {
            GameObject rootObj = self.GetParent<UI>().GameObject;
            
            // 查找或创建MainContent容器
            Transform mainContent = rootObj.transform.Find("MainContent");
            if (mainContent == null)
            {
                // 尝试查找可能的父容器
                Transform[] allTransforms = rootObj.GetComponentsInChildren<Transform>();
                foreach (Transform t in allTransforms)
                {
                    if (t.name.Contains("Content") || t.name.Contains("Main") || t.name.Contains("Panel"))
                    {
                        mainContent = t;
                        break;
                    }
                }
            }
            
            if (mainContent != null)
            {
                // 配置水平布局
                var horizontalLayout = mainContent.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                if (horizontalLayout == null)
                {
                    horizontalLayout = mainContent.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                }
                
                // 配置水平布局参数
                horizontalLayout.spacing = 20f; // 左右面板间距
                horizontalLayout.padding = new UnityEngine.RectOffset(20, 20, 20, 20);
                horizontalLayout.childAlignment = UnityEngine.TextAnchor.UpperLeft;
                horizontalLayout.childControlWidth = true;
                horizontalLayout.childControlHeight = true;
                horizontalLayout.childForceExpandWidth = true;
                horizontalLayout.childForceExpandHeight = true;
            }
        }
        
        /// <summary>
        /// 配置垂直布局组件
        /// </summary>
        private static void ConfigureVerticalLayout(this UIServerSelectComponent self, GameObject parentObj, string listName)
        {
            // 检查并添加VerticalLayoutGroup
            var layoutGroup = parentObj.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = parentObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            }
            
            // 配置布局参数
            layoutGroup.spacing = 10f; // 间距
            layoutGroup.padding = new UnityEngine.RectOffset(10, 10, 10, 10); // 内边距
            layoutGroup.childAlignment = UnityEngine.TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childScaleWidth = false;
            layoutGroup.childScaleHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            
            // 检查并添加ContentSizeFitter
            var sizeFitter = parentObj.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = parentObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            }
            
            // 配置自适应大小
            sizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        }
        
        /// <summary>
        /// 配置列表项的RectTransform以适应LayoutGroup
        /// </summary>
        private static void ConfigureItemRectTransform(this UIServerSelectComponent self, GameObject item)
        {
            RectTransform rectTransform = item.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            // 重置锚点和位置，让LayoutGroup接管位置控制
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
            rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
            
            // 确保有合适的高度
            if (rectTransform.sizeDelta.y <= 0)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 60f); // 默认高度60
            }
            
            // 添加LayoutElement组件来控制尺寸
            var layoutElement = item.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = item.AddComponent<UnityEngine.UI.LayoutElement>();
            }
            
            // 配置LayoutElement
            layoutElement.minHeight = 50f;
            layoutElement.preferredHeight = 60f;
            layoutElement.flexibleHeight = 0f;
            layoutElement.flexibleWidth = 1f;
        }
        
        /// <summary>
        /// 配置模板对象，确保具有必要的组件
        /// </summary>
        private static void ConfigureTemplate(this UIServerSelectComponent self, GameObject template)
        {
            // 确保有Button组件
            if (template.GetComponent<Button>() == null)
            {
                template.AddComponent<Button>();
            }
            
            // 确保有Image组件（Button需要）
            Image image = template.GetComponent<Image>();
            if (image == null)
            {
                image = template.AddComponent<Image>();
                image.color = Color.white;
            }
            image.raycastTarget = true;
            
            // 确保RectTransform配置正确
            RectTransform rectTransform = template.GetComponent<RectTransform>();
            if (rectTransform != null && rectTransform.sizeDelta.y <= 0)
            {
                rectTransform.sizeDelta = new Vector2(300f, 60f);
            }
            
            // 确保有Text组件用于显示名称
            if (template.GetComponentInChildren<Text>() == null)
            {
                GameObject textObj = new GameObject("NameText");
                textObj.transform.SetParent(template.transform, false);
                
                Text nameText = textObj.AddComponent<Text>();
                nameText.text = "模板文本";
                nameText.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                nameText.fontSize = 16;
                nameText.color = Color.black;
                nameText.alignment = TextAnchor.MiddleCenter;
                
                // 配置Text的RectTransform
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }
        }
        
        /// <summary>
        /// 检测并修复数据绑定错误
        /// </summary>
        private static void DetectAndFixDataBinding(this UIServerSelectComponent self)
        {
            // 检查是否存在明显的绑定错误
            bool needSwap = false;
            
            if (self.zoneListParent != null && self.serverListParent != null)
            {
                string zoneParentName = self.zoneListParent.name.ToLower();
                string serverParentName = self.serverListParent.name.ToLower();
                
                // 如果名称搞反了，自动交换
                if (zoneParentName.Contains("server") || serverParentName.Contains("zone"))
                {
                    needSwap = true;
                }
                
                // 检查相对位置（左侧是区组，右侧是服务器）
                Vector3 zonePos = self.zoneListParent.transform.position;
                Vector3 serverPos = self.serverListParent.transform.position;
                
                if (zonePos.x > serverPos.x)
                {
                    needSwap = true;
                }
            }
            
            // 检查模板绑定
            if (self.zoneItemTemplate != null && self.serverItemTemplate != null)
            {
                string zoneTemplateName = self.zoneItemTemplate.name.ToLower();
                string serverTemplateName = self.serverItemTemplate.name.ToLower();
                
                if (zoneTemplateName.Contains("server") || serverTemplateName.Contains("zone"))
                {
                    // 交换模板
                    GameObject temp = self.zoneItemTemplate;
                    self.zoneItemTemplate = self.serverItemTemplate;
                    self.serverItemTemplate = temp;
                }
            }
            
            // 如果需要交换父容器
            if (needSwap)
            {
                GameObject temp = self.zoneListParent;
                self.zoneListParent = self.serverListParent;
                self.serverListParent = temp;
            }
        }
        
        /// <summary>
        /// 设置按钮点击事件
        /// </summary>
        private static void SetupButtonClick(this UIServerSelectComponent self, GameObject item, string itemName, System.Action clickAction)
        {
            // 确保有Button组件
            Button button = item.GetComponent<Button>();
            if (button == null)
            {
                button = item.AddComponent<Button>();
            }
            
            // 确保有Image组件（Button需要）
            Image image = button.GetComponent<Image>();
            if (image == null)
            {
                image = item.AddComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0.1f);
            }
            
            // 配置按钮和图像
            button.interactable = true;
            image.raycastTarget = true;
            
            // 设置Canvas层级确保不被遮挡
            Canvas itemCanvas = item.GetComponent<Canvas>();
            if (itemCanvas == null)
            {
                itemCanvas = item.AddComponent<Canvas>();
                itemCanvas.overrideSorting = true;
                itemCanvas.sortingOrder = 100;
            }
            
            // 添加GraphicRaycaster
            if (item.GetComponent<GraphicRaycaster>() == null)
            {
                item.AddComponent<GraphicRaycaster>();
            }
            
            // 绑定点击事件
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                clickAction?.Invoke();
            });
            
            // 添加EventTrigger作为备用点击检测
            self.AddEventTrigger(item, clickAction);
        }
        
        /// <summary>
        /// 添加EventTrigger作为备用点击检测
        /// </summary>
        private static void AddEventTrigger(this UIServerSelectComponent self, GameObject item, System.Action clickAction)
        {
            UnityEngine.EventSystems.EventTrigger trigger = item.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
            {
                trigger = item.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            // 清除现有触发器
            trigger.triggers.Clear();
            
            // 添加PointerClick事件
            UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => 
            {
                clickAction?.Invoke();
            });
            trigger.triggers.Add(entry);
        }
        
        /// <summary>
        /// 安全地获取Scene
        /// </summary>
        private static Scene GetSceneSafely(this UIServerSelectComponent self)
        {
            // 方式1：直接调用Scene()
            Scene scene = self.Scene();
            if (scene != null && !scene.IsDisposed)
            {
                return scene;
            }
            
            // 方式2：通过Parent获取
            Entity current = self.Parent;
            while (current != null && !current.IsDisposed)
            {
                if (current is Scene parentScene)
                {
                    return parentScene;
                }
                current = current.Parent;
            }
            
            // 方式3：通过Root获取
            Entity root = self.Root();
            if (root != null && !root.IsDisposed && root is Scene rootScene)
            {
                return rootScene;
            }
            
            Log.Error("UIServerSelectSystem: 无法获取Scene");
            return null;
        }
        
        /// <summary>
        /// 处理服务器选择
        /// </summary>
        private static void HandleServerSelection(this UIServerSelectComponent self, ServerInfo server)
        {
            try
            {
                if (server == null)
                {
                    Log.Error("UIServerSelectSystem: server参数为null");
                    return;
                }
                
                // 立即获取Scene和相关信息，避免异步调用时UI组件被销毁
                Scene scene = self.GetSceneSafely();
                if (scene == null)
                {
                    return;
                }
                
                // 立即更新ServerListComponent
                ServerListComponent serverListComp = scene.GetComponent<ServerListComponent>();
                if (serverListComp != null)
                {
                    serverListComp.CurrentServer = server;
                }
                
                // 立即更新主界面（如果存在）
                try
                {
                    self.UpdateMainUIServerDisplayImmediate(server, scene);
                }
                catch (System.Exception ex)
                {
                    Log.Error($"UIServerSelectSystem: 更新主界面显示失败: {ex.Message}");
                }
                
                // 异步关闭界面
                self.CloseUIAsync(scene).Coroutine();
                
            }
            catch (System.Exception e)
            {
                Log.Error($"UIServerSelectSystem: 处理服务器选择失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 立即更新主界面服务器显示
        /// </summary>
        private static void UpdateMainUIServerDisplayImmediate(this UIServerSelectComponent self, ServerInfo server, Scene scene)
        {
            try
            {
                Log.Info("UIServerSelectSystem: 开始更新主界面服务器显示");
                
                if (server == null)
                {
                    Log.Error("UIServerSelectSystem: server参数为null");
                    return;
                }
                
                UIComponent uiComponent = scene.GetComponent<UIComponent>();
                if (uiComponent == null)
                {
                    Log.Warning("UIServerSelectSystem: UIComponent为null");
                    return;
                }
                
                UI selectServerUI = uiComponent.Get(UIType.UISelectServer);
                if (selectServerUI != null)
                {
                    UISelectServerComponent selectServerComp = selectServerUI.GetComponent<UISelectServerComponent>();
                    if (selectServerComp != null)
                    {
                        Log.Info($"UIServerSelectSystem: 设置currentSelectedServer为 {server.ServerName}");
                        // 修复EntityRef类型赋值问题
                        selectServerComp.currentSelectedServer = server;
                        Log.Info("UIServerSelectSystem: currentSelectedServer设置完成，开始更新UI");
                        // 立即更新主界面显示
                        // 调用正确的UpdateUI方法
                        selectServerComp.UpdateUI();
                        Log.Info("UIServerSelectSystem: UI更新完成");
                    }
                    else
                    {
                        Log.Warning("UIServerSelectSystem: UISelectServerComponent为null");
                    }
                }
                else
                {
                    Log.Warning("UIServerSelectSystem: UISelectServer UI不存在");
                }
                
            }
            catch (System.Exception e)
            {
                Log.Error($"UIServerSelectSystem: 更新主界面显示失败: {e.Message}");
                Log.Error($"UIServerSelectSystem: 异常堆栈: {e.StackTrace}");
            }
        }
        
        /// <summary>
        /// 异步关闭UI界面
        /// </summary>
        private static async ETTask CloseUIAsync(this UIServerSelectComponent self, Scene scene)
        {
            try
            {
                // 短暂延迟，确保UI操作完成
                await scene.GetComponent<TimerComponent>().WaitAsync(100);
                
                if (scene != null && !scene.IsDisposed)
                {
                    await UIHelper.Remove(scene, UIType.UIServerSelect);
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"UIServerSelectSystem: 异步关闭UI失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 递归查找子对象
        /// </summary>
        private static GameObject FindChildByName(this UIServerSelectComponent self, Transform parent, string name)
        {
            if (parent.name == name)
                return parent.gameObject;
                
            foreach (Transform child in parent)
            {
                GameObject found = self.FindChildByName(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }
        
        /// <summary>
        /// 初始化界面
        /// </summary>
        private static async ETTask InitializeUI(this UIServerSelectComponent self)
        {
            // 检查UI元素是否完整
            if (self.zoneItemTemplate == null || self.zoneListParent == null || 
                self.serverItemTemplate == null || self.serverListParent == null)
            {
                return;
            }
            
            // 从服务器获取服务器列表数据
            Scene scene = self.Scene();
            ServerListComponent serverListComp = scene?.GetComponent<ServerListComponent>();
            
            if (serverListComp == null)
            {
                serverListComp = scene.AddComponent<ServerListComponent>();
            }
            
            // 从服务器加载服务器列表  
            bool success = await serverListComp.LoadServerListFromServer();
            if (!success)
            {
                Log.Error("Failed to load server list from server");
                return;
            }
            
            // 创建区组列表
            self.CreateZoneList(serverListComp.ZoneList);
            
            // 默认选中第一个区组
            if (serverListComp.ZoneList.Count > 0)
            {
                ServerZone firstZone = serverListComp.ZoneList[0];
                self.SelectZone(firstZone);
            }
        }
        
        /// <summary>
        /// 创建区组列表
        /// </summary>
        private static void CreateZoneList(this UIServerSelectComponent self, List<ServerZone> zoneList)
        {
            self.ClearZoneItems();
            
            foreach (ServerZone zone in zoneList)
            {
                GameObject zoneItem = self.CreateZoneItem(zone);
                if (zoneItem != null)
                {
                    self.zoneItems.Add(zoneItem);
                }
            }
        }
        
        /// <summary>
        /// 创建区组项
        /// </summary>
        private static GameObject CreateZoneItem(this UIServerSelectComponent self, ServerZone zone)
        {
            if (self.zoneItemTemplate == null || self.zoneListParent == null)
            {
                Log.Error("UIServerSelectSystem: 创建区组项失败 - 模板或父对象为空");
                return null;
            }
            
            GameObject zoneItem = GameObject.Instantiate(self.zoneItemTemplate, self.zoneListParent.transform);
            zoneItem.SetActive(true);
            zoneItem.name = $"ZoneItem_{zone.ZoneId}";
            
            // 配置RectTransform以适应LayoutGroup
            self.ConfigureItemRectTransform(zoneItem);
            
            // 设置区组名称
            self.SetZoneText(zoneItem, zone.ZoneName);
            
            // 绑定点击事件 - 增强版本
            self.SetupButtonClick(zoneItem, zone.ZoneName, () => self.SelectZone(zone));
            
            // 保存区组ID在名字中
            zoneItem.name = $"ZoneItem_{zone.ZoneId}";
            
            return zoneItem;
        }
        
        /// <summary>
        /// 设置区组文本
        /// </summary>
        private static void SetZoneText(this UIServerSelectComponent self, GameObject zoneItem, string zoneName)
        {
            // 方式1：直接查找Text组件
            Text nameText = zoneItem.GetComponent<Text>();
            if (nameText != null)
            {
                nameText.text = zoneName;
                return;
            }
            
            // 方式2：查找子对象中的Text组件
            nameText = zoneItem.GetComponentInChildren<Text>();
            if (nameText != null)
            {
                nameText.text = zoneName;
                return;
            }
            
            // 方式3：查找特定名称的子对象
            Transform[] allChildren = zoneItem.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.name.Contains("Text") || child.name.Contains("Name") || child.name.Contains("Label"))
                {
                    Text childText = child.GetComponent<Text>();
                    if (childText != null)
                    {
                        childText.text = zoneName;
                        return;
                    }
                }
            }
            
            // 方式4：尝试在模板配置中创建的Text
            Transform nameTextTransform = zoneItem.transform.Find("NameText");
            if (nameTextTransform != null)
            {
                Text createdText = nameTextTransform.GetComponent<Text>();
                if (createdText != null)
                {
                    createdText.text = zoneName;
                    return;
                }
            }
        }
        
        /// <summary>
        /// 选择区组
        /// </summary>
        private static void SelectZone(this UIServerSelectComponent self, ServerZone zone)
        {
            self.currentZone = zone;
            self.currentZoneId = zone.ZoneId;
            
            // 更新区组选中状态
            self.UpdateZoneSelection(zone.ZoneId);
            
            // 创建该区组的服务器列表
            self.CreateServerList(zone.ServerList);
            
            // 默认选中推荐服务器或第一个服务器
            ServerInfo defaultServer = self.GetDefaultServer(zone.ServerList);
            if (defaultServer != null)
            {
                self.SelectServer(defaultServer);
            }
        }
        
        /// <summary>
        /// 更新区组选中状态
        /// </summary>
        private static void UpdateZoneSelection(this UIServerSelectComponent self, int selectedZoneId)
        {
            foreach (GameObject zoneItem in self.zoneItems)
            {
                if (zoneItem.name.StartsWith("ZoneItem_"))
                {
                    string zoneIdStr = zoneItem.name.Substring("ZoneItem_".Length);
                    if (int.TryParse(zoneIdStr, out int zoneId))
                    {
                        bool isSelected = zoneId == selectedZoneId;
                        
                        // 更新选中状态显示
                        Image background = zoneItem.GetComponent<Image>();
                        if (background != null)
                        {
                            // 选中时显示蓝色背景，未选中时显示白色背景
                            background.color = isSelected ? new Color(0.2f, 0.6f, 1f, 0.8f) : Color.white;
                        }
                        
                        // 更新文本颜色
                        Text nameText = zoneItem.GetComponentInChildren<Text>();
                        if (nameText != null)
                        {
                            nameText.color = isSelected ? Color.white : Color.black;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 创建服务器列表
        /// </summary>
        private static void CreateServerList(this UIServerSelectComponent self, List<ServerInfo> serverList)
        {
            self.ClearServerItems();
            
            foreach (ServerInfo server in serverList)
            {
                GameObject serverItem = self.CreateServerItem(server);
                if (serverItem != null)
                {
                    self.serverItems.Add(serverItem);
                }
            }
        }
        
        /// <summary>
        /// 创建服务器项
        /// </summary>
        private static GameObject CreateServerItem(this UIServerSelectComponent self, ServerInfo server)
        {
            if (self.serverItemTemplate == null || self.serverListParent == null)
            {
                Log.Error("UIServerSelectSystem: 创建服务器项失败 - 模板或父对象为空");
                return null;
            }
            
            GameObject serverItem = GameObject.Instantiate(self.serverItemTemplate, self.serverListParent.transform);
            serverItem.SetActive(true);
            serverItem.name = $"ServerItem_{server.ServerId}";
            
            // 配置RectTransform以适应LayoutGroup
            self.ConfigureItemRectTransform(serverItem);
            
            // 设置服务器信息
            self.UpdateServerItemUI(serverItem, server);
            
            // 绑定点击事件 - 点击服务器直接选择并关闭界面
            // 为了避免异步调用中UI组件被销毁的问题，我们预先获取必要的信息
            self.SetupButtonClick(serverItem, server.ServerName, () => self.HandleServerSelection(server));
            
            // 保存服务器ID在名字中
            serverItem.name = $"ServerItem_{server.ServerId}";
            
            return serverItem;
        }
        
        /// <summary>
        /// 更新服务器项UI
        /// </summary>
        private static void UpdateServerItemUI(this UIServerSelectComponent self, GameObject serverItem, ServerInfo server)
        {
            // 服务器名称
            self.SetServerText(serverItem, server.ServerName, new[] { "ServerName", "Name", "Text", "NameText" });
            
            // 服务器状态
            string statusText = self.GetStatusText(server.Status);
            Color statusColor = self.GetStatusColor(server.Status);
            Text statusTextComp = self.FindAndSetText(serverItem, statusText, new[] { "ServerStatus", "Status" });
            if (statusTextComp != null)
            {
                statusTextComp.color = statusColor;
            }
            
            // 在线人数
            string onlineText = $"{server.OnlineCount}/{server.MaxCount}";
            self.FindAndSetText(serverItem, onlineText, new[] { "OnlineCount", "Count", "Player" });
            
            // 推荐标签
            self.SetTagVisibility(serverItem, server.IsRecommend, new[] { "RecommendTag", "Recommend" });
            
            // 新服标签  
            self.SetTagVisibility(serverItem, server.IsNew, new[] { "NewTag", "New" });
        }
        
        /// <summary>
        /// 设置服务器文本的通用方法
        /// </summary>
        private static void SetServerText(this UIServerSelectComponent self, GameObject serverItem, string text, string[] possibleNames)
        {
            self.FindAndSetText(serverItem, text, possibleNames);
        }
        
        /// <summary>
        /// 查找并设置文本
        /// </summary>
        private static Text FindAndSetText(this UIServerSelectComponent self, GameObject parent, string text, string[] possibleNames)
        {
            // 方式1：直接查找Text组件
            Text directText = parent.GetComponent<Text>();
            if (directText != null)
            {
                directText.text = text;
                return directText;
            }
            
            // 方式2：通过名称查找子对象
            foreach (string name in possibleNames)
            {
                Transform childTransform = parent.transform.Find(name);
                if (childTransform != null)
                {
                    Text childText = childTransform.GetComponent<Text>();
                    if (childText != null)
                    {
                        childText.text = text;
                        return childText;
                    }
                }
            }
            
            // 方式3：在所有子对象中查找包含关键词的
            Text[] allTexts = parent.GetComponentsInChildren<Text>();
            foreach (Text textComp in allTexts)
            {
                foreach (string name in possibleNames)
                {
                    if (textComp.name.ToLower().Contains(name.ToLower()))
                    {
                        textComp.text = text;
                        return textComp;
                    }
                }
            }
            
            // 方式4：如果是服务器名称，优先使用第一个Text组件
            bool isServerName = possibleNames.Contains("ServerName") || possibleNames.Contains("Name");
            if (isServerName && allTexts.Length > 0)
            {
                allTexts[0].text = text;
                return allTexts[0];
            }
            
            // 方式5：创建新的Text组件
            if (allTexts.Length == 0)
            {
                GameObject textObj = new GameObject("AutoCreatedText");
                textObj.transform.SetParent(parent.transform, false);
                
                Text newText = textObj.AddComponent<Text>();
                newText.text = text;
                newText.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                newText.fontSize = 14;
                newText.color = Color.black;
                newText.alignment = TextAnchor.MiddleCenter;
                
                // 配置RectTransform
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                return newText;
            }
            
            return null;
        }
        
        /// <summary>
        /// 设置标签可见性
        /// </summary>
        private static void SetTagVisibility(this UIServerSelectComponent self, GameObject parent, bool visible, string[] possibleNames)
        {
            foreach (string name in possibleNames)
            {
                Transform tagTransform = parent.transform.Find(name);
                if (tagTransform != null)
                {
                    tagTransform.gameObject.SetActive(visible);
                    return;
                }
            }
        }
        
        /// <summary>
        /// 选择服务器
        /// </summary>
        private static void SelectServer(this UIServerSelectComponent self, ServerInfo server)
        {
            self.selectedServer = server;
            
            // 更新服务器选中状态
            self.UpdateServerSelection(server.ServerId);
        }

        /// <summary>
        /// 确认选择服务器（发送网络请求）
        /// </summary>
        public static async ETTask ConfirmSelectServer(this UIServerSelectComponent self)
        {
            ServerInfo selectedServer = self.selectedServer;
            if (selectedServer == null)
            {
                Log.Error("No server selected");
                return;
            }

            Scene scene = self.Scene();
            ServerListComponent serverListComp = scene.GetComponent<ServerListComponent>();
            
            if (serverListComp == null)
            {
                Log.Error("ServerListComponent not found");
                return;
            }

            // 调用服务器选择接口
            var result = await serverListComp.SelectServer(selectedServer.ServerId);
            
            if (!result.success)
            {
                Log.Error($"Failed to select server: {selectedServer.ServerName}");
                return;
            }

            Log.Info($"Successfully selected server: {selectedServer.ServerName}, address: {result.address}");
            
            // 这里可以继续连接到选定的服务器
            // 例如：连接到Game服务器，进入游戏等
            
            // 关闭服务器选择界面
            await UIHelper.Remove(scene, UIType.UIServerSelect);
        }
        
        /// <summary>
        /// 更新服务器选中状态
        /// </summary>
        private static void UpdateServerSelection(this UIServerSelectComponent self, int selectedServerId)
        {
            foreach (GameObject serverItem in self.serverItems)
            {
                if (serverItem.name.StartsWith("ServerItem_"))
                {
                    string serverIdStr = serverItem.name.Substring("ServerItem_".Length);
                    if (int.TryParse(serverIdStr, out int serverId))
                    {
                        bool isSelected = serverId == selectedServerId;
                        
                        // 更新选中状态显示
                        Image background = serverItem.GetComponent<Image>();
                        if (background != null)
                        {
                            background.color = isSelected ? new Color(1f, 0.8f, 0.2f, 0.8f) : Color.white;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取默认服务器
        /// </summary>
        private static ServerInfo GetDefaultServer(this UIServerSelectComponent self, List<ServerInfo> serverList)
        {
            // 优先返回推荐服务器
            foreach (ServerInfo server in serverList)
            {
                if (server.IsRecommend && server.Status != ServerStatus.Maintenance)
                {
                    return server;
                }
            }
            
            // 其次返回新服务器
            foreach (ServerInfo server in serverList)
            {
                if (server.IsNew && server.Status != ServerStatus.Maintenance)
                {
                    return server;
                }
            }
            
            // 最后返回第一个可用服务器
            foreach (ServerInfo server in serverList)
            {
                if (server.Status != ServerStatus.Maintenance)
                {
                    return server;
                }
            }
            
            return serverList.Count > 0 ? serverList[0] : null;
        }
        
        /// <summary>
        /// 获取状态文本
        /// </summary>
        private static string GetStatusText(this UIServerSelectComponent self, ServerStatus status)
        {
            return status switch
            {
                ServerStatus.Maintenance => "维护中",
                ServerStatus.Smooth => "流畅",
                ServerStatus.Normal => "良好",
                ServerStatus.Crowded => "拥挤",
                ServerStatus.Full => "爆满",
                _ => "未知"
            };
        }
        
        /// <summary>
        /// 获取状态颜色
        /// </summary>
        private static Color GetStatusColor(this UIServerSelectComponent self, ServerStatus status)
        {
            return status switch
            {
                ServerStatus.Maintenance => Color.gray,
                ServerStatus.Smooth => Color.green,
                ServerStatus.Normal => Color.yellow,
                ServerStatus.Crowded => new Color(1f, 0.5f, 0f),
                ServerStatus.Full => Color.red,
                _ => Color.white
            };
        }
        
        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private static void OnCloseBtnClick(this UIServerSelectComponent self)
        {
            UIHelper.Remove(self.Scene(), UIType.UIServerSelect).Coroutine();
        }
        
        /// <summary>
        /// 确认按钮点击
        /// </summary>
        private static async ETTask OnConfirmBtnClick(this UIServerSelectComponent self)
        {
            ServerInfo selectedServer = self.selectedServer;
            if (selectedServer == null)
            {
                Log.Warning("UIServerSelectSystem: 没有选中的服务器");
                return;
            }
            
            try
            {
                // 调用新的确认选择服务器方法（包含网络请求）
                await self.ConfirmSelectServer();
            }
            catch (System.Exception e)
            {
                Log.Error($"UIServerSelectSystem: 确认选择失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 清空区组项
        /// </summary>
        private static void ClearZoneItems(this UIServerSelectComponent self)
        {
            foreach (GameObject item in self.zoneItems)
            {
                if (item != null)
                {
                    GameObject.Destroy(item);
                }
            }
            self.zoneItems.Clear();
        }
        
        /// <summary>
        /// 清空服务器项
        /// </summary>
        private static void ClearServerItems(this UIServerSelectComponent self)
        {
            foreach (GameObject item in self.serverItems)
            {
                if (item != null)
                {
                    GameObject.Destroy(item);
                }
            }
            self.serverItems.Clear();
        }
        
        /// <summary>
        /// 清空所有项
        /// </summary>
        private static void ClearItems(this UIServerSelectComponent self)
        {
            self.ClearZoneItems();
            self.ClearServerItems();
        }
    }
}
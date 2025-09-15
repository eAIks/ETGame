using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ET.Client
{
    [UIEvent(UIType.UIServerSelect)]
    public class UIServerSelectEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            Log.Info("UIServerSelectEvent: OnCreate方法被调用");
            Log.Info($"UIServerSelectEvent: uiLayer = {uiLayer}");
            
            try
            {
                Log.Info("UIServerSelectEvent: 进入try块");
                string assetsName = $"Assets/Bundles/UI/Demo/{UIType.UIServerSelect}.prefab";
                Log.Info($"UIServerSelectEvent: 尝试加载预制体: {assetsName}");
                
                GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
                if (bundleGameObject == null)
                {
                    Log.Error($"UIServerSelectEvent: 预制体加载失败: {assetsName}");
                    
                    // 临时创建一个简单的UI用于测试
                    bundleGameObject = CreateTempServerSelectUI();
                }
                
                Transform parentLayer = uiComponent.UIGlobalComponent.GetLayer((int)uiLayer);
                Log.Info($"UIServerSelectEvent: 父级Layer: {parentLayer?.name}");
                
                GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, parentLayer);
                Log.Info($"UIServerSelectEvent: GameObject创建成功: {gameObject.name}");
                
                UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UIServerSelect, gameObject);
                
                Log.Info("UIServerSelectEvent: 准备添加UIServerSelectComponent组件");
                UIServerSelectComponent component = ui.AddComponent<UIServerSelectComponent>();
                
                if (component != null)
                {
                    Log.Info("UIServerSelectEvent: UIServerSelectComponent组件添加成功");
                }
                else
                {
                    Log.Error("UIServerSelectEvent: UIServerSelectComponent组件添加失败");
                }
                
                Log.Info("UIServerSelectEvent: UI创建成功");
                return ui;
            }
            catch (System.Exception e)
            {
                Log.Error($"UIServerSelectEvent: 创建UI失败: {e.Message}");
                Log.Error($"异常堆栈: {e.StackTrace}");
                throw;
            }
        }

        public override void OnRemove(UIComponent uiComponent)
        {
            Log.Info("UIServerSelectEvent: UI移除");
        }
        
        /// <summary>
        /// 创建临时的服务器选择UI（用于测试）
        /// </summary>
        private GameObject CreateTempServerSelectUI()
        {
            Log.Info("UIServerSelectEvent: 创建临时服务器选择UI");
            
            // 创建主Canvas
            GameObject canvasObj = new GameObject("UIServerSelect_Temp");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // 确保在其他UI之上
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // 检查是否已存在EventSystem，避免重复创建
            if (UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            // 创建背景
            GameObject backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(canvasObj.transform, false);
            
            Image backgroundImg = backgroundObj.AddComponent<Image>();
            backgroundImg.color = new Color(0, 0, 0, 0.8f);
            
            RectTransform backgroundRect = backgroundObj.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            
            // 创建主面板
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            Image panelImg = panelObj.AddComponent<Image>();
            panelImg.color = new Color(0.2f, 0.2f, 0.3f, 0.95f);
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // 创建标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "选择服务器";
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 32;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.85f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // 创建左侧区组列表区域
            GameObject zoneAreaObj = new GameObject("ZoneArea");
            zoneAreaObj.transform.SetParent(panelObj.transform, false);
            
            Image zoneAreaImg = zoneAreaObj.AddComponent<Image>();
            zoneAreaImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            
            RectTransform zoneAreaRect = zoneAreaObj.GetComponent<RectTransform>();
            zoneAreaRect.anchorMin = new Vector2(0.02f, 0.1f);
            zoneAreaRect.anchorMax = new Vector2(0.28f, 0.8f);
            zoneAreaRect.offsetMin = Vector2.zero;
            zoneAreaRect.offsetMax = Vector2.zero;
            
            // 创建区组列表标题
            GameObject zoneTitle = new GameObject("ZoneTitle");
            zoneTitle.transform.SetParent(zoneAreaObj.transform, false);
            
            Text zoneTitleText = zoneTitle.AddComponent<Text>();
            zoneTitleText.text = "服务器区组";
            zoneTitleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            zoneTitleText.fontSize = 20;
            zoneTitleText.color = Color.yellow;
            zoneTitleText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform zoneTitleRect = zoneTitle.GetComponent<RectTransform>();
            zoneTitleRect.anchorMin = new Vector2(0, 0.9f);
            zoneTitleRect.anchorMax = new Vector2(1, 1);
            zoneTitleRect.offsetMin = Vector2.zero;
            zoneTitleRect.offsetMax = Vector2.zero;
            
            // 创建区组列表父对象
            GameObject zoneListParent = new GameObject("ZoneListParent");
            zoneListParent.transform.SetParent(zoneAreaObj.transform, false);
            
            RectTransform zoneListRect = zoneListParent.GetComponent<RectTransform>();
            zoneListRect.anchorMin = new Vector2(0, 0);
            zoneListRect.anchorMax = new Vector2(1, 0.9f);
            zoneListRect.offsetMin = Vector2.zero;
            zoneListRect.offsetMax = Vector2.zero;
            
            // 创建右侧服务器列表区域
            GameObject serverAreaObj = new GameObject("ServerArea");
            serverAreaObj.transform.SetParent(panelObj.transform, false);
            
            Image serverAreaImg = serverAreaObj.AddComponent<Image>();
            serverAreaImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            
            RectTransform serverAreaRect = serverAreaObj.GetComponent<RectTransform>();
            serverAreaRect.anchorMin = new Vector2(0.32f, 0.1f);
            serverAreaRect.anchorMax = new Vector2(0.98f, 0.8f);
            serverAreaRect.offsetMin = Vector2.zero;
            serverAreaRect.offsetMax = Vector2.zero;
            
            // 创建服务器列表标题
            GameObject serverTitle = new GameObject("ServerTitle");
            serverTitle.transform.SetParent(serverAreaObj.transform, false);
            
            Text serverTitleText = serverTitle.AddComponent<Text>();
            serverTitleText.text = "服务器列表";
            serverTitleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            serverTitleText.fontSize = 20;
            serverTitleText.color = Color.yellow;
            serverTitleText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform serverTitleRect = serverTitle.GetComponent<RectTransform>();
            serverTitleRect.anchorMin = new Vector2(0, 0.9f);
            serverTitleRect.anchorMax = new Vector2(1, 1);
            serverTitleRect.offsetMin = Vector2.zero;
            serverTitleRect.offsetMax = Vector2.zero;
            
            // 创建服务器列表父对象
            GameObject serverListParent = new GameObject("ServerListParent");
            serverListParent.transform.SetParent(serverAreaObj.transform, false);
            
            RectTransform serverListRect = serverListParent.GetComponent<RectTransform>();
            serverListRect.anchorMin = new Vector2(0, 0);
            serverListRect.anchorMax = new Vector2(1, 0.9f);
            serverListRect.offsetMin = Vector2.zero;
            serverListRect.offsetMax = Vector2.zero;
            
            // 创建按钮区域
            GameObject btnAreaObj = new GameObject("ButtonArea");
            btnAreaObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform btnAreaRect = btnAreaObj.GetComponent<RectTransform>();
            btnAreaRect.anchorMin = new Vector2(0, 0.02f);
            btnAreaRect.anchorMax = new Vector2(1, 0.08f);
            btnAreaRect.offsetMin = Vector2.zero;
            btnAreaRect.offsetMax = Vector2.zero;
            
            // 创建确认按钮
            GameObject confirmBtn = CreateButton("ConfirmBtn", "确认选择", btnAreaObj.transform, new Vector2(0.6f, 0), new Vector2(0.85f, 1));
            
            // 创建关闭按钮
            GameObject closeBtn = CreateButton("CloseBtn", "关闭", btnAreaObj.transform, new Vector2(0.15f, 0), new Vector2(0.4f, 1));
            
            // 创建模板项（隐藏）
            GameObject zoneItemTemplate = CreateZoneItemTemplate(zoneListParent.transform);
            GameObject serverItemTemplate = CreateServerItemTemplate(serverListParent.transform);
            
            // 添加ReferenceCollector组件
            ReferenceCollector rc = canvasObj.AddComponent<ReferenceCollector>();
            rc.data.Add(new ReferenceCollectorData() { key = "ZoneListParent", gameObject = zoneListParent });
            rc.data.Add(new ReferenceCollectorData() { key = "ServerListParent", gameObject = serverListParent });
            rc.data.Add(new ReferenceCollectorData() { key = "ZoneItemTemplate", gameObject = zoneItemTemplate });
            rc.data.Add(new ReferenceCollectorData() { key = "ServerItemTemplate", gameObject = serverItemTemplate });
            rc.data.Add(new ReferenceCollectorData() { key = "CloseBtn", gameObject = closeBtn });
            rc.data.Add(new ReferenceCollectorData() { key = "ConfirmBtn", gameObject = confirmBtn });
            
            Log.Info("UIServerSelectEvent: 临时UI创建完成");
            return canvasObj;
        }
        
        private GameObject CreateButton(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.4f, 0.8f, 1f);
            
            Button btn = btnObj.AddComponent<Button>();
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            // 按钮文字
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            
            Text btnText = btnTextObj.AddComponent<Text>();
            btnText.text = text;
            btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            btnText.fontSize = 18;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;
            
            return btnObj;
        }
        
        private GameObject CreateZoneItemTemplate(Transform parent)
        {
            GameObject itemObj = new GameObject("ZoneItemTemplate");
            itemObj.transform.SetParent(parent, false);
            itemObj.SetActive(false);
            
            Image itemImg = itemObj.AddComponent<Image>();
            itemImg.color = Color.white;
            
            Button itemBtn = itemObj.AddComponent<Button>();
            
            RectTransform itemRect = itemObj.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 0);
            itemRect.anchorMax = new Vector2(1, 0);
            itemRect.sizeDelta = new Vector2(0, 40);
            
            // 区组名称文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(itemObj.transform, false);
            
            Text itemText = textObj.AddComponent<Text>();
            itemText.text = "区组名称";
            itemText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            itemText.fontSize = 16;
            itemText.color = Color.black;
            itemText.alignment = TextAnchor.MiddleLeft;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);
            
            return itemObj;
        }
        
        private GameObject CreateServerItemTemplate(Transform parent)
        {
            GameObject itemObj = new GameObject("ServerItemTemplate");
            itemObj.transform.SetParent(parent, false);
            itemObj.SetActive(false);
            
            Image itemImg = itemObj.AddComponent<Image>();
            itemImg.color = Color.white;
            
            Button itemBtn = itemObj.AddComponent<Button>();
            
            RectTransform itemRect = itemObj.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 0);
            itemRect.anchorMax = new Vector2(1, 0);
            itemRect.sizeDelta = new Vector2(0, 60);
            
            // 服务器名称
            CreateServerItemText(itemObj.transform, "ServerName", "服务器名称", new Vector2(0, 0.5f), new Vector2(0.5f, 1), 16, Color.black);
            // 服务器状态
            CreateServerItemText(itemObj.transform, "ServerStatus", "良好", new Vector2(0.5f, 0.5f), new Vector2(0.75f, 1), 14, Color.green);
            // 在线人数
            CreateServerItemText(itemObj.transform, "OnlineCount", "100/2000", new Vector2(0.75f, 0.5f), new Vector2(1f, 1), 12, Color.gray);
            
            // 推荐标签
            GameObject recommendTag = new GameObject("RecommendTag");
            recommendTag.transform.SetParent(itemObj.transform, false);
            recommendTag.SetActive(false);
            
            Text recommendText = recommendTag.AddComponent<Text>();
            recommendText.text = "推荐";
            recommendText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            recommendText.fontSize = 12;
            recommendText.color = Color.red;
            recommendText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform recommendRect = recommendTag.GetComponent<RectTransform>();
            recommendRect.anchorMin = new Vector2(0, 0);
            recommendRect.anchorMax = new Vector2(0.2f, 0.5f);
            recommendRect.offsetMin = Vector2.zero;
            recommendRect.offsetMax = Vector2.zero;
            
            // 新服标签
            GameObject newTag = new GameObject("NewTag");
            newTag.transform.SetParent(itemObj.transform, false);
            newTag.SetActive(false);
            
            Text newText = newTag.AddComponent<Text>();
            newText.text = "新服";
            newText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            newText.fontSize = 12;
            newText.color = Color.yellow;
            newText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform newRect = newTag.GetComponent<RectTransform>();
            newRect.anchorMin = new Vector2(0.2f, 0);
            newRect.anchorMax = new Vector2(0.4f, 0.5f);
            newRect.offsetMin = Vector2.zero;
            newRect.offsetMax = Vector2.zero;
            
            return itemObj;
        }
        
        private void CreateServerItemText(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            
            Text itemText = textObj.AddComponent<Text>();
            itemText.text = text;
            itemText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            itemText.fontSize = fontSize;
            itemText.color = color;
            itemText.alignment = TextAnchor.MiddleLeft;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);
        }
    }
}
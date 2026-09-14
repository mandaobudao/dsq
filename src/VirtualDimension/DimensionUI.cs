using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace VirtualDimension
{
    /// <summary>
    /// Alt+5 IMGUI window: browse the shared dimension inventory and configure
    /// each item's cap in the 0..10,000,000 range.
    /// </summary>
    public static class DimensionUI
    {
        public static bool Visible;
        public static bool TextFieldFocused;

        private const int WindowId = 901005;
        private const float GripSize = 20f;
        private const float TitleBarHeight = 24f;
        private static Rect _windowRect;
        private static bool _rectInited;
        private static bool _resizing;
        private static Vector2 _resizeStart;
        private static Rect _resizeStartRect;

        private static Vector2 _scroll;
        private static string _search = string.Empty;
        private static readonly Dictionary<int, string> _buffers = new Dictionary<int, string>();
        private static string _focusedControl = string.Empty;

        private static GUIStyle _winStyle;
        private static GUIStyle _headerStyle;
        private static GUIStyle _normalStyle;
        private static GUIStyle _mutedStyle;
        private static GUIStyle _countStyle;
        private static GUIStyle _gripStyle;
        private static Texture2D _bg;

        public static void Toggle()
        {
            Visible = !Visible;
            if (Visible && !_rectInited)
            {
                float w = VDMod.WindowWidthEntry != null ? VDMod.WindowWidthEntry.Value : VDMod.DEFAULT_WINDOW_WIDTH;
                float h = VDMod.WindowHeightEntry != null ? VDMod.WindowHeightEntry.Value : VDMod.DEFAULT_WINDOW_HEIGHT;
                w = Mathf.Clamp(w, 620f, Screen.width - 40f);
                h = Mathf.Clamp(h, 380f, Screen.height - 40f);
                _windowRect = new Rect(
                    Mathf.Max(20f, (Screen.width - w) * 0.5f),
                    Mathf.Max(20f, (Screen.height - h) * 0.5f),
                    w, h);
                _rectInited = true;
            }
        }

        public static void OnGUI()
        {
            if (!Visible)
                return;
            InitStyles();
            // Window follows _windowRect exactly; the bottom-right grip resizes it.
            GUILayout.Window(WindowId, _windowRect, WindowFunc, string.Empty, _winStyle,
                GUILayout.Width(_windowRect.width), GUILayout.Height(_windowRect.height));
        }

        private static void InitStyles()
        {
            if (_winStyle != null)
                return;

            Font font = Font.CreateDynamicFontFromOSFont(
                new[] { "Microsoft YaHei", "微软雅黑", "SimHei", "黑体", "Arial Unicode MS", "Arial" }, 14);

            _winStyle = new GUIStyle(GUI.skin.window) { font = font };
            _winStyle.normal.background = MakeTex(new Color(0.09f, 0.06f, 0.13f, 0.98f));
            _winStyle.padding = new RectOffset(12, 12, 10, 12);
            _winStyle.fontSize = 14;

            _headerStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 18, fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.85f, 0.72f, 1f) } };
            _normalStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 13,
                normal = { textColor = Color.white } };
            _mutedStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 12,
                normal = { textColor = new Color(0.70f, 0.66f, 0.78f) } };
            _countStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 13, alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(0.82f, 0.95f, 1f) } };
        }

        private static Texture2D MakeTex(Color color)
        {
            if (_bg == null)
            {
                _bg = new Texture2D(1, 1);
                _bg.SetPixel(0, 0, color);
                _bg.Apply();
            }
            return _bg;
        }

        private static void WindowFunc(int id)
        {
            DimensionStorage dim = DimensionStorage.Instance;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("虚拟维度空间（全星系共享存储）", _headerStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("关闭 [Esc]", GUILayout.Width(90f)))
                Visible = false;
            GUILayout.EndHorizontal();

            int kinds = 0;
            long total = 0;
            foreach (KeyValuePair<int, int> kv in dim.Counts)
            {
                if (kv.Value > 0)
                {
                    kinds++;
                    total += kv.Value;
                }
            }
            GUILayout.Label($"存储物品种类：{kinds}    物品总量：{total:N0}    每种上限范围：0 ~ {VDMod.HARD_MAX_LIMIT:N0}（建筑默认 {VDMod.DEFAULT_BUILDING_LIMIT}，物品默认 {VDMod.DEFAULT_ITEM_LIMIT:N0}）",
                _mutedStyle);

            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("搜索：", _normalStyle, GUILayout.Width(48f));
            GUI.SetNextControlName("vd_search");
            _search = GUILayout.TextField(_search, GUILayout.Width(300f));
            GUILayout.Label("输入物品名或ID；留空显示已存储/已设置的物品", _mutedStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);

            _focusedControl = GUI.GetNameOfFocusedControl();
            TextFieldFocused = _focusedControl.StartsWith("vd_", StringComparison.Ordinal);

            // Scrollview fills whatever the user resized the window to.
            float scrollHeight = Mathf.Max(120f, _windowRect.height - TitleBarHeight - 176f);
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(scrollHeight));

            List<int> rows = BuildRows(_search);
            foreach (int itemId in rows)
                DrawRow(dim, itemId);

            if (rows.Count == 0)
                GUILayout.Label("（没有匹配的物品）", _mutedStyle);

            GUILayout.EndScrollView();

            GUILayout.Label("提示：星际供应槽把超出上限一半的部分存入维度（只出不进）；只有设置为星际需求的站点才从维度收货（补至上限一半）。本地小飞机照常工作，可与星际模式组合。跨星球无需飞船。",
                _mutedStyle);

            GUILayout.EndVertical();

            DrawResizeGrip();

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Visible = false;
                Event.current.Use();
            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, TitleBarHeight));
        }

        /// <summary>Bottom-right drag handle; size is persisted to the config on release.</summary>
        private static void DrawResizeGrip()
        {
            Rect grip = new Rect(_windowRect.width - GripSize - 6f,
                _windowRect.height - TitleBarHeight - GripSize - 4f, GripSize, GripSize);
            if (_gripStyle == null)
            {
                _gripStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.62f, 0.52f, 0.80f) } };
            }
            GUI.Label(grip, "◢", _gripStyle);

            Event e = Event.current;
            if (e.type == EventType.MouseDown && grip.Contains(e.mousePosition))
            {
                _resizing = true;
                _resizeStart = e.mousePosition;
                _resizeStartRect = _windowRect;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _resizing)
            {
                Vector2 d = e.mousePosition - _resizeStart;
                _windowRect.width = Mathf.Clamp(_resizeStartRect.width + d.x, 620f, Screen.width - 40f);
                _windowRect.height = Mathf.Clamp(_resizeStartRect.height + d.y, 380f, Screen.height - 40f);
                e.Use();
            }
            else if (e.type == EventType.MouseUp && _resizing)
            {
                _resizing = false;
                if (VDMod.WindowWidthEntry != null)
                    VDMod.WindowWidthEntry.Value = _windowRect.width;
                if (VDMod.WindowHeightEntry != null)
                    VDMod.WindowHeightEntry.Value = _windowRect.height;
                e.Use();
            }
        }

        private static List<int> BuildRows(string search)
        {
            List<int> result = new List<int>();
            DimensionStorage dim = DimensionStorage.Instance;

            if (string.IsNullOrWhiteSpace(search))
            {
                HashSet<int> union = new HashSet<int>();
                foreach (int id in dim.Counts.Keys)
                    if (dim.GetCount(id) > 0)
                        union.Add(id);
                foreach (int id in dim.Limits.Keys)
                    union.Add(id);
                result.AddRange(union);
                result.Sort((a, b) =>
                {
                    int c = dim.GetCount(b).CompareTo(dim.GetCount(a));
                    return c != 0 ? c : a.CompareTo(b);
                });
                return result;
            }

            search = search.Trim();
            if (int.TryParse(search, NumberStyles.Integer, CultureInfo.InvariantCulture, out int byId))
            {
                if (LDB.items.Select(byId) != null)
                    result.Add(byId);
                return result;
            }

            foreach (ItemProto proto in LDB.items.dataArray)
            {
                if (proto == null || string.IsNullOrEmpty(proto.Name))
                    continue;
                if (proto.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    proto.ID.ToString(CultureInfo.InvariantCulture)
                        .IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    result.Add(proto.ID);
                if (result.Count >= 200)
                    break;
            }
            result.Sort();
            return result;
        }

        private static void DrawRow(DimensionStorage dim, int itemId)
        {
            ItemProto proto = LDB.items.Select(itemId);
            if (proto == null)
                return;

            int count = dim.GetCount(itemId);
            int limit = dim.GetLimit(itemId);
            bool custom = dim.Limits.ContainsKey(itemId);
            bool isBuilding = proto.IsEntity;

            if (!_buffers.TryGetValue(itemId, out string buf) || string.IsNullOrEmpty(buf))
                buf = _buffers[itemId] = limit.ToString(CultureInfo.InvariantCulture);
            bool editingThis = _focusedControl == "vd_limit_" + itemId;

            GUILayout.BeginHorizontal(GUILayout.Height(30f));

            StringBuilder nameSb = new StringBuilder(64);
            nameSb.Append(proto.Name);
            if (isBuilding)
                nameSb.Append(" [建筑]");
            if (custom)
                nameSb.Append(" [自定义]");
            GUILayout.Label(nameSb.ToString(), _normalStyle, GUILayout.Width(340f));

            GUILayout.Label(count.ToString("N0", CultureInfo.InvariantCulture), _countStyle, GUILayout.Width(130f));
            GUILayout.Label("/", _mutedStyle, GUILayout.Width(10f));
            GUILayout.Label(limit.ToString("N0", CultureInfo.InvariantCulture), _normalStyle, GUILayout.Width(110f));

            GUI.SetNextControlName("vd_limit_" + itemId);
            buf = GUILayout.TextField(buf, GUILayout.Width(80f));
            _buffers[itemId] = buf;
            if (GUILayout.Button("应用", GUILayout.Width(46f)))
                ApplyLimit(dim, itemId, buf);

            float nv = GUILayout.HorizontalSlider(limit, 0f, VDMod.HARD_MAX_LIMIT,
                GUILayout.Width(170f));
            if (Math.Abs(nv - limit) > 0.5f)
            {
                int newLimit = Mathf.RoundToInt(nv);
                dim.SetLimit(itemId, newLimit);
                if (!editingThis)
                    _buffers[itemId] = newLimit.ToString(CultureInfo.InvariantCulture);
            }

            if (GUILayout.Button("0", GUILayout.Width(34f)))
                SetAndSync(dim, itemId, 0);
            if (GUILayout.Button("50", GUILayout.Width(34f)))
                SetAndSync(dim, itemId, VDMod.DEFAULT_BUILDING_LIMIT);
            if (GUILayout.Button("200万", GUILayout.Width(48f)))
                SetAndSync(dim, itemId, VDMod.DEFAULT_ITEM_LIMIT);
            if (GUILayout.Button("1000万", GUILayout.Width(52f)))
                SetAndSync(dim, itemId, VDMod.HARD_MAX_LIMIT);
            if (GUILayout.Button("默认", GUILayout.Width(44f)))
            {
                dim.ResetLimit(itemId);
                _buffers[itemId] = dim.GetLimit(itemId).ToString(CultureInfo.InvariantCulture);
            }

            GUILayout.EndHorizontal();
        }

        private static void SetAndSync(DimensionStorage dim, int itemId, int value)
        {
            dim.SetLimit(itemId, value);
            _buffers[itemId] = value.ToString(CultureInfo.InvariantCulture);
        }

        private static void ApplyLimit(DimensionStorage dim, int itemId, string text)
        {
            if (TryParseLimit(text, out int v))
                SetAndSync(dim, itemId, v);
        }

        /// <summary>Parses plain integers and shorthand like "2w"/"2W" (= 20000).</summary>
        private static bool TryParseLimit(string text, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;
            text = text.Trim().Replace(",", string.Empty).Replace(" ", string.Empty);
            int mult = 1;
            char last = text[text.Length - 1];
            if (last == 'w' || last == 'W' || last == '万')
            {
                mult = 10000;
                text = text.Substring(0, text.Length - 1);
            }
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double raw))
                return false;
            value = Mathf.Clamp((int)Math.Round(raw * mult), 0, VDMod.HARD_MAX_LIMIT);
            return true;
        }
    }
}

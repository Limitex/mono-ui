
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

# region Enums

enum TimelineType
{
    None = 0,
    Enter = 1,
    Leave = 2,
}

enum UserlineType
{
    None = 0,
    Online = 1,
    Offline = 2,
}

# endregion

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WorldLogManager : UdonSharpBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private ScrollRect timelineScrollRect;
    [SerializeField] private ScrollRect userlineScrollRect;

    [Header("View Containers")]
    [SerializeField] private Transform timelineContainer;
    [SerializeField] private Transform userlineContainer;

    [Header("Item Templates")]
    [SerializeField] private GameObject timelineItemTemplate;
    [SerializeField] private GameObject userlineItemTemplate;

    [Header("Statistics")]
    [SerializeField] private TMP_Text totalUsersText;
    [SerializeField] private TMP_Text onlineUsersText;
    [SerializeField] private TMP_Text totalActivityText;
    [SerializeField] private TMP_Text onlineCountText;

    #region Constant Fields

    private const string ENTER_TEXT = "Entered the room";
    private const string LEAVE_TEXT = "Left the room";
    private const string ONLINE = "In Instance";
    private const string OFFLINE = "Not in Instance";
    private const string ONLINE_SUFFIX = "{0} online";
    private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";

    private readonly Color ONLINE_COLOR = new Color(r: 0x2D / 255f, g: 0xB7 / 255f, b: 0x89 / 255f);
    private readonly Color OFFLINE_COLOR = new Color(r: 0xE2 / 255f, g: 0x36 / 255f, b: 0x6F / 255f);
    private readonly Color FOREGROUND_COLOR = new Color(r: 0xF9 / 255f, g: 0xF9 / 255f, b: 0xF9 / 255f);
    private readonly Color MUTED_COLOR = new Color(r: 0xA1 / 255f, g: 0xA1 / 255f, b: 0xA9 / 255f);

    private readonly int[] TIMELINE_NAME_PATH = new int[] { 0, 1, 0, 0 };
    private readonly int[] TIMELINE_TIME_PATH = new int[] { 0, 1, 0, 1 };
    private readonly int[] TIMELINE_TYPE_PATH = new int[] { 0, 1, 1, 1 };
    private readonly int[] TIMELINE_ICON_PATH = new int[] { 0, 1, 1, 0 };
    private readonly int[] USERLINE_NAME_PATH = new int[] { 1, 0 };
    private readonly int[] USERLINE_TYPE_PATH = new int[] { 1, 1 };
    private readonly int[] USERLINE_ICON_PATH = new int[] { 0, 0, 0 };

    #endregion

    #region UdonSynced Fields

    [UdonSynced(UdonSyncMode.None)] private byte[] bytes;

    #endregion

    #region Fields

    private int totalUsers = 0;

    #endregion

    #region Timeline Struct Fields

    private string[] struct_timeline_name = new string[0];
    private DateTime[] struct_timeline_time = new DateTime[0];
    private TimelineType[] struct_timeline_type = new TimelineType[0];
    private Transform[] struct_timeline_transform = new Transform[0];
    private TextMeshProUGUI[] struct_timeline_name_tmp = new TextMeshProUGUI[0];
    private TextMeshProUGUI[] struct_timeline_time_tmp = new TextMeshProUGUI[0];
    private TextMeshProUGUI[] struct_timeline_type_tmp = new TextMeshProUGUI[0];
    private Image[] struct_timeline_icon = new Image[0];

    private void AddTimelineStructureItem(string name, DateTime time, TimelineType type, Transform transform, TextMeshProUGUI nameTmp, TextMeshProUGUI timeTmp, TextMeshProUGUI typeTmp, Image icon)
    {
        int oldLength = struct_timeline_name.Length;
        int newLength = oldLength + 1;

        string[] newNames = new string[newLength];
        DateTime[] newTimes = new DateTime[newLength];
        TimelineType[] newTypes = new TimelineType[newLength];
        Transform[] newTransforms = new Transform[newLength];
        TextMeshProUGUI[] newNameTmps = new TextMeshProUGUI[newLength];
        TextMeshProUGUI[] newTimeTmps = new TextMeshProUGUI[newLength];
        TextMeshProUGUI[] newTypeTmps = new TextMeshProUGUI[newLength];
        Image[] newIcons = new Image[newLength];

        for (int i = 0; i < oldLength; i++)
        {
            newNames[i] = struct_timeline_name[i];
            newTimes[i] = struct_timeline_time[i];
            newTypes[i] = struct_timeline_type[i];
            newTransforms[i] = struct_timeline_transform[i];
            newNameTmps[i] = struct_timeline_name_tmp[i];
            newTimeTmps[i] = struct_timeline_time_tmp[i];
            newTypeTmps[i] = struct_timeline_type_tmp[i];
            newIcons[i] = struct_timeline_icon[i];
        }

        newNames[oldLength] = name;
        newTimes[oldLength] = time;
        newTypes[oldLength] = type;
        newTransforms[oldLength] = transform;
        newNameTmps[oldLength] = nameTmp;
        newTimeTmps[oldLength] = timeTmp;
        newTypeTmps[oldLength] = typeTmp;
        newIcons[oldLength] = icon;

        struct_timeline_name = newNames;
        struct_timeline_time = newTimes;
        struct_timeline_type = newTypes;
        struct_timeline_transform = newTransforms;
        struct_timeline_name_tmp = newNameTmps;
        struct_timeline_time_tmp = newTimeTmps;
        struct_timeline_type_tmp = newTypeTmps;
        struct_timeline_icon = newIcons;
    }

    #endregion

    #region Userline Struct Fields

    private string[] struct_userline_name = new string[0];
    private UserlineType[] struct_userline_type = new UserlineType[0];
    private Transform[] struct_userline_transform = new Transform[0];
    private TextMeshProUGUI[] struct_userline_name_tmp = new TextMeshProUGUI[0];
    private TextMeshProUGUI[] struct_userline_type_tmp = new TextMeshProUGUI[0];
    private Image[] struct_userline_icon = new Image[0];

    private void AddUserlineStructureItem(string name, UserlineType type, Transform transform, TextMeshProUGUI nameTmp, TextMeshProUGUI typeTmp, Image icon)
    {
        int oldLength = struct_userline_name.Length;
        int newLength = oldLength + 1;

        string[] newNames = new string[newLength];
        UserlineType[] newTypes = new UserlineType[newLength];
        Transform[] newTransforms = new Transform[newLength];
        TextMeshProUGUI[] newNameTmps = new TextMeshProUGUI[newLength];
        TextMeshProUGUI[] newTypeTmps = new TextMeshProUGUI[newLength];
        Image[] newIcons = new Image[newLength];

        for (int i = 0; i < oldLength; i++)
        {
            newNames[i] = struct_userline_name[i];
            newTypes[i] = struct_userline_type[i];
            newTransforms[i] = struct_userline_transform[i];
            newNameTmps[i] = struct_userline_name_tmp[i];
            newTypeTmps[i] = struct_userline_type_tmp[i];
            newIcons[i] = struct_userline_icon[i];
        }

        newNames[oldLength] = name;
        newTypes[oldLength] = type;
        newTransforms[oldLength] = transform;
        newNameTmps[oldLength] = nameTmp;
        newTypeTmps[oldLength] = typeTmp;
        newIcons[oldLength] = icon;

        struct_userline_name = newNames;
        struct_userline_type = newTypes;
        struct_userline_transform = newTransforms;
        struct_userline_name_tmp = newNameTmps;
        struct_userline_type_tmp = newTypeTmps;
        struct_userline_icon = newIcons;
    }

    private int FindUserLineStructureIndex(string name)
    {
        for (int i = 0; i < struct_userline_name.Length; i++)
        {
            if (struct_userline_name[i] == name)
            {
                return i;
            }
        }
        return -1;
    }

    #endregion

    #region Hierarchy Path Getters

    private Transform GetComponentByHierarchyPath(Transform parent, int[] hierarchyPath)
    {
        if (parent == null || hierarchyPath == null) return null;

        Transform current = parent;
        int pathLength = hierarchyPath.Length;

        for (int i = 0; i < pathLength && current != null; i++)
        {
            current = current.GetChild(hierarchyPath[i]);
        }
        return current;
    }

    private TextMeshProUGUI GetTimelineNameText(Transform transform) =>
        GetComponentByHierarchyPath(transform, TIMELINE_NAME_PATH).GetComponent<TextMeshProUGUI>();

    private TextMeshProUGUI GetTimelineTimeText(Transform transform) =>
        GetComponentByHierarchyPath(transform, TIMELINE_TIME_PATH).GetComponent<TextMeshProUGUI>();

    private TextMeshProUGUI GetTimelineTypeText(Transform transform) =>
        GetComponentByHierarchyPath(transform, TIMELINE_TYPE_PATH).GetComponent<TextMeshProUGUI>();

    private Image GetTimelineIcon(Transform transform) =>
        GetComponentByHierarchyPath(transform, TIMELINE_ICON_PATH).GetComponent<Image>();

    private TextMeshProUGUI GetUserlineNameText(Transform transform) =>
        GetComponentByHierarchyPath(transform, USERLINE_NAME_PATH).GetComponent<TextMeshProUGUI>();

    private TextMeshProUGUI GetUserlineTypeText(Transform transform) =>
        GetComponentByHierarchyPath(transform, USERLINE_TYPE_PATH).GetComponent<TextMeshProUGUI>();

    private Image GetUserlineIcon(Transform transform) =>
        GetComponentByHierarchyPath(transform, USERLINE_ICON_PATH).GetComponent<Image>();

    #endregion

    #region Enum Converters

    private string GetTimelineTypeString(TimelineType timelineType)
    {
        switch (timelineType)
        {
            case TimelineType.Enter:
                return ENTER_TEXT;
            case TimelineType.Leave:
                return LEAVE_TEXT;
            default:
                return string.Empty;
        }
    }

    private Color GetTimelineTypeColor(TimelineType timelineType)
    {
        switch (timelineType)
        {
            case TimelineType.Enter:
                return ONLINE_COLOR;
            case TimelineType.Leave:
                return OFFLINE_COLOR;
            default:
                return Color.white;
        }
    }

    private string GetUserlineTypeString(UserlineType userlineType)
    {
        switch (userlineType)
        {
            case UserlineType.Online:
                return ONLINE;
            case UserlineType.Offline:
                return OFFLINE;
            default:
                return string.Empty;
        }
    }

    private Color GetUserlineTypeColor(UserlineType userlineType)
    {
        switch (userlineType)
        {
            case UserlineType.Online:
                return ONLINE_COLOR;
            case UserlineType.Offline:
                return OFFLINE_COLOR;
            default:
                return Color.white;
        }
    }

    private Color GetUserlineTextColor(UserlineType userlineType)
    {
        switch (userlineType)
        {
            case UserlineType.Online:
                return FOREGROUND_COLOR;
            case UserlineType.Offline:
                return MUTED_COLOR;
            default:
                return Color.white;
        }
    }

    #endregion

    #region Syncing Functions

    private void SyncBytes()
    {
        string totalUsersText = totalUsers.ToString();
        string timelineData = ConvertTimelineStructToString();
        string userlineData = ConvertUserlineStructToString();
        string data = totalUsersText + "\v" + timelineData + "\v" + userlineData;
        bytes = System.Text.Encoding.UTF8.GetBytes(data);
    }

    private void LoadBytes()
    {
        if (bytes == null || bytes.Length == 0) return;
        string data = System.Text.Encoding.UTF8.GetString(bytes);
        string[] lines = data.Split('\v');
        totalUsers = int.Parse(lines[0]);
        LoadTimelineStructFromString(lines[1]);
        LoadUserlineStructFromString(lines[2]);

        int players = VRCPlayerApi.GetPlayerCount();
        UpdateTexts(totalUsers, players, struct_timeline_name.Length, players);
    }

    private string ConvertTimelineStructToString()
    {
        string result = string.Empty;
        for (int i = 0; i < struct_timeline_name.Length; i++)
        {
            result += struct_timeline_name[i] + "\t";
            result += struct_timeline_time[i].ToString(DATE_FORMAT) + "\t";
            result += (int)struct_timeline_type[i] + "\n";
        }
        return result;
    }

    private string ConvertUserlineStructToString()
    {
        string result = string.Empty;
        for (int i = 0; i < struct_userline_name.Length; i++)
        {
            result += struct_userline_name[i] + "\t";
            result += (int)struct_userline_type[i] + "\n";
        }
        return result;
    }

    private void LoadTimelineStructFromString(string data)
    {
        string[] lines = data.Split('\n');
        for (int i = struct_timeline_name.Length; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split('\t');
            if (parts.Length != 3) continue;
            string name = parts[0];
            DateTime time = DateTime.ParseExact(parts[1], DATE_FORMAT, null);
            TimelineType type = (TimelineType)int.Parse(parts[2]);

            AddTimelineItem(name, time, type);
        }
    }

    private void LoadUserlineStructFromString(string data)
    {
        string[] lines = data.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split('\t');
            if (parts.Length != 2) continue;
            string name = parts[0];
            UserlineType type = (UserlineType)int.Parse(parts[1]);

            if (i < struct_userline_name.Length)
            {
                UpdateUserlineInstance(i, type);
            }
            else
            {
                AddUserlineItem(name, type);
            }
        }
    }

    #endregion

    #region Time Management Functions

    private DateTime GetCurrentUtcTime()
    {
        return DateTime.UtcNow;
    }

    private string FormatLocalTime(DateTime utcTime)
    {
        DateTime localTime = utcTime.ToLocalTime();
        return localTime.ToString(DATE_FORMAT);
    }

    #endregion

    #region UI Management Functions

    private void AddTimelineItem(string name, DateTime time, TimelineType type)
    {
        GameObject item = Instantiate(timelineItemTemplate, timelineContainer);
        Transform transform = item.transform;
        TextMeshProUGUI nameTmp = GetTimelineNameText(transform);
        TextMeshProUGUI timeTmp = GetTimelineTimeText(transform);
        TextMeshProUGUI typeTmp = GetTimelineTypeText(transform);
        Image icon = GetTimelineIcon(transform);
        nameTmp.text = name;
        timeTmp.text = FormatLocalTime(time);
        typeTmp.text = GetTimelineTypeString(type);
        icon.color = GetTimelineTypeColor(type);
        AddTimelineStructureItem(name, time, type, transform, nameTmp, timeTmp, typeTmp, icon);
        item.SetActive(true);
        ScrollTimelineToBottom();
    }

    private void AddUserlineItem(string name, UserlineType type)
    {
        GameObject item = Instantiate(userlineItemTemplate, userlineContainer);
        Transform transform = item.transform;
        TextMeshProUGUI nameTmp = GetUserlineNameText(transform);
        TextMeshProUGUI typeTmp = GetUserlineTypeText(transform);
        Image icon = GetUserlineIcon(transform);
        nameTmp.text = name;
        typeTmp.text = GetUserlineTypeString(type);
        icon.color = GetUserlineTypeColor(type);
        AddUserlineStructureItem(name, type, transform, nameTmp, typeTmp, icon);
        item.SetActive(true);
        ScrollUserlineToBottom();
    }

    private void UpdateUserlineInstance(int index, UserlineType type)
    {
        struct_userline_type[index] = type;
        struct_userline_type_tmp[index].text = GetUserlineTypeString(type);
        struct_userline_name_tmp[index].color = GetUserlineTextColor(type);
        struct_userline_icon[index].color = GetUserlineTypeColor(type);
    }

    private void UpdateTexts(int totalUsers, int onlineUsers, int totalActivity, int onlines)
    {
        totalUsersText.text = totalUsers.ToString();
        onlineUsersText.text = onlineUsers.ToString();
        totalActivityText.text = totalActivity.ToString();
        onlineCountText.text = string.Format(ONLINE_SUFFIX, onlines);
    }

    private void ScrollTimelineToBottom()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)timelineScrollRect.transform);
        timelineScrollRect.normalizedPosition = new Vector2(timelineScrollRect.normalizedPosition.x, 0f);
    }

    private void ScrollUserlineToBottom()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)userlineScrollRect.transform);
        userlineScrollRect.normalizedPosition = new Vector2(userlineScrollRect.normalizedPosition.x, 0f);
    }

    #endregion

    #region Unity Callbacks

    void Start()
    {

    }

    #endregion

    #region Udon Callbacks (Sync)

    public override void OnPreSerialization()
    {
        SyncBytes();
    }

    public override void OnDeserialization()
    {
        LoadBytes();
    }

    #endregion

    #region Udon Callbacks (Player)

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!Networking.IsOwner(gameObject)) return;

        AddTimelineItem(player.displayName, GetCurrentUtcTime(), TimelineType.Enter);
        int userlineIndex = FindUserLineStructureIndex(player.displayName);
        if (userlineIndex == -1)
        {
            AddUserlineItem(player.displayName, UserlineType.Online);
            totalUsers++;
        }
        else
        {
            UpdateUserlineInstance(userlineIndex, UserlineType.Online);
        }

        int players = VRCPlayerApi.GetPlayerCount();
        UpdateTexts(totalUsers, players, struct_timeline_name.Length, players);

        RequestSerialization();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (!Networking.IsOwner(gameObject)) return;

        AddTimelineItem(player.displayName, GetCurrentUtcTime(), TimelineType.Leave);

        int userlineIndex = FindUserLineStructureIndex(player.displayName);
        if (userlineIndex != -1)
        {
            UpdateUserlineInstance(userlineIndex, UserlineType.Offline);
        }

        int players = VRCPlayerApi.GetPlayerCount() - 1;
        UpdateTexts(totalUsers, players, struct_timeline_name.Length, players);

        RequestSerialization();
    }

    #endregion
}

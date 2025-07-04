﻿
using System;
using System.Text;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Limitex.MonoUI.Udon
{
    #region Enums

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

    [AddComponentMenu("")]
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
        [SerializeField] private TMP_Text syncDataSizeText;

        [Header("Toast Settings")]
        [SerializeField] private TMP_Text toastText;
        [SerializeField] private TMP_Text toastDescriptionText;
        [SerializeField] private Animator Animator;
        [SerializeField] private string toastTriggerName = "MonoUI_Toast_isTrigger";

        [Header("Color Settings")]
        [SerializeField] private Color _onlineColor;
        [SerializeField] private Color _offlineColor;
        [SerializeField] private Color _foregroundColor;
        [SerializeField] private Color _mutedColor;

        #region Constant Fields

        private const string ENTER_TEXT = "Entered the room";
        private const string LEAVE_TEXT = "Left the room";
        private const string ONLINE = "In Instance";
        private const string OFFLINE = "Not in Instance";
        private const string ONLINE_SUFFIX = "{0} online";
        private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";

        private readonly Encoding DATA_ENCODING = Encoding.UTF8;

        private readonly int[] TIMELINE_NAME_PATH = new int[] { 0, 1, 0, 0 };
        private readonly int[] TIMELINE_TIME_PATH = new int[] { 0, 1, 0, 1 };
        private readonly int[] TIMELINE_TYPE_PATH = new int[] { 0, 1, 1, 1 };
        private readonly int[] TIMELINE_INFO_ICON_PATH = new int[] { 0, 1, 1, 0, 0 };
        private readonly int[] TIMELINE_ENTER_ICON_PATH = new int[] { 0, 1, 1, 0, 1 };
        private readonly int[] TIMELINE_LEFT_ICON_PATH = new int[] { 0, 1, 1, 0, 2 };
        private readonly int[] USERLINE_NAME_PATH = new int[] { 1, 0 };
        private readonly int[] USERLINE_TYPE_PATH = new int[] { 1, 1 };
        private readonly int[] USERLINE_ICON_PATH = new int[] { 0, 1, 0 };

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

        private Transform GetTimelineInfoIcon(Transform transform) =>
            GetComponentByHierarchyPath(transform, TIMELINE_INFO_ICON_PATH);

        private Transform GetTimelineEnterIcon(Transform transform) =>
            GetComponentByHierarchyPath(transform, TIMELINE_ENTER_ICON_PATH);

        private Transform GetTimelineLeftIcon(Transform transform) =>
            GetComponentByHierarchyPath(transform, TIMELINE_LEFT_ICON_PATH);

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
                    return _onlineColor;
                case TimelineType.Leave:
                    return _offlineColor;
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
                    return _onlineColor;
                case UserlineType.Offline:
                    return _offlineColor;
                default:
                    return Color.white;
            }
        }

        private Color GetUserlineTextColor(UserlineType userlineType)
        {
            switch (userlineType)
            {
                case UserlineType.Online:
                    return _foregroundColor;
                case UserlineType.Offline:
                    return _mutedColor;
                default:
                    return Color.white;
            }
        }

        #endregion

        #region Syncing Functions

        private void SyncBytes()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(totalUsers.ToString());
            sb.Append('\v');
            sb.Append(ConvertTimelineStructToString());
            sb.Append('\v');
            sb.Append(ConvertUserlineStructToString());
            bytes = DATA_ENCODING.GetBytes(sb.ToString());
        }

        private void LoadBytes()
        {
            if (bytes == null || bytes.Length == 0) return;
            string data = DATA_ENCODING.GetString(bytes);
            string[] lines = data.Split('\v');
            totalUsers = int.Parse(lines[0]);
            LoadTimelineStructFromString(lines[1]);
            LoadUserlineStructFromString(lines[2]);

            int players = VRCPlayerApi.GetPlayerCount();
            UpdateTexts(totalUsers, players, struct_timeline_name.Length, players);
        }

        private string ConvertTimelineStructToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < struct_timeline_name.Length; i++)
            {
                sb.Append(struct_timeline_name[i]);
                sb.Append('\t');
                sb.Append(struct_timeline_time[i].ToString(DATE_FORMAT));
                sb.Append('\t');
                sb.Append((int)struct_timeline_type[i]);
                sb.Append('\n');
            }
            return sb.ToString();
        }

        private string ConvertUserlineStructToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < struct_userline_name.Length; i++)
            {
                sb.Append(struct_userline_name[i]);
                sb.Append('\t');
                sb.Append((int)struct_userline_type[i]);
                sb.Append('\n');
            }
            return sb.ToString();
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
            Image icon = SelectTimelineIcon(transform, type);
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

        private string GetSyncDataSize()
        {
            double dataSize = bytes == null ? 0 : bytes.Length;
            string[] units = { "byte", "KB", "MB", "GB" };
            int unitIndex = 0;

            while (dataSize >= 1024 && unitIndex < units.Length - 1)
            {
                dataSize /= 1024;
                unitIndex++;
            }

            string format = unitIndex == 0 ? "{0} {1}" : "{0:F2} {1}";
            return string.Format(format, dataSize, units[unitIndex]);
        }

        private Image SelectTimelineIcon(Transform transform, TimelineType type)
        {
            if (type == TimelineType.Enter)
            {
                Transform icon = GetTimelineEnterIcon(transform);
                icon.gameObject.SetActive(true);
                return icon.GetComponent<Image>();
            }
            else if (type == TimelineType.Leave)
            {
                Transform icon = GetTimelineLeftIcon(transform);
                icon.gameObject.SetActive(true);
                return icon.GetComponent<Image>();
            }
            else
            {
                Transform icon = GetTimelineInfoIcon(transform);
                icon.gameObject.SetActive(true);
                return icon.GetComponent<Image>();
            }
        }

        #endregion

        #region Animation Functions

        public void ShowToast(string text, string description)
        {
            toastText.text = text;
            toastDescriptionText.text = description;
            Animator.SetTrigger(toastTriggerName);
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
            syncDataSizeText.text = GetSyncDataSize();
        }

        public override void OnDeserialization()
        {
            LoadBytes();
            syncDataSizeText.text = GetSyncDataSize();
        }

        #endregion

        #region Udon Callbacks (Player)

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            ShowToast($"{player.displayName} joined the world!", $"Welcome to the world, {player.displayName}!");

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
            ShowToast($"{player.displayName} left the world!", $"Goodbye, {player.displayName}!");

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
}

using System;
using System.Text;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Limitex.MonoUI.Udon
{
    #region Enums

    enum LogType : byte
    {
        Enter = 0,
        Leave = 1
    }

    #endregion

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SimpleLogManager : UdonSharpBehaviour
    {
        [Header("Header")]
        [SerializeField] private Text _headerPlayerText;
        [SerializeField] private Text _headerByteText;
        [SerializeField] private string _headerPlayerTextFormat;

        [Header("Color")]
        [SerializeField] private Color _timestampColor;
        [SerializeField] private Color _enterColor;
        [SerializeField] private Color _leaveColor;
        [SerializeField] private Color _defaultColor;

        [Header("Log")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _parentTransform;
        [SerializeField] private GameObject _textPrefab;

        [Header("Settings")]
        [SerializeField] private int _dateFontSize;

        #region Constant Fields

        private const string ENTER_TEXT = "Enter";
        private const string LEAVE_TEXT = "Leave";
        private const int UINT_SIZE = 4;
        private const int BYTE_SIZE = 1;
        private const int ENTRY_SIZE = UINT_SIZE + BYTE_SIZE + BYTE_SIZE; // timestamp + logType + nameLength
        private readonly DateTime EPOCH = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc); // 2025-01-01 00:00:00 UTC as reference point

        #endregion

        #region Serialized Data

        private int _serializedDataBytes = 0;

        [UdonSynced(UdonSyncMode.None)] private byte[] _serializedData = new byte[0];

        private void AddData(DateTime timestamp, LogType logType, string logText)
        {
            byte[] logTextBytes = Encoding.UTF8.GetBytes(logText);
            byte playerBytesLength = (byte)Mathf.Min(logTextBytes.Length, 255);
            byte[] newSerializedData = new byte[_serializedData.Length + ENTRY_SIZE + playerBytesLength];
            Buffer.BlockCopy(_serializedData, 0, newSerializedData, 0, _serializedData.Length);
            int offset = _serializedData.Length;
            Buffer.BlockCopy(BitConverter.GetBytes((uint)(timestamp - EPOCH).TotalSeconds), 0, newSerializedData, offset, UINT_SIZE);
            offset += UINT_SIZE;
            newSerializedData[offset] = (byte)logType;
            offset += BYTE_SIZE;
            newSerializedData[offset] = playerBytesLength;
            offset += BYTE_SIZE;
            Buffer.BlockCopy(logTextBytes, 0, newSerializedData, offset, playerBytesLength);
            _serializedData = newSerializedData;
            _serializedDataBytes = _serializedData.Length;
        }

        private void GetData(int offset, out DateTime timestamp, out LogType logType, out string logText, out int dataLength)
        {
            timestamp = EPOCH.AddSeconds(BitConverter.ToUInt32(_serializedData, offset));
            offset += UINT_SIZE;
            logType = (LogType)_serializedData[offset];
            offset += BYTE_SIZE;
            byte playerBytesLength = _serializedData[offset];
            offset += BYTE_SIZE;
            logText = Encoding.UTF8.GetString(_serializedData, offset, playerBytesLength);
            dataLength = ENTRY_SIZE + playerBytesLength;
        }

        #endregion

        #region Udon Callbacks

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            DateTime timestamp = DateTime.UtcNow;
            LogType logType = LogType.Enter;
            string logText = player.displayName;
            AddData(timestamp, logType, logText);
            AddLine(timestamp, logType, logText);
            UpdateHeader(_serializedData.Length, VRCPlayerApi.GetPlayerCount());
            RequestSerialization();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            DateTime timestamp = DateTime.UtcNow;
            LogType logType = LogType.Leave;
            string logText = player.displayName;
            AddData(timestamp, logType, logText);
            AddLine(timestamp, logType, logText);
            UpdateHeader(_serializedData.Length, VRCPlayerApi.GetPlayerCount() - 1);
            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            for (int i = _serializedDataBytes; i < _serializedData.Length;)
            {
                GetData(i, out DateTime timestamp, out LogType logType, out string logText, out int dataLength);
                AddLine(timestamp, logType, logText);
                i += dataLength;
            }
            _serializedDataBytes = _serializedData.Length;
            UpdateHeader(_serializedData.Length, VRCPlayerApi.GetPlayerCount());
        }

        #endregion

        #region UI Helper

        private void AddLine(DateTime timestamp, LogType logType, string logText)
        {
            GameObject item = Instantiate(_textPrefab, _parentTransform);
            Text text = item.GetComponent<Text>();
            DateTime local = timestamp.ToLocalTime();

            StringBuilder sb = new StringBuilder();
            sb.Append(Colorize(
                string.Format("{0} {1:HH:mm:ss}",
                    Resize(local.ToString("yyyy/MM/dd"), _dateFontSize),
                    local
                ),
                _timestampColor
            ));
            sb.Append(' ');
            sb.Append(GetLogTypeString(logType));
            sb.Append(' ');
            sb.Append(Colorize(logText, _defaultColor));

            text.text = sb.ToString();
            item.SetActive(true);
            ScrollToBottom();
        }

        private void UpdateHeader(int byteLength, int plyaerCount)
        {
            _headerPlayerText.text = string.Format(_headerPlayerTextFormat, plyaerCount);
            _headerByteText.text = FormatByteSize(byteLength);
        }

        private void ScrollToBottom()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_scrollRect.transform);
            _scrollRect.normalizedPosition = new Vector2(_scrollRect.normalizedPosition.x, 0f);
        }

        #endregion

        #region Helper Methods

        private string GetLogTypeString(LogType logType)
        {
            if (logType == LogType.Enter) return Colorize(ENTER_TEXT, _enterColor);
            if (logType == LogType.Leave) return Colorize(LEAVE_TEXT, _leaveColor);
            return string.Empty;
        }

        private string FormatByteSize(int bytes)
        {
            string[] units = { "byte", "KB", "MB", "GB" };
            int unitIndex = 0;
            double size = bytes;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return string.Format("{0:0.##} {1}", size, units[unitIndex]);
        }

        private string Colorize(string text, Color color)
        {
            return $"<color=#{ColorToHexRGB(color)}>{text}</color>";
        }

        private string Resize(string text, int size)
        {
            return $"<size={size}>{text}</size>";
        }

        private string ColorToHexRGB(Color color)
        {
            int r = (int)(color.r * 255);
            int g = (int)(color.g * 255);
            int b = (int)(color.b * 255);

            return string.Format("{0:X2}{1:X2}{2:X2}", r, g, b);
        }

        #endregion
    }
}

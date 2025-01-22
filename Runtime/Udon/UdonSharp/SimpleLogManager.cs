using System;
using System.Text;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Limitex.MonoUI.Udon
{
    enum LogType : byte
    {
        Enter = 0,
        Leave = 1
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SimpleLogManager : UdonSharpBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _parentTransform;
        [SerializeField] private GameObject _textPrefab;

        private const string ENTER_TEXT = "Enter";
        private const string LEAVE_TEXT = "Leave";
        private const int UINT_SIZE = 4;
        private const int BYTE_SIZE = 1;
        private const int ENTRY_SIZE = UINT_SIZE + BYTE_SIZE + BYTE_SIZE; // timestamp + logType + nameLength
        private readonly DateTime EPOCH = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc); // 2025-01-01 00:00:00 UTC as reference point

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
            RequestSerialization();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            DateTime timestamp = DateTime.UtcNow;
            LogType logType = LogType.Leave;
            string logText = player.displayName;
            AddData(timestamp, logType, logText);
            AddLine(timestamp, logType, logText);
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
        }

        #endregion

        #region Helper Methods

        private void AddLine(DateTime timestamp, LogType logType, string logText)
        {
            GameObject item = Instantiate(_textPrefab, _parentTransform);
            Text text = item.GetComponent<Text>();
            text.text = $"{timestamp.ToLocalTime():HH:mm:ss} {GetLogTypeString(logType)} {logText}";
            item.SetActive(true);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_scrollRect.transform);
            _scrollRect.normalizedPosition = new Vector2(_scrollRect.normalizedPosition.x, 0f);
        }

        private string GetLogTypeString(LogType logType)
        {
            if (logType == LogType.Enter) return ENTER_TEXT;
            if (logType == LogType.Leave) return LEAVE_TEXT;
            return string.Empty;
        }

        #endregion
    }
}

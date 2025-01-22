using System;
using System.Text;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

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
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform parentTransfrom;
        [SerializeField] private GameObject textTransform;

        private const int INT_SIZE = 4, LONG_SIZE = 8, ENUM_SIZE = 1;
        private const int ENTRY_SIZE = INT_SIZE + LONG_SIZE + ENUM_SIZE;

        #region Serialized Data

        private int serializedDataBytes = 0;

        [UdonSynced(UdonSyncMode.None)] private byte[] serializedData = new byte[0];

        private void AddData(DateTime utc_timestamp, LogType log_type, string player_name)
        {
            byte[] player_name_bytes = Encoding.UTF8.GetBytes(player_name);
            int player_bytes_length = player_name_bytes.Length;
            byte[] newSerializedData = new byte[serializedData.Length + ENTRY_SIZE + player_bytes_length];
            Buffer.BlockCopy(serializedData, 0, newSerializedData, 0, serializedData.Length);
            int offset = serializedData.Length;
            Buffer.BlockCopy(BitConverter.GetBytes(utc_timestamp.Ticks), 0, newSerializedData, offset, LONG_SIZE);
            offset += LONG_SIZE;
            newSerializedData[offset] = (byte)log_type;
            offset += ENUM_SIZE;
            Buffer.BlockCopy(BitConverter.GetBytes(player_bytes_length), 0, newSerializedData, offset, INT_SIZE);
            offset += INT_SIZE;
            Buffer.BlockCopy(player_name_bytes, 0, newSerializedData, offset, player_bytes_length);
            serializedData = newSerializedData;
            serializedDataBytes = serializedData.Length;
        }

        private void GetData(int offset, out DateTime utc_timestamp, out LogType logType, out string player_name, out int data_length)
        {
            utc_timestamp = new DateTime(BitConverter.ToInt64(serializedData, offset));
            offset += LONG_SIZE;
            logType = (LogType)serializedData[offset];
            offset += ENUM_SIZE;
            int player_bytes_length = BitConverter.ToInt32(serializedData, offset);
            offset += INT_SIZE;
            player_name = Encoding.UTF8.GetString(serializedData, offset, player_bytes_length);
            data_length = ENTRY_SIZE + player_bytes_length;
        }

        #endregion

        #region Udon Callbacks

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            DateTime utc_timestamp = DateTime.UtcNow;
            LogType logType = LogType.Enter;
            string player_name = player.displayName;
            AddData(utc_timestamp, logType, player_name);
            AddLine(utc_timestamp, logType, player_name);
            RequestSerialization();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            DateTime utc_timestamp = DateTime.UtcNow;
            LogType logType = LogType.Leave;
            string player_name = player.displayName;
            AddData(utc_timestamp, logType, player_name);
            AddLine(utc_timestamp, logType, player_name);
            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            for (int i = serializedDataBytes; i < serializedData.Length;)
            {
                GetData(i, out DateTime timestamp, out LogType logType, out string playerName, out int data_length);
                AddLine(timestamp, logType, playerName);
                i += data_length;
            }
            serializedDataBytes = serializedData.Length;
        }

        #endregion

        #region Helper Methods

        private void AddLine(DateTime utc_timestamp, LogType log_type, string player_name)
        {
            GameObject item = Instantiate(textTransform, parentTransfrom);
            Text text = item.GetComponent<Text>();
            text.text = $"{utc_timestamp:HH:mm:ss} {GetLogTypeString(log_type)} ({player_name})";
            item.SetActive(true);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollRect.transform);
            scrollRect.normalizedPosition = new Vector2(scrollRect.normalizedPosition.x, 0f);
        }

        private string GetLogTypeString(LogType logType)
        {
            switch (logType)
            {
                case LogType.Enter:
                    return "Enter";
                case LogType.Leave:
                    return "Leave";
                default:
                    return "Unknown";
            }
        }

        #endregion
    }
}

using System.Text;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Limitex.MonoUI.Udon
{
    #region Enums

    enum Platform
    {
        Standalone = 0,
        Android = 1,
        iOS = 2
    }

    #endregion

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlatformStatisticsManager : UdonSharpBehaviour
    {
        [SerializeField] private TMP_Text totalPlayerCount;
        [SerializeField] private TMP_Text standaloneCount;
        [SerializeField] private TMP_Text androidCount;
        [SerializeField] private TMP_Text iosCount;

        #region  Constants

        private readonly Encoding ENCODING = Encoding.UTF8;

#if UNITY_STANDALONE
        private const Platform PLATFORM = Platform.Standalone;
#elif UNITY_ANDROID
        private const Platform PLATFORM = Platform.Android;
#elif UNITY_IOS
        private const Platform PLATFORM = Platform.iOS;
#endif

        #endregion

        #region Data Structures

        private string[] usernames = new string[0];
        private Platform[] platforms = new Platform[0];

        private void AddDataItem(string username, Platform platform)
        {
            string[] newUsernames = new string[usernames.Length + 1];
            Platform[] newPlatforms = new Platform[platforms.Length + 1];
            for (int i = 0; i < usernames.Length; i++)
            {
                newUsernames[i] = usernames[i];
                newPlatforms[i] = platforms[i];
            }
            newUsernames[usernames.Length] = username;
            newPlatforms[platforms.Length] = platform;
            usernames = newUsernames;
            platforms = newPlatforms;
        }

        private void RemoveDataItem(string username)
        {
            for (int i = 0; i < usernames.Length; i++)
            {
                if (usernames[i] == username)
                {
                    string[] newUsernames = new string[usernames.Length - 1];
                    Platform[] newPlatforms = new Platform[platforms.Length - 1];
                    for (int j = 0; j < i; j++)
                    {
                        newUsernames[j] = usernames[j];
                        newPlatforms[j] = platforms[j];
                    }
                    for (int j = i + 1; j < usernames.Length; j++)
                    {
                        newUsernames[j - 1] = usernames[j];
                        newPlatforms[j - 1] = platforms[j];
                    }
                    usernames = newUsernames;
                    platforms = newPlatforms;
                    return;
                }
            }
        }

        #endregion

        #region UdonSynced

        [UdonSynced(UdonSyncMode.None)] private byte[] serializedData = new byte[0];

        private void Serialize()
        {
            int totalLength = 1;
            for (int i = 0; i < usernames.Length; i++)
            {
                byte[] usernameBytes = ENCODING.GetBytes(usernames[i]);
                totalLength += 1 + usernameBytes.Length + 1;
            }
            serializedData = new byte[totalLength];
            int offset = 0;
            serializedData[offset++] = (byte)(usernames.Length & 0xFF);
            for (int i = 0; i < usernames.Length; i++)
            {
                byte[] usernameBytes = ENCODING.GetBytes(usernames[i]);
                serializedData[offset++] = (byte)(usernameBytes.Length & 0xFF);
                for (int j = 0; j < usernameBytes.Length; j++)
                    serializedData[offset++] = usernameBytes[j];
                serializedData[offset++] = (byte)((int)platforms[i] & 0xFF);
            }
        }

        private void Deserialize()
        {
            if (serializedData == null || serializedData.Length == 0)
            {
                usernames = new string[0];
                platforms = new Platform[0];
                return;
            }
            int offset = 0;
            int usernamesLength = serializedData[offset++];
            usernames = new string[usernamesLength];
            platforms = new Platform[usernamesLength];
            for (int i = 0; i < usernamesLength; i++)
            {
                int usernameLength = serializedData[offset++];
                byte[] usernameBytes = new byte[usernameLength];
                for (int j = 0; j < usernameLength; j++)
                    usernameBytes[j] = serializedData[offset++];
                usernames[i] = ENCODING.GetString(usernameBytes);
                platforms[i] = (Platform)serializedData[offset++];
            }
        }

        #endregion

        #region Udon Events

        public override void OnPreSerialization()
        {
            Serialize();
        }

        public override void OnDeserialization()
        {
            Deserialize();
            UpdatePlatformCounts(VRCPlayerApi.GetPlayerCount());
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal)
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            if (!Networking.IsOwner(gameObject))
                return;

            AddDataItem(player.displayName, PLATFORM);
            UpdatePlatformCounts(VRCPlayerApi.GetPlayerCount());
            RequestSerialization();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            RemoveDataItem(player.displayName);
            UpdatePlatformCounts(VRCPlayerApi.GetPlayerCount() - 1);
            RequestSerialization();
        }

        #endregion

        public void UpdatePlatformCounts(int plyaerCount)
        {
            int standalone = 0, android = 0, ios = 0, none = 0;

            for (int i = 0; i < usernames.Length; i++)
            {
                switch (platforms[i])
                {
                    case Platform.Standalone:
                        standalone++;
                        break;
                    case Platform.Android:
                        android++;
                        break;
                    case Platform.iOS:
                        ios++;
                        break;
                    default:
                        none++;
                        break;

                }
            }

            if (none > 0)
            {
                Debug.LogWarning("PlatformStatisticsManager: " + none + " players have an invalid platform.");
            }

            totalPlayerCount.text = plyaerCount.ToString();
            standaloneCount.text = standalone.ToString();
            androidCount.text = android.ToString();
            iosCount.text = ios.ToString();
        }
    }
}

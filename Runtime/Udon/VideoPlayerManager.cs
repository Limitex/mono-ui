
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDKBase;
using VRC.Udon;

namespace Limitex.MonoUI.Udon
{
    enum RepeatMode
    {
        None,
        Repeat,
        RepeatOnce
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VideoPlayerManager : UdonSharpBehaviour
    {
        [Header("AV Pro Core")]
        [SerializeField] private VRCAVProVideoPlayer videoPlayer;

        [Header("VRC Components")]
        [SerializeField] private VRCUrlInputField urlInputField;

        [Header("Animation Controller")]
        [SerializeField] private Animator animator;

        [Header("Settings")]
        [SerializeField] private float refreshTime;

        [Header("uGUI Components")]
        [SerializeField] private Slider progress;
        [SerializeField] private Slider underProgress;
        [SerializeField] private TMP_Text timeText;

        #region Fields

        private float refreshTimeCash = 0f;
        private RepeatMode currentRepeatMode = RepeatMode.None;

        #endregion

        #region External References (Animator Controller)

        public void Play()
        {
            videoPlayer.Play();
            SetProgress(0);
        }

        public void Pause()
        {
            videoPlayer.Pause();
            SetProgress(0);
        }

        public void Stop()
        {
            videoPlayer.Stop();
            SetProgress(0);
        }

        #region Repeat

        public void RepeatNone() => currentRepeatMode = RepeatMode.None;

        public void Repeat() => currentRepeatMode = RepeatMode.Repeat;

        public void RepeatOnce() => currentRepeatMode = RepeatMode.RepeatOnce;

        #endregion

        #endregion

        #region External References (uGUI Components)

        public void OnClickAddQueue()
        {
            urlInputField.SetUrl(VRCUrl.Empty);
        }

        public void OnClickInterrupt()
        {
            videoPlayer.LoadURL(urlInputField.GetUrl());
            urlInputField.SetUrl(VRCUrl.Empty);
            if (videoPlayer.IsPlaying)
            {
                videoPlayer.Stop();
                animator.SetTrigger("VideoOnStop");
            }
            animator.SetTrigger("VideoOnLoad");
        }

        public void OnClickCansel()
        {
            urlInputField.SetUrl(VRCUrl.Empty);
        }

        #endregion

        #region Udon Sharp Callbacks

        public override void OnVideoReady()
        {
            videoPlayer.Play();
            animator.SetTrigger("VideoOnPlay");
        }

        public override void OnVideoStart()
        {

        }

        public override void OnVideoPause()
        {

        }

        public override void OnVideoEnd()
        {
            videoPlayer.Stop();
            animator.SetTrigger("VideoOnStop");
        }

        public override void OnVideoLoop()
        {

        }

        public override void OnVideoError(VideoError videoError)
        {
            Debug.LogError("MonoUI: VideoPlayerManager: OnVideoError: " + videoError);
            animator.SetTrigger("VideoOnStop");
            SetProgress(0);
        }

        #endregion

        #region Unity Callbacks

        void Start()
        {

        }

        private void Update()
        {
            refreshTimeCash += Time.deltaTime;
            if (refreshTimeCash <= refreshTime) return;
            else refreshTimeCash = 0f;

            float currentTime = videoPlayer.GetTime();
            float duration = videoPlayer.GetDuration();
            float normalizedTime = duration == 0 ? 0 : currentTime / duration;

            SetProgress(normalizedTime);
            SetTimeText(currentTime, duration);
        }

        #endregion

        #region Helper Methods

        private void SetProgress(float value)
        {
            progress.value = value;
            underProgress.value = value;
        }

        private void SetTimeText(float currentTime, float duration)
        {
            bool useHourFormat = duration >= 3600;
            timeText.text = FormatSecondsToTime(currentTime, useHourFormat) + " / " + FormatSecondsToTime(duration, useHourFormat);
        }

        private string FormatSecondsToTime(float totalSeconds, bool useHourFormat)
        {
            int hours = Mathf.FloorToInt(totalSeconds / 3600);
            int minutes = Mathf.FloorToInt((totalSeconds % 3600) / 60);
            int seconds = Mathf.FloorToInt(totalSeconds % 60);
            if (useHourFormat)
                return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
            else
                return string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }

        #endregion
    }
}


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

        private bool isPlaying = false;
        private float refreshTimeCash = 0f;
        private RepeatMode currentRepeatMode = RepeatMode.None;

        #endregion

        #region External References (Animator Controller)

        public void Play()
        {
            videoPlayer.Play();
        }

        public void Pause()
        {
            videoPlayer.Pause();
        }

        public void Stop()
        {
            videoPlayer.Stop();
            SetProgress(0);
        }

        public void Progress()
        {
            if (!isPlaying) return;
            float duration = videoPlayer.GetDuration();
            float value = progress.value;
            videoPlayer.SetTime(value);
            SetTimeText(value, duration);
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
            VideoReady();
            animator.SetTrigger("VideoOnPlay");
            progress.minValue = 0;
            progress.maxValue = videoPlayer.GetDuration();
            underProgress.minValue = 0;
            underProgress.maxValue = videoPlayer.GetDuration();
        }

        public override void OnVideoStart()
        {

        }

        public override void OnVideoPause()
        {

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
            if (!isPlaying) return;
            refreshTimeCash += Time.deltaTime;
            if (refreshTimeCash <= refreshTime) return;
            else refreshTimeCash = 0f;

            float currentTime = videoPlayer.GetTime();
            float duration = videoPlayer.GetDuration();

            SetProgress(currentTime);
            SetTimeText(currentTime, duration);

            if (currentTime == duration)
            {
                isPlaying = false;
                _OnVideoEnd();
            }
        }

        #endregion

        #region Update Callbacks

        private void _OnVideoEnd()
        {
            SetProgress(0);

            switch (currentRepeatMode)
            {
                case RepeatMode.None:
                    videoPlayer.Stop();
                    animator.SetTrigger("VideoOnStop");
                    break;
                case RepeatMode.Repeat:
                case RepeatMode.RepeatOnce:
                    VideoReady();
                    break;
            }
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

        private void VideoReady()
        {
            videoPlayer.SetTime(0);
            videoPlayer.Play();
            isPlaying = true;
        }

        #endregion
    }
}

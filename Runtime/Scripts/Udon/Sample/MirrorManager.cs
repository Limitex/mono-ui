using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Limitex.MonoUI.Udon
{
    enum MirrorType
    {
        HQ,
        LQ
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MirrorManager : UdonSharpBehaviour
    {
        [SerializeField] private VRC_MirrorReflection _hqMirrorReflection;
        [SerializeField] private VRC_MirrorReflection _lqMirrorReflection;

        private void Start()
        {
            SetMirror(MirrorType.HQ);
        }

        public void OnClickHQButton() => SetMirror(MirrorType.HQ);

        public void OnClickLQButton() => SetMirror(MirrorType.LQ);

        private void SetMirror(MirrorType mirrorType)
        {
            _hqMirrorReflection.enabled = mirrorType == MirrorType.HQ;
            _lqMirrorReflection.enabled = mirrorType == MirrorType.LQ;
        }
    }
}

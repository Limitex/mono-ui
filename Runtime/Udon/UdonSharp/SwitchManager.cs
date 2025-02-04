
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Limitex.MonoUI.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SwitchManager : UdonSharpBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Toggle toggle;
        [SerializeField] private string switchParameterName;

        void Start() => animator.SetBool(switchParameterName, toggle.isOn);

        public void ToggleSwitch() => animator.SetBool(switchParameterName, toggle.isOn);
    }
}


using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Limitex.MonoUI.Udon
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SwitchManager : MonoUIBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string switchParameterName;

        private int parameterHash;

        void Start()
        {
            parameterHash = Animator.StringToHash(switchParameterName);
            InvokeAllHandlers();
        }

        void OnEnable() => InvokeAllHandlers();

        protected override void OnToggleValueChanged(Toggle toggle) => animator.SetBool(parameterHash, toggle.isOn);
    }
}


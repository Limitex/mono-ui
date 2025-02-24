using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Limitex.MonoUI.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DialogManager : UdonSharpBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _parametorName;

        private int _parameterHash;

        private void Start() => _parameterHash = Animator.StringToHash(_parametorName);

        public void OnClickOpenButton() => _animator.SetBool(_parameterHash, true);

        public void OnClickCloseButton() => _animator.SetBool(_parameterHash, false);
    }
}

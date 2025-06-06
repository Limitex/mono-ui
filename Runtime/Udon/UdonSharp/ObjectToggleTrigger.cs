using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Limitex.MonoUI.Udon
{
    [AddComponentMenu("Mono UI/MI Object Toggle Trigger")]
    [RequireComponent(typeof(Toggle))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ObjectToggleTrigger : MonoUIBehaviour
    {
        [SerializeField] public ObjectToggle _targetObjectToggle;

        public void SetToggleState(bool isActive)
        {
            toggle.isOn = isActive;
        }

        protected override void OnToggleValueChanged(Toggle toggle)
        {
            if (_targetObjectToggle == null)
            {
                Debug.LogError("Target ObjectToggle is not assigned.", this);
                return;
            }
            _targetObjectToggle.TriggerAction(toggle.isOn);
        }
    }
}

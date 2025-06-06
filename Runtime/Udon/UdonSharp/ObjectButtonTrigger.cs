using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
namespace Limitex.MonoUI.Udon
{
    [AddComponentMenu("Mono UI/MI Object Button Trigger")]
    [RequireComponent(typeof(Button))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ObjectButtonTrigger : MonoUIBehaviour
    {
        [SerializeField] public ObjectButton _targetObjectButton;

        protected override void OnButtonClick(Button button)
        {
            if (_targetObjectButton == null)
            {
                Debug.LogError("Target ObjectButton is not assigned.", this);
                return;
            }

            _targetObjectButton.TriggerAction();
        }
    }
}

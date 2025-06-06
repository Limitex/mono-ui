using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Limitex.MonoUI.Udon
{
    enum ButtonType
    {
        Invert,
        Shift,
    }

    [AddComponentMenu("Mono UI/MI Object Button")]
    [RequireComponent(typeof(Button))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ObjectButton : MonoUIBehaviour
    {
        [SerializeField] private bool _isGlobal;
        [SerializeField] private ButtonType _buttonType = ButtonType.Invert;
        [SerializeField] private Transform[] _targets;

        private const int MAX_MASK_SIZE = sizeof(int) * 8;

        [UdonSynced(UdonSyncMode.None)] private int _mask = 0;

        private int MaxTargets;

        private void Start()
        {
            MaxTargets = Mathf.Min(_targets.Length, MAX_MASK_SIZE);

            if (_targets.Length > MAX_MASK_SIZE)
            {
                Debug.LogWarning($"Total targets count {_targets.Length} " +
                    $"exceeds maximum mask size of {MAX_MASK_SIZE}. " +
                    $"Only the first {MAX_MASK_SIZE} will be processed.");
            }

            for (int i = 0; i < MaxTargets; i++)
            {
                if (_targets[i] == null)
                {
                    Debug.LogError($"Target at index {i} is null. Please assign a Transform.", this);
                    continue;
                }

                SetMask(i, _targets[i].gameObject.activeSelf);
            }
        }

        private void SetMask(int index, bool value)
        {
            if (value)
            {
                _mask |= (1 << index);
            }
            else
            {
                _mask &= ~(1 << index);
            }
        }

        private bool GetMask(int index)
        {
            return (_mask & (1 << index)) != 0;
        }

        private void InvertMask()
        {
            // Inverts all bits within the 'MaxTargets' range.

            // Create a mask consisting of 'MaxTargets' number of ones.
            int inversionMask = (1 << MaxTargets) - 1;

            // Use the XOR operator to flip every bit within the mask.
            _mask ^= inversionMask;
        }


        private void ShiftMask()
        {
            // Performs a circular left shift (rotate left).

            // Store the state of the most significant bit (MSB).
            bool mostSignificantBit = (_mask & (1 << (MaxTargets - 1))) != 0;

            // Shift the entire mask one bit to the left.
            _mask <<= 1;

            // If the original MSB was 1, wrap it around to the least significant bit (LSB).
            if (mostSignificantBit)
            {
                _mask |= 1;
            }

            // Clear any bits outside the range of MaxTargets.
            int sequenceMask = (1 << MaxTargets) - 1;
            _mask &= sequenceMask;
        }

        private void ApplyMaskToTargets()
        {
            for (int i = 0; i < MaxTargets; i++)
            {
                _targets[i].gameObject.SetActive(GetMask(i));
            }
        }

        public override void OnDeserialization()
        {
            ApplyMaskToTargets();
        }

        public void TriggerAction()
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            if (_buttonType == ButtonType.Invert)
            {
                InvertMask();
            }
            else if (_buttonType == ButtonType.Shift)
            {
                ShiftMask();
            }
            else
            {
                Debug.LogError($"Unknown button type: {_buttonType}");
                return;
            }

            ApplyMaskToTargets();

            if (!_isGlobal)
            {
                return;
            }

            RequestSerialization();
        }

        protected override void OnButtonClick(Button button)
        {
            TriggerAction();
        }
    }
}


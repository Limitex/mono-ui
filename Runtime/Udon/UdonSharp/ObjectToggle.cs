﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Limitex.MonoUI.Udon
{
    [AddComponentMenu("Mono UI/MI Object Toggle")]
    [RequireComponent(typeof(Toggle))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ObjectToggle : MonoUIBehaviour
    {
        [SerializeField] private bool _isGlobal;
        [SerializeField] private bool _defaultTargetState = true;
        [SerializeField] private Transform[] _targets;

        [HideInInspector] public ObjectToggleTrigger[] _linkedToggles;

        public const string LinkedTogglesName = nameof(_linkedToggles);

        private const int MAX_MASK_SIZE = sizeof(int) * 8;

        [UdonSynced(UdonSyncMode.None)] private bool _invertMask = false;

        private int _mask = 0;
        private int MaxTargets;
        private bool InitialToggleState;

        private void Start()
        {
            MaxTargets = Mathf.Min(_targets.Length, MAX_MASK_SIZE);
            InitialToggleState = toggle.isOn;

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
                    Debug.LogWarning($"Target at index {i} is null. Please assign a Transform.", this);
                    SetMask(i, _defaultTargetState);
                    continue;
                }

                SetMask(i, _targets[i].gameObject.activeSelf);
            }

            ApplyMaskToTargets();
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

        private void ApplyMaskToTargets()
        {
            for (int i = 0; i < MaxTargets; i++)
            {
                if (_targets[i] == null)
                {
                    Debug.LogWarning($"Target at index {i} is null. Please assign a Transform.", this);
                    continue;
                }

                _targets[i].gameObject.SetActive(GetMask(i) ^ _invertMask);
            }

            bool currentToggleState = _invertMask ^ InitialToggleState;
            toggle.isOn = currentToggleState;

            foreach (var linkedToggle in _linkedToggles)
            {
                if (linkedToggle == null) continue;
                linkedToggle.SetToggleState(currentToggleState);
            }
        }

        public override void OnDeserialization()
        {
            ApplyMaskToTargets();
        }

        public void TriggerAction(bool isActive)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            _invertMask = isActive ^ InitialToggleState;

            ApplyMaskToTargets();

            if (!_isGlobal)
            {
                return;
            }

            RequestSerialization();
        }

        public bool GetToggleState()
        {
            return toggle.isOn;
        }

        protected override void OnToggleValueChanged(Toggle toggle)
        {
            TriggerAction(toggle.isOn);
        }
    }
}

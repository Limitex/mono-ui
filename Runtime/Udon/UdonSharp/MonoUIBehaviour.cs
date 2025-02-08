using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UdonSharp;

namespace Limitex.MonoUI.Udon
{
    [DisallowMultipleComponent]
    public class MonoUIBehaviour : UdonSharpBehaviour
    {
        [HideInInspector] public Button button;
        [HideInInspector] public Toggle toggle;
        [HideInInspector] public Slider slider;
        [HideInInspector] public Scrollbar scrollbar;
        [HideInInspector] public ScrollRect scrollRect;
        [HideInInspector] public TMP_Dropdown tmpDropdown;


        public void OnButtonClick()
        {
            if (button != null) OnButtonClick(button);
            else Debug.LogWarning("Button reference is null");
        }

        public void OnToggleValueChanged()
        {
            if (toggle != null) OnToggleValueChanged(toggle);
            else Debug.LogWarning("Toggle reference is null");
        }

        public void OnSliderValueChanged()
        {
            if (slider != null) OnSliderValueChanged(slider);
            else Debug.LogWarning("Slider reference is null");
        }

        public void OnScrollbarValueChanged()
        {
            if (scrollbar != null) OnScrollbarValueChanged(scrollbar);
            else Debug.LogWarning("Scrollbar reference is null");
        }

        public void OnScrollRectValueChanged()
        {
            if (scrollRect != null) OnScrollRectValueChanged(scrollRect);
            else Debug.LogWarning("ScrollRect reference is null");
        }

        public void OnDropdownValueChanged()
        {
            if (tmpDropdown != null) OnDropdownValueChanged(tmpDropdown);
            else Debug.LogWarning("Dropdown reference is null");
        }


        public virtual void OnButtonClick(Button button) { }

        public virtual void OnToggleValueChanged(Toggle toggle) { }

        public virtual void OnSliderValueChanged(Slider slider) { }

        public virtual void OnScrollbarValueChanged(Scrollbar scrollbar) { }

        public virtual void OnScrollRectValueChanged(ScrollRect scrollRect) { }

        public virtual void OnDropdownValueChanged(TMP_Dropdown dropdown) { }
    }
}

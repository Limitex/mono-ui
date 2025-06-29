
using UdonSharp;
using UnityEngine;

namespace Limitex.MonoUI.Udon
{
    #region Enum

    enum ButtonDirection
    {
        Left,
        Right
    }

    #endregion

    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CarouselManager : UdonSharpBehaviour
    {
        [Header("Carousel")]
        [SerializeField] private int defaultCurrentIndex = 0;
        [SerializeField] private Transform[] images;
        [SerializeField] private Transform[] navigationEnableds;

        #region External References

        public void OnLeftControlButtonPressed() => OnControlButtonPressed_handler(ButtonDirection.Left);
        public void OnRightControlButtonPressed() => OnControlButtonPressed_handler(ButtonDirection.Right);

        #endregion

        #region Fields

        private int currentIndex;

        #endregion

        #region Unity Callbacks

        void Start()
        {
            if (!ValidateArrays()) return;
            SetCarouselPage(defaultCurrentIndex);
        }

        #endregion

        #region Custom Callbacks

        void OnControlButtonPressed_handler(ButtonDirection direction)
        {
            int newIndex = currentIndex + (direction == ButtonDirection.Left ? -1 : 1);
            if (newIndex >= 0 && newIndex < images.Length)
                SetCarouselPage(newIndex);
        }

        #endregion

        #region Helpers

        private void SetCarouselPage(int index)
        {
            for (int i = 0; i < images.Length; i++)
            {
                bool isCurrentPage = i == index;
                images[i].gameObject.SetActive(isCurrentPage);
                navigationEnableds[i].gameObject.SetActive(isCurrentPage);
            }
            currentIndex = index;
        }

        private bool ValidateArrays()
        {
            if (images == null || navigationEnableds == null)
            {
                Debug.LogError("Images or NavigationEnableds array is null.");
                return false;
            }
            if (images.Length != navigationEnableds.Length)
            {
                Debug.LogError("Images and NavigationEnableds arrays must have the same length.");
                return false;
            }
            if (defaultCurrentIndex < 0 || defaultCurrentIndex >= images.Length)
            {
                Debug.LogError("DefaultCurrentIndex is out of range.");
                return false;
            }
            return true;
        }

        #endregion
    }
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class SuckButton : MonoBehaviour, IPointerDownHandler
    {
        public UnityEvent onPointerDown;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!inactive)
            {
                clickFrame = Time.frameCount + 1;
                onPointerDown?.Invoke();
            }
        }
        
        
        private static int clickFrame;

        public static bool CanGameClick => Time.frameCount > clickFrame;

        private bool inactive;
        private Image image;
        public void SetActive(bool active)
        {
            inactive = !active;

            if (image == null)
                image = GetComponent<Image>();

            if (image != null)
            {
                image.enabled = !inactive;
                image.raycastTarget = !inactive;
            }
        }
    }
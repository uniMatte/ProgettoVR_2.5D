using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace WeArt.TouchUI
{
    /// <summary>
    /// TouchToggle — class to handle toggle pressing using standard Unity collision system.
    /// </summary>
    public class TouchToggle : MonoBehaviour
    {
        private Toggle _toggle;
        private BoxCollider _collider;
        private RectTransform _rectTransform;

        private static float _stopInteractivityAfterClick = 0.5f;
        private static float _colliderZValue = 0.002f;
        private bool _canPress = true;

        private void Awake()
        {
            _toggle = GetComponentInParent<Toggle>();
            _collider = GetComponent<BoxCollider>();
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            AdjustBoxColliderToToggleSize();
        }

        private void OnEnable()
        {
            _toggle.interactable = true;
            _collider.enabled = true;
            _canPress = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_canPress && _toggle.interactable) Press(collision.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_canPress && _toggle.interactable) Press(other.gameObject);
        }

        private void AdjustBoxColliderToToggleSize()
        {
            Vector3 boxSize = new Vector3(
                _rectTransform.sizeDelta.x * _rectTransform.localScale.x,
                _rectTransform.sizeDelta.y * _rectTransform.localScale.y,
                _colliderZValue);

            _collider.size = boxSize;
        }

        private void Press(GameObject presser)
        {
            if (!presser.name.Contains("Haptic")) return;

            if (_canPress && _toggle.interactable) {
                _toggle.isOn = !_toggle.isOn;

                //StartCoroutine(StopInteractivityFor(_stopInteractivityAfterClick));
                if (gameObject.activeInHierarchy) StartCoroutine(EnableInteractivityAfterPress());
            }
        }

        /*private IEnumerator StopInteractivityFor(float seconds)
        {
            _toggle.interactable = false;
            _collider.enabled = false;

            yield return new WaitForSeconds(seconds);

            _toggle.interactable = true;
            _collider.enabled = true;
        }*/

        private IEnumerator EnableInteractivityAfterPress()
        {
            _canPress = false;
            _collider.enabled = false;

            yield return new WaitForSeconds(_stopInteractivityAfterClick);

            _canPress = true;
            _collider.enabled = true;
        }
    }
}

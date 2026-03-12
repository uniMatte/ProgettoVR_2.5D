using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class BaseButton : MonoBehaviour
{
    public VrPhysicalButton vrPhysicalButton;
    public GameObject button;

        /// <summary>
        /// The OnCollisionEnter.
        /// </summary>
        /// <param name="collision">The collision<see cref="Collision"/>.</param>
        private void OnCollisionEnter(Collision collision){
            if(collision.body.gameObject == button)
            {
                vrPhysicalButton.OnPushButton?.Invoke();
            }
        }

        private void  OnTriggerEnter(Collider other)
        {
            if(other.gameObject == button)
            {
                vrPhysicalButton.OnPushButton?.Invoke();
            }
        }

        /// <summary>
        /// The OnCollisionStay.
        /// </summary>
        /// <param name="collision">The collision<see cref="Collision"/>.</param>
        private void OnCollisionStay(Collision collision) {
            if(collision.body.gameObject == button)
            {
                vrPhysicalButton.OnStayButton?.Invoke();
            }
        }

          private void  OnTriggerStay(Collider other)
        {
            if(other.gameObject == button)
            {
                vrPhysicalButton.OnStayButton?.Invoke();
            }
        }

        /// <summary>
        /// The OnCollisionExit.
        /// </summary>
        /// <param name="collision">The collision<see cref="Collision"/>.</param>
        private void OnCollisionExit(Collision collision)
        {
            if(collision.body.gameObject == button)
            {
                vrPhysicalButton.OnReleaseButton?.Invoke();
            }
        }

          private void  OnTriggerExit(Collider other)
        {
            if(other.gameObject == button)
            {
                vrPhysicalButton.OnReleaseButton?.Invoke();
            }
        }

}

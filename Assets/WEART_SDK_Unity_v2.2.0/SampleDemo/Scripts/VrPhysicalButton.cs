using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VrPhysicalButton : MonoBehaviour
{
    [SerializeField] Transform button;
    [SerializeField] Transform baseButton;
    //[SerializeField] bool allowMultipleInteractions = false;
    //[SerializeField] bool enableDebugMessages = false;

    public UnityEvent OnPushButton;
    public UnityEvent OnStayButton;
    public UnityEvent OnReleaseButton;

    //private bool isPressed = false;
    private float _startDistance = 1f;

    void Start()
    {
        _startDistance = Vector3.Distance(baseButton.position, button.position);
    }

    void OnVaseButtonEnter(Collider other)
    {

    }

    void LateUpdate()
    {
    }

    private void OnDrawGizmosSelected()
    {

    }

    public void ResetParams()
    {
        //isPressed = false;
    }
}

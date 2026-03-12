using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using WeArt.Components;
using WeArt.Core;

public class WeArtXRIBridge : MonoBehaviour
{
    // Direct Interactors references
    [SerializeField] XRDirectInteractor leftDirectInteractor, rightDirectInteractor;

    // Register to events 
    private void OnEnable()
    {
        WeArtGraspProvider.OnGrasp += XRIGrasp;
        WeArtGraspProvider.OnRelease += XRIRelease;
    }
    
    // Unregister from events
    private void OnDisable()
    {
        WeArtGraspProvider.OnGrasp -= XRIGrasp;
        WeArtGraspProvider.OnRelease -= XRIRelease;
    }

    // Handle grasp event
    public void XRIGrasp(GameObject grabbed, HandSide hand) {
        switch (hand)
        {
            case HandSide.Left:
                HandleMultiHandGrasp(grabbed, leftDirectInteractor, rightDirectInteractor, HandSideFlags.Right);
                break;
            case HandSide.Right:
                HandleMultiHandGrasp(grabbed, rightDirectInteractor, leftDirectInteractor, HandSideFlags.Left);
                break;
        }
    }

    /// <summary>
    /// Handle multi-hand grasping logic
    /// </summary>
    /// <param name="grabbed">Object grabbed</param>
    /// <param name="first">Hand that is going to grab the object</param>
    /// <param name="second">Hand that will release the object</param>
    /// <param name="side">Side of the hand that will release the object</param>
    void HandleMultiHandGrasp(GameObject grabbed, XRDirectInteractor first, XRDirectInteractor second, HandSideFlags side) {
        if (grabbed.GetComponent<XRGrabInteractable>().selectMode == InteractableSelectMode.Single)
        {
            if (grabbed.GetComponent<XRGrabInteractable>().interactorsSelecting.Count == 0)
            {
                first.StartManualInteraction(grabbed.GetComponent<IXRSelectInteractable>());
            }
            else
            {
                if (grabbed.GetComponent<XRGrabInteractable>().interactorsSelecting[0] == second.GetComponent<IXRSelectInteractor>() &&
                    grabbed.GetComponent<WeArtTouchableObject>().GraspingHandController.GetHandSideFlag() == side)
                {
                    grabbed.GetComponent<WeArtTouchableObject>().GraspingHandController.GetComponent<WeArtHandGraspingSystem>().ReleaseTouchableObject();
                }
                else
                {
                    grabbed.GetComponent<WeArtTouchableObject>().OtherGraspingHandController.GetComponent<WeArtHandGraspingSystem>().ReleaseTouchableObject();
                }
                first.StartManualInteraction(grabbed.GetComponent<IXRSelectInteractable>());
            }
        }
        else
        {
            first.StartManualInteraction(grabbed.GetComponent<IXRSelectInteractable>());
        }
    }

    // Handle release event
    public void XRIRelease(GameObject released, HandSide hand) {
        switch (hand)
        {
            case HandSide.Left:
                if(released.GetComponent<XRGrabInteractable>().interactorsSelecting.Contains(leftDirectInteractor.GetComponent<IXRSelectInteractor>()))
                    leftDirectInteractor.EndManualInteraction();
                break;
            case HandSide.Right:
                if (released.GetComponent<XRGrabInteractable>().interactorsSelecting.Contains(rightDirectInteractor.GetComponent<IXRSelectInteractor>()))
                    rightDirectInteractor.EndManualInteraction();
                break;
        }
    }
}

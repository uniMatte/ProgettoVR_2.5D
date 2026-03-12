using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using WeArt.Components;
using WeArt.Core;
using TMPro;
using UnityEditor;

public class WeArtEasyGraspRecorder : MonoBehaviour
{
    private WeArtHandController _handControllerRight;
    private WeArtHandController _handControllerLeft;
    private WeArtHandController[] _weArtHandControllers;
    [SerializeField] TextMeshProUGUI _rightHandText;
    [SerializeField] TextMeshProUGUI _leftHandText;

    private WeArtTouchableObject _rightHandRecordedObject;
    private WeArtTouchableObject _leftHandRecordedObject;

    void Start()
    {
        FindAllReferences();
    }

    void FindAllReferences()
    {


#if UNITY_6000_0_OR_NEWER
        _weArtHandControllers = FindObjectsByType<WeArtHandController>(FindObjectsSortMode.None);
#else
        _weArtHandControllers = FindObjectsOfType<WeArtHandController>();
#endif

        _handControllerRight = null;
        _handControllerLeft = null;

        foreach (var item in _weArtHandControllers)
        {
            if (item._handSide == HandSide.Right)
            {
                _handControllerRight = item;
            }

            if (item._handSide == HandSide.Left)
            {
                _handControllerLeft = item;
            }
        }
    }

    void Update()
    {
        //Right Hand
        if (_handControllerRight != null && _handControllerRight.GetGraspingSystem() != null)
        {
            _rightHandText.text = "Right Hand:";

            if (_handControllerRight.GetGraspingSystem().GetGraspedObject() != null && _handControllerRight.GetGraspingSystem().GetGraspedObject().GraspingType == GraspingType.Physical)
            {
                if (_rightHandRecordedObject == null)
                {
                    _rightHandText.text = "Right Hand: " + _handControllerRight.GetGraspingSystem().GetGraspedObject().name + " (Press P on the keyboard to record).";
                }
                else
                {
                    _rightHandText.color = Color.green;
                    _rightHandText.text = "Right Hand: " + _handControllerRight.GetGraspingSystem().GetGraspedObject().name + " Recorded!";
                }

                if (Input.GetKeyDown(KeyCode.P))
                {
                    _rightHandRecordedObject = _handControllerRight.GetGraspingSystem().GetGraspedObject();
                    _rightHandRecordedObject.SaveEasyGraspPose();
                }
            }
            else
            {
                _rightHandText.color = Color.white;
                _rightHandRecordedObject = null;
                _rightHandText.text = "Right Hand: " + "You need to grasp an object with grasping type set to physical, before recording the pose.";
            }

        }
        else
        {
            _rightHandText.text = "Right hand not set up:";
        }

        //Left Hand
        if (_handControllerLeft != null && _handControllerLeft.GetGraspingSystem() != null)
        {
            _leftHandText.text = "Left Hand:";

            if (_handControllerLeft.GetGraspingSystem().GetGraspedObject() != null && _handControllerLeft.GetGraspingSystem().GetGraspedObject().GraspingType == GraspingType.Physical)
            {
                if (_leftHandRecordedObject == null)
                {
                    _leftHandText.text = "Left Hand: " + _handControllerLeft.GetGraspingSystem().GetGraspedObject().name + " (Press P on the keyboard to record)."; ;
                }
                else
                {
                    _leftHandText.color = Color.green;
                    _leftHandText.text = "Left Hand: " + _handControllerLeft.GetGraspingSystem().GetGraspedObject().name +" Recorded!";
                }

                if (Input.GetKeyDown(KeyCode.P))
                {
                    _leftHandRecordedObject = _handControllerLeft.GetGraspingSystem().GetGraspedObject();
                    _leftHandRecordedObject.SaveEasyGraspPose();
                }
            }
            else
            {
                _leftHandText.color = Color.white;
                _leftHandRecordedObject = null;
                _leftHandText.text = "Left Hand: " + "You need to grasp an object with grasping type set to physical, before recording the pose.";
            }

        }
        else
        {
            _leftHandText.text = "Left hand not set up:";
        }
    }
}

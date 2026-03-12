using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeArt.Core;

public class ViveTrackerCalibrationManager : MonoBehaviour
{
    public HandSide handSide;
    [Header("Vive Tracker Transforms")]
    public Transform RightViveTracker;
    public Transform RightViveTrackerTarget;
    public Transform RightViveTrackerTransformation;
    public Transform LeftViveTracker;
    public Transform LeftViveTrackerTarget;
    public Transform LeftViveTrackerTransformation;
    public Transform OffsetOrigin;
    Vector3 translateVector;
    float angleOffset;
    Vector3 positionOffset;
    void Start()
    {
        
    }

    void Update()
    {
        RightViveTrackerTransformation.position = RightViveTracker.position;
        
        translateVector = RightViveTrackerTransformation.position + RightViveTracker.up;
        translateVector.y = RightViveTrackerTransformation.position.y;
        RightViveTrackerTransformation.LookAt(translateVector, Vector3.up);

        LeftViveTrackerTransformation.position = LeftViveTracker.position;

        translateVector = LeftViveTrackerTransformation.position + LeftViveTracker.up;
        translateVector.y = LeftViveTrackerTransformation.position.y;
        LeftViveTrackerTransformation.LookAt(translateVector, Vector3.up);


        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (handSide == HandSide.Right)
            {
                angleOffset = RightViveTrackerTarget.rotation.eulerAngles.y - RightViveTrackerTransformation.rotation.eulerAngles.y;
                OffsetOrigin.Rotate(0, angleOffset, 0, Space.Self);

                positionOffset = RightViveTrackerTarget.position - RightViveTracker.position;
                OffsetOrigin.position += positionOffset;
            }
            else
            {
                angleOffset = LeftViveTrackerTarget.rotation.eulerAngles.y - LeftViveTrackerTransformation.rotation.eulerAngles.y;
                OffsetOrigin.Rotate(0, angleOffset, 0, Space.Self);

                positionOffset = LeftViveTrackerTarget.position - LeftViveTracker.position;
                OffsetOrigin.position += positionOffset;
            }
        }
    }
}

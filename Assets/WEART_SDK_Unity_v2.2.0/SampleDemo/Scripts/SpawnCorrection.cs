using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCorrection : MonoBehaviour
{
    public Transform XrRig;
    public Transform VrCamera;
    public Transform SpawnDirection;
    public Transform ProjectionObject;

    float height;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);
        

        PositionReset();

    }

    void PositionReset()
    {
        height = VrCamera.position.y - SpawnDirection.position.y;

        Vector3 viewDirection = VrCamera.position + VrCamera.forward;
        viewDirection = new Vector3(viewDirection.x, VrCamera.position.y, viewDirection.z);

        ProjectionObject.position = VrCamera.position;
        ProjectionObject.LookAt(viewDirection);

        Quaternion rotationOffset = SpawnDirection.rotation * Quaternion.Inverse(ProjectionObject.rotation);
        XrRig.rotation = rotationOffset * XrRig.rotation;

        Vector3 positionOffset = SpawnDirection.position + new Vector3(0, height, 0) - VrCamera.position;

        XrRig.position += positionOffset;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PositionReset();
        }
    }
}

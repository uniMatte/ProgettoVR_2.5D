using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetRigidbodies : MonoBehaviour
{
    [SerializeField]
    private List<Transform> _bodies = new List<Transform>();

    private List<Vector3> _positions = new List<Vector3>();
    private List<Vector3> _rotations = new List<Vector3>();

    void Start()
    {
        foreach (var b in _bodies)
        {
            _positions.Add(b.position);
            _rotations.Add(b.rotation.eulerAngles);
        }
    }

    public void ResetRigidBodies()
    {

        for (int i = 0; i < _bodies.Count; i++)
        {
            if (_bodies[i]!= null)
            {
                Rigidbody rB = _bodies[i].GetComponent<Rigidbody>();
                if (rB != null)
                {
                    rB.angularVelocity = Vector3.zero;

#if UNITY_6000_0_OR_NEWER
                    rB.linearVelocity = Vector3.zero;
#else
                    rB.velocity = Vector3.zero;
#endif
                }

                _bodies[i].position = _positions[i];
                _bodies[i].rotation = Quaternion.Euler(_rotations[i]);
            }
        }
    }
   
    void Update()
    {
        
    }
}

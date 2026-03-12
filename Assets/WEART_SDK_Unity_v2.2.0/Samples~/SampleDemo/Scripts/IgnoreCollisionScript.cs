using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollisionScript : MonoBehaviour
{
    [SerializeField] Collider target;
    [SerializeField] Collider[] ignoredCollision;

    void Start()
    {
        for (int i = 0; i < ignoredCollision.Length; i++)
        {
            Physics.IgnoreCollision(target, ignoredCollision[i]);
        }
    }

}

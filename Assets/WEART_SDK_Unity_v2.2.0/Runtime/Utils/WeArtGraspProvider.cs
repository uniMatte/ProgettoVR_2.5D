using System;
using UnityEngine;
using WeArt.Core;

public static class WeArtGraspProvider
{
    // Actions triggered when an object is ready/not ready to be grasped. Only for proximity objects
    public static event Action<GameObject, HandSide> OnGraspReady;
    public static event Action<GameObject, HandSide> OnGraspNotReady;

    // Actions triggered when an object is grasped/released
    public static event Action<GameObject, HandSide> OnGrasp;
    public static event Action<GameObject, HandSide> OnRelease;

    /// <summary>
    /// Calls the grasp ready event, passing the grabbed object and the hand used
    /// </summary>
    /// <param name="grabbed"></param>
    /// <param name="hand"></param>
    public static void InvokeGraspReady(GameObject grabbed, HandSide hand)
    {
        OnGraspReady?.Invoke(grabbed, hand);
    }

    /// <summary>
    /// Calls the grasp not ready event, passing the grabbed object and the hand used
    /// </summary>
    /// <param name="grabbed"></param>
    /// <param name="hand"></param>
    public static void InvokeGraspNotReady(GameObject grabbed, HandSide hand)
    {
        OnGraspNotReady?.Invoke(grabbed, hand);
    }

    /// <summary>
    /// Calls the grasp event, passing the grabbed object and the hand used
    /// </summary>
    /// <param name="grabbed"></param>
    /// <param name="hand"></param>
    public static void InvokeGrasp(GameObject grabbed, HandSide hand)
    {
        OnGrasp?.Invoke(grabbed, hand);
    }

    /// <summary>
    /// Calls the grasp event, passing the grabbed object and the hand used
    /// </summary>
    /// <param name="grabbed"></param>
    /// <param name="hand"></param>
    public static void InvokeRelease(GameObject grabbed, HandSide hand)
    {
        OnRelease?.Invoke(grabbed, hand);
    }
}
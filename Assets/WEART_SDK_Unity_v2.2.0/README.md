# Unity SDK

## Introduction

Welcome to the WEART Unity SDK documentation.

The WEART SDK allows the creation of haptic experiences in Unity by enabling interfacing with the TouchDIVER and TouchDIVER Pro devices for both PC and Android Standalone applications. The new SDK handles both platforms and allows development and testing from the Unity Editor, as well as the ability to build for Windows and standalone headsets:

* \ref idle_run_logic "Start and Stop device execution"
* \ref sensors_calibration_logic "Calibrate Thimble's sensors (only for TouchDIVER Pro)"
* \ref thimble_tracking_object "Receive tracking data from the devices"
* \ref thimble_sensors_object "Retrieve raw data from the device"
* \ref haptic_object "Send haptic effects to the devices (actuations)"
* \ref status_tracker "Read status information from the device"
* \ref hand_system "Virtual hand system for object interaction"
  * Physics-based interaction, ensuring hands do not penetrate objects with colliders
  * Grasping system managed through pressure points, leveraging physics-based constraints
  * Interaction with anchored objects, utilizing the physics engine for realistic behavior
  * Easy Grasp system, allowing objects to be grasped without physical interaction
  * Integration with Unity XR interaction
  * Gesture system for creating new interactions
  * Runtime offset adjustment, allowing adaptation to different trackers
  * UI interaction support
  * Surface exploration to maximize tactile feedback

#### TouchDIVER Pro 

![](./TDPro.png) 


#### TouchDIVER 

![](./TDmain.png)

@note For TouchDIVER it is possible to use the same WEART SDK on both platforms (Windows and Android).
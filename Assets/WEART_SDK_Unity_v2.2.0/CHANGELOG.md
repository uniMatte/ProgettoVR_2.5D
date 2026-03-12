# Change Log

All notable changes to this project will be documented in this file.

## [2.2.0]

### Changed

* Changed run/idle logic. Now, at start, the devices are in the IDLE state. Calibrating using ghost hands will trigger the enable run. It is possible to stop the devices from running by calling the StopRun() method of the WeArtController class
* Show warning sensor calibration expired date on device status panel
* BleConnectionPanel class: Device connection/disconnection are restricted to the IDLE state
* WeArtDeviceObject class: Moved device initialization on Awake method
* DLL handles battery swap
* WEARTRightHand/WEARTLeftHand prefab: Optimized hand physics by reducing the number of colliders from 26 to 17
* When using the Proximity system, it is now possible to grasp/release objects with two hands
* Now is possible to grasp Proximity touchable objects that have a child set as a trigger with the WeArtChildCollider component
* Triggers that have the WeArtChildCollider component and are children of a touchable object set as Proximity no longer generate haptic effects.
* WeArtHapticObject class: Fixed a bug that caused a console error where, in some cases, the effect property of a TouchableObject was not retrieved, resulting in a NullPointerException
* Support for delivering volume haptic effects to objects that use the Proximity system
* A touchable object set as Proximity, once grasped, now applies simulated haptic effects using the configured values
* WeArtTouchableObject class: When the \_graspable property is set to false, all grasp-related settings and events are hidden
* Rewritten BluetoothManager component to remove bugs and improve BLE connection/disconnection handling
* Solved "no actuation after device 0 disconnection" bug
* Moved WeArtButton class inside the SDK
* WeArtController class: Renamed the \_debugMessages property to \_debugMessagesOnConsole to distinguish it from the new property \_debugMessagesOnFile
* WeArtControllerEditor class: Added a new section in the Inspector dedicated to debug logs

### Added

* New WeArtController method to stop a device from running: StopRun()
* New WeArtController method to trigger sensors calibration: StartSensorsCalibration()
* New sensors calibration callbacks to handle the calibration process in a component called WeArtSensorsCalibration, with different events
* New Stop Run button to the BleConnectionPanel to stop devices from running 
* New Sensors Calibration button to the BleConnectionPanel to open the sensors calibration screen if devices are in idle
* SensorsCalibrationPanel class: New SensorsCalibrationPanel screen to manage the entire devices sensors calibration process
* WeArtStatusTracker class: Added a public ConnectedDevices property linked to the private \_connectedDevices, allowing real-time access to the list of actually connected devices
* WeArtStatusTracker class: Added a public CurrentStatus property linked to the private \_currentStatus, allowing real-time access to the current WeArtApp status
* Added GetDeviceStatus coroutine to periodically ask device status when in idle
* Added handling the case where one hand cannot start running while the other has already started.
* On BLEConnectionPanel component added a check to force show calibration when system turn to IDLE from RUNNING mode
* WeArtHandController class: Added EnableOrDisableHandVisibility method, which takes a boolean input to make the hand visible or not
* WeArtWatchPanel class: Added the private batteryPanel property and the EnableOrDisablePanels method, which takes a boolean input to make the panels visible or not
* WeArtHandGraspingSystem class: Created the EnableOrDisableHandColliders method to handle cases where the hand should not be physical during grasp
* WeArtHandsSystem class: Created new WeArtHandsSystem component, where the physical realism level of the hands can be specified through the public handPhysicsType property. With High, the hands use more colliders for increased realism; with Low, the number of colliders is reduced to 7
* Added Proximity grasping system
* WeArtController class: Added a public UseExternalGraspSystem property linked to the private \_useExternalGraspSystem to allow the possibility of using an external grasp system instead of the proprietary one, e.g., OpenXR/UnityXR.
* WeArtHandGraspingSystem class: Added the public FingerClosureGraspThreshold property linked to the private \_fingerClosureGraspThreshold and the public FingerClosureReleaseThreshold property linked to the private \_fingerClosureReleaseThreshold properties to allow the Proximity system to determine when the hand should grasp or release an object
* WeArtHandGraspingSystem class: Added the public HideDuringGrasp property linked to the private \_hideDuringGrasp and the public NotPhysicalDuringGrasp property linked to the private \_NotPhysicalDuringGrasp to control whether the hand should be visible or physical during the grasp phase
* WeArtTouchableObject class: Created four events — OnGraspReady, OnGraspNotReady, OnGrasp and OnRelease. The first two handle whether the hand enters or exits the conditions required for grasping, while the latter two manage the grasp and release of the object. The events give two parameters: a GameObject representing the grasped/released object, and an enum specifying which hand (right or left) performed the action. These events can be used by an external bridge script to link events with the methods of the selected grasping system
* WeArtDeviceTrackingObject class: Added new offset presets for right and left hands in Unity XR Interaction TDPro/Meta Quest/XRI
* WeArtHandsSystem class: Now is possible to change physics hand simulation in runtime
* WeArtGraspProvider class: New static class with four actions that handle the grasp, release, ready to grasp and not ready to grasp, allowing external systems to subscribe and implement their own logic
* XRInteraction Sample Demo: New ready to use demo for working with the Unity XR Interaction grasp system with different interactions
* WeArtController class: Added the new boolean \_debugMessagesOnFile property to enable or disable writing log messages to a file when running in standalone mode
* WeArtCommon class: Added the new HandsRenderType enum to handle when the hands should be visible: Always_Visible to always show the hands, Invisible_During_Grasp to hide them while grasping an object, and Always_Invisible to always hide the hands.
* WeArtHandsSystem class: Added the new public enum HandsRenderType property linked to the private \_handsRenderType property to manage when the hands should be visible.
* New .dll checks for conditions before send "enable run on" message: .dll checks for fw integrity/version, battery levels, battery charging ect before send enable run on packet.


### Removed

* Sending "enable run off" no longer automatically disconnects BLE devices.
* WeArtHandGraspingSystem class: Removed the public HideDuringGrasp property and the related \_hideDuringGrasp property.

## \[2.1.2]

### Changed

* The TouchDIVER G1 don't work with this version
* Logs file and trace information about the SDK

### Added

* New TrackingPerformace param on WeArtController standalone editor component to manage the BLE tracking sent data frequency LOW = 60ms and HIGH = 30ms

### Removed

* TouchDIVER G1 support



## \[2.1.1]

### Changed

* Finger tracking algorithm
* Optimized BLE communication channel

## \[2.1.0]

### Added

* Universal Rendering Pipeline (URP) is now standard for the SDK
* Support for Unity 6 in both PC and Android Standalone
* TouchDIVER Pro support for Android (Standalone)
* WeArtDeviceObject now replaces the two WeArtDevice components (\_firstDevice, \_secondDevice) on the WeArtController component.
* WeArtDevice class: Abstract base class to be extended by subclasses for handling communication with either the TD or TD Pro device via their respective DLLs.
* WeArtDeviceTD: Subclass of WeArtDevice for managing the TouchDIVER device and communicating with WeartMiddleware.dll in Standalone applications.
* WeArtDeviceTDPro: Subclass of WeArtDevice for managing the TouchDIVER Pro device and communicating with WeartApp.dll in Standalone applications.
* MockDevice folder includes:

  * MockDevice component for simulating a TD Pro device using BLE, enabling testing of WeartApp.dll in the Editor.
  * MockPackages static class with hardcoded messages to simulate communication with TD Pro. (Consider removing?)

* Added libtrackingTDPro.so for managing TD Pro finger tracking in Standalone applications.
* Battery is now being displayed on the VR hands for both TD and TDPro
* For Touch Diver Pro, signal strength is displayed on the Middleware Status panel as well as on the VR hands
* New easier way to record poses for Easy Grasp System
* Text mesh pro version inside the package file to allow compatibility with newer Unity versions.
* Touchable Objects now have the field called Disable Dynamic Force. If enabled, the fingers will feel the absolute value of force set on the object instead of of having the force being changed by pressure
* SaveCurrentRigidbodyParams() from touchable objects now saves more parameters of the rigidbody
* Raw Data works with TDPro, we can now retrieve information such as Accelerometer, Gyroscope and TimeOfFlightDistance from all fingers

### Changed

* CONNECT\_CONFIRM\_TDPRO packet added to DeviceCommands for TD Pro device connection confirm in Standalone applications.
* WeArtClientBLE now supports communication with both WeartMiddleware and WeartApp DLL
* In the TouchDIVERInfo component, the ChangeHandSide button is now set to non-interactable when using TD Pro.
* Grasping a touchable object will now remove its rigidbody and on release, the rigidbody will be created again and the saved values will be applied
* PlaygroundDemo and SimpleTemperature scenes start with the default device generation as TDPro

### Removed

* The WeArtDevice MonoBehaviour component for Standalone applications has been removed and replaced by WeArtDeviceObject.

## \[2.0.0]

### Added

* TouchDIVER Pro support for PC
* Ability to switch between TouchDIVER and TouchDIVER Pro from WeArtController
* Touchable object can now start with other touchable objects as children
* Touchable object now has direct methods that change individually the temperature force and texture, using simple parameters like floats and enums
* Automatic search for touchable object's firstCollider when the child game object that was holding the reference gets deleted
* Automatic recognition of addition or removal of child objects from touchable objects recursively
* Easy grasp system that allows to record a pose while grasping a touchable object and then the object can be snap grasped
* Ability to touch and grab children colliders of WeArtTouchableObject
* Ability to change scenes that use WeArt SDK with the prefab called WeArtSceneManager
* WeArtVolumeEffects is a component that can be placed on a touchable object with a trigger collider. It will apply the effects of the touchable object to all other touchable objects that enter its trigger
* WeArtAnchoredObject is placed on the touchable objects that have an anchored interaction, such as doors, levers, wheels and drawers
* WeArtHandSurfaceExploration, present on the hand controller game object. Will allow for an easier exploration of surfaces on touchable objects that have SurfaceExploration enabled
* WeArtHandGraspingSystem contains the grasping logic separated from the hand controller
* More stable interaction for bringing a grasped object inside trigger touchable objects
* Billboard component is a component that provides different billboard behaviors for World Space Canvases. Can be used for any game object at scene.
* Shortcuts component is a component that provides the possibility to assign the key shortcuts for useful SDK services. At this moment there is implemented the restart of calibration.
* Gesture System: it is possible to create and track the hand gestures of WeArtHandControllers.
* Teleportation System — A teleportation feature that operates using gestures. You can configure a "prepare teleport" gesture to display a teleportation laser that indicates the target location, showing whether teleportation to that spot is possible (it checks for obstacles and validates the target position) and "launch teleport" gesture to teleport to position. The teleportation target can be set as either the name of a game object or a physical layer defined in the project settings.
* WeArtTouchableObject — added public setter methods for each haptic property: SetHapticForce, SetTemperature, SetTextureType, SetTextureVelocity and SetTextureVolume.
* WeArtWristOrientationObject component that reads the TouchDiver Pro rotation data from WEART App directly. It can be used as tracking source for WeArtDeviceTrackingObject, but it requires the implementation of position tracking.

### Changed

* Touchable object does not have anymore the fields ForcedVelocity and VolumeTexture, now they belong exclusively to the Texture. In order to change Forced velocity or Volume Texture, the Texture needs to be changed
* Touchable object and Haptic object, now have inside the inspector the Texture properties, which now include Forced Velocity and Volume Texture
* Haptic object's AddEffect method now uses an effect and an optional touchable object. The touchable object reference will subscribe the haptic object directly to the touchable object and update the effect accordingly to the touchable object's changes
* Haptic object's RemoveEffect method now has no parameters as only one effect can be subscribed to the haptic object at a time
* Touchable objects no longer use collision events as all the computation now is done inside the Haptic object
* WristPanel now can be hide from inspector using it's flag. Before it will be hidden it will hide another marked panels.
* WeArtHapticObject — in case the WeArtController is used at previous scene and marked as DontDestroyOnLoad, the WeArtHapticObject enables actuations at Start method if WeArtController is previously calibrated.
* Application Status panel now supports both TD and TD\_Pro devices. Some important data displayed as icons.

### Removed

* WeArtTouchEffect's WeArtImpactInfo no longer uses collision multiplier
* Unity 2019 and Unity 2020 support

## \[1.3.0]

### Added

* With the same SDK we can now use it on both PC and Android Standalone directly on the headsets
* BleConnectionPanel is responsible for showing the touch divers available on Standalone and gives the ability to connect those to the application. The touch divers connected are responsible for changing the CalibrationManager hands positions that are available for the calibration
* ActuationPanel is able to display the effects that are applied on the haptic objects
* WristPanel presents the following functionalities from left  to right

  * Show/Hide BleConnectionPanel
  * Show/Hide MiddlewareStatusDisplay
  * Reset calibration process
  * Show/Hide the Actuation Panel

* Add WeArtTemperatureVolume that changes the temperature of the touchable objects that enter or exit the volume. This is present only when importing the sample scene
* Added new offset for tracking device:

  * HTC Wrist trackers
  * HTC XR Elite controllers
  * Pico 4 Enterprise and Pico Neo 4

### Changed

* WeArtController now has both PC and Standalone functionality, having settings for both platforms in the inspector
* Sample scene updated with the newest features
* CalibrationManager automatically shows the corresponding hands on calibration based on the touch divers connected
* During Running state, only connected hand/s is/are showed
* Improved surface exploration
* Physical hands only have four haptic objects
* Improved the hands interaction with trigger collider touchable objects, allowing to feel objects when the hand is inside a different effect trigger volume
* Scripts removed from ghost hands present in hand prefabs(WEARTRightHand/WEARTLeftHand) and replaced with WeArtGhostHandController
* Only one instance of WeArtHandController and WeArtDeviceTrackingObject are present for each hand prefab
* Improved hand grasping by adding capsule collider proximity checkers instead of box colliders
* WeArtTouchDIVER class renamed WeArtDevice
* Removed fake hands references
* All texts on MiddlewareStatus panel based on TextMeshPro format
* BLE Plugin .aar  file updated from version 2.4.0 to 2.4.3 (Working on Quest 2, HTC XR Elite, Pico 4 Enterprise and Pico 3 Neo)
* Preset offsets using OpenXR with Meta Quest and Pico XR Devices

In some particular conditions (noisy environment), we still have some performance issues using Quest 2 for Standalone applications

### Fixed

* Fixed Unity version 2019 and 2020 interaction with mesh colliders
* Fixed WeArtTouchableObject's OnDisable() not affecting WeArtHandController for non trigger colliders

## \[1.2.0]

### Added

* Add new physic hand system
* Add middleware status display component and scripts
* Add Raw Thimble Sensor data tracking (WeArtThimbleSensorObject component)
* Added sample scenes
* Editing at run-time textures
* Add calibration procedure start/stop and listener
* Add calibration UX prefab
* Add hand grasping events
* Add new default tracking message and values for closure and abduction
* Debug Actuations

## \[1.1.0]

### Added

* Added sample scenes
* Editing at run-time textures
* Add calibration procedure start/stop and listener
* Add hand grasping events
* Add new default tracking message and values for closure and abduction

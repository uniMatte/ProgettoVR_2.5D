using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using WEART;

namespace WeArt.Bluetooth
{
    /// <summary>
    /// BluetoothManager — is the main class for BLE interactions. 
    /// </summary>
    public class BluetoothManager
    {

        #region FIELDS

        #region PRIVATE

        /// <summary>
        /// The list of required Android permissions to be able to scan Bluetooth devices.
        /// Only request location permissions for Android < 12.
        /// </summary>
        private readonly string[] requiredAndroidPermissions =
#if UNITY_ANDROID && UNITY_2021_1_OR_NEWER
            new string[]
            {
        // Only BLE permissions needed for Android 12+ (SDK 31+)
        "android.permission.BLUETOOTH_SCAN",
        "android.permission.BLUETOOTH_CONNECT"
            };
#else
    new string[]
    {
        // For older Android versions, need location too
        Permission.FineLocation,
        Permission.CoarseLocation,
        "android.permission.BLUETOOTH_ADMIN",
        "android.permission.BLUETOOTH_SCAN",
        "android.permission.BLUETOOTH",
        "android.permission.BLUETOOTH_CONNECT"
    };
#endif


        /// <summary>
        /// If BLE permissions have been granted or not. 
        /// </summary>
        private bool _permissionsGranted = false;

        /// <summary>
        /// Proxy variable to use BluetoothManager class from BLE Android Native plugin. 
        /// </summary>
        private AndroidJavaObject bluetoothManager;

        /// <summary>
        /// Proxy variable to use BluetoothScanner class from BLE Android Native plugin. 
        /// </summary>
        private AndroidJavaObject bluetoothScanner;

        /// <summary>
        /// List of BLE devices within current Application session.
        /// </summary>
        private List<BleDevice> BleDevices { get; } = new List<BleDevice>();

        #endregion PRIVATE

        #region PUBLIC

        /// <summary>
        /// If the BLE scanning is active or not.
        /// </summary>
        public bool IsScanning { get; private set; }
      
        #endregion PUBLIC

        #endregion FIELDS

        #region DELEGATES/EVENTS

        /// <summary>
        /// Event when Device status is changing
        /// </summary>
        public delegate void dDeviceStatus<DeviceStatusInfo>(DeviceStatusInfo deviceStatusInfo);
        public event dDeviceStatus<DeviceStatusInfo> DeviceStatusInfoReady;

        #endregion DELEGATE/EVENTS

        #region CLASSES

        public class DeviceStatusInfo : EventArgs
        {
            public string MacAddress { get; set; }
            public DeviceStatus DeviceStatus { get; set; }

            public DeviceStatusInfo(string macAddress, DeviceStatus deviceStatus)
            {
                MacAddress = macAddress;
                DeviceStatus = deviceStatus;
            }
        }

        #endregion CLASSES

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor of the class, calls the request of required permissions from device and init the core variable for further work.
        /// </summary>
        /// <param name="deviceConnectionHandler">Method to call when connection to device is finished.</param>
        /// <param name="deviceReadyHandler">Method to call when device is ready for transhipment.</param>
        /// <param name="deviceDisconnectionHandler">Method to call when the disconnection from devices is finished(optional).</param>
        /// <param name="errorReceivedHandler">Method to call when error is occurred.</param>
        public BluetoothManager(Action<BleDevice> deviceConnectionHandler, Action<BleDevice> deviceReadyHandler, Action<BleDevice> deviceDisconnectionHandler, Action<int, string> errorReceivedHandler)
        {
            CallPermissions(this.requiredAndroidPermissions);
            Init(deviceConnectionHandler, deviceReadyHandler, deviceDisconnectionHandler, errorReceivedHandler);
        }

        #endregion CONSTRUCTORS

        #region METHODS

        #region PRIVATE

        /// <summary>
        /// Inits the internal BluetoothManager proxy to communicate with plugin.
        /// </summary>
        /// <param name="deviceConnectionHandler">Method to call when connection to device is finished.</param>
        /// <param name="deviceReadyHandler">Method to call when device is ready for transhipment.</param>
        /// <param name="deviceDisconnectionHandler">Method to call when the disconnection from devices is finished(optional).</param>
        /// <param name="errorReceivedHandler">Method to call when error is occurred.</param>
        private void Init(Action<BleDevice> deviceConnectionHandler, Action<BleDevice> deviceReadyHandler, Action<BleDevice> deviceDisconnectionHandler, Action<int, string> errorReceivedHandler)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            var connectionCallback = new ConnectionCallback(this);
            connectionCallback.OnDeviceConnected += SetDeviceStatusAsConnected;
            connectionCallback.OnDeviceConnected += deviceConnectionHandler;
            connectionCallback.OnDeviceReady += deviceReadyHandler;
            connectionCallback.OnDeviceDisconnected += OnDisconnectedAction;
            connectionCallback.OnDeviceDisconnected += deviceDisconnectionHandler;

            var errorCallback = new ErrorCallback();
            errorCallback.OnErrorReceived += errorReceivedHandler;
            errorCallback.OnErrorReceived += OnErrorAction;

            bluetoothManager = new AndroidJavaObject("it.weart.bluetooth.BleManager", currentActivity, errorCallback, connectionCallback);

            string pluginVersion = GetPluginVersion() ?? "UNDEFINED";

            WeartLogger.Log(LogLevel.DEBUG, PackTag.EVENTS, $"Android BLE plugin version {pluginVersion}");

        }

        /// <summary>
        /// Handles the device disconnection.
        /// </summary>
        /// <param name="device"></param>
        private void OnDisconnectedAction(BleDevice device)
        {

            WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"Device {device.DeviceMacAddress} disconnected. Removing it from BLE Device list...", device.DeviceID);

            BleDevices.RemoveAll(d => d.DeviceMacAddress == device.DeviceMacAddress);

            device.DeviceStatus = DeviceStatus.Unavailable;

            DeviceStatusInfo statusInfo = new DeviceStatusInfo(device.DeviceMacAddress, device.DeviceStatus);
            DeviceStatusInfoReady?.Invoke(statusInfo);

        }

        /// <summary>
        /// Updates device status when plugin calls OnRemoved callback.
        /// This callback is invoked when a device disappear from scanning results (?)
        /// </summary>
        /// <param name="device"></param>
        private void OnRemovedAction(BleDevice device)
        {

            WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"Device {device.DeviceMacAddress} removed from scanned list", device.DeviceID);

            BleDevice existingDevice = BleDevices.FirstOrDefault(d => d.DeviceMacAddress == device.DeviceMacAddress);

            if (existingDevice != null && existingDevice.DeviceStatus == DeviceStatus.Available)
            {
                BleDevices.RemoveAll(d => d.DeviceMacAddress == device.DeviceMacAddress);
            }

        }

        /// <summary>
        /// Internal handler of different errors.
        /// </summary>
        /// <param name="errorCode">Code of error from native plugin.</param>
        /// <param name="errorMessage">Error description.</param>
        private void OnErrorAction(int errorCode, string errorMessage)
        {
            WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"Android BLE plugin invoked OnError! Code:{errorCode}, Message:{errorMessage}");
        }

        #region UPDATE_DEVICE_STATUS

        /// <summary>
        /// Handle OnDeviceFound event.
        /// </summary>
        /// <param name="device">The BLE device reported by the scanner.</param>
        private void OnDeviceFoundAction(BleDevice device)
        {

            WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"BLE - OnDeviceFoundAction called: {device.DeviceMacAddress}, ID: {device.DeviceID}");

            BleDevice existingDevice = BleDevices.FirstOrDefault(d => d.DeviceMacAddress == device.DeviceMacAddress);

            if (existingDevice == null)
            {
                WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"BLE - Adding device {device.DeviceMacAddress} to BleDevices list");
                BleDevices.Add(device);
            }
            else if (existingDevice != null && existingDevice.DeviceStatus != DeviceStatus.Available)
            {
                WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"BLE - Device {existingDevice.DeviceMacAddress} became AVAILABLE");
                existingDevice.DeviceStatus = DeviceStatus.Available;
            }
            else
            {
                // Should never reach this branch...
                WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"BLE - Device {device.DeviceMacAddress} reached unexpected branch in OnDeviceFoundAction!");
            }
        
        }

        /// <summary>
        /// Set status of BLE device as Connected.
        /// </summary>
        /// <param name="device"></param>
        private void SetDeviceStatusAsConnected(BleDevice device)
        {

            BleDevice existingDevice = BleDevices.FirstOrDefault(d => d.DeviceMacAddress == device.DeviceMacAddress);

            if (existingDevice != null)
            {
                
                WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"BLE - Device {existingDevice.DeviceMacAddress} status became CONNECTED");
                existingDevice.DeviceStatus = DeviceStatus.Connected;

                DeviceStatusInfo statusInfo = new DeviceStatusInfo(existingDevice.DeviceMacAddress, existingDevice.DeviceStatus);
                DeviceStatusInfoReady?.Invoke(statusInfo);

            }
            else
            {
                WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"BLE - Device {device.DeviceMacAddress} status should become CONNECTED but does not exist in BleDevices!");
            }

        }

        /// <summary>
        /// Set status of BLE device as Connecting.
        /// </summary>
        /// <param name="device"></param>
        private void SetDeviceStatusAsConnecting(BleDevice device)
        {
            
            BleDevice existingDevice = BleDevices.FirstOrDefault(d => d.DeviceMacAddress == device.DeviceMacAddress);

            if (existingDevice != null)
            {
                
                WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"BLE - Device {existingDevice.DeviceMacAddress} status became CONNECTING");
                existingDevice.DeviceStatus = DeviceStatus.Connecting;

                DeviceStatusInfo statusInfo = new DeviceStatusInfo(existingDevice.DeviceMacAddress, existingDevice.DeviceStatus);
                DeviceStatusInfoReady?.Invoke(statusInfo);

            }
            else
            {
                WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"BLE - Device {device.DeviceMacAddress} status should become CONNECTING but does not exist in BleDevices!");
            }
        
        }

        /// <summary>
        /// Set status of BLE device as Disconnecting.
        /// </summary>
        /// <param name="device"></param>
        private void SetDeviceStatusAsDisconnecting(BleDevice device)
        {
            
            BleDevice existingDevice = BleDevices.FirstOrDefault(d => d.DeviceMacAddress == device.DeviceMacAddress);

            if (existingDevice != null)
            {
                
                WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"BLE - Device {existingDevice.DeviceMacAddress} status became DISCONNECTING");
                existingDevice.DeviceStatus = DeviceStatus.Disconnecting;

                DeviceStatusInfo statusInfo = new DeviceStatusInfo(existingDevice.DeviceMacAddress, existingDevice.DeviceStatus);
                DeviceStatusInfoReady?.Invoke(statusInfo);

            }
            else
            {
                WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"BLE - Device {device.DeviceMacAddress} status should become DISCONNECTING but does not exist in BleDevices!");
            }
        
        }

        #endregion UPDATE_DEVICE_STATUS

        #region ANDROID_PERMISSIONS

        /// <summary>
        /// Waits for till required Android permissions will be received. 
        /// </summary>
        private async Task WaitsPermissionsAsync()
        {
            while (!_permissionsGranted)
            {
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// Call for permissions withing Unity. With possibility to handle result callbacks for each permission.
        /// </summary>
        /// <param name="permissions">Array of permissions that will be called for.</param>
        private void CallPermissions(string[] permissions)
        {
#if UNITY_2021_1_OR_NEWER
            var callbacks = new PermissionCallbacks();

            callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
            callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
            Permission.RequestUserPermissions(permissions, callbacks);
#endif
        }

        private void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
        {
            WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"{permissionName} Permission Denied And Dont Ask Again");
            if (permissionName.Equals("android.permission.BLUETOOTH_SCAN")) _permissionsGranted = false;
        }
        
        private void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"{permissionName} Permission Denied.");
            if (permissionName.Equals("android.permission.BLUETOOTH_SCAN")) _permissionsGranted = false;
        }

        private void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"{permissionName} Granted");
            if (permissionName.Equals("android.permission.BLUETOOTH_SCAN")) _permissionsGranted = true;
        }
      
        #endregion ANDROID_PERMISSIONS

        #endregion PRIVATE

        #region PUBLIC 

        /// <summary>
        /// Gets the current version of BLE Android plugin.
        /// </summary>
        /// <returns></returns>
        public string GetPluginVersion()
        {
            return bluetoothManager.Call<string>("getVersion");
        }
        
        /// <summary>
        /// Starts the scan for bluetooth devices.
        /// </summary>
        /// <param name="deviceFoundHandler">Method to call when new device is found.</param>
        /// /// <param name="deviceRemovedHandler">Method to call when discovered device stops being available for connection.</param>
        public async void StartScanning(Action<BleDevice> deviceFoundHandler, Action<BleDevice> deviceRemovedHandler)
        {

            if (IsScanning)
            {
                WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, "StartScanning called while already scanning...");
                return;
            }

            if (!_permissionsGranted)
            {
                WeartLogger.Log(LogLevel.DEBUG, PackTag.EVENTS, "BLE permission not granted yet, trying to acquire it...");
                await WaitsPermissionsAsync();
            }

            var scanResultCallback = new ScanResultCallback(this);
            scanResultCallback.OnDeviceFound += OnDeviceFoundAction;
            scanResultCallback.OnDeviceFound += deviceFoundHandler;
            scanResultCallback.OnDeviceRemoved += OnRemovedAction;
            scanResultCallback.OnDeviceRemoved += deviceRemovedHandler;

            bluetoothScanner?.Dispose();
            bluetoothScanner = bluetoothManager.Call<AndroidJavaObject>("getBluetoothScanner");

            int statusCode = bluetoothScanner.Call<int>("startScan", scanResultCallback);

            if (statusCode > 0)
            {
                WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"BLE scanning returned an error! Code: {statusCode}");
                return;
            }

            WeartLogger.Log(LogLevel.DEBUG, PackTag.EVENTS, "BLE start scanning...");

            IsScanning = true;

        }
        
        /// <summary>
        /// Stops the scan for bluetooth devices.
        /// </summary>
        public void StopScanning()
        {
            if (bluetoothScanner == null)
            {
                WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, "StopScanning called but bluetoothScanner is NULL!");
                return;
            }

            bluetoothScanner.Call("stopScan");

            WeartLogger.Log(LogLevel.DEBUG, PackTag.EVENTS, "BLE stop scanning!");

            IsScanning = false;
        
        }

        /// <summary>
        /// Gets the list with currently available devices.
        /// </summary>
        /// <returns>List with scanned BleDevices. Devices without name are ignored. </returns>
        public List<BleDevice> GetAvailableBleDevices()
        {
            AndroidJavaObject kotlinMap = this.bluetoothScanner.Call<AndroidJavaObject>("getDevices");
            int size = kotlinMap.Call<int>("size");

            if (size < 1)
            {
                kotlinMap.Dispose();
                return null;
            }

            AndroidJavaObject keySet = kotlinMap.Call<AndroidJavaObject>("keySet");
            AndroidJavaObject iterator = keySet.Call<AndroidJavaObject>("iterator");

            List<BleDevice> deviceList = new List<BleDevice>();

            while (iterator.Call<bool>("hasNext"))
            {
                string macAddress = iterator.Call<string>("next");
                string deviceName = kotlinMap.Call<string>("get", macAddress);

                if (!String.IsNullOrEmpty(deviceName)) deviceList.Add(new BleDevice(macAddress, deviceName));
            }

            kotlinMap.Dispose();
            keySet.Dispose();
            iterator.Dispose();
            
            return deviceList;
        }

        /// <summary>
        /// Connects to device using its MAC address and callback to handle data from device.
        /// </summary>
        /// <param name="device">Ble device to connect.</param>
        /// <param name="dataReceiveHandler">Method to call when data received from device.</param>
        public void ConnectToDevice(BleDevice device, Action<BleDevice, sbyte[]> dataReceiveHandler)
        {
            
            SetDeviceStatusAsConnecting(device);

            var dataReceivedCallback = new DataReceivedCallback(this);

            dataReceivedCallback.OnDataReceived += dataReceiveHandler;

            int statusCode = bluetoothManager.Call<int>("connectToDevice",
                                                        device.DeviceMacAddress,
                                                        dataReceivedCallback);

            if (statusCode > 0)
            {
                WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"ConnectToDevice ({device.DeviceMacAddress}) failed! Status code is {statusCode}!");
            }
            else
            {
                WeartLogger.Log(LogLevel.DEBUG, PackTag.EVENTS, $"Connecting to device with MAC {device.DeviceMacAddress}...");
            }

        }
        
        /// <summary>
        /// Disconnects device.
        /// </summary>
        /// <param name="device">BLE device to disconnect.</param>
        public void DisconnectFromDevice(BleDevice device)
        {
            
            int statusCode = bluetoothManager.Call<int>("disconnect", 
                                                        device.DeviceMacAddress);
            
            if (statusCode > 0)
            {
                WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"DisconnectDevice ({device.DeviceMacAddress}) failed! Status code is {statusCode}!");
            }
            else
            {
                SetDeviceStatusAsDisconnecting(device);
                WeartLogger.Log(LogLevel.DEBUG, PackTag.EVENTS, $"Disconnecting from device with MAC {device.DeviceMacAddress}...");
            }
         
        }
        
        /// <summary>
        /// Sends data to connected device.
        /// </summary>
        /// <param name="device">Connected device.</param>
        /// <param name="sendingData">Data to be sent to device.</param>
        public void SendData(BleDevice device, byte[] sendingData)
        {

            string deviceMacAddress = device.DeviceMacAddress;

            if (!BleDevices.Any(d => d.DeviceMacAddress == deviceMacAddress && d.DeviceStatus == DeviceStatus.Connected))
            {
                
                if (!BleDevices.Any(d => d.DeviceMacAddress == deviceMacAddress))
                {
                    WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"SendData called but the device ({deviceMacAddress}) is not present in ConnectedDevices!");
                }
                else
                {
                    WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"SendData called but the device ({deviceMacAddress}) status is not CONNECTED!");
                }

                return;

            }

            AndroidJavaObject dataManager = bluetoothManager.Call<AndroidJavaObject>("getDataManager");

            int statusCode = dataManager.Call<int>("sendData", deviceMacAddress, sendingData);

            if (statusCode > 0)
            {
                WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"SendData ({device.DeviceMacAddress}) failed! Status code is {statusCode}!");
            }

            dataManager.Dispose();
        
        }
        
        /// <summary>
        /// Checks if such BleDevice is connected to this BluetoothManager.
        /// </summary>
        /// <param name="device">BLE Device to be checked</param>
        /// <returns>"True" if connected and "False" otherwise.</returns>
        public bool ContainsConnectedDevice(BleDevice device)
        {
            return BleDevices.Any(d => d.DeviceMacAddress == device.DeviceMacAddress && d.DeviceStatus == DeviceStatus.Connected);
        }

        /// <summary>
        /// Returns the number of devices that are currently connected.
        /// </summary>
        public int GetConnectedDeviceNumber() => BleDevices.Count(d => d.DeviceStatus == DeviceStatus.Connected);

        /// <summary>
        /// Returns the name of the device with the given MAC address, 
        /// or an empty string if it is not present in BleDevices.
        /// </summary>
        /// <param name="deviceMacAddress">MAC address of the device.</param>
        public string GetDeviceName(string deviceMacAddress)
        {
            return BleDevices.FirstOrDefault(d => d.DeviceMacAddress == deviceMacAddress)?.DeviceName ?? string.Empty;
        }

        /// <summary>
        /// Gets device from BleDevices by its MAC Address. 
        /// If this device is absent in the list it returns null.
        /// </summary>
        /// <param name="deviceMacAddress"></param>
        /// <returns></returns>
        public BleDevice GetDeviceFromMac(string deviceMacAddress)
        {
            
            return BleDevices.FirstOrDefault(d => d.DeviceMacAddress.Equals(deviceMacAddress));
   
        }

        /// <summary>
        /// Returns known BLE Device Status.
        /// </summary>
        /// <param name="bleDevice"></param>
        /// <returns></returns>
        public DeviceStatus GetDeviceStatus(BleDevice bleDevice)
        {
            
            return BleDevices.FirstOrDefault(d => d.DeviceMacAddress == bleDevice.DeviceMacAddress)?.DeviceStatus ?? DeviceStatus.Unavailable;
        
        }
        
        /// <summary>
        /// Checks if the device is connected with plugin. Useful to handle the delayed OnDisconnected events. 
        /// </summary>
        /// <param name="bleDevice"></param>
        /// <returns></returns>
        public bool CheckDeviceConnectionInPlugin(BleDevice bleDevice)
        {
            AndroidJavaObject kotlinDeviceList = this.bluetoothManager.Call<AndroidJavaObject>("getConnectedAddresses");
            int size = kotlinDeviceList.Call<int>("size");

            if (size < 1)
            {
                kotlinDeviceList.Dispose();
                return false;
            }
            
            for (int i = 0; i < size; i++)
            {
                string device = kotlinDeviceList.Call<string>("get", i);

                if (bleDevice.DeviceMacAddress.Equals(device))
                {
                    kotlinDeviceList.Dispose();
                    return true;
                }
            }

            kotlinDeviceList.Dispose();
            
            return false;
        }
                   
        /// <summary>
        /// Extracts the MAC Addresses from provided string and returns the first match or returns null if there is no MAC Address.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ExtractMacAddressFromMessage(string input)
        {
            string pattern = "([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})";
            Regex regex = new Regex(pattern);

            MatchCollection matches = regex.Matches(input);

            if (matches.Count > 0)
            {
                string macAddress = matches[0].Value.ToUpper();
                
                return macAddress;
            }
            
            return null;
        }

        #endregion PUBLIC

        #endregion METHODS

    }

}

using UnityEngine;
using System;
using WeArt.Components;
using WEARTPRO;

namespace WeArt.Bluetooth
{
    /// <summary>
    /// ScanResultCallback — proxy callback class to receive scan updates from BluetoothManager.
    /// </summary>
    public class ScanResultCallback : AndroidJavaProxy
    {
        /// <summary>
        /// Invokes if the new device is discovered.
        /// </summary>
        public Action<BleDevice> OnDeviceFound;
        /// <summary>
        /// Invokes if the discovered device stops being available for connection.
        /// </summary>
        public Action<BleDevice> OnDeviceRemoved;
        /// <summary>
        /// Invokes if scan failed.
        /// </summary>
        public Action<int> OnScanFailed;

        /// <summary>
        /// Link to bluetooth manager to handle the BLE Devices.
        /// </summary>
        private BluetoothManager bluetoothManager;

        public ScanResultCallback(BluetoothManager bluetoothManager) : base("it.weart.bluetooth.ScanResultCallback")
        {
            this.bluetoothManager = bluetoothManager;
        }

        /// <summary>
        /// Method calls by Android Native plugin within device scanning when new device is discovered.
        /// </summary>
        /// <param name="deviceMacAddress"></param>
        /// <param name="name"></param>
        public void onDeviceFound(string deviceMacAddress, string name)
        {

            WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"BLE - onDeviceFound called: {deviceMacAddress}, {name}");

            if (WeArtController.Instance.DeviceGeneration == Core.DeviceGeneration.TD && !name.Contains("Weart")) return;
            if (WeArtController.Instance.DeviceGeneration == Core.DeviceGeneration.TD_Pro && !name.Contains("WaTdPro")) return;

            this.OnDeviceFound?.Invoke(new BleDevice(deviceMacAddress, name));
        
        }

        /// <summary>
        /// Method calls by Android Native plugin within device scanning when discovered device stops being available for connection.
        /// </summary>
        /// <param name="deviceMacAddress"></param>
        public void onDeviceRemoved(string deviceMacAddress)
        {

            WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"BLE - onDeviceRemoved called: {deviceMacAddress}");

            string deviceName = bluetoothManager.GetDeviceName(deviceMacAddress);

            WeartLogger.Log(LogLevel.VERBOSE, PackTag.EVENTS, $"BLE - GetScannedDeviceName returned: {deviceName}");

            if (WeArtController.Instance.DeviceGeneration == Core.DeviceGeneration.TD && !deviceName.Contains("Weart")) return;
            if (WeArtController.Instance.DeviceGeneration == Core.DeviceGeneration.TD_Pro && !deviceName.Contains("WaTdPro")) return;

            this.OnDeviceRemoved?.Invoke(this.bluetoothManager.GetDeviceFromMac(deviceMacAddress));
        
        }

        /// <summary>
        /// Method calls by Android Native plugin within device scanning if something went wrong.
        /// </summary>
        /// <param name="errorCode"></param>
        public void onScanFailed(int errorCode)
        {
            
            WeartLogger.Log(LogLevel.ERROR, PackTag.EVENTS, $"DEVICE SCAN IS FAILED, ERROR CODE {errorCode}");

            this.OnScanFailed?.Invoke(errorCode);
        
        }
    }
}
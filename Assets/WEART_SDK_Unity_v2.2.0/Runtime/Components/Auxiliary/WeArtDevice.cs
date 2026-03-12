using System;
using System.Collections;
using UnityEngine;
using WeArt.Bluetooth;
using WeArt.Components;
using WeArt.Core;
using WeArt.Utils;
using WeartSdkPRO;
using DeviceID = WeArt.Core.DeviceID;

public abstract class WeArtDevice
{
    /// <summary>
    /// Hand Side
    /// </summary>
    protected HandSide _handSide;

    /// <summary>
    /// Device ID
    /// </summary>
    protected DeviceID _deviceID;

    /// <summary>
    /// Device Generation
    /// </summary>
    protected DeviceGeneration _deviceGeneration;

    /// <summary>
    /// BLE property
    /// </summary>
    protected BleDevice BleDevice { get; private set; }

    /// <summary>
    /// Get the protected BleDevice instance
    /// </summary>
    public BleDevice GetBleDevice() => BleDevice;

    /// <summary>
    /// Bluetooth manager property
    /// </summary>
    internal BluetoothManager _bluetoothManager;

    /// <summary>
    /// Action for receiving hand side of connecting device from Weart App
    /// </summary>
    public Action<BleDevice, HAND_SIDE, int> OnHandSideReceivedFromWeartApp;

    /// <summary>
    /// Boolean true if device is Running
    /// </summary>
    internal bool _deviceIsRunning { get; set; } = false;

    /// <summary>
    /// Ping Device (Virtual because its implementation on subclasses is optional)
    /// </summary>
    public virtual void PingDevice() { }

    /// <summary>
    /// Virtual method to EnableRun ON/OFF (Abstract because it has to implemented on subclasses)
    /// </summary>
    /// <param name="enable"></param>
    /// <param name="pack"></param>
    public abstract void RunDevice(bool enable, byte[] pack = null);

    /// <summary>
    /// Sets the link to the BluetoothManager and BlePanel. Its implementation is optional on subclasses
    /// </summary>
    /// <param name="bluetoothManager"></param>
    public virtual void SetBluetoothManager(BluetoothManager bluetoothManager) { _bluetoothManager = bluetoothManager; }

    /// <summary>
    /// Virtual method for starting WeArtDevice
    /// </summary>
    public abstract void Start();

    /// <summary>
    /// Virtual method for sending actuation data from WeArtDeviceObject actuation coroutine
    /// </summary>
    public abstract void TrySendActuationData();

    /// <summary>
    /// Virtual method for sending get device status from WeArtDeviceObject status coroutine
    /// </summary>
    public abstract void TrySendGetDeviceStatus();

    /// <summary>
    /// Virtual method to prepare start device and start calibration
    /// </summary>
    public abstract void PrepareStartDeviceAndCalibration();

    /// <summary>
    /// Virtual method to prepare start calibration
    /// </summary>
    public abstract void PrepareStartCalibration();

    /// <summary>
    /// Virtual method to prepare start IMU calibration
    /// </summary>
    public abstract void PrepareStartSensorsCalibration();

    /// <summary>
    /// Virtual method to prepare stop run
    /// </summary>
    public abstract void PrepareStopRun();

    /// <summary>
    /// Connects to TouchDiver/TouchDIVERPro ble device.
    /// </summary>
    /// <param name="device"></param>
    public virtual void ConnectDevice(BleDevice device, HandSide handSide)
    {
        if (BleDevice != null) DisconnectDevice();

        BleDevice = device;

        // Need to update _handSide only for TD, as TD Pro updates it using HandSide pack received from device
        if (_deviceGeneration == DeviceGeneration.TD)
            _handSide = handSide;

        ConnectDeviceRoutine(BleDevice);

        //Manage device connection BLE and communication
    }

    /// <summary>
    /// Disconnects connected TouchDIVER/TouchDIVERPro ble device.
    /// </summary>
    public void DisconnectDevice()
    {
        if (BleDevice == null) return;

        _bluetoothManager?.DisconnectFromDevice(BleDevice);
        WeArtController.Instance.UpdateDeviceStatus(BleDevice);

        BleDevice = null;
    }

    /// <summary>
    /// Handles TouchDIVER/TouchDIVERPro ble disconnection from device.
    /// </summary>
    public void DeviceDisconnected()
    {
        if (BleDevice == null) return;
        
        WeArtController.Instance.UpdateDeviceStatus(BleDevice);

        BleDevice = null;
    }

    /// <summary>
    /// Stops actuation coroutine if it running.
    /// </summary>
    public void StopActuationRoutine()
    {
        if (!_deviceIsRunning) return;

        _deviceIsRunning = false;
    }

    /// <summary>
    /// Connect to device method
    /// </summary>
    /// <param name="device"></param>
    private void ConnectDeviceRoutine(BleDevice device)
    {
        _bluetoothManager?.ConnectToDevice(device, ReceiveData);
    }

    /// <summary>
    /// Handles incoming data from a BLE device.
    /// This method can be overridden by derived classes to implement specific behavior
    /// </summary>
    /// <param name="device"></param>
    /// <param name="data"></param>
    public abstract void ReceiveData(BleDevice device, sbyte[] data);

    /// <summary>
    /// Sending data to connected device.
    /// </summary>
    /// <param name="data"></param>
    public void SendData(byte[] data)
    {
        _bluetoothManager?.SendData(BleDevice, data);
    }
}

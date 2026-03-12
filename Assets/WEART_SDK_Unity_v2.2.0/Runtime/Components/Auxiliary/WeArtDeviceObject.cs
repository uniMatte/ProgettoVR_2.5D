using System.Collections;
using UnityEngine;
using WeArt.Bluetooth;
using WeArt.Components;
using WeArt.Core;
using WeartSdkPRO;
using DeviceID = WeArt.Core.DeviceID;

public class WeArtDeviceObject : MonoBehaviour
{
    [SerializeField]
    internal HandSide _handSide;

    [SerializeField]
    internal DeviceID _deviceID;

    /// <summary>
    /// WeArtDevice
    /// </summary>
    private WeArtDevice _device = null;

    /// <summary>
    /// Exposer for BLE Device property of WeArtDevice
    /// </summary>
    public BleDevice BleDevice => _device?.GetBleDevice(); // Avoid NullReferenceException

    /// <summary>
    /// Actuation coroutine
    /// </summary>
    private Coroutine actuationCoroutine;

    /// <summary>
    /// Status coroutine
    /// </summary>
    private Coroutine statusCoroutine;

    /// <summary>
    /// Finger tracking performance level
    /// </summary>
    private TrackingPerformance _trackingPerformance = TrackingPerformance.HIGH;

    private void Awake()
    {
        // read tracking performance from WeArtController
        _trackingPerformance = WeArtController.Instance._trackingPerformance;

        if (WeArtController.Instance.DeviceGeneration == DeviceGeneration.TD)
            _device = new WeArtDeviceTD(WeArtController.Instance.DeviceGeneration, _handSide, _deviceID);
        else
        {
            _device = new WeArtDeviceTDPro(WeArtController.Instance.DeviceGeneration, _handSide, _deviceID, _trackingPerformance);
            _device.OnHandSideReceivedFromWeartApp += OnHandSideReceived;
        }
        _device.Start();
    }


    /// <summary>
    /// Start is called before the first frame, to create relative WeArtDevice Instances based on Device Generation
    /// </summary>
    private void Start()
    {

        // Start the coroutine for sending actuation data every 60ms
        actuationCoroutine = StartCoroutine(SendActuationDataRoutine());

        // Start the coroutine for sending get device status every 3s
        statusCoroutine = StartCoroutine(SendGetDeviceStatusRoutine());
    
    }

    /// <summary>
    /// Callback for receiving hand side from Weart App
    /// </summary>
    /// <param name="weartDevice"></param>
    /// <param name="handSide"></param>
    /// <param name="deviceID"></param>
    public void OnHandSideReceived(BleDevice BleDevice, HAND_SIDE handSide, int deviceID)
    {
        if (deviceID == (int)_deviceID)
        {
            WeArtController.Instance.OnConnectionHandSideNotity(BleDevice, handSide, deviceID);
        }
    }

    /// <summary>
    /// Update function is called every frame
    /// </summary>
    private void Update()
    {
        // Do something on Update life cycle function if needed
    }

    /// <summary>
    /// Coroutine for triggering Actuation data send
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendActuationDataRoutine()
    {
        // Will be always alive, but filtered by WeArtDevice
        while (true)
        {
            _device?.TrySendActuationData();
            yield return new WaitForSeconds(0.06f); // 60ms
        }
    }

    /// <summary>
    /// Coroutine for periodically requesting device status.
    /// </summary>
    private IEnumerator SendGetDeviceStatusRoutine()
    {
        yield return new WaitForSeconds(10f);
        while (true)
        {
            _device?.TrySendGetDeviceStatus();
            yield return new WaitForSeconds(3f); // 3s
        }
    }

    /// <summary>
    /// Send enable run
    /// </summary>
    public void EnableRunningON()
    {
        _device.PrepareStartDeviceAndCalibration();
    }

    /// <summary>
    /// Send algo reset (calibration)
    /// </summary>
    public void EnableRunningAlgoCalibration()
    {
        _device.PrepareStartCalibration();
    }

    /// <summary>
    /// Start IMU calibration via WEART App DLL
    /// </summary>
    /// <param name="enable"></param>
    public void StartSensorsCalibration()
    {
        _device.PrepareStartSensorsCalibration();
    }

    /// <summary>
    /// Stop devices from RUN via WEART App DLL
    /// </summary>
    /// <param name="enable"></param>
    public void StopRun()
    {
        _device.PrepareStopRun();
    }

    /// <summary>
    /// Connect device
    /// </summary>
    /// <param name="device"></param>
    public void ConnectDevice(BleDevice device, HandSide handSide)
    {
        _device.ConnectDevice(device, handSide);
    }

    /// <summary>
    /// Disconnect device
    /// </summary>
    public void DisconnectDevice()
    {
        _device.DisconnectDevice();
    }

    /// <summary>
    /// Sets the link to the BluetoothManager and BlePanel.
    /// </summary>
    /// <param name="bluetoothManager"></param>
    public void SetBluetoothManager(BluetoothManager bluetoothManager)
    {
        _device.SetBluetoothManager(bluetoothManager);
    }

    public void SendBLEData(byte[] data)
    {
        _device.SendData(data);
    }
}

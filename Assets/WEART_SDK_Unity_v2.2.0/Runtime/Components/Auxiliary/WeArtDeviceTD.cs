using System;
using UnityEngine;
using UnityEngine.Events;
using WeArt.Core;
using WeArt.Bluetooth;
using WeArt.Components;
using WEART;
using static WEART.Communication;
using static WEART.ActuationCommons;
using static WeArt.Bluetooth.BluetoothManager;
using DeviceID = WeArt.Core.DeviceID;
using System.Linq;
using static WEART.Status;
using WeArt.Utils;


public class WeArtDeviceTD : WeArtDevice
{
    #region EVENTS

    public class TouchDIVERStatusUpdated : UnityEvent<object> { } //To be defined
    private TouchDIVERStatusUpdated _touchDIVERStatusUpdated;

    // Create a delegate for the event
    public delegate void dDeviceInfo<DeviceInfoEventArgs>(DeviceInfoEventArgs e);
    public event dDeviceInfo<DeviceInfoEventArgs> DeviceInfoReady;

    #endregion

    #region VARIABLES

    public byte[] oldDataFirst = null;
    public byte[] oldDataSecond = null;

    internal static WeartSdkProtocol weartSdkProtocol;

    #endregion

    /// <summary>
    /// Constructor of WeArtDevice class for TD
    /// </summary>
    /// <param name="deviceGeneration"></param>
    /// <param name="handSide"></param>
    /// <param name="deviceID"></param>
    public WeArtDeviceTD(DeviceGeneration deviceGeneration, HandSide handSide, DeviceID deviceID)
    {
        _deviceGeneration = deviceGeneration;
        _handSide = handSide;
        _deviceID = deviceID;
    }

    /// <summary>
    /// Start WeArtDevice components
    /// </summary>
    public override void Start()
    {
        Debug.Log("WeArtDevice TD initialized");
        weartSdkProtocol = new WeartSdkProtocol();

        if (_weartCommunication != null)
        {
            _weartCommunication.MessageReceivedFromMiddleware += OnTrackingOutputMessageReceived;
            _weartCommunication.PackReceivedFromMiddleware += OnPackReceivedFromMiddleware;
            DeviceInfoReady += _weartCommunication.DeviceInfoReady; // Send device info to Middleware DLL
            deviceCurrentStatusReady += DeviceStatusFromMiddleware;
        }
        else
        {
            WeArtLog.Log("Communication instance is not available!");
        }
        // Additional setup specific to TD
    }

    /// <summary>
    /// Sets the link to the BluetoothManager and BlePanel.
    /// </summary>
    /// <param name="bluetoothManager"></param>
    public override void SetBluetoothManager(BluetoothManager bluetoothManager)
    {
        _bluetoothManager = bluetoothManager;
        // Set here methods from _bluetoothManager, so we are sure it is not null reference
        _bluetoothManager.DeviceStatusInfoReady += ReceiveDeviceStatus;
    }

    /// <summary>
    /// Enables / disables sending WEART actuation data request from connected TD device.
    /// </summary>
    /// <param name="enable"></param>
    /// <param name="pack"></param>
    public override void RunDevice(bool enable, byte[] pack = null)
    {
        if (BleDevice == null) return;

        if (enable)
        {
            SendData(DeviceCommands.ENABLING_RUNNING_ON);

            _deviceIsRunning = true;
            return;
        }
        StopActuationRoutine();
        SendData(DeviceCommands.ENABLING_RUNNING_OFF);
    }

    
    /// <summary>
    /// Start continuous sending the Actuation data request to connected device.
    /// </summary>
    /// <returns></returns>
    public override void TrySendActuationData()
    {
        if (BleDevice == null) return;
        if (_deviceIsRunning)
        {
            (byte[] dataToSend, bool needToBeSend) = ComposeActuationPacket((int)_deviceID, _handSide);
            if (needToBeSend)
                SendData(dataToSend);
        }
    }

    /// <summary>
    /// Start continuous sending the getDeviceStatus request to connected device.
    /// </summary>
    /// <returns></returns>
    public override void TrySendGetDeviceStatus()
    {
        if (BleDevice == null) return;
        if (!_deviceIsRunning)
        {
            SendData(DeviceCommands.GET_DEVICE_STATUS);
        }
    }

    public override void PingDevice()
    {
        Debug.Log("Hello from TD device class!");
    }


    /// <summary>
    /// Callback to receive data from connected TD device.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="data"></param>
    public override void ReceiveData(BleDevice device, sbyte[] data)
    {
        if (BleDevice == null) return;
        byte[] byteArray = new byte[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            byteArray[i] = (byte)data[i];
        }

        string message = BitConverter.ToString(byteArray).Replace("-", " ");

        if (_weartCommunication == null)
        {
            message += "\nDevice sends data, but communication instance is not available!";
            WeArtLog.Log(message);

            return;
        }
        _weartCommunication.SendMessageToMiddleware(byteArray, (int)_deviceID);
    }

    /// <summary>
    /// Message received from Middleware containing tracking algorithm message with all closure values
    /// </summary>
    /// <param name="trackingData"></param>
    private void OnTrackingOutputMessageReceived(string trackingData)
    {
        // Do something with Tracking Data
    }

    /// <summary>
    /// Extract data from actuation buffers basing on ID of the device we want to send data
    /// </summary>
    /// <param name="deviceID"></param>
    /// <param name="handSide"></param>
    /// <returns></returns>
    private (byte[], bool) ComposeActuationPacket(int deviceID, HandSide handSide)
    {
        // Associa correttamente deviceID in base a HandSide
        deviceID = (deviceID == 0 && handSide == HandSide.Left) ? 1 :
                   (deviceID == 1 && handSide == HandSide.Right) ? 0 :
                   deviceID;

        // Selezione della coda e dei dati vecchi basati su deviceID
        var (queue, oldData) = (deviceID == 0)
            ? (queueFirst, oldDataFirst)
            : (queueSecond, oldDataSecond);

        byte[] data;
        bool needToSend;

        if (!queue.IsEmpty && queue.TryPeek(out data))
        {
            needToSend = oldData == null || !oldData.SequenceEqual(data);
        }
        else
        {
            data = oldData ?? nullActPacket;
            needToSend = false;            
        }

        // Aggiorna i dati vecchi
        if (deviceID == 0) oldDataFirst = data.Clone() as byte[];
        else oldDataSecond = data.Clone() as byte[];

        return (weartSdkProtocol.RequestActAndTrackData(data), needToSend);
    }




    /// <summary>
    /// Byte pack received from Middleware to be sent to the device specified with deviceID
    /// </summary>
    /// <param name="pack"></param>
    /// <param name="deviceID"></param>
    private void OnPackReceivedFromMiddleware(byte[] pack, int deviceID)
    {
        if (WeArtUtility.EnableRunningPack(pack) && (DeviceID)deviceID == _deviceID)
        {
            if (WeArtUtility.RunON(pack))
                RunDevice(true, pack);
            else
                DisconnectDevice();
        }
        else
        {
            if ((DeviceID)deviceID == _deviceID)
            {
                try
                {
                    SendData(pack);
                }
                catch (Exception e)
                {
                    WeArtLog.Log("Error on sending data to Device nr. " + deviceID.ToString() + "\n" + e.Message);
                }
            }
            else
            {
                return;
            }
        }
    }



    /// <summary>
    /// Callback of DeviceStatusInfoReady that updates DeviceStatusInfo with the latest values
    /// </summary>
    /// <param name="deviceStatusInfo"></param>
    private void ReceiveDeviceStatus(DeviceStatusInfo deviceStatusInfo)
    {
        if (BleDevice != null)
        {
            if (BleDevice.DeviceMacAddress == deviceStatusInfo.MacAddress)
            {
                bool connected = deviceStatusInfo.DeviceStatus == DeviceStatus.Connected;
                if (deviceStatusInfo.DeviceStatus == DeviceStatus.Unavailable)
                {
                    DeviceDisconnected();
                }
                GenerateDeviceInfoEvent(connected, (int)_deviceID, (int)_handSide);
            }
        }
    }

    /// <summary>
    /// Event raise to be called by Middleware to propagate DeviceInfo
    /// </summary>
    /// <param name="connected"></param>
    /// <param name="deviceID"></param>
    /// <param name="handSide"></param>
    public void GenerateDeviceInfoEvent(bool connected, int deviceID, int handSide)
    {
        HandSide handSideM = SwitchHandSideForMiddleware((HandSide)handSide);
        DeviceInfoEventArgs info = new DeviceInfoEventArgs(connected, deviceID, (int)handSideM);
        DeviceInfoReady?.Invoke(info);
    }

    /// <summary>
    /// Switch hand side for Middleware
    /// </summary>
    /// <param name="handside"></param>
    /// <returns></returns>
    private HandSide SwitchHandSideForMiddleware(HandSide handside)
    {
        return handside == HandSide.Right ? HandSide.Left : HandSide.Right;
    }

    /// <summary>
    /// Callback of changing device status event coming from Middleware
    /// </summary>
    /// <param name="deviceStatusInfoFromMiddleware"></param>
    private void DeviceStatusFromMiddleware(DeviceStatusInfoFromMiddleware deviceStatusInfoFromMiddleware)
    {
        if ((DeviceID)deviceStatusInfoFromMiddleware.DeviceID == _deviceID)
        {
            if (deviceStatusInfoFromMiddleware.CurrentStatus == CURRENT_STATUS.CONNECTED)
            {
                WeArtController.Instance.AfterConnectionSuccessful();
            }
        }
    }

    /// <summary>
    /// Ovveride method PrepareStartDeviceAndCalibration for TD device (not implemented)
    /// </summary>
    public override void PrepareStartDeviceAndCalibration() { return; }

    /// <summary>
    /// Ovveride method PrepareStartCalibration for TD device (not implemented)
    /// </summary>
    public override void PrepareStartCalibration() { return; }

    /// <summary>
    /// Ovveride method PrepareStartSensorsCalibration for TD device (not implemented)
    /// </summary>
    public override void PrepareStartSensorsCalibration() { return; }

    /// <summary>
    /// Ovveride method PrepareStopRun for TD device (not implemented)
    /// </summary>
    public override void PrepareStopRun() { return; }

}

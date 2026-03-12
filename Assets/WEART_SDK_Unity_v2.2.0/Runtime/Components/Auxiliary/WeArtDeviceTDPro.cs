using System;
using UnityEngine;
using UnityEngine.Events;
using WeArt.Core;
using WeArt.Bluetooth;
using WeArt.Components;
using static WEARTPRO.CommunicationTDPro;
using static WEARTPRO.Actuation;
using static WEARTPRO.Actuations.Utils;
using static WeArt.Bluetooth.BluetoothManager;
using DeviceID = WeArt.Core.DeviceID;
using DeviceID_DLL = WeartSdkPRO.DeviceID;
using System.Linq;
using static WEARTPRO.StatusManager;
using WeArt.Utils;
using WeartSdkPRO;
using WEARTPRO;

public class WeArtDeviceTDPro : WeArtDevice
{
    #region EVENTS

    public class TouchDIVERStatusUpdated : UnityEvent<object> { } // To be defined
    private TouchDIVERStatusUpdated _touchDIVERStatusUpdated;

    // Create a delegate for the event
    public delegate void dDeviceInfo<DeviceInfoEventArgs>(DeviceInfoEventArgs e);
    public event dDeviceInfo<DeviceInfoEventArgs> DeviceInfoReady;

    #endregion

    internal static WeartSdkProtocol weartSdkProtocol;

    public byte[] oldDataFirstTDPro = null;
    public byte[] oldDataSecondTDPro = null;

    private TrackingPerformance _trackingPerformance;

    /// <summary>
    /// Constructor of WeArtDevice class for TD Pro
    /// </summary>
    /// <param name="deviceGeneration"></param>
    /// <param name="handSide"></param>
    /// <param name="deviceID"></param>
    public WeArtDeviceTDPro(DeviceGeneration deviceGeneration, HandSide handSide, DeviceID deviceID, TrackingPerformance trackingPerformance)
    {
        _deviceGeneration = deviceGeneration;
        _handSide = handSide;
        _deviceID = deviceID;
        _trackingPerformance = trackingPerformance;

        _weartCommunicationTDPro.SetTrackingPerformance(_trackingPerformance);
    }

    /// <summary>
    /// Start WeArtDevice TD Pro components
    /// </summary>
    public override void Start()
    {
        if (_weartCommunicationTDPro != null)
        {
            _weartCommunicationTDPro.MessageReceivedFromWeartApp += OnStringMessageReceived;
            _weartCommunicationTDPro.PackReceivedFromWeartApp += OnPackReceivedFromWeartApp;
            DeviceInfoReady += _weartCommunicationTDPro.DeviceInfoReady; //  Send device info to Weart App DLL, needed for transitional state on BLE
            deviceCurrentStatusReadyFromWeartApp += DeviceStatusFromWeartApp;
            _weartCommunicationTDPro.ReceiverAgent.OnHandSideRetrieved += OnHandSideRetrieved;
        }
        else
        {
            WeArtLog.Log("Communication instance is not available!");
        }

        weartSdkProtocol = new WeartSdkProtocol(_weartCommunicationTDPro.ProtocolType); // Based on protocol type, it will remove preamble AA or not
        // Additional setup specific to TD
    }

    /// <summary>
    /// Callback for retrieving hand side of connecting TD Pro device from WeartApp DLL
    /// </summary>
    /// <param name="weartDevice"></param>
    /// <param name="handSide"></param>
    /// <param name="deviceID"></param>
    private void OnHandSideRetrieved(WEARTDeviceG2 weartDevice, HAND_SIDE handSide, int deviceID)
    {
        // Implement OnHandSideRetrieved
        if (handSide == HAND_SIDE.RIGHT)
            _handSide = HandSide.Right;
        else if (handSide == HAND_SIDE.LEFT)
            _handSide = HandSide.Left;
        OnHandSideReceivedFromWeartApp?.Invoke(BleDevice, handSide, deviceID);
    }

    #region Methods

    /// <summary>
    /// Sets the link to the BluetoothManager and BlePanel.
    /// </summary>
    /// <param name="bluetoothManager"></param>
    public  override void SetBluetoothManager(BluetoothManager bluetoothManager)
    {
        _bluetoothManager = bluetoothManager;
        // Set here methods from _bluetoothManager, so we are sure it is not null reference
        _bluetoothManager.DeviceStatusInfoReady += ReceiveDeviceStatus;
    }

    /// <summary>
    /// Enables / disables sending WEART actuation data request from connected TD Pro device.
    /// </summary>
    /// <param name="enable"></param>
    /// <param name="pack"></param>
    public override void RunDevice(bool enable, byte[] pack = null)
    {
        if (BleDevice == null) return;

        if (enable)
        {
            _deviceIsRunning = true;
        }
        else
        { 
            StopActuationRoutine();
        }

        SendData(pack);

    }

    /// <summary>
    /// Ovveride method PrepareStartDeviceAndCalibration for TD device calling WEART APP DLL
    /// </summary>
    public override void PrepareStartDeviceAndCalibration()
    {
        _weartCommunicationTDPro.RunDevice((int)_deviceID, _trackingPerformance);
    }

    /// <summary>
    /// Ovveride method PrepareStartCalibration for TD device calling WEART APP DLL
    /// </summary>
    public override void PrepareStartCalibration() 
    {
        _weartCommunicationTDPro.ResetAlgo((int)_deviceID);
    }

    /// <summary>
    /// Ovveride method PrepareStartCalibration for TD device calling WEART APP DLL
    /// </summary>
    public override void PrepareStartSensorsCalibration()
    {
        _weartCommunicationTDPro.SendCalibrateSensors((int)_deviceID);
    }

    /// <summary>
    /// Ovveride method PrepareStopRun for TD device calling WEART APP DLL
    /// </summary>
    public override void PrepareStopRun()
    {
        _weartCommunicationTDPro.SendStopRun((int)_deviceID);
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
            WEARTPRO.WeartLogger.Log(   WEARTPRO.LogLevel.VERBOSE, WEARTPRO.PackTag.REQ,
                                        Utils.logHexPack(DeviceCommands.GET_DEVICE_STATUS_TDPRO), 
                                        (WeartSdkPRO.DeviceID)this._deviceID, 
                                        WEARTPRO.PackDescription.GetDeviceStatus);
            SendData(DeviceCommands.GET_DEVICE_STATUS_TDPRO);
        }
    }

    #endregion

    #region Communication


    public override void PingDevice()
    {
        Debug.Log("Hello from TD Pro device class!");

        // Implement other logics if needed
    }

    #endregion

    #region BLE 

    /// <summary>
    /// Callback to receive data from connected TD device.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="data"></param>
    public override void ReceiveData(BleDevice device, sbyte[] data)
    {
 
        if (data == null || data.Length < 1)
        {
            WeArtLog.LogFile(WeArt.Utils.LogEnums.LogLevel.ERROR, WeArt.Utils.LogEnums.PackTag.EVENTS, "Received empty data pack!");
            return;
        }

        // Convert sbyte[] to byte[]
        byte[] byteArray = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            byteArray[i] = (byte)data[i];
        }

        // Convert to string log
        string message = BitConverter.ToString(byteArray).Replace("-", " ");

        if (_weartCommunicationTDPro == null)
        {
            message += "\nDevice sends data, but communication instance is not available!";
            WeArtLog.LogFile(WeArt.Utils.LogEnums.LogLevel.ERROR, WeArt.Utils.LogEnums.PackTag.EVENTS, message);
            return;
        }

        // Send modified data
        _weartCommunicationTDPro.SendMessageToWeartApp(byteArray, (int)_deviceID);
    }

    #endregion

    /// <summary>
    /// Message received from Weart App containing string message
    /// </summary>
    /// <param name="message"></param>
    private void OnStringMessageReceived(string message)
    {

    }

    /// <summary>
    /// Extract data from actuation buffers basing on ID of the device we want to send data
    /// </summary>
    /// <param name="deviceID"></param>
    /// <param name="handSide"></param>
    /// <returns></returns>
    private (byte[], bool) ComposeActuationPacket(int deviceID, HandSide handSide)
    {
        // Selezione della coda e dei dati vecchi basati su deviceID
        var (queue, oldData) = (deviceID == 0)
            ? (queueFirstTDPro, oldDataFirstTDPro)
            : (queueSecondTDPro, oldDataSecondTDPro);

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
        if (deviceID == 0) oldDataFirstTDPro = data.Clone() as byte[];
        else oldDataSecondTDPro = data.Clone() as byte[];

        return (weartSdkProtocol.RequestActuationData(data, (DeviceID_DLL)deviceID), needToSend);
    }

    /// <summary>
    /// Byte pack received from Middleware to be sent to the device specified with deviceID
    /// </summary>
    /// <param name="pack"></param>
    /// <param name="deviceID"></param>
    private void OnPackReceivedFromWeartApp(byte[] pack, int deviceID)
    {
        if ((DeviceID)deviceID == _deviceID)
        {
            // If it is not an enableRunning (on or off), just forward it.
            // Otherwise handle enanbling/disablig run
            bool isEnableRunningPack = WeArtUtility.EnableRunningPack(pack, _deviceGeneration);
            if (!isEnableRunningPack)
            {
                try
                {
                    SendData(pack);
                }
                catch (Exception e)
                {
                    WeArtLog.LogFile(WeArt.Utils.LogEnums.LogLevel.ERROR, WeArt.Utils.LogEnums.PackTag.EVENTS, "Error on sending data to Device nr. " + deviceID.ToString() + "\n" + e.Message);
                }
            }
            else
            {
                if(pack[3] == 0) // ENABLE RUN OFF
                {
                    RunDevice(false, pack);
                }
                else if(pack[3] == 1) // ENABLE RUN ON
                {
                    RunDevice(true, pack);
                }
                else if(pack[3] == 2) // ENABLE RUN RESET ALGO
                {
                    // Just forward 
                    SendData(pack);
                }
                else
                {
                    // DO NOTHING
                }
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
    private void DeviceStatusFromWeartApp(DeviceStatusInfoFromWeartApp deviceStatusInfoFromMiddleware)
    {
        if ((DeviceID)deviceStatusInfoFromMiddleware.DeviceID == _deviceID)
        {
            if (deviceStatusInfoFromMiddleware.CurrentStatus == CURRENT_STATUS.CONNECTED)
            {
                WeArtController.Instance.AfterConnectionSuccessful();
            }
        }
    }
}
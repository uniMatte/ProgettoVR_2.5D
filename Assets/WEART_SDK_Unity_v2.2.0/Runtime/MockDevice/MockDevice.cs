using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WEART;
using static WEARTPRO.CommunicationTDPro;
using static WEART.ActuationCommons;
using static WeArt.Bluetooth.BluetoothManager;
using DeviceID = WeArt.Core.DeviceID;
using PackTag = WeArt.Utils.LogEnums.PackTag;
using LogLevel = WeArt.Utils.LogEnums.LogLevel;
using PackDescription = WeArt.Utils.LogEnums.PackDescription;
using System.Linq;
using static WEART.Status;
using System;
using WeArt.Core;
using WeArt.Components;
using WeArt.Bluetooth;
using WeartSdkPRO;
using WeArt.Utils;

namespace WeArt.MockDevice
{

    public class MockDevice : MonoBehaviour
    {
        private bool isConnected = false;
        private bool isRunning = false;

        // Device ID (0 o 1) configurabile dall'Inspector
        [SerializeField] private DeviceID _deviceID = 0;
        [SerializeField] private int _batteryLevel = 100; // Valore da 0 a 127
        [SerializeField] private bool _isCharging = false; // Checkbox per lo stato di carica

        void Start()
        {
            if (_weartCommunicationTDPro != null)
            {
                Debug.Log("Comunication is Opened");
                _weartCommunicationTDPro.PackReceivedFromWeartApp += OnPackReceivedFromWeartApp;
                WEARTPRO.StatusManager.deviceCurrentStatusReadyFromWeartApp += DeviceStatusFromMiddleware;
            }
            else
            {
                WeArtLog.Log("Communication instance of DLL for TD Pro is not available!");
            }

            Debug.Log("Starting connection phase...");
            StartCoroutine(ConnectionSequence(MockPackages.CONNECT_CONFIRM_RESPONSE_MOCK));
        }

        IEnumerator ConnectionSequence(byte[] pack)
        {
            Debug.Log($"Sending {WeartSdkPRO.Utils.logHexPack(pack)}");

            yield return new WaitForSeconds(0.25f); // Attesa di 500ms tra i pacchetti

            _weartCommunicationTDPro.SendMessageToWeartApp(pack, (int)_deviceID);

            yield return new WaitForSeconds(0.25f);

            // Avvia la fase di Running
            //StartCoroutine(RunningSequence());
        }

        IEnumerator RunningSequence()
        {
            isRunning = true;
            Debug.Log("Starting Running mode...");

            while (isRunning && isConnected)
            {
                byte[] trackingData = MockPackages.TRACKING_MOCK;
                byte[] newPack = new byte[trackingData.Length + 1];
                Array.Copy(trackingData, newPack, trackingData.Length);

                // Costruzione del byte 6 con batteria e stato di carica
                byte batteryByte = (byte)(_batteryLevel & 0x7F); // Prende solo i 7 bit meno significativi
                if (_isCharging)
                {
                    batteryByte |= 0x80; // Imposta il bit più significativo a 1 se in carica
                }

                newPack[6] = batteryByte; // Aggiorna il pacchetto con il valore della batteria
                newPack[newPack.Length - 1] = ComputePackChecksum(newPack, newPack.Length - 1);

                Debug.Log($"Sending Tracking Data: " + WeartSdkPRO.Utils.logHexPack(newPack));
                _weartCommunicationTDPro.SendMessageToWeartApp(newPack, (int)_deviceID);

                yield return new WaitForSeconds(0.03f); // Attesa di 30ms
            }


        }

        /// <summary>
        /// Byte pack received from WeartApp to be sent to the device specified with deviceID
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="deviceID"></param>
        private void OnPackReceivedFromWeartApp(byte[] pack, int deviceID)
        {
            Debug.Log("Received from WeartApp DLL: " + WeartSdkPRO.Utils.logHexPack(pack));

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

        public void RunDevice(bool enable)
        {
            StartCoroutine(SendPackMock(MockPackages.ENABLING_RUNNING_ON_RESPONSE_MOCK));
            if (enable)
            {
                isConnected = true;
                StartCoroutine(RunningSequence());
            }
        }

        private void SendData(byte[] pack)
        {
            switch (pack[0])
            {
                case 0x02: // Config param
                    switch (pack[4])
                    {
                        case 0x32: // Device firmware version
                            StartCoroutine(SendPackMock(MockPackages.GET_DEVICE_FIRMWARE_RESPONSE_MOCK));
                            break;
                        case 0x30: // Device serial number
                            StartCoroutine(SendPackMock(MockPackages.GET_SERIAL_NUMBER_RESPONSE_MOCK));
                            break;
                        case 0x03: // Device hand side
                            StartCoroutine(SendPackMock(MockPackages.GET_DEVICE_HAND_SIDE_RESPONSE_MOCK));
                            break;
                        case 0x11: // Device texture map
                            StartCoroutine(SendPackMock(MockPackages.GET_TEXTURE_MAP_RESPONSE_MOCK));
                            break;
                    }
                    break;
                case 0x0A: // Device Status
                    StartCoroutine(SendPackMock(MockPackages.GET_DEVICE_STATUS_RESPONSE_MOCK));
                    break;
                case 0x0E: // Enable Run 

                    if (pack[3] == 0) // ENABLE RUN OFF
                    {
                        isRunning = false;
                    }
                    else if(pack[3] == 1) // ENABLE RUN ON
                    {
                        RunDevice(true);
                    }
                    else if( pack[3] == 2) // ENABLE RUN RESET ALGO
                    {
                        // DO NOTHING
                    }
                    else
                    {
                        // DO NOTHING
                    }
                        break;

            }
        }

        IEnumerator SendPackMock(byte[] pack)
        {
            byte[] newPack = new byte[pack.Length + 1];

            Array.Copy(pack, newPack, pack.Length);

            newPack[newPack.Length - 1] = ComputePackChecksum(pack, pack.Length - 1);

            yield return new WaitForSeconds(0.25f);

            Debug.Log($"Sending to Weart App DLL: {WeartSdkPRO.Utils.logHexPack(newPack)}");

            _weartCommunicationTDPro.SendMessageToWeartApp(newPack, (int)_deviceID);

            yield return new WaitForSeconds(0.25f);
        }

        /// <summary>
        /// Calculates checksum of the pack
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public byte ComputePackChecksum(byte[] dataPack, int size)
        {
            byte checksum = 0;
            for (int idx = 0; idx <= size; idx++)
            {
                checksum = (byte)(checksum ^ dataPack[idx]);
            }
            checksum = (byte)~checksum;
            return checksum;
        }

        /// <summary>
        /// Callback of changing device status event coming from Middleware
        /// </summary>
        /// <param name="deviceStatusInfoFromMiddleware"></param>
        private void DeviceStatusFromMiddleware(DeviceStatusInfoFromWeartApp deviceStatusInfoFromMiddleware)
        {
            if ((DeviceID)deviceStatusInfoFromMiddleware.DeviceID == _deviceID)
            {
                if (deviceStatusInfoFromMiddleware.CurrentStatus == WeartSdkPRO.CURRENT_STATUS.CONNECTED)
                {
                    WeArtController.Instance.AfterConnectionSuccessful();
                }
            }
        }
    }
}

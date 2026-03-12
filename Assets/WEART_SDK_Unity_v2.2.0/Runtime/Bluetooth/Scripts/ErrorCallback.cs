using UnityEngine;
using System;
using WeArt.Utils;

namespace WeArt.Bluetooth
{
    /// <summary>
    /// ErrorCallback — proxy callback class to receive and handle or debug errors from BluetoothManager.
    /// </summary>
    public class ErrorCallback : AndroidJavaProxy
    {
        public ErrorCallback() : base("it.weart.bluetooth.exception.ErrorCallback") {}

        /// <summary>
        /// Invokes if occurred an error in the BLE plugin.
        /// </summary>
        public Action<int, string> OnErrorReceived;
        
        /// <summary>
        /// Method calls by Native Android plugin if something went wrong during its activity.
        /// </summary>
        /// <param name="errorCode">Code of error. Refer to the plugin documentation.</param>
        /// <param name="message">Message with error description.</param>
        public void onError(int errorCode, string message)
        {
            WeArtLog.LogFile(Utils.LogEnums.LogLevel.ERROR, Utils.LogEnums.PackTag.EVENTS, $"PLUGIN CALLED onError EVENT: {errorCode}: {message}");

            this.OnErrorReceived?.Invoke(errorCode, message);
        }
    }
}
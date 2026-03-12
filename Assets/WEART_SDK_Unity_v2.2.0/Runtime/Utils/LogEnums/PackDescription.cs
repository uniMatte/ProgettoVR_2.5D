namespace WeArt.Utils.LogEnums
{
    // This enum has to be kept updatet according to DLL "PackDescription.cs"
    public enum PackDescription
    {
        None,
        GetDeviceStatus,        // Device Status
        GetBatteryStatus,        // Battery Status
        GetConfigParam_FV,      // Firmware Version
        GetConfigParam_HS,      // Hand Side
        GetConfigParam_SN,      // Serial Number
        GetConfigParam_TM,      // Texture Map
        GetConfigParam_ICT,     // IMU Calibration time
        SetConfigParam_ICT,     // IMU Calibration time
        EnableRunning,          // Enable Run 
        ActuationPack,          // Actuation Pack 
        TrackingPack,           // Tracking Pack
        ImuSampleRateSelection, // Imu Sample Rate
        ConnectConfirm,         // Connect Confirm
        CalibrateSensors        // Calbibrate IMU
    }
}

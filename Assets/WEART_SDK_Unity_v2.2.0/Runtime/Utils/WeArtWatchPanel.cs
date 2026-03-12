using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WeArt.Core;
using WeArt.Messages;

public class WeArtWatchPanel : MonoBehaviour
{
    [SerializeField] HandSide _handSide;
    [SerializeField] GameObject _signalPanel;
    [SerializeField] GameObject _batteryPanel;
    [SerializeField] Image _signal1;
    [SerializeField] Image _signal2;
    [SerializeField] Image _signal3;

    [SerializeField] Image _batteryLevel;
    [SerializeField] Transform _noBattery;
    [SerializeField] Image _charging;

    private RectTransform _batteryTransform;
    private GameObject _batteryLevelObj, _noBatteryObj, _chargingObj;

    private void Awake()
    {
        _batteryTransform = _batteryLevel.GetComponent<RectTransform>();
        _batteryLevelObj = _batteryLevel.gameObject;
        _noBatteryObj = _noBattery.gameObject;
        _chargingObj = _charging.gameObject;
    }

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _signalPanel.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetData(TouchDiverProStatusData data)
    {
        if (data.SignalStrength == -100 || data.SignalStrength == 0)
        {
            SetSignalData(_signal1, new Color(0.4078431f, 0.4078431f, 0.4078431f), true);
            SetSignalData(_signal2, new Color(0.4078431f, 0.4078431f, 0.4078431f), true);
            SetSignalData(_signal3, new Color(0.4078431f, 0.4078431f, 0.4078431f), true);
        }
        else {
            if (data.SignalStrength >= -60)
            {
                SetSignalData(_signal1, new Color(0, 1, 0.007843138f), true);
                SetSignalData(_signal2, new Color(0, 1, 0.007843138f), true);
                SetSignalData(_signal3, new Color(0, 1, 0.007843138f), true);
            }
            else {
                if (data.SignalStrength < -70)
                {
                    SetSignalData(_signal1, Color.red, true);
                    SetSignalData(_signal2, Color.red, false);
                    SetSignalData(_signal3, Color.red, false);
                }
                else {
                    SetSignalData(_signal1, new Color(1, 0.6470f, 0), true);
                    SetSignalData(_signal2, new Color(1, 0.6470f, 0), true);
                    SetSignalData(_signal3, Color.gray, false);
                }
            }
        }

        if (data.Master.Charging)
        {
            SetBatteryVisibility(true, false, false);
        }
        else
        {
            if (data.Master.BatteryLevel < 0)
            {
                SetBatteryVisibility(false, false, true);
            }
            else {
                if (data.Master.BatteryLevel < 10)
                {
                    SetBatteryVisibility(false, false, true);
                }
                else
                {
                    SetBatteryVisibility(false, true, false);

                    Vector2 sizeDelta = _batteryTransform.sizeDelta;
                    sizeDelta.x = data.Master.BatteryLevel;
                    _batteryTransform.sizeDelta = sizeDelta;

                    if (data.Master.BatteryLevel < 30)
                    {
                        _batteryLevel.color = new Color(1, 0.6470f, 0);
                    }
                    else
                    {
                        _batteryLevel.color = new Color(0.7960785f, 1, 0);
                    }
                }
            }
        }
    }

    void SetBatteryVisibility(bool chargingEnabled, bool batteryLevelEnabled, bool noBatteryEnabled) {
        if (_chargingObj.activeSelf != chargingEnabled) _chargingObj.SetActive(chargingEnabled);
        if (_batteryLevelObj.activeSelf != batteryLevelEnabled) _batteryLevelObj.SetActive(batteryLevelEnabled);
        if (_noBatteryObj.activeSelf != noBatteryEnabled) _noBatteryObj.SetActive(noBatteryEnabled);
    }

    void SetSignalData(Image signal, Color c, bool enable) {
        if(signal.color != c) signal.color = c;
        if(signal.gameObject.activeInHierarchy != enable) signal.gameObject.SetActive(enable);
    }

    public void SetBatteryG1(DeviceStatusData data)
    {
        Vector2 sizeDelta = _batteryTransform.sizeDelta;
        sizeDelta.x = data.BatteryLevel;
        _batteryTransform.sizeDelta = sizeDelta;

        if (data.Charging)
        {
            _charging.gameObject.SetActive(true);
            _batteryLevel.gameObject.SetActive(false);
            _noBattery.gameObject.SetActive(false);
        }
        else
        {
            if (data.BatteryLevel < 10)
            {
                _charging.gameObject.SetActive(false);
                _batteryLevel.gameObject.SetActive(false);
                _noBattery.gameObject.SetActive(true);
                return;
            }

            _charging.gameObject.SetActive(false);
            _batteryLevel.gameObject.SetActive(true);
            _noBattery.gameObject.SetActive(false);

            if (data.BatteryLevel < 30)
            {
                _batteryLevel.color = new Color(1, 0.6470f, 0);
            }
            else
            {
                _batteryLevel.color = new Color(0.7960785f, 1, 0);
            }
        }
    }

    public void SetG1()
    {
        _signalPanel.SetActive(false);
    }

    public HandSide GetHandSide()
    {
        return _handSide;
    }

    public void EnableOrDisablePanels(bool value) { 
        _batteryPanel.SetActive(value);
        _signalPanel.SetActive(value);
    }
}

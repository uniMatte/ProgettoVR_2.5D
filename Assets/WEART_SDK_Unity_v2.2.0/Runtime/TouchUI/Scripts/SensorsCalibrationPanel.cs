using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeArt.Components;
using WeArt.Core;


namespace WeArt.TouchUI
{
    [RequireComponent(typeof(WeArtSensorsCalibration))]
    /// <summary>
    /// UI Panel for sensors calibration management
    /// </summary>
    public class SensorsCalibrationPanel : MonoBehaviour
    {
        #region Fields
        [Header("WeArt elements")]
        [SerializeField] internal WeArtStatusTracker tracker;
        [SerializeField] internal BleConnectionPanel bleConnectionPanel; 

        [Header("Canvas elements")]
        [SerializeField] internal GameObject startPanel;
        [SerializeField] internal GameObject warningPanel;
        [SerializeField] internal GameObject calibrationPanel;
        [SerializeField] internal GameObject completePanel;
        [SerializeField] internal Button startCalibButton;
        [SerializeField] internal Button cancelPanelButton;
        [SerializeField] internal Button closePanelButton;
        [SerializeField] internal Image calibrationBar;
        [SerializeField] internal TextMeshProUGUI startCalibText;
        [SerializeField] internal TextMeshProUGUI finalCalibText;
        [SerializeField] internal Sprite successIcon;
        [SerializeField] internal Sprite failIcon;
        [SerializeField] internal Image resultIcon;
        [SerializeField] internal Color successColor;
        [SerializeField] internal Color failColor;

        private WeArtSensorsCalibration sensorsCalibrationManager;
        private int _calibrationStep = 0;
        private int _nOfDevicesCalibrated = 0;
        private int _calibratingTime = 15;
        private bool _isCalibrating = false;
        private bool _calibrationError = false;
        private Coroutine _calibratingCoroutine;

        #endregion

        #region Default Methods

        private void OnEnable()
        {
            Init();
        }

        private void Start()
        {
            sensorsCalibrationManager = GetComponent<WeArtSensorsCalibration>();
            SubscribePanelButtons();
            SubscribeToCalibrationEvents();
            Init();
        }
        private void Init()
        {
            if (WeArtController.Instance.DeviceGeneration != DeviceGeneration.TD_Pro) return;

            _calibrationStep = 0;
            _nOfDevicesCalibrated = 0;
            calibrationBar.fillAmount = 0;
            HandleCalibrationStepUI();
        }

        #endregion

        #region Subscribe Methods

        /// <summary>
        /// Subscribes to the sensors calibration events
        /// </summary>
        private void SubscribeToCalibrationEvents() {
            sensorsCalibrationManager._OnSensorsCalibrationResultFail.RemoveAllListeners();
            sensorsCalibrationManager._OnSensorsCalibrationResultFail.AddListener(OnSensorsCalibrationFail);
            sensorsCalibrationManager._OnSensorsCalibrationResultSuccess.RemoveAllListeners();
            sensorsCalibrationManager._OnSensorsCalibrationResultSuccess.AddListener(OnSensorsCalibrationSuccess);
            sensorsCalibrationManager._OnSensorsCalibrationStartFail.RemoveAllListeners();
            sensorsCalibrationManager._OnSensorsCalibrationStartFail.AddListener(OnSensorsCalibrationStartFail);
            sensorsCalibrationManager._OnSensorsCalibrationStartSuccess.RemoveAllListeners();
            sensorsCalibrationManager._OnSensorsCalibrationStartSuccess.AddListener(OnSensorsCalibrationStartSuccess);
        }

        /// <summary>
        /// Subscribes the buttons on the panel.
        /// </summary>
        private void SubscribePanelButtons()
        {
            startCalibButton.onClick.AddListener(NextCalibrationStep);
            cancelPanelButton.onClick.AddListener(Init);
            closePanelButton.onClick.AddListener(CloseCalibrationPanel);
        }

        #endregion

        #region Update UI

        /// <summary>
        /// Handles UI elements visibility
        /// </summary>
        private void HandleCalibrationStepUI() {
            switch (_calibrationStep)
            {
                case 0:
                    startPanel.SetActive(true);
                    warningPanel.SetActive(false);
                    calibrationPanel.SetActive(false);
                    completePanel.SetActive(false);
                    startCalibButton.gameObject.SetActive(true);
                    startCalibButton.interactable = true;
                    UpdateStartUI();
                    break;
                case 1:
                    startPanel.SetActive(false);
                    warningPanel.SetActive(true);
                    calibrationPanel.SetActive(false);
                    completePanel.SetActive(false);
                    startCalibButton.gameObject.SetActive(true);
                    startCalibButton.interactable = true;
                    break;
                case 2:
                    startPanel.SetActive(false);
                    warningPanel.SetActive(false);
                    calibrationBar.fillAmount = 0;
                    calibrationPanel.SetActive(true);
                    completePanel.SetActive(false);
                    startCalibButton.gameObject.SetActive(true);
                    startCalibButton.interactable = false;
                    _calibrationError = false;
                    _isCalibrating = false;
                    WeArtController.Instance.StartSensorsCalibration();
                    break;
                case 3:
                    startPanel.SetActive(false);
                    warningPanel.SetActive(false);
                    calibrationPanel.SetActive(false);
                    completePanel.SetActive(true);
                    UpdateFinalUI();
                    break;
            }
        }

        /// <summary>
        /// Handles UI elements visibility on first screen
        /// </summary>
        private void UpdateStartUI() {
            if (tracker != null && tracker.ConnectedDevices != null && tracker.ConnectedDevices.Count() > 0)
            {
                startCalibText.color = successColor;
                switch (tracker.ConnectedDevices.Count())
                {
                    case 1:
                        HandSide handSide = tracker.ConnectedDevices.First().HandSide;
                        startCalibText.text = $"{handSide.ToString()} device is connected and ready to be calibrated";
                        break;
                    case 2:
                        startCalibText.text = "Both devices are connected and ready to be calibrated";
                        break;
                }
            }
            else {
                startCalibButton.interactable = false;
                startCalibText.color = failColor;
                startCalibText.text = "No device is connected";
            }
        }

        /// <summary>
        /// Handles UI elements visibility on final screen
        /// </summary>
        private void UpdateFinalUI() {

            startCalibButton.gameObject.SetActive(_calibrationError);
            startCalibButton.interactable = _calibrationError;
            closePanelButton.interactable = true;
            _calibrationStep = 0;
            _nOfDevicesCalibrated = 0;

            if (_isCalibrating && _calibrationError) {
                _isCalibrating = false;
                finalCalibText.color = failColor;
                resultIcon.sprite = failIcon;
                finalCalibText.text = "The device moved during calibration. Make sure it is stable before proceeding with the calibration.";
                return;
            }

            if (_isCalibrating && !_calibrationError)
            {
                _isCalibrating = false;
                finalCalibText.color = successColor;
                resultIcon.sprite = successIcon;
                finalCalibText.text = "Sensors calibration finished successfully!";
                return;
            }

            if (!_isCalibrating && _calibrationError) {
                finalCalibText.color = failColor;
                resultIcon.sprite = failIcon;
                finalCalibText.text = "Something goes wrong during starting calibration progress. Please retry!";
                return;
            }
        }

        private void CloseCalibrationPanel() {
            bleConnectionPanel.HideSensorsCalibrationPanel();
        }

        #endregion

        #region Calibration

        /// <summary>
        /// Moves to the next step of sensors calibration process
        /// </summary>
        private void NextCalibrationStep()
        {
            _calibrationStep++;
            HandleCalibrationStepUI();
        }

        /// <summary>
        /// Starts the calibration process
        /// </summary>
        private void StartCalibrationProcess() {
            if (_isCalibrating) return;

            _isCalibrating = true;
            _calibratingCoroutine = StartCoroutine(CalibratingProcess());
        }

        private IEnumerator CalibratingProcess() {
            float time = 0;

            while (time < _calibratingTime) { 
                time += Time.deltaTime;

                calibrationBar.fillAmount = time / _calibratingTime;
                yield return null;
            }
        }

        // Sensors calibration events
        private void OnSensorsCalibrationFail(HandSide handSide) {

            if (_calibrationError || !_isCalibrating) return;

            _calibrationError = true;
            if (_calibratingCoroutine != null) StopCoroutine(_calibratingCoroutine);
            NextCalibrationStep();
        }

        private void OnSensorsCalibrationSuccess(HandSide handSide) {
            if (!_isCalibrating) return;

            _nOfDevicesCalibrated++;
            if (_nOfDevicesCalibrated != tracker.ConnectedDevices.Count) return;

            if (_calibratingCoroutine != null) StopCoroutine(_calibratingCoroutine);
            NextCalibrationStep();
        }

        private void OnSensorsCalibrationStartFail(HandSide handSide) {
            if (_calibrationError) return;

            _calibrationError = true;
            NextCalibrationStep();
        }

        private void OnSensorsCalibrationStartSuccess(HandSide handSide) {
            if (_isCalibrating) return;

            _calibrationError = false;
            closePanelButton.interactable = false;
            StartCalibrationProcess();
        }

        #endregion
    }
}

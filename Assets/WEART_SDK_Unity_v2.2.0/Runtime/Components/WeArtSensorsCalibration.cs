using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WeArt.Core;
using WeArt.Messages;
using WeArt.Utils;

namespace WeArt.Components
{
    public class WeArtSensorsCalibration : MonoBehaviour
    {
        [System.Serializable]
        public class SensorsCalibrationEvent : UnityEvent<HandSide> { }

        // Events
        [Header("Sensors Calibration Events")]

        // Unity Events
        [SerializeField]
        internal SensorsCalibrationEvent _OnSensorsCalibrationEnter;
        [SerializeField]
        internal SensorsCalibrationEvent _OnSensorsCalibrationExit;
        [SerializeField]
        internal SensorsCalibrationEvent _OnSensorsCalibrationStartSuccess;
        [SerializeField]
        internal SensorsCalibrationEvent _OnSensorsCalibrationStartFail;
        [SerializeField]
        internal SensorsCalibrationEvent _OnSensorsCalibrationResultSuccess;
        [SerializeField]
        internal SensorsCalibrationEvent _OnSensorsCalibrationResultFail;

        private Queue<IWeArtMessage> _messagesQueue = new Queue<IWeArtMessage>();
        private bool _handlingQueue;
        
        private void Init()
        {
            var client = WeArtController.Instance.Client;
            client.OnConnectionStatusChanged -= OnConnectionChanged;
            client.OnConnectionStatusChanged += OnConnectionChanged;
            client.OnMessage -= OnMessageReceived;
            client.OnMessage += OnMessageReceived;
        }

        private void OnEnable()
        {
            Init();
        }

        private void Update()
        {
            if (_messagesQueue.Count > 0 && !_handlingQueue)
            {
                StartCoroutine(MessageQueueHandler());
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
                Init();
        }
#endif

        internal void OnConnectionChanged(bool connected)
        {

        }

        private void HandleSensorsCalibrationStatus(Messages.SensorsCalibrationStatus status)
        {
            switch (status.Status)
            {
                case Core.SensorsCalibrationStatus.CALIBRATING:
                    WeArtLog.Log("_OnSensorsCalibrationEnter invoked");
                    _OnSensorsCalibrationEnter?.Invoke(status.HandSide);
                    break;
                    
                case Core.SensorsCalibrationStatus.NOT_CALIBRATING:
                    WeArtLog.Log("_OnSensorsCalibrationExit invoked");
                    _OnSensorsCalibrationExit?.Invoke(status.HandSide);
                    break;
            }
        }

        private void HandleSensorsCalibrationResult(Messages.SensorsCalibrationResult result)
        {

            if (result.Success == Core.SensorsCalibrationResult.SUCCESSFULLY_STARTED)
            {
                _OnSensorsCalibrationStartSuccess?.Invoke(result.HandSide);
                WeArtLog.Log("_OnSensorsCalibrationStartSuccess invoked");
                return;
            }
            else if (result.Success == Core.SensorsCalibrationResult.FAILED_STARTING)
            {
                _OnSensorsCalibrationStartFail?.Invoke(result.HandSide);
                WeArtLog.Log("_OnSensorsCalibrationStartFail invoked");
                return;
            }
            else if (result.Success == Core.SensorsCalibrationResult.SUCCESSFULLY_CALIBRATED)
            {
                _OnSensorsCalibrationResultSuccess?.Invoke(result.HandSide);
                WeArtLog.Log("_OnSensorsCalibrationResultSuccess invoked");
                return;
            }
            else if (result.Success == Core.SensorsCalibrationResult.FAILED_CALIBRATING)
            {
                _OnSensorsCalibrationResultFail?.Invoke(result.HandSide);
                WeArtLog.Log("_OnSensorsCalibrationResultFail invoked");
                return;
            }

            WeArtLog.Log("Received invalid Messages.SensorsCalibrationResult", LogType.Error);

        }

        private void OnMessageReceived(WeArtClient.MessageType type, IWeArtMessage message)
        {
            if (type != WeArtClient.MessageType.MessageReceived) return;
            if (message is Messages.SensorsCalibrationStatus || message is Messages.SensorsCalibrationResult)
            {
                _messagesQueue.Enqueue(message);
            }
        }

        private IEnumerator MessageQueueHandler()
        {
            _handlingQueue = true;
            
            while (_messagesQueue.Count > 0)
            {
                MessageHandler(_messagesQueue.Dequeue());
                
                yield return null;
            }

            _handlingQueue = false;
        }
        
        private void MessageHandler(IWeArtMessage message)
        {
            if (message is Messages.SensorsCalibrationStatus sensorsCalibrationStatus)
            {
                HandleSensorsCalibrationStatus(sensorsCalibrationStatus);
                return;
            }

            if (message is Messages.SensorsCalibrationResult sensorsCalibrationResult)
            {
                HandleSensorsCalibrationResult(sensorsCalibrationResult);
            }     
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using WeArt.Components;
using WeArt.Core;
using WeArt.Utils.LogEnums;

namespace WeArt.Utils
{

    internal class WeArtLogRunner : MonoBehaviour
    {
        
        private Coroutine _saveCoroutine;

        public void StartSaving(float interval)
        {
            if (_saveCoroutine == null)
                _saveCoroutine = StartCoroutine(SaveRoutine(interval));
        }

        public void StopSaving()
        {
            if (_saveCoroutine != null)
            {
                StopCoroutine(_saveCoroutine);
                _saveCoroutine = null;
            }
        }

        private IEnumerator SaveRoutine(float interval)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                WeArtLog.SaveLogFile();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            WeArtLog.LogPauseHandler(pause);
        }

        private void OnApplicationQuit()
        {
            WeArtLog.LogQuitHandler();
        }

    }

    /// <summary>
    /// Utility class used to log events and messages in the <see cref="WeArt"/> framework
    /// </summary>
    public static class WeArtLog
    {

        private static readonly object _initLock = new object();
        private static readonly object _logLock = new object();
        private static WeArtLogRunner _runner;
        private static StreamWriter _writer;
        private static FileStream _fileStream;

        private const int MaxBufferSize = 256 * 1024; // 256 KB

        private static LogLevel _currentProjectLogLevel;
        private static float _saveInterval = 30f; // seconds

        private static string Timestamp => $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
        private static StringBuilder _log = new StringBuilder();
        private static string _folderPath;
        private static string _filePath;
        
        private static bool _isLogging;
        private static int _maxLogFiles = 5;
   
        /// <summary>
        /// Starts the logging data.
        /// </summary>
        /// <param name="projectLogLevel">Max Log Level data that will be saved in the file.</param>
        public static void StartLogging(LogLevel projectLogLevel)
        {
            _currentProjectLogLevel = projectLogLevel;

            // Skip if logging to file is disabled
            if (!WeArtController.Instance._debugMessagesOnFile)
                return;

            _folderPath = Path.Combine(Application.persistentDataPath, "LogFiles");

            if (_filePath is null)
            {
                var fileName = $"LogFile_{Timestamp}.dat";
                _filePath = Path.Combine(_folderPath, fileName);

                if (!Directory.Exists(_folderPath)) Directory.CreateDirectory(_folderPath);

                if (!File.Exists(_filePath)) CheckLogFilesQuantity();

                LogFile(LogLevel.DEBUG, PackTag.EVENTS, $"Logging started with verbosity level: {projectLogLevel}");

                // Open persistent writer
                _fileStream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(_fileStream, Encoding.UTF8);
                _writer.AutoFlush = false;

            }

            StartPeriodicLogFileSavings();
        
        }

        /// <summary>
        /// Stops logging data.
        /// </summary>
        public static void StopLogging()
        {
            
            LogFile(LogLevel.DEBUG, PackTag.EVENTS, "Logging is stopped...");

            SaveLogFile();

            _filePath = null;
            _isLogging = false;

            _runner?.StopSaving();

            if (_runner != null)
            {
                UnityEngine.Object.Destroy(_runner.gameObject);
                _runner = null;
            }

            _writer?.Close();
            _writer?.Dispose();
            _writer = null;

            _fileStream?.Close();
            _fileStream?.Dispose();
            _fileStream = null;

        }

        /// <summary>
        /// Handles the logging state for OnPause Event from Unity Engine.
        /// </summary>
        /// <param name="pauseStatus"></param>
        public static void LogPauseHandler(bool pauseStatus)
        {
                        
            string content = pauseStatus ? $"Application is pausing..." : "Application resumed";
            LogFile(LogLevel.DEBUG, PackTag.EVENTS, content);

            if (pauseStatus)
            {
                SaveLogFile();
            }

        }

        /// <summary>
        /// Handles the logging state for OnQuit Event from Unity Engine.
        /// </summary>
        /// <param name="pauseStatus"></param>
        public static void LogQuitHandler()
        {

            LogFile(LogLevel.DEBUG, PackTag.EVENTS, "Application is closing...");

            SaveLogFile();

        }

        /// <summary>
        /// Logs the data to the internal logfile. 
        /// </summary>
        /// <param name="logLevel">Enum of log level.</param> 
        /// <param name="packTag">Enum of packing tag.</param> 
        /// <param name="content">Main content of log.</param>
        /// <param name="deviceID">Device ID that generated the log content. Optional.</param>
        /// <param name="packDescription"> Optional.</param>
        public static void LogFile(LogLevel logLevel, PackTag packTag, string content, DeviceID deviceID = DeviceID.None, PackDescription packDescription = PackDescription.None)
        {
            
            // Ensure logging is started
            if (_filePath is null)
            {
                lock (_initLock)
                {
                    if (_filePath is null)
                        StartLogging(WeArtController.Instance._projectLogLevel);
                }
            }

            // Skip if log level is not applicable
            if (!IsApplicableLogLevel(logLevel))
                return;

            string logDescription = string.Empty;
            string logContent;

            bool hasDevice = deviceID != DeviceID.None;
            bool hasPackDescription = packDescription != PackDescription.None;

            if (hasDevice)
            {
                if (hasPackDescription)
                {
                    // Device with pack description
                    logDescription = $"[{logLevel}] [{(int)deviceID}] [{packTag}] {packDescription}\n";
                    logContent = $"[{logLevel}] [{(int)deviceID}] [PACK] {content}\n\n";
                }
                else
                {
                    // Device without pack description
                    logContent = $"[{logLevel}] [{(int)deviceID}] [{packTag}] {content}\n\n";
                }
            }
            else
            {
                if (hasPackDescription)
                {
                    // No device, but with pack description
                    logDescription = $"[{logLevel}] [{packTag}] {packDescription}\n";
                    logContent = $"[{logLevel}] [PACK] {content}\n\n";
                }
                else
                {
                    // No device, no pack description
                    logContent = $"[{logLevel}] [{packTag}] {content}\n\n";
                }
            }

            if (!(_filePath is null))
            {
                
                bool shouldFlush = false;

                lock (_logLock)
                {
                    if (!string.IsNullOrEmpty(logDescription))
                        _log.Append($"[{Timestamp}] " + logDescription);

                    _log.Append($"[{Timestamp}] " + logContent);

                    if (_log.Length > MaxBufferSize)
                        shouldFlush = true;
                }

                if (shouldFlush)
                {
                    SaveLogFile();
                }

            }

            // Print to debug
            Log($"[WEART] {logDescription + logContent}", GetLogTypeFromLogLevel(logLevel));
        
        }

        /// <summary>
        /// Periodic saves the log data to the log file at the device.
        /// </summary>
        private static void StartPeriodicLogFileSavings()
        {
            
            if (_isLogging) return;

            _isLogging = true;

            if (_runner == null)
            {
                GameObject go = new GameObject("WeArtLogRunner");
                UnityEngine.Object.DontDestroyOnLoad(go);
                _runner = go.AddComponent<WeArtLogRunner>();
            }

            _runner.StartSaving(_saveInterval);
        
        }

        internal static void InternalSaveFromRunner()
        {
            SaveLogFile();
        }

        /// <summary>
        /// Saves the logfile to device.
        /// </summary>
        public static void SaveLogFile()
        {
            
            string contentToWrite;

            lock (_logLock)
            {
                if (_log.Length < 1)
                    return;

                contentToWrite = _log.ToString();
                _log.Clear();
            }

            _writer?.Write(contentToWrite);
            _writer?.Flush(); // Force write to disk
        
        }

        /// <summary>
        /// Checks the quantity of saved log files at the device and deletes the eldest file if their quantity is greater than _maxLogFiles.
        /// </summary>
        private static void CheckLogFilesQuantity()
        {
            var files = new DirectoryInfo(_folderPath).GetFiles();

            if (files.Length < _maxLogFiles) return;
            
            var eldestFile = files[0];

            for (int i = 1; i < files.Length; i++)
            {
                if (files[i].CreationTime < eldestFile.CreationTime)
                {
                    eldestFile = files[i];
                }
            }
                    
            File.Delete(eldestFile.FullName);
        }
        
        /// <summary>
        /// Checks the loglevel of new entity to figure out if it can be written to logfile.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        private static bool IsApplicableLogLevel(LogLevel logLevel)
        {
            return (int)_currentProjectLogLevel >= (int)logLevel;
        } 
        
        /// <summary>Logs a message in the debug console</summary>
        /// <param name="message">The string message</param>
        /// <param name="logType">The kind of log</param>
        /// <param name="onlyInDevelopmentBuild">True if this log should be ignored in normal builds</param>
        /// <param name="callerPath">The path of the caller (optional)</param>
        public static void Log(
            object message,
            LogType logType = LogType.Log,
            bool onlyInDevelopmentBuild = false,
            [CallerFilePath] string callerPath = "")
        {
            if (onlyInDevelopmentBuild && Debug.isDebugBuild)
                return;

            // Skip if logging to file is disabled
            if (!WeArtController.Instance._debugMessagesOnConsole)
                return;

            //string logMessage = $"{PathToContextString(callerPath)}: {message}";
            string logMessage = $"{message}";

            if (logType == LogType.Log)
                Debug.Log(logMessage);

            else if (logType == LogType.Warning)
                Debug.LogWarning(logMessage);

            else if (logType == LogType.Error || logType == LogType.Exception)
                Debug.LogError(logMessage);

            else if (logType == LogType.Assert)
                Debug.LogAssertion(logMessage);

        }

        private static LogType GetLogTypeFromLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.ERROR:
                    return LogType.Error;
                case LogLevel.DEBUG:
                    return LogType.Log;
                case LogLevel.VERBOSE:
                    return LogType.Log;
            }

            return LogType.Log;
        }
        
        private static readonly Dictionary<string, string> PathToContext = new Dictionary<string, string>();

        private static string PathToContextString(string path)
        {
            if (PathToContext.TryGetValue(path, out string context))
                return context;

            context = Path.GetFileNameWithoutExtension(path);
            context = $"<b>[{context}]</b>";

            PathToContext[path] = context;
            return context;
        }
    }
}
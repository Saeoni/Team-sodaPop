//using Meryel.UnityCodeAssist.Serilog;
//using Meryel.UnityCodeAssist.Serilog.Core;


using System;
using System.Runtime.InteropServices;
using Meryel.Serilog;
using Meryel.Serilog.Core;
using Meryel.Serilog.Events;
using Meryel.UnityCodeAssist.Logger;
using Meryel.UnityCodeAssist.ProjectData;
using Meryel.UnityCodeAssist.Synchronizer.Model;
using UnityEditor;
using UnityEngine;
#pragma warning disable IDE0005

#pragma warning restore IDE0005


#nullable enable


namespace Meryel.UnityCodeAssist.Editor.Logger
{
    //[InitializeOnLoad]
    public static class ELogger
    {
        // Change 'new LoggerConfiguration().MinimumLevel.Debug();' if you change these values
        private const LogEventLevel fileMinLevel = LogEventLevel.Debug;
        private const LogEventLevel outputWindowMinLevel = LogEventLevel.Information;
        private static readonly LoggingLevelSwitch? fileLevelSwitch;
        private static readonly LoggingLevelSwitch? outputWindowLevelSwitch;

        //static bool IsInitialized { get; set; }

        private static ILogEventSink? _outputWindowSink;
        private static readonly ILogEventSink? _memorySink;

        //**-- make it work with multiple clients
        private static string? _vsInternalLog;


        static ELogger()
        {
            fileLevelSwitch = null;
            outputWindowLevelSwitch = null;
            _memorySink = null;

            var isFirst = false;
            const string stateName = "isFirst";
            if (!SessionState.GetBool(stateName, false))
            {
                isFirst = true;
                SessionState.SetBool(stateName, true);
            }

            var projectPath = CommonTools.GetProjectPath();
            var outputWindowSink = new Lazy<ILogEventSink>(() => new UnityOutputWindowSink(null));

            Init(isFirst, projectPath, outputWindowSink);

            if (isFirst)
                LogHeader(Application.unityVersion, projectPath);
        }

        public static string? FilePath => UnityCodeAssist.Logger.ELogger.UnityFilePath;
        public static string? VSFilePath => UnityCodeAssist.Logger.ELogger.VisualStudioFilePath;

        public static string? VsInternalLog
        {
            get => _vsInternalLog;
            set
            {
                _vsInternalLog = value;
                OnVsInternalLogChanged?.Invoke();
            }
        }

        //**-- UI for these two
        private static bool OptionsIsLoggingToFile => true;
        private static bool OptionsIsLoggingToOutputWindow => true;
        public static event Action? OnVsInternalLogChanged;


        public static string GetInternalLogContent()
        {
            return _memorySink == null
                ? string.Empty
                : ((MemorySink)_memorySink).Export();
        }

        public static int GetErrorCountInInternalLog()
        {
            return _memorySink == null ? 0 : ((MemorySink)_memorySink).ErrorCount;
        }

        public static int GetWarningCountInInternalLog()
        {
            return _memorySink == null ? 0 : ((MemorySink)_memorySink).WarningCount;
        }

        /// <summary>
        ///     Empty method for invoking static class ctor
        /// </summary>
        public static void Bump()
        {
        }


        private static void LogHeader(string unityVersion, string solutionDir)
        {
            var os = RuntimeInformation.OSDescription;
            var assisterVersion = Assister.Version;
            var syncModel = Utilities.Version;
            var hash = CommonTools.GetHashForLogFile(solutionDir);
            var port = Utilities.GetPortForMQTTnet(solutionDir);
            Log.Debug(
                "Beginning logging {OS}, Unity {U}, Unity Code Assist {A}, Communication Protocol {SM}, Project: '{Dir}', Project Hash: {Hash}, Port: {Port}",
                os, unityVersion, assisterVersion, syncModel, solutionDir, hash, port);
        }


        public static void Init(bool isFirst, string solutionDir, Lazy<ILogEventSink> outputWindowSink)
        {
            //var solutionHash = Common.CommonTools.GetHashOfPath(solutionDir);
            var solutionHash = CommonTools.GetHashForLogFile(solutionDir); // dir is osSafePath
            _outputWindowSink ??= outputWindowSink.Value;
            var sinkWrapper = new Lazy<ILogEventSink>(() => _outputWindowSink);

            UnityCodeAssist.Logger.ELogger.Init(
                UnityCodeAssist.Logger.ELogger.State.FullyInitialized,
                UnityCodeAssist.Logger.ELogger.PackagePriority.High,
                solutionDir, solutionHash, "UnityCodeAssist", Domain.Unity,
                sinkWrapper, null, null, null, null);
        }

        public static void OnOptionsChanged()
        {
            // Since we don't use LogEventLevel.Fatal, we can use it for disabling sinks

            var isLoggingToFile = OptionsIsLoggingToFile;
            var targetFileLevel = isLoggingToFile ? fileMinLevel : LogEventLevel.Fatal;
            if (fileLevelSwitch != null)
                fileLevelSwitch.MinimumLevel = targetFileLevel;

            var isLoggingToOutputWindow = OptionsIsLoggingToOutputWindow;
            var targetOutputWindowLevel = isLoggingToOutputWindow ? outputWindowMinLevel : LogEventLevel.Fatal;
            if (outputWindowLevelSwitch != null)
                outputWindowLevelSwitch.MinimumLevel = targetOutputWindowLevel;
        }
    }
}
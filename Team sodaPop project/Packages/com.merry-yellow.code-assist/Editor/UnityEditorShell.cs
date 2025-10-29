using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Meryel.Serilog;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
#pragma warning disable IDE0005

#pragma warning restore IDE0005


#nullable enable


namespace Meryel.UnityCodeAssist.Editor.Shell
{
    public class UnityEditorShell
    {
        // we are using unity actions for posterity in case we want to inspect those in-editor someday
        private static readonly List<UnityAction> ActionsQueue;

        static UnityEditorShell()
        {
            ActionsQueue = new List<UnityAction>();
            EditorApplication.update += OnUpdate;
        }

        public static string DefaultShellApp
        {
            get
            {
#if UNITY_EDITOR_WIN
                return "cmd.exe";
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                //return "bash";
                return System.IO.File.Exists("/bin/zsh") ? "/bin/zsh" : "/bin/bash";
#else
                Serilog.Log.Error("invalid platform");
                return "invalid-platform";
#endif
            }
        }

        // while running the Unity Editor update loop, we'll unqueue any tasks if such exist.
        // actions can be 
        private static void OnUpdate()
        {
            while (ActionsQueue.Count > 0)
                lock (ActionsQueue)
                {
                    var action = ActionsQueue[0];
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "error invoking shell action");
                    }
                    finally
                    {
                        ActionsQueue.RemoveAt(0);
                    }
                }
        }

        private static void Enqueue(UnityAction action)
        {
            lock (ActionsQueue)
            {
                ActionsQueue.Add(action);
            }
        }

        public static ShellCommandEditorToken Execute(string cmd)
        {
            var shellCommandEditorToken = new ShellCommandEditorToken();
            ThreadPool.QueueUserWorkItem(delegate
            {
                Process? process = null;

                try
                {
                    var processStartInfo = CreateProcessStartInfo(cmd);

                    // in case the command was already killed from the editor when the thread was queued
                    if (shellCommandEditorToken.IsKillRequested) return;

                    process = Process.Start(processStartInfo);
                    SetupProcessCallbacks(process, processStartInfo, shellCommandEditorToken);
                    ReadProcessOutput(process, shellCommandEditorToken);
                }
                catch (Exception e)
                {
                    Log.Error(e, "error starting shell");
                    process?.Close();

                    Enqueue(() =>
                    {
                        shellCommandEditorToken.FeedLog(UnityShellLogType.Error, e.ToString());
                        shellCommandEditorToken.MarkAsDone(-1);
                    });
                }
            });
            return shellCommandEditorToken;
        }

        private static ProcessStartInfo CreateProcessStartInfo(string cmd)
        {
            var processStartInfo = new ProcessStartInfo(DefaultShellApp);
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            processStartInfo.Arguments = "-c";
#elif UNITY_EDITOR_WIN
            processStartInfo.Arguments = "/c";
#endif

            processStartInfo.Arguments += " \"" + cmd + " \"";
            processStartInfo.CreateNoWindow = true;
            processStartInfo.ErrorDialog = true;
            processStartInfo.UseShellExecute = false;
            //processStartInfo.WorkingDirectory = options.WorkingDirectory == null ? "./" : options.WorkingDirectory;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.StandardOutputEncoding = Encoding.UTF8;
            processStartInfo.StandardErrorEncoding = Encoding.UTF8;
            return processStartInfo;
        }

        private static void SetupProcessCallbacks(Process process, ProcessStartInfo processStartInfo,
            ShellCommandEditorToken shellCommandEditorToken)
        {
            shellCommandEditorToken.BindProcess(process);

            process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
            {
                Log.Error("error on shell.ErrorDataReceived: {data}", e.Data);
            };
            process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
            {
                Log.Debug("shell.OutputDataReceived: {data}", e.Data);
            };
            process.Exited += delegate(object sender, EventArgs e) { Log.Debug("shell.Exited: {data}", e.ToString()); };
        }

        private static void ReadProcessOutput(Process process, ShellCommandEditorToken shellCommandEditorToken)
        {
            do
            {
                var line = process.StandardOutput.ReadLine();
                if (line == null) break;

                line = line.Replace("\\", "/");
                Enqueue(delegate { shellCommandEditorToken.FeedLog(UnityShellLogType.Log, line); });
            } while (true);

            while (true)
            {
                var error = process.StandardError.ReadLine();
                if (string.IsNullOrEmpty(error)) break;

                Enqueue(delegate { shellCommandEditorToken.FeedLog(UnityShellLogType.Error, error); });
            }

            process.WaitForExit();
            var exitCode = process.ExitCode;
            process.Close();
            Enqueue(() => { shellCommandEditorToken.MarkAsDone(exitCode); });
        }
    }

    public class ShellCommandEditorToken
    {
        private Process? _process;

        public bool IsKillRequested { get; private set; }

        public bool HasError { get; private set; }

        public int ExitCode { get; private set; }

        public bool IsDone { get; private set; }
        public event UnityAction<UnityShellLogType, string>? OnLog;
        public event UnityAction<int>? OnExit;

        internal void BindProcess(Process process)
        {
            _process = process;
        }

        internal void FeedLog(UnityShellLogType unityShellLogType, string log)
        {
            OnLog?.Invoke(unityShellLogType, log);

            if (unityShellLogType == UnityShellLogType.Error) HasError = true;
        }

        public void Kill()
        {
            if (IsKillRequested) return;

            IsKillRequested = true;
            if (_process != null)
            {
                _process.Kill();
                _process = null;
            }
            else
            {
                MarkAsDone(137);
            }
        }

        internal void MarkAsDone(int exitCode)
        {
            ExitCode = exitCode;
            IsDone = true;
            OnExit?.Invoke(exitCode);
        }

        /// <summary>
        ///     This method is intended for compiler use. Don't call it in your code.
        /// </summary>
        public ShellCommandAwaiter GetAwaiter()
        {
            return new ShellCommandAwaiter(this);
        }
    }

    public struct ShellCommandAwaiter : ICriticalNotifyCompletion
    {
        private readonly ShellCommandEditorToken _shellCommandEditorToken;

        public ShellCommandAwaiter(ShellCommandEditorToken shellCommandEditorToken)
        {
            _shellCommandEditorToken = shellCommandEditorToken;
        }

        public int GetResult()
        {
            return _shellCommandEditorToken.ExitCode;
        }

        public bool IsCompleted => _shellCommandEditorToken.IsDone;

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (IsCompleted)
                continuation();
            else
                _shellCommandEditorToken.OnExit += _ => { continuation(); };
        }
    }

    public enum UnityShellLogType
    {
        Log,
        Error
    }

    public class ShellCommandYieldable : CustomYieldInstruction
    {
        private readonly ShellCommandEditorToken _shellCommandEditorToken;

        public ShellCommandYieldable(ShellCommandEditorToken shellCommandEditorToken)
        {
            _shellCommandEditorToken = shellCommandEditorToken;
        }

        public override bool keepWaiting => !_shellCommandEditorToken.IsDone;
    }
}
using System;
using Meryel.Serilog;
using Meryel.Serilog.Core;
using Meryel.Serilog.Events;
using UnityEngine;
//using Meryel.UnityCodeAssist.Serilog;
//using Meryel.UnityCodeAssist.Serilog.Core;
//using Meryel.UnityCodeAssist.Serilog.Events;
//using Meryel.UnityCodeAssist.Serilog.Configuration;
#pragma warning disable IDE0005

#pragma warning restore IDE0005


#nullable enable


namespace Meryel.UnityCodeAssist.Editor.Logger
{
    public class UnityOutputWindowSink : ILogEventSink
    {
        private readonly IFormatProvider? _formatProvider;

        public UnityOutputWindowSink(IFormatProvider? formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent? logEvent)
        {
            if (logEvent == null)
                return;

            var message = logEvent.RenderMessage(_formatProvider, false);

            switch (logEvent.Level)
            {
                //case LogEventLevel.Verbose:
                //case LogEventLevel.Debug:
                case LogEventLevel.Information:
                    Debug.Log(message);
                    break;
                case LogEventLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                    Debug.LogError(message);
                    break;
            }
        }

        public void Dispose()
        {
        }
    }
}
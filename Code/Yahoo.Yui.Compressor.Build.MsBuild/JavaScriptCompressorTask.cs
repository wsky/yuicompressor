﻿using System;
using System.Globalization;

namespace Yahoo.Yui.Compressor.Build.MsBuild
{
    using EcmaScript.NET;

    public class JavaScriptCompressorTask : CompressorTask
    {
        private readonly IJavaScriptCompressor compressor;

        private CultureInfo threadCulture;

        public bool ObfuscateJavaScript { get; set; }

        public bool PreserveAllSemicolons { get; set; }

        public bool DisableOptimizations { get; set; }

        public string ThreadCulture { get; set; }

        public bool IsEvalIgnored { get; set; }

        public JavaScriptCompressorTask() : this(new JavaScriptCompressor())
        {
        }

        public JavaScriptCompressorTask(IJavaScriptCompressor compressor) : base(compressor)
        {
            this.compressor = compressor;
            ObfuscateJavaScript = true;
            TaskEngine.LogAdditionalTaskParameters = this.LogTaskParameters;
        }

        protected override void SetBuildParameters()
        {
            base.SetBuildParameters();
            ParseThreadCulture();
            compressor.DisableOptimizations = DisableOptimizations;
            compressor.IgnoreEval = IsEvalIgnored;
            compressor.ObfuscateJavascript = ObfuscateJavaScript;
            compressor.PreserveAllSemicolons = PreserveAllSemicolons;
            compressor.ThreadCulture = threadCulture;
            compressor.Encoding = TaskEngine.Encoding;
            compressor.ErrorReporter = new CustomErrorReporter(TaskEngine.LogType);
        }

        public override bool Execute()
        {
            try
            {
                return base.Execute();
            }
            catch (EcmaScriptException ecmaScriptException)
            {
                Log.LogError("An error occurred in parsing the Javascript file.");
                if (ecmaScriptException.LineNumber == -1)
                {
                    Log.LogError("[ERROR] {0} ********", ecmaScriptException.Message);
                }
                else
                {
                    Log.LogError(
                        "[ERROR] {0} ******** Line: {2}. LineOffset: {3}. LineSource: \"{4}\"",
                        ecmaScriptException.Message,
                        string.IsNullOrEmpty(ecmaScriptException.SourceName)
                            ? string.Empty
                            : "Source: {1}. " + ecmaScriptException.SourceName,
                        ecmaScriptException.LineNumber,
                        ecmaScriptException.ColumnNumber,
                        ecmaScriptException.LineSource);
                }
                return false;
            }
        }

        private void LogTaskParameters()
        {
            TaskEngine.Log.LogBoolean("Obfuscate Javascript", ObfuscateJavaScript);
            TaskEngine.Log.LogBoolean("Preserve semi colons", PreserveAllSemicolons);
            TaskEngine.Log.LogBoolean("Disable optimizations", DisableOptimizations);
            TaskEngine.Log.LogBoolean("Is Eval Ignored", IsEvalIgnored);
            TaskEngine.Log.LogMessage(
                "Line break position: "
                + (LineBreakPosition <= -1 ? "None" : LineBreakPosition.ToString(CultureInfo.InvariantCulture)));
            TaskEngine.Log.LogMessage("Thread Culture: " + threadCulture.DisplayName);
        }

        private void ParseThreadCulture()
        {
            if (string.IsNullOrEmpty(ThreadCulture))
            {
                threadCulture = CultureInfo.InvariantCulture;
                return;
            }

            try
            {
                switch (ThreadCulture.ToLowerInvariant())
                {
                    case "iv":
                    case "ivl":
                    case "invariantculture":
                    case "invariant culture":
                    case "invariant language":
                    case "invariant language (invariant country)":
                        {
                            threadCulture = CultureInfo.InvariantCulture;
                            break;
                        }
                    default:
                        {
                            threadCulture = CultureInfo.CreateSpecificCulture(ThreadCulture);
                            break;
                        }
                }
            }
            catch
            {
                throw new ArgumentException("Thread Culture: " + ThreadCulture + " is invalid.", "ThreadCulture");
            }
        }
    }
}
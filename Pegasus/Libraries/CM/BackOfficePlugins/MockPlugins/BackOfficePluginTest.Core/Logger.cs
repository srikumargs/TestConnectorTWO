using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sage.Connector.DomainContracts.BackOffice;

namespace BackOfficePluginTest.Core
{
    public class Logger : ILogging
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerFilePath"></param>
        /// <param name="callerLineNumber"></param>
        public void Write(
            Object callersThis,
            LogLevel level, string message, 
            [CallerMemberName]string  callerMemberName = "", 
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            WriteCore(callersThis, level, message, null, null, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callersThis"></param>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="options"></param>
        /// <param name="dataTags"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerFilePath"></param>
        /// <param name="callerLineNumber"></param>
        public void Write(
            Object callersThis,
            LogLevel level, string message, IDictionary<string, string> options, IDictionary<string, string> dataTags,
            [CallerMemberName]string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            WriteCore(callersThis, level, message, options, dataTags, callerMemberName, callerFilePath, callerLineNumber);
        }

        private void WriteCore(Object callersThis, LogLevel level, string message, IDictionary<string, string> options, IDictionary<string, string> dataTags,
            string callerMemberName,
            string callerFilePath, int callerLineNumber)
        {
            Debug.Print("Log: Level {0} - {1}{2}Member: {3} on Line: {5} of File: {4} ", level.ToString(), message, Environment.NewLine, callerMemberName, callerFilePath, callerLineNumber);
        }
    }
}

/*
 * Created by SharpDevelop.
 * User: ddur
 * Date: 12.1.2016.
 * Time: 20:15
 * 
 */
using System;
using log4net;

namespace OpenCover.Framework.Utility
{
    /// <summary>
    /// LogHelper.
    /// </summary>
    public static class LogHelper
    {
        const string loggerName = "OpenCover";

        /// <summary>
        /// Use to inform user about handled exception where appropriate (failed IO, Access Rights etc..)
        /// </summary>
        /// <param name="e"></param>
        public static void InformUser(this Exception e)
        {
            LogManager.GetLogger(loggerName).InfoFormat ("An {0} occurred: {1} ", e.GetType(), e.Message);
        }

        /// <summary>
        /// Use to inform user
        /// </summary>
        /// <param name="message"></param>
        public static void InformUser(this string message)
        {
            LogManager.GetLogger(loggerName).InfoFormat (message);
        }

        /// <summary>
        /// Use to inform user
        /// </summary>
        /// <param name="message"></param>
        public static void InformUserSoft(this string message)
        {
            LogManager.GetLogger(loggerName).DebugFormat(message);
        }
    }
}

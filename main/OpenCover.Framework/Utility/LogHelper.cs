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
        /// <summary>
        /// Use to inform user about handled exception where appropriate (failed IO, Access Rights etc..)
        /// </summary>
        /// <param name="e"></param>
        public static void InformUser(Exception e)
        {
            LogManager.GetLogger("OpenCover").InfoFormat ("An {0} occured: {1} ", e.GetType(), e.Message);
        }
    }
}

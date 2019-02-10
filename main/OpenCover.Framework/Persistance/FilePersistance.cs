//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using OpenCover.Framework.Model;
using log4net;
using File = System.IO.File;

namespace OpenCover.Framework.Persistance
{
    /// <summary>
    /// Simple file persistence of the model
    /// </summary>
    public class FilePersistance : BasePersistance
    {
        private readonly ILog _logger;

        /// <summary>
        /// Construct a file persistence object
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="logger"></param>
        public FilePersistance(ICommandLine commandLine, ILog logger) : base(commandLine, null)
        {
            _logger = logger;
        }

        private string _fileName;

        /// <summary>
        /// Initialise the file persistence
        /// </summary>
        /// <param name="fileName">The filename to save to</param>
        /// <param name="loadExisting"></param>
        public bool Initialise(string fileName, bool loadExisting)
        {
            return HandleFileAccess(() => {
                _fileName = fileName;
                if (loadExisting && File.Exists(fileName))
                {
                    LoadCoverageFile();
                }
                // test the file location can be accessed
                using (var fs = File.OpenWrite(fileName))
                    fs.Close();
            }, fileName);
        }

        internal bool HandleFileAccess(Action loadFile, string fileName)
        {
            try
            {
                loadFile();
            }
            catch (DirectoryNotFoundException ex) // issue #456
            {
                _logger.Info(
                    string.Format(
                        "Could not find the directory of the supplied coverage file '{0}', please check the path and try again.",
                        fileName));
                _logger.Debug(ex.Message, ex);
                return false;
            }
            catch (IOException ex) // issue #458
            {
                _logger.Info(
                    string.Format(
                        "Could not access the location of the supplied coverage file '{0}', please check the path and your permissions and try again.",
                        fileName));
                _logger.Debug(ex.Message, ex);
                return false;
            }
            catch (UnauthorizedAccessException ex) // issue #458
            {
                _logger.Info(
                    string.Format(
                        "Could not access the location of the supplied coverage file '{0}', please check the path and your permissions and try again.",
                        fileName));
                _logger.Debug(ex.Message, ex);
                return false;
            }
            catch (NotSupportedException ex) // issue #577
            {
                _logger.Info(
                    string.Format(
                        "Could not access the location of the supplied coverage file '{0}', please check the path and your permissions and try again.",
                        fileName));
                _logger.Debug(ex.Message, ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Info(
                    string.Format(
                        "Could not access the location of the supplied coverage file '{0}' for the following reason: {1}.",
                        fileName, ex.Message));
                _logger.Debug(ex.Message, ex);
                return false;
            }
            return true;
        }

        private void LoadCoverageFile()
        {
            try
            {
                _logger.Info(string.Format("Loading coverage file {0}", _fileName));
                ClearCoverageSession();
                var serializer = new XmlSerializer (typeof(CoverageSession),
                                                    new[] { typeof(Module), typeof(Model.File), typeof(Class) });
                using (var fs = new FileStream(_fileName, FileMode.Open)) {
                    using (var reader = new StreamReader(fs, new UTF8Encoding())) {
                        ReassignCoverageSession((CoverageSession)serializer.Deserialize(reader));
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Info(string.Format("Failed to load coverage file {0}", _fileName), ex);
                ClearCoverageSession();
            }
        }

        /// <summary>
        /// we are done and the data needs one last clean up
        /// </summary>
        public override void Commit()
        {
            _logger.Info("Committing...");
            base.Commit();
            SaveCoverageFile();
        }

        private bool SaveCoverageFile()
        {
            return HandleFileAccess(() => {
                var serializer = new XmlSerializer(typeof(CoverageSession),
                                                   new[] { typeof(Module), typeof(Model.File), typeof(Class) });

                using (var fs = new FileStream(_fileName, FileMode.Create))
                using (var writer = new StreamWriter(fs, new UTF8Encoding()))
                {
                    serializer.Serialize(writer, CoverageSession);
                }
            }, _fileName);
        }
    }
}

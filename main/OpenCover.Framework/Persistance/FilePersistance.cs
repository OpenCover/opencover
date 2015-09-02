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
        public void Initialise(string fileName, bool loadExisting)
        {
            _fileName = fileName;
            if (loadExisting && File.Exists(fileName))
            {
                LoadCoverageFile();
            }
        }

        private void LoadCoverageFile()
        {
            try
            {
                _logger.Info(string.Format("Loading coverage file {0}", _fileName));
                ClearCoverageSession();
                var serializer = new XmlSerializer(typeof(CoverageSession),
                                                    new[] { typeof(Module), typeof(Model.File), typeof(Class) });
                var fs = new FileStream(_fileName, FileMode.Open);
                var reader = new StreamReader(fs, new UTF8Encoding());
                var session = (CoverageSession)serializer.Deserialize(reader);
                reader.Close();
                ReassignCoverageSession(session);
            }
            catch(Exception ex)
            {
                _logger.Info(string.Format("Failed to load coverage file {0}", _fileName), ex);
                ClearCoverageSession();
            }
        }

        public override void Commit()
        {
            _logger.Info("Committing...");
            base.Commit();
            SaveCoverageFile();
        }

        private void SaveCoverageFile()
        {
            var serializer = new XmlSerializer(typeof (CoverageSession),
                                               new[] {typeof (Module), typeof (Model.File), typeof (Class)});
            var fs = new FileStream(_fileName, FileMode.Create);
            var writer = new StreamWriter(fs, new UTF8Encoding());
            serializer.Serialize(writer, CoverageSession);
            writer.Close();
        }
    }
}

// Copyright (c) https://github.com/ddur
// This code is distributed under MIT license

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Utility
{
    /// <summary>
    /// FileType enum
    /// </summary>
    public enum FileType : byte {
        /// <summary>
        /// Unsupported file extension
        /// </summary>
        Unsupported,

        /// <summary>
        /// File extension is ".cs"
        /// </summary>
        CSharp

    }
    /// <summary>StringTextSource (ReadOnly)
    /// <remarks>Line and column counting starts at 1.</remarks>
    /// </summary>
    public class CodeCoverageStringTextSource
    {
        /// <summary>
        /// File Type guessed by file-name extension
        /// </summary>
        public FileType FileType { get { return _fileType; } }
        private readonly FileType _fileType = FileType.Unsupported;

        /// <summary>
        /// Path to source file
        /// </summary>
        public string FilePath { get { return _filePath; } }
        private readonly string _filePath = string.Empty;

        /// <summary>
        /// Source file found or not
        /// </summary>
        public bool FileFound { get { return _fileFound; } }
        private readonly bool _fileFound;

        /// <summary>
        /// Last write DateTime
        /// </summary>
        public DateTime FileTime { get { return _fileTime; } }
        private readonly DateTime _fileTime = DateTime.MinValue;

        private readonly string _textSource;

        private struct LineInfo {
            public int Offset;
            public int Length;
        }
        private readonly LineInfo[] _lines = new LineInfo[0];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filePath"></param>
        public CodeCoverageStringTextSource(string source, string filePath)
        {
            _fileFound = source != null;

            if (!string.IsNullOrWhiteSpace (filePath)) {
                _filePath = filePath;
                if (_filePath.IndexOfAny(Path.GetInvalidPathChars()) < 0
                    && Path.GetExtension(_filePath).ToLowerInvariant() == ".cs" ) {
                    _fileType = FileType.CSharp;
                }
                if (_fileFound) {
                    try { 
                        _fileTime = System.IO.File.GetLastWriteTimeUtc (this._filePath); 
                    } catch (Exception e) {
                        e.InformUser();
                    }
                }

            }

            _textSource = string.IsNullOrEmpty(source) ? string.Empty : source;

            if (_textSource != string.Empty) {
                _lines = InitLines ();
            }

        }

        private LineInfo[] InitLines ()
        {
            int offset = 0;
            int counter = 0;
            var lineInfoList = new List<LineInfo>();
            foreach (var ch in _textSource) {
                if (NextChar(ch)) { // newLine detected - add line
                    lineInfoList.Add(new LineInfo { Offset = offset, Length = counter - offset });
                    offset = counter;
                }
                ++counter;
            }
            // Add last line
            lineInfoList.Add(new LineInfo { Offset = offset, Length = counter - offset });
            return lineInfoList.ToArray();
        }

        private const ushort carriageReturn = 0xD;
        private const ushort lineFeed = 0xA;

        private bool cr;
        private bool lf;

        private bool NextChar(ushort ch)
        {
            bool lineEnd = false;
            switch (ch) {
                case carriageReturn:
                    if (lf || cr) {
                        lf = false; // cr after cr|lf
                        lineEnd = true;
                    }
                    cr = true; // cr found
                    break;
                case lineFeed:
                    if (lf) { // lf after lf
                        lineEnd = true;
                    }
                    lf = true; // lf found
                    break;
                default:
                    if (cr || lf) { // any non-line-end char after any line-end
                        cr = false;
                        lf = false;
                        lineEnd = true;
                    }
                    break;
            }
            return lineEnd;
        }

        /// <summary>Return text/source using SequencePoint line/col info
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public string GetText(SequencePoint sp) {
            return GetText(sp.StartLine, sp.StartColumn, sp.EndLine, sp.EndColumn );
        }

        /// <summary>Return text at Line/Column/EndLine/EndColumn position
        /// <remarks>Line and Column counting starts at 1.</remarks>
        /// </summary>
        /// <param name="startLine"></param>
        /// <param name="startColumn"></param>
        /// <param name="endLine"></param>
        /// <param name="endColumn"></param>
        /// <returns></returns>
        public string GetText(int startLine, int startColumn, int endLine, int endColumn) {

            var text = new StringBuilder();
            string line;
            bool argOutOfRange;

            if (startLine==endLine) {

                #region One-Line request
                line = GetLine(startLine);

                argOutOfRange = startColumn > endColumn || startColumn > line.Length;
                if (!argOutOfRange)
                {
                    var actualStartColumn = (startColumn < 1) ? 1 : startColumn;
                    var actualEndColumn = (endColumn > line.Length + 1) ? line.Length + 1 : endColumn;
                    text.Append(line.Substring(actualStartColumn - 1, actualEndColumn - actualStartColumn));
                }
                #endregion

            } else if (startLine<endLine) {

                #region Multi-line request

                #region First line
                line = GetLine(startLine);

                argOutOfRange = startColumn > line.Length;
                if (!argOutOfRange) {
                    var actualStartColumn = (startColumn < 1) ? 1 : startColumn;
                    text.Append(line.Substring(actualStartColumn - 1));
                }
                #endregion

                #region More than two lines
                for ( int lineIndex = startLine + 1; lineIndex < endLine; lineIndex++ ) {
                    text.Append ( GetLine ( lineIndex ) );
                }
                #endregion

                #region Last line
                line = GetLine(endLine);

                argOutOfRange = endColumn < 1;
                if (!argOutOfRange) {
                    var actualEndColumn = (endColumn > line.Length + 1) ? line.Length + 1 : endColumn;
                    text.Append(line.Substring(0, actualEndColumn - 1));
                }
                #endregion

                #endregion

            } 
            return text.ToString();
        }

        /// <summary>
        /// Return number of lines in source
        /// </summary>
        public int LinesCount {
            get {
                return _lines.Length;
            }
        }

        /// <summary>Return SequencePoint enumerated line
        /// </summary>
        /// <param name="lineNo"></param>
        /// <returns></returns>
        public string GetLine ( int lineNo ) {

            string retString = string.Empty;

            if ( lineNo > 0 && lineNo <= _lines.Length ) {
                LineInfo lineInfo = _lines[lineNo-1];
                retString = _textSource.Substring(lineInfo.Offset, lineInfo.Length);
            }

            return retString;
        }

        /// <summary>
        /// True if referenceTime != 0 and FileTime > referenceTime
        /// </summary>
        /// <param name="referenceTime"></param>
        /// <returns></returns>
        public bool IsChanged (DateTime referenceTime) {
            return referenceTime != DateTime.MinValue && _fileTime > referenceTime;
        }

        /// <summary>
        /// Get line-parsed source from file name
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CodeCoverageStringTextSource GetSource(string filePath) {

            var retSource = new CodeCoverageStringTextSource (null, filePath); // null indicates source-file not found
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (var reader = new StreamReader(stream, Encoding.Default, true))
                    {
                        stream.Position = 0;
                        retSource = new CodeCoverageStringTextSource(reader.ReadToEnd(), filePath);
                    }
                }
                catch (Exception e)
                {
                    // Source is optional (for excess-branch removal), application can continue without it
                    e.InformUser(); // Do not throw ExitApplicationWithoutReportingException
                }
            }
            else
            {
                String.Format("Source file {0} not found", filePath).InformUserSoft();
            }

            return retSource;
        }

    }
}

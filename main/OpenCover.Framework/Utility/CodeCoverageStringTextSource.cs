// Copyright (c) https://github.com/ddur
// This code is distributed under MIT license

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        CSharp,

        /// <summary>
        /// File extension is ".vb"
        /// </summary>
        VBasic
    }
    /// <summary>StringTextSource (ReadOnly)
    /// <remarks>Line and column counting starts at 1.</remarks>
    /// </summary>
    public class CodeCoverageStringTextSource
    {
        /// <summary>
        /// File Type by file name extension
        /// </summary>
        public FileType FileType = FileType.Unsupported;
        private readonly string textSource;
        private struct lineInfo {
            public int Offset;
            public int Length;
        }
        private readonly lineInfo[] lines;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source"></param>
        public CodeCoverageStringTextSource(string source)
        {
            if (string.IsNullOrEmpty(source)) {
                this.textSource = string.Empty;
            } else {
                this.textSource = source;
            }

            lineInfo line;
            var lineInfoList = new List<lineInfo>();
            int offset = 0;
            int counter = 0;
            bool newLine = false;
            bool cr = false;
            bool lf = false;

            if (textSource != string.Empty) {
                foreach ( ushort ch in textSource ) {
                    switch (ch) {
                        case 0xD:
                            if (lf||cr) {
                                newLine = true; // cr after cr|lf
                            } else {
                                cr = true; // cr found
                            }
                            break;
                        case 0xA:
                            if (lf) {
                                newLine = true; // lf after lf
                            } else {
                                lf = true; // lf found
                            }
                            break;
                        default:
                            if (cr||lf) {
                                newLine = true; // any non-line-end char after any line-end
                            }
                            break;
                    }
                    if (newLine) { // newLine detected - add line
                        line = new lineInfo();
                        line.Offset = offset;
                        line.Length = counter - offset;
                        lineInfoList.Add(line);
                        offset = counter;
                        cr = false;
                        lf = false;
                        newLine = false;
                    }
                    ++counter;
                }
                
                // Add last line
                line = new lineInfo();
                line.Offset = offset;
                line.Length = counter - offset;
                lineInfoList.Add(line);
    
                // Store to readonly field
                lines = lineInfoList.ToArray();
            }
        }

        /// <summary>Return text/source using SequencePoint line/col info
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public string GetText(SequencePoint sp) {
            return this.GetText(sp.StartLine, sp.StartColumn, sp.EndLine, sp.EndColumn );
        }

        /// <summary>Return text at Line/Column/EndLine/EndColumn position
        /// <remarks>Line and Column counting starts at 1.</remarks>
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="Column"></param>
        /// <param name="EndLine"></param>
        /// <param name="EndColumn"></param>
        /// <returns></returns>
        public string GetText(int Line, int Column, int EndLine, int EndColumn) {

            var text = new StringBuilder();
            string line;
            bool argOutOfRange;

            if (Line==EndLine) {

                #region One-Line request
                line = GetLine(Line);

                //Debug.Assert(!(Column < 1), "Column < 1");
                //Debug.Assert(!(Column > EndColumn), "Column > EndColumn");
                //Debug.Assert(!(EndColumn > line.Length + 1), string.Format ("Single Line EndColumn({0}) > line.Length({1})",EndColumn, line.Length ));
                //Debug.Assert(!(EndColumn > line.Length + 1), line);

                argOutOfRange = Column < 1
                    ||   Column > EndColumn
                    ||   EndColumn > line.Length;
                if (!argOutOfRange) {
                    text.Append(line.Substring(Column-1,EndColumn-Column));
                }
                #endregion

            } else if (Line<EndLine) {

                #region Multi-line request

                #region First line
                line = GetLine(Line);

                //Debug.Assert(!(Column < 1), "Column < 1");
                //Debug.Assert(!(Column > line.Length), string.Format ("First MultiLine EndColumn({0}) > line.Length({1})",EndColumn, line.Length ));

                argOutOfRange = Column < 1
                    ||   Column > line.Length;
                if (!argOutOfRange) {
                    text.Append(line.Substring(Column-1));
                }
                #endregion

                #region More than two lines
                for ( int lineIndex = Line+1; lineIndex < EndLine; lineIndex++ ) {
                    text.Append ( GetLine ( lineIndex ) );
                }
                #endregion

                #region Last line
                line = GetLine(EndLine);

                //Debug.Assert(!(EndColumn < 1), "EndColumn < 1");
                //Debug.Assert(!(EndColumn > line.Length), string.Format ("Last MultiLine EndColumn({0}) > line.Length({1})",EndColumn, line.Length ));

                argOutOfRange = EndColumn < 1
                    ||   EndColumn > line.Length;
                if (!argOutOfRange) {
                    text.Append(line.Substring(0,EndColumn));
                }
                #endregion

                #endregion

            } else {
                //Debug.Fail("Line > EndLine");
            }
            return text.ToString();
        }

        /// <summary>
        /// Return number of lines in source
        /// </summary>
        public int LinesCount {
            get {
                return lines.Length;
            }
        }

        /// <summary>Return SequencePoint enumerated line
        /// </summary>
        /// <param name="LineNo"></param>
        /// <returns></returns>
        public string GetLine ( int LineNo ) {

            string retString = String.Empty;

            if ( LineNo > 0 && LineNo <= lines.Length ) {
                lineInfo lineInfo = lines[LineNo-1];
                retString = textSource.Substring(lineInfo.Offset, lineInfo.Length);
            } else {
                //Debug.Fail( "Line number out of range" );
            }

            return retString;
        }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="ToIndent"></param>
        /// <param name="TabSize"></param>
        /// <returns></returns>
        public static string IndentTabs ( string ToIndent, int TabSize ) {
            
            string retString = ToIndent;
            if ( ToIndent.Contains ( "\t" ) ) {
                int counter = 0;
                int remains = 0;
                int repeat = 0;
                char prevChar = char.MinValue;
                var indented = new StringBuilder();
                foreach ( char currChar in ToIndent ) {
                    if ( currChar == '\t' ) {
                        remains = counter % TabSize;
                        repeat = remains == 0 ? TabSize : remains;
                        indented.Append( ' ', repeat );
                    } else {
                        indented.Append ( currChar, 1 );
                        if ( char.IsLowSurrogate(currChar)
                            && char.IsHighSurrogate(prevChar)
                           ) { --counter; }
                    }
                    prevChar = currChar;
                    ++counter;
                }
                retString = indented.ToString();
            }
            return retString;
        }

        /// <summary>
        /// Get line-parsed source from file name
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static CodeCoverageStringTextSource GetSource(string filename) {

            var retSource = new CodeCoverageStringTextSource (string.Empty);
            try {
                using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
                    try {
                        stream.Position = 0;
                        using (var reader = new StreamReader (stream, Encoding.Default, true)) {
                            retSource = new CodeCoverageStringTextSource(reader.ReadToEnd());
                            switch (Path.GetExtension(filename).ToLowerInvariant()) {
                                case ".cs":
                                    retSource.FileType = FileType.CSharp;
                                    break;
                                case ".vb":
                                    retSource.FileType = FileType.VBasic;
                                    break;
                                default:
                                    retSource.FileType = FileType.Unsupported;
                                    break;
                            }
                        }
                    } catch {}
                }
            } catch {}

            return retSource;
        }

    }
}

/*
 * Created by SharpDevelop.
 * User: ddur
 * Date: 8.1.2016.
 * Time: 15:14
 *
 */
using System;
using NUnit.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Utility;

namespace OpenCover.Test.Framework.Utility
{
    [TestFixture]
    public class CodeCoverageStringTextSourceTest
    {
        [Test]
        public void ConstructWithNullString()
        {
            // arrange
            var source = new CodeCoverageStringTextSource(null, "");
            
            // assert
            Assert.True (source.LinesCount == 0);
            
            // act
            var result = source.GetLine(1); // not existing line index
            
            // assert
            Assert.True (result == string.Empty);
            
            // act
            result = source.GetLine(0); // invalid line index
            
            // assert
            Assert.True (result == string.Empty);
        }

        [Test]
        public void ConstructWithEmptyString()
        {
            // arrange
            var source = new CodeCoverageStringTextSource(string.Empty, "");
            
            // assert
            Assert.True (source.LinesCount == 0);
            
            // act
            var result = source.GetLine(1); // not existing line index
            
            // assert
            Assert.True (result == string.Empty);
            
            // act
            result = source.GetLine(0); // invalid line index
            
            // assert
            Assert.True (result == string.Empty);

            // act
            var sp = new SequencePoint { StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 6};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == string.Empty);

            // act
            sp = new SequencePoint { StartLine = -1, StartColumn = -1, EndLine = -2, EndColumn = 6};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == string.Empty);
        }

        [Test]
        public void ConstructWithSingleLine()
        {
            // arrange
            const string input = "single line";
            var source = new CodeCoverageStringTextSource(input, "");
            
            // assert
            Assert.True (source.LinesCount == 1);
            
            // act
            var result = source.GetLine(1); // existing line index
            
            // assert
            Assert.True (result == input);
            
            // act
            result = source.GetLine(0); // invalid line index
            
            // assert
            Assert.True (result == string.Empty);
            
            // act
            var sp = new SequencePoint { StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 7};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "single");
            
            // act with too small StartColumn
            sp = new SequencePoint { StartLine = 1, StartColumn = -1, EndLine = 1, EndColumn = 7};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "single");
            
            // act with too large StartColumn
            sp = new SequencePoint { StartLine = 1, StartColumn = 19, EndLine = 1, EndColumn = 20};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "");
            
            // act with too small EndColumn
            sp = new SequencePoint { StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 0};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "");
            
            // act with too large EndColumn
            sp = new SequencePoint { StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 20};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "single line");
        }

        [Test]
        public void ConstructWithTwoLines()
        {
            // arrange
            const string input = "\tfirst line\n\tsecond line\r";
            var source = new CodeCoverageStringTextSource(input, "");
            
            // assert
            Assert.True (source.LinesCount == 2);
            
            // act with existing line index
            var result = source.GetLine(1);
            
            // assert
            Assert.True (result == "\tfirst line\n");
            
            // act with existing line index
            result = source.GetLine(2);
            
            // assert
            Assert.True (result == "\tsecond line\r");
            
            // act with invalid line index
            result = source.GetLine(0);
            
            // assert
            Assert.True (result == string.Empty);
            
            // act
            var sp = new SequencePoint { StartLine = 2, StartColumn = 9, EndLine = 2, EndColumn = 13};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "line");
            
            // act with two lines request
            sp = new SequencePoint { StartLine = 1, StartColumn = 8, EndLine = 2, EndColumn = 13};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "line\n\tsecond line");
            
            // act with extended two lines request
            sp = new SequencePoint { StartLine = 1, StartColumn = -8, EndLine = 2, EndColumn = 30};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "\tfirst line\n\tsecond line\r");
            
            // act with invalid first line request
            sp = new SequencePoint { StartLine = 1, StartColumn = 28, EndLine = 2, EndColumn = 30};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "\tsecond line\r");
            
            // act with invalid first line and invalid second line request
            sp = new SequencePoint { StartLine = 1, StartColumn = 28, EndLine = 2, EndColumn = 0};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "");
        }

        [Test]
        public void ConstructWithTwoLinesNoCrLfAtEof()
        {
            // arrange
            const string input = "\tfirst line\r\tsecond line";
            var source = new CodeCoverageStringTextSource(input, "");
            
            // assert
            Assert.True (source.LinesCount == 2);
            
            // act
            var result = source.GetLine(1); // existing line index
            
            // assert
            Assert.True (result == "\tfirst line\r");
            
            // act
            result = source.GetLine(2); // existing line index
            
            // assert
            Assert.True (result == "\tsecond line");
            
            // act
            result = source.GetLine(0); // invalid line index
            
            // assert
            Assert.True (result == string.Empty);
            
            // act on first line
            var sp = new SequencePoint { StartLine = 1, StartColumn = 8, EndLine = 1, EndColumn = 12};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "line");
            
            // act on second line
            sp = new SequencePoint { StartLine = 2, StartColumn = 9, EndLine = 2, EndColumn = 13};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "line");
        }

        [Test]
        public void ConstructWithFiveLines()
        {
            // arrange
            const string input = "\tfirst line\n \n\tthird line\r\n \r   fifth line\r";
            var source = new CodeCoverageStringTextSource(input, "");
            
            // assert
            Assert.True (source.LinesCount == 5);

            // act
            var result = source.GetLine(1); // existing line index
            
            // assert
            Assert.True (result == "\tfirst line\n");
            
            // act
            result = source.GetLine(2); // existing line index
            
            // assert
            Assert.True (result == " \n");
            
            // act
            result = source.GetLine(3); // existing line index
            
            // assert
            Assert.True (result == "\tthird line\r\n");
            
            // act
            result = source.GetLine(4); // existing line index
            
            // assert
            Assert.True (result == " \r");
            
            // act
            result = source.GetLine(5); // existing line index
            
            // assert
            Assert.True (result == "   fifth line\r");
            
            // act
            result = source.GetLine(9); // invalid line index

            // assert
            Assert.True (result == string.Empty);
            
            // act third line request
            var sp = new SequencePoint { StartLine = 3, StartColumn = 8, EndLine = 3, EndColumn = 12};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "line");
            
            // act invalid two lines request
            sp = new SequencePoint { StartLine = 1, StartColumn = 8, EndLine = 2, EndColumn = 13};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "line\n \n");
            
            // act valid two lines request
            sp = new SequencePoint { StartLine = 1, StartColumn = 8, EndLine = 2, EndColumn = 2};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "line\n ");
            
            // act three lines request
            sp = new SequencePoint { StartLine = 1, StartColumn = 8, EndLine = 3, EndColumn = 12};
            result = source.GetText(sp);
            
            // assert
            Assert.True (result == "line\n \n\tthird line");
        }
        
        [Test]
        public void CountLinesWithLineFeed()
        {
            
            // arrange
            const string input = "\n\n\n\n\n\n\n";
            var source = new CodeCoverageStringTextSource(input, "");
            
            // assert
            Assert.True (source.LinesCount == 7);

            // act
            var result = source.GetLine(1); // existing line index
            
            // assert
            Assert.True (result == "\n");
            
        }
        
        [Test]
        public void CountLinesWithCrLf()
        {
            
            // arrange
            const string input = "\r\n\r\n\r\n\r\n";
            var source = new CodeCoverageStringTextSource(input, "");
            
            // assert
            Assert.True (source.LinesCount == 4);

            // act
            var result = source.GetLine(1); // existing line index
            
            // assert
            Assert.True (result == "\r\n");
            
        }
        
        [Test]
        public void CountLinesWithMixedLineEnd()
        {
            
            // arrange
            const string input = "\r\r\r\n \r\n \r\n \r \n \n\n\n\r\n\n";
            //                     1 2   3    4    5  6  7  8 910  1112
            var source = new CodeCoverageStringTextSource(input, "");
            
            // assert
            Assert.True (source.LinesCount == 12);

            // act
            var result = source.GetLine(1); // existing line index
            
            // assert
            Assert.True (result == "\r");
            
        }
        
        [Test]
        public void GetSource()
        {
            var timeReference = DateTime.UtcNow; System.Threading.Thread.Sleep(100);
            string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid();
            string cSharpFileName = fileName+".cs";
            string vBasicFileName = fileName+".vb";
            string[] lines = { "First line", "Second line", "Third line" };

            // act on not existing file
            var source = CodeCoverageStringTextSource.GetSource(cSharpFileName);

            // assert
            Assert.True (!ReferenceEquals(source, null));
            Assert.True (source.FileType == FileType.CSharp);
            Assert.True (source.FilePath == cSharpFileName);
            Assert.False (source.FileFound);
            Assert.True (source.FileTime == DateTime.MinValue);
            Assert.False (source.IsChanged (source.FileTime));
            Assert.False (source.IsChanged (DateTime.MinValue));
            Assert.False (source.IsChanged (DateTime.UtcNow));
            Assert.False (source.IsChanged (timeReference));

            // arrange
            System.IO.File.WriteAllLines(cSharpFileName, lines);

            // act on existing file
            source = CodeCoverageStringTextSource.GetSource(cSharpFileName);

            // assert
            Assert.True (!ReferenceEquals(source, null));
            Assert.True (source.FileType == FileType.CSharp);
            Assert.True (source.FilePath == cSharpFileName);
            Assert.True (source.FileFound);
            Assert.True (source.FileTime == System.IO.File.GetLastWriteTimeUtc (cSharpFileName));
            Assert.False (source.IsChanged (source.FileTime));
            Assert.False (source.IsChanged (DateTime.MinValue));
            Assert.False (source.IsChanged (DateTime.UtcNow));
            Assert.True (source.IsChanged (timeReference));

            // destroy temp file
            System.IO.File.Delete(cSharpFileName);

            // arrange
            System.IO.File.WriteAllLines(vBasicFileName, lines);
            // act on existing file
            source = CodeCoverageStringTextSource.GetSource(vBasicFileName);

            // assert
            Assert.True (!ReferenceEquals(source, null));
            Assert.True (source.FileType == FileType.Unsupported);
            Assert.True (source.FilePath == vBasicFileName);
            Assert.True (source.FileFound);
            Assert.True (source.FileTime == System.IO.File.GetLastWriteTimeUtc (vBasicFileName));
            Assert.False (source.IsChanged (source.FileTime));
            Assert.False (source.IsChanged (DateTime.MinValue));
            Assert.False (source.IsChanged (DateTime.UtcNow));
            Assert.True (source.IsChanged (timeReference));

            // destroy temp file
            System.IO.File.Delete(vBasicFileName);
        }
    }
}

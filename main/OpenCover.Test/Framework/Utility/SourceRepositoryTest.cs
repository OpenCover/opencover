/*
 * Created by SharpDevelop.
 * User: ddur
 * Date: 9.1.2016.
 * Time: 17:52
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Utility;

namespace OpenCover.Test.Framework.Utility
{
    [TestFixture]
    public class SourceRepositoryTest
    {
        
        [Test]
        public void Create()
        {
            var sRepo = new SourceRepository();
            Assert.True (sRepo.IsReadOnly == false);
            Assert.True (sRepo.Count == 0);

        }
        
        [Test]
        public void CreateAddRemoveKeyAndValue()
        {
            var sRepo = new SourceRepository();
            var source = new CodeCoverageStringTextSource("", "");
            const uint fileId = 1;
            sRepo.Add (fileId, source);
            Assert.True (sRepo.Count == 1);

            Assert.True (sRepo.ContainsKey(fileId));
            Assert.True (sRepo.Remove(fileId));
            Assert.True (sRepo.Count == 0);
            Assert.False (sRepo.Remove(fileId));
            Assert.True (sRepo.Count == 0);
        }
        
        [Test]
        public void CreateAddIndexerTryGetValue()
        {
            var sRepo = new SourceRepository();
            Assert.True (sRepo.Count == 0);

            var source = new CodeCoverageStringTextSource("", "");
            const uint fileId = 1;

            Assert.That ( delegate { sRepo[fileId] = source; }, Throws.Nothing );
            Assert.True (sRepo.Count == 1);
            Assert.True (ReferenceEquals(sRepo[fileId], source));

            CodeCoverageStringTextSource getSource = null;
            sRepo.TryGetValue (fileId, out getSource);
            Assert.True (ReferenceEquals(getSource, source));
        }
        
        [Test]
        public void CreateAddRemoveKeyValuePair()
        {
            var sRepo = new SourceRepository();
            Assert.True (sRepo.Count == 0);

            var source = new CodeCoverageStringTextSource("", "");
            const uint fileId = 1;

            sRepo.Add (new KeyValuePair<uint,CodeCoverageStringTextSource>(fileId, source));
            Assert.True (sRepo.Contains(new KeyValuePair<uint, CodeCoverageStringTextSource>(fileId, source)));
            Assert.True (sRepo.Remove(new KeyValuePair<uint, CodeCoverageStringTextSource>(fileId, source)));
            Assert.False (sRepo.Remove(new KeyValuePair<uint, CodeCoverageStringTextSource>(fileId, source)));

            sRepo.Clear();
            Assert.True (sRepo.Count == 0);

        }
        
        [Test]
        public void CreateAddClear()
        {
            var sRepo = new SourceRepository();
            Assert.True (sRepo.Count == 0);

            var source = new CodeCoverageStringTextSource("", "");
            const uint fileId = 1;

            sRepo.Add (fileId, source);
            Assert.True (sRepo.Count == 1);

            sRepo.Clear();
            Assert.True (sRepo.Count == 0);

        }
        
        [Test]
        public void CreateGetKeysValuesCopyEnumerate()
        {
            var sRepo = new SourceRepository();
            Assert.True (sRepo.IsReadOnly == false);
            Assert.True (sRepo.Count == 0);

            var source1 = new CodeCoverageStringTextSource("abc", "");
            const uint fileId1 = 1;
            sRepo.Add (fileId1, source1);
            Assert.True (sRepo.Count == 1);
            Assert.True (sRepo.Keys.Count == 1);
            Assert.True (sRepo.Values.Count == 1);

            var source2 = new CodeCoverageStringTextSource("def", "");
            const uint fileId2 = 2;
            sRepo.Add (fileId2, source2);
            Assert.True (sRepo.Count == 2);

            var array = new KeyValuePair<uint, CodeCoverageStringTextSource>[2];
            Assert.That (delegate { sRepo.CopyTo(array, 0); }, Throws.Nothing);

            // IDictionary is not ordered
            Assert.True (array[0].Key == fileId1 || array[1].Key == fileId2);
            Assert.True (array[0].Value == source1 || array[1].Value == source2);

            Assert.True (array[1].Key != default(uint));
            Assert.True (array[1].Value != default(CodeCoverageStringTextSource));

            // covers generic enumerator
            int count = 0;
            foreach (var item in sRepo) {
                Assert.True (item.Key != default(uint));
                Assert.True (item.Value != default(CodeCoverageStringTextSource));
                count += 1;
            }
            Assert.True (count == 2);

            // covers GetEnumerator
            count = 0;
            var e = ((IEnumerable)sRepo).GetEnumerator();
            while (e.MoveNext()) {
                count += 1;
            }
            Assert.True (count == 2);
        }
                
        
        [Test]
        public void CreateGetSourceAndSequencePoints()
        {
            const uint fileId1 = 1;
            const string sourceString = "abc { def }";
            var source = new CodeCoverageStringTextSource(sourceString, "");

            var sRepo = new SourceRepository();
            sRepo[fileId1] = source;

            var spLeft = new SequencePoint() {
                FileId = 1,
                StartLine = 1,
                EndLine = 1,
                StartColumn = 5,
                EndColumn = 6
            };

            var spRight = new SequencePoint() {
                FileId = 1,
                StartLine = 1,
                EndLine = 1,
                StartColumn = 11,
                EndColumn = 12
            };

            var spInvalid = new SequencePoint() {
                FileId = 2,
                StartLine = 1,
                EndLine = 1,
                StartColumn = 11,
                EndColumn = 12
            };

            Assert.True (sRepo.GetCodeCoverageStringTextSource(0) == null);
            Assert.True (sRepo.GetCodeCoverageStringTextSource(1) == source);
            Assert.True (sRepo.GetCodeCoverageStringTextSource(2) == null);
            Assert.True (sRepo.GetCodeCoverageStringTextSource(1).GetLine(1) == sourceString);
            Assert.True (sRepo.GetCodeCoverageStringTextSource(1).GetText(spLeft) == "{");
            Assert.True (sRepo.GetCodeCoverageStringTextSource(1).GetText(spRight) == "}");
            
            Assert.True (sRepo.GetSequencePointText(null) == "");
            Assert.True (sRepo.GetSequencePointText(spInvalid) == "");

            Assert.True (sRepo.GetSequencePointText(spLeft) == "{");
            Assert.True (sRepo.GetSequencePointText(spRight) == "}");
        }
    }
}

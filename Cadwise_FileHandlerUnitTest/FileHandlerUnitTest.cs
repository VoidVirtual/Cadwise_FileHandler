using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Cadwise_FileHandlerUnitTest;

namespace Cadwise_FileHandlerUnitTest
{
    public static class CustomAssert
    {
        public static void AreEqual(char[] b1, char[] b2)
        {
            for (int i = 0; i < b1.Length && i < b2.Length; i++)
            {
                if (b1[i] != b2[i])
                    Assert.IsTrue(false);
            }
            if (b1.Length < b2.Length)
            {
                if (b2[b1.Length] != '\0')
                    Assert.IsTrue(false);
            }
            if (b2.Length < b1.Length)
            {
                if (b1[b2.Length] != '\0')
                    Assert.IsTrue(false);
            }
            Assert.IsTrue(true);
        }
        public static void AreEqual(FilterableBuffer b1, FilterableBuffer b2)
        {
            if (!b1.CurrentWordSymbols.SequenceEqual(b2.CurrentWordSymbols))
                Assert.IsTrue(false);
            AreEqual(b1.Buffer, b2.Buffer);
        }
    }
    [TestClass]
    public class FileHandlerUnitTest
    {
        public static void AssertBufferFilter(int wordLength, bool removing, char[] original, FilterableBuffer primer)
        {
            var filter = new TextFilter(wordLength, removing, original.Length);
            filter.FilterBuffer(original);
            CustomAssert.AreEqual(filter, primer);
        }
        public static void AssertTwoBuffersFilter(int wordLength, bool removing, Tuple<char[], char[]> originals, Tuple<char[], char[]> primers)
        {
            var filter = new TextFilter(wordLength, removing, originals.Item1.Length);
            filter.FilterBuffer(originals.Item1);
            CustomAssert.AreEqual(filter.Buffer, primers.Item1);
            filter.FilterBuffer(originals.Item2);
            CustomAssert.AreEqual(filter.Buffer, primers.Item2);
        }
        [TestMethod]
        public void TestBufferFilter1()
        {
            var primer = new FilterableBuffer();
            primer.Buffer = new char[] { 'a', 'b', 'c', ' ', '\n',
                                         'h', 'j', 'k',
                                       };
            char[] original = new char[] { 'a', 'b', 'c', ' ', 'd', 'e', '\n',
                                         'f','g', ' ', 'h', 'j', 'k',
                                       };
            AssertBufferFilter(3, false, original, primer);
        }
        [TestMethod]
        public void TestBufferFilter2()
        {
            var primer = new FilterableBuffer();
            primer.CurrentWordSymbols = new Queue<char>(new[] { 'h', 'j' });
            primer.Buffer = new char[] { 'a', 'b', 'c', ' ', '\n',

                                       };
            char[] original = new char[] { 'a', 'b', 'c', ' ', 'e', 'd', '\n',
                                         'f','g', ' ', 'h', 'j',
                                       };
            AssertBufferFilter(3, false, original, primer);
        }
        [TestMethod]
        public void TestBufferFilter_Punctuation1()
        {
            var primer = new FilterableBuffer();
            primer.Buffer = new char[] { 'a' };
            char[] original = new char[] { '(', 'a', ')', };
            AssertBufferFilter(0, true, original, primer);
        }
        [TestMethod]
        public void TestBufferFilter_Punctuation2()
        {
            var primer = new FilterableBuffer();
            primer.Buffer = new char[] { };
            primer.CurrentWordSymbols = new Queue<char> { };
            char[] original = new char[] { '(', 'a', ')', };
            AssertBufferFilter(2, true, original, primer);
        }
        [TestMethod]
        public void TestBufferFilter_Punctuation3()
        {
            var primer = new FilterableBuffer();
            primer.Buffer = new char[] { 'a', ' ', 'b', };
            primer.CurrentWordSymbols = new Queue<char> { };
            char[] original = new char[] { 'a', ',', ' ', 'b', };
            AssertBufferFilter(0, true, original, primer);
        }
        [TestMethod]
        public void TestTwoBuffersFilter1()
        {
            var original1 = new char[] { 'a', 'b', 'c', ' ', 'd' };
            var original2 = new char[] { 'e', 'f', };
            var primer1 = new char[] { 'a', 'b', 'c', ' ' };
            var primer2 = new char[] { 'd', 'e', 'f', };
            AssertTwoBuffersFilter(3, false, new Tuple<char[], char[]>(original1, original2), new Tuple<char[], char[]>(primer1, primer2));
        }
        [TestMethod]
        public void TestTwoBuffersFilter2()
        {
            var original1 = new char[] { 'a', 'b', 'c', 'd' };
            var original2 = new char[] { 'e', 'f', };
            var primer1 = new char[] { 'a', 'b', 'c', 'd' };
            var primer2 = new char[] { 'e', 'f', };
            AssertTwoBuffersFilter(3, false, new Tuple<char[], char[]>(original1, original2), new Tuple<char[], char[]>(primer1, primer2));
        }
        [TestMethod]
        public void TestTwoBuffersFilter_Punctuation()
        {
            var original1 = new char[] { 'a', 'b', 'c', ' ', '(', 'e' };
            var original2 = new char[] { ')' };
            var primer1 = new char[] { 'a', 'b', 'c', ' ' };
            var primer2 = new char[] { };
            AssertTwoBuffersFilter(3, true, new Tuple<char[], char[]>(original1, original2), new Tuple<char[], char[]>(primer1, primer2));
        }
        [TestMethod]
        public void TestFilterBufferTwoSpaces()
        {
            var primer = new FilterableBuffer();
            primer.Buffer = new char[] { 'a', ' ', ' ', 'b', };
            char[] original = new char[] { 'a', ' ', ' ', 'b', };
            AssertBufferFilter(1, false, original, primer);
        }
        [TestMethod]
        public void TestFileHandlerException1()
        {
            var fileHandler = new FileHandler();
            var args = new FiltrationArgs
            {
                From = "C:/NonExistingFile1.txt",
                To = "F:/NonExistingFile2.txt",
                Removing = false,
                Length = 0,
                ProcessID = 0
            };
            try
            {
                fileHandler.Filter(args);
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
        }
        [TestMethod]
        public void TestFileHandlerException2()
        {
            var fileHandler = new FileHandler();
            var args = new FiltrationArgs
            {
                From = "C:/Over32GBSizefile.txt",
                To = "DRIVE4GB:/To.txt",
                Removing = false,
                Length = 0,
                ProcessID = 0
            };
            try
            {
                fileHandler.Filter(args);
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
        }
    }
}

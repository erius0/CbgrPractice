using Microsoft.VisualStudio.TestTools.UnitTesting;
using Task3;

namespace Task3Test
{
    [TestClass]
    public class WordCounterTests
    {
        private readonly WordCounter wordCounter = new WordCounter();

        [TestMethod]
        public void CountingTest()
        {
            long expected = 5;
            long actual = wordCounter.ProcessString("one two three four five");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MultipleSpacesTest()
        {
            long expected = 5;
            long actual = wordCounter.ProcessString("one two    three   four        five");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TrailingAndLeadingSpacesTest()
        {
            long expected = 5;
            long actual = wordCounter.ProcessString("   one two three four five    ");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MixedTest()
        {
            long expected = 7;
            long actual = wordCounter.ProcessString("   one    two    three  four      five  \n six   \n \n seven  ");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParallelCountingTest()
        {
            long expected = 5;
            long actual = wordCounter.ProcessString("one two three four five", doParallel: true);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParallelMultipleSpacesTest()
        {
            long expected = 5;
            long actual = wordCounter.ProcessString("one two    three   four        five", doParallel: true);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParallelTrailingAndLeadingSpacesTest()
        {
            long expected = 5;
            long actual = wordCounter.ProcessString("   one two three four five    ", doParallel: true);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParallelMixedTest()
        {
            long expected = 7;
            long actual = wordCounter.ProcessString("   one    two    three  four      five  \n six   \n \n seven  ", doParallel: true);
            Assert.AreEqual(expected, actual);
        }
    }
}
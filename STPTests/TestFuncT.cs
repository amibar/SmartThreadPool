using System;
using Amib.Threading;
using NUnit.Framework;

namespace SmartThreadPoolTests
{
    /// <summary>
    /// Summary description for TestCancel.
    /// </summary>
    [TestFixture]
    [Category("TestFuncT")]
    public class TestFuncT
    {
        private SmartThreadPool _stp;

        [SetUp]
        public void Init()
        {
            _stp = new SmartThreadPool();
        }

        [TearDown]
        public void Fini()
        {
            _stp.Shutdown();
        }

        [Test]
        public void FuncInt1()
        {
            IWorkItemResult<int> wir =
                _stp.QueueWorkItem(new Func<int, int>(Increment), 1);

            int y = wir.GetResult();

            Assert.AreEqual(y, 2);
        }

        [Test]
        public void FuncInt2()
        {
            IWorkItemResult<int> wir = _stp.QueueWorkItem(new Func<int>(MaxInt));

            int result = wir.GetResult();

            Assert.AreEqual(result, int.MaxValue);
        }

        [Test]
        public void FuncBool()
        {
            IWorkItemResult<bool> wir = _stp.QueueWorkItem(new Func<bool, bool>(Not), true);

            bool result = wir.Result;

            Assert.AreEqual(result, false);
        }

        [Test]
        public void FuncString1()
        {
            IWorkItemResult<string> wir = _stp.QueueWorkItem(new Func<string, string, string>(string.Concat), "ABC", "xyz");

            string result = wir.Result;

            Assert.AreEqual(result, "ABCxyz");
        }

        [Test]
        public void FuncString2()
        {
            IWorkItemResult<string> wir = _stp.QueueWorkItem(new Func<string, int, int, string>(Substring), "ABCDEF", 1, 2);

            string result = wir.Result;

            Assert.AreEqual(result, "BC");
        }

        [Test]
        public void FuncIntArray()
        {
            int[] numbers = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            IWorkItemResult<int[]> wir = _stp.QueueWorkItem(new Func<int[], int, int, int, int[]>(SubArray), numbers, 1, 2, 3);

            int[] result = wir.Result;

            Assert.AreEqual(result, new int[] { 2, 3, 2, 3, 2, 3, });
        }

        private int Increment(int x)
        {
            return x + 1;
        }

        private int MaxInt()
        {
            return int.MaxValue;
        }

        private bool Not(bool flag)
        {
            return !flag;
        }

        private string Substring(string s, int startIndex, int length)
        {
            return s.Substring(startIndex, length);
        }

        private int[] SubArray(int[] numbers, int startIndex, int length, int repeat)
        {
            int[] result = new int[length * repeat];
            for (int i = 0; i < repeat; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    result[i * length + j] = numbers[startIndex + j];
                }
            }

            return result;
        }
    }
}

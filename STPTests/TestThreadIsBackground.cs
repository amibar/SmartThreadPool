using System.Threading;
using NUnit.Framework;
using Amib.Threading;

namespace SmartThreadPoolTests
{
	/// <summary>
    /// Summary description for TestThreadIsBackground.
	/// </summary>
	[TestFixture]
    [Category("TestThreadIsBackground")]
	public class TestThreadIsBackground
	{
		[Test]
        public void TestIsBackground()
		{
            CheckIsBackground(true);
		}

		[Test]
        public void TestNotIsBackground()
		{
            CheckIsBackground(false);
		}

        private static void CheckIsBackground(bool isBackground)
	    {
	        STPStartInfo stpStartInfo = new STPStartInfo();
	        stpStartInfo.AreThreadsBackground = isBackground;

	        SmartThreadPool stp = new SmartThreadPool(stpStartInfo);

            IWorkItemResult<bool> wir = stp.QueueWorkItem(() => GetCurrentThreadIsBackground());

	        bool resultIsBackground = wir.GetResult();

	        stp.WaitForIdle();

            Assert.AreEqual(isBackground, resultIsBackground);
	    }

	    private static bool GetCurrentThreadIsBackground()
	    {
	        return Thread.CurrentThread.IsBackground;
	    }
	}
}
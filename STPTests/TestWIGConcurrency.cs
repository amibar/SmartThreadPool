using System;
using System.Threading;

using NUnit.Framework;

using Amib.Threading;

namespace WorkItemsGroupTests
{
	/// <summary>
	/// Summary description for TestWIGConcurrency.
	/// </summary>
	[TestFixture]
	[Category("TestWIGConcurrency")]
	public class TestWIGConcurrency
	{
		private Random _randGen;
		private int [] _concurrentOps;
		private int _concurrencyPerWig;
		private bool _success;

	    [Test]
		public void TestConcurrencies()
		{
			Concurrency(1, 1, 10);
			Concurrency(1, 1, 100);

			Concurrency(1, 5, 10);
			Concurrency(1, 5, 100);

			Concurrency(5, 5, 10);
			Concurrency(5, 5, 100);
		}

		private void Concurrency(
			int concurrencyPerWig,
			int wigsCount,
			int workItemsCount)
		{
			Console.WriteLine(
				"Testing : concurrencyPerWig = {0}, wigsCount = {1}, workItemsCount = {2}",
				concurrencyPerWig,
				wigsCount,
				workItemsCount);

			_success = true;
			_concurrencyPerWig = concurrencyPerWig;
			_randGen = new Random(0);

			STPStartInfo stpStartInfo = new STPStartInfo();
			stpStartInfo.StartSuspended = true;

			SmartThreadPool stp = new SmartThreadPool(stpStartInfo);

			_concurrentOps = new int[wigsCount];

			IWorkItemsGroup [] wigs = new IWorkItemsGroup[wigsCount];

			for(int i = 0; i < wigs.Length; ++i)
			{
				wigs[i] = stp.CreateWorkItemsGroup(_concurrencyPerWig);
				for(int j = 0; j < workItemsCount; ++j)
				{
					wigs[i].QueueWorkItem(new WorkItemCallback(this.DoWork), i);
				}

				wigs[i].Start();
			}

			stp.Start();

			stp.WaitForIdle();

			Assert.IsTrue(_success);

			stp.Shutdown();
		}

		private object DoWork(object state)
		{
			int wigsIndex = (int)state;

			int val = Interlocked.Increment(ref _concurrentOps[wigsIndex]);
			_success = _success && (val <= _concurrencyPerWig);

			int waitTime = _randGen.Next(50);
			Thread.Sleep(waitTime);

			val = Interlocked.Decrement(ref _concurrentOps[wigsIndex]);
			_success = _success && (val >= 0);

			return null;
		}
	}
}

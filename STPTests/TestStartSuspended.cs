using System.Threading;

using NUnit.Framework;

using Amib.Threading;

namespace SmartThreadPoolTests
{
	/// <summary>
	/// Summary description for TestStartSuspended.
	/// </summary>
	[TestFixture]
	[Category("TestStartSuspended")]
	public class TestStartSuspended
	{
	    [Test]
		public void StartSuspended()
		{
			STPStartInfo stpStartInfo = new STPStartInfo();
			stpStartInfo.StartSuspended = true;

			SmartThreadPool stp = new SmartThreadPool(stpStartInfo);

			stp.QueueWorkItem(new WorkItemCallback(this.DoWork));

			Assert.IsFalse(stp.WaitForIdle(200));

			stp.Start();

			Assert.IsTrue(stp.WaitForIdle(200));
		}

		[Test]
		public void WIGStartSuspended()
		{
			SmartThreadPool stp = new SmartThreadPool();

			WIGStartInfo wigStartInfo = new WIGStartInfo();
			wigStartInfo.StartSuspended = true;

			IWorkItemsGroup wig = stp.CreateWorkItemsGroup(10, wigStartInfo);

			wig.QueueWorkItem(new WorkItemCallback(this.DoWork));

			Assert.IsFalse(wig.WaitForIdle(200));

			wig.Start();

			Assert.IsTrue(wig.WaitForIdle(200));
		}

		[Test]
		public void STPAndWIGStartSuspended()
		{
			STPStartInfo stpStartInfo = new STPStartInfo();
			stpStartInfo.StartSuspended = true;

			SmartThreadPool stp = new SmartThreadPool(stpStartInfo);

			WIGStartInfo wigStartInfo = new WIGStartInfo();
			wigStartInfo.StartSuspended = true;

			IWorkItemsGroup wig = stp.CreateWorkItemsGroup(10, wigStartInfo);

			wig.QueueWorkItem(new WorkItemCallback(this.DoWork));

			Assert.IsFalse(wig.WaitForIdle(200));

			wig.Start();

			Assert.IsFalse(wig.WaitForIdle(200));

			stp.Start();

			Assert.IsTrue(wig.WaitForIdle(5000), "WIG is not idle");
            Assert.IsTrue(stp.WaitForIdle(5000), "STP is not idle");
		}

		[Test]
		public void TwoWIGsStartSuspended()
		{
			SmartThreadPool stp = new SmartThreadPool();

			WIGStartInfo wigStartInfo = new WIGStartInfo();
			wigStartInfo.StartSuspended = true;

			IWorkItemsGroup wig1 = stp.CreateWorkItemsGroup(10, wigStartInfo);
			IWorkItemsGroup wig2 = stp.CreateWorkItemsGroup(10, wigStartInfo);

			wig1.QueueWorkItem(new WorkItemCallback(this.DoWork));
			wig2.QueueWorkItem(new WorkItemCallback(this.DoWork));

			Assert.IsFalse(wig1.WaitForIdle(200));
			Assert.IsFalse(wig2.WaitForIdle(200));

			wig1.Start();

			Assert.IsTrue(wig1.WaitForIdle(200));
			Assert.IsFalse(wig2.WaitForIdle(200));

			wig2.Start();

			Assert.IsTrue(wig1.WaitForIdle(0));
			Assert.IsTrue(wig2.WaitForIdle(200));
		}


		private object DoWork(object state)
		{
			Thread.Sleep(100);
			return null;
		}

	}
}

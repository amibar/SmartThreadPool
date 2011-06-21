using System;
using System.Threading;

using NUnit.Framework;

using Amib.Threading;

namespace SmartThreadPoolTests
{
	/// <summary>
    /// Summary description for TestParallelMethods.
	/// </summary>
	[TestFixture]
	[Category("TestParallelMethods")]
    public class TestParallelMethods
	{
		[Test]
		public void TestJoin() 
		{ 
			SmartThreadPool stp = new SmartThreadPool();

		    SafeCounter sc = new SafeCounter();

            stp.Join(
                sc.Increment,
                sc.Increment,
                sc.Increment);

            Assert.AreEqual(3, sc.Counter);

		    for (int j = 0; j < 10; j++)
		    {
                sc.Reset();

                Action[] actions = new Action[1000];
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i] = sc.Increment;
                }

                stp.Join(actions);

                Assert.AreEqual(actions.Length, sc.Counter);
		    }

            stp.Shutdown();
		}

	    private class SafeCounter
        {
            private int _counter;

            public void Increment()
            {
                Interlocked.Increment(ref _counter);
            }

            public int Counter
            {
                get { return _counter; }
            }

            public void Reset()
            {
                _counter = 0;
            }
        }

	    [Test]
	    public void TestChoice() 
	    { 
	        SmartThreadPool stp = new SmartThreadPool();

	        int index = stp.Choice(
	            () => Thread.Sleep(1000),
	            () => Thread.Sleep(1500),
	            () => Thread.Sleep(500));

	        Assert.AreEqual(2, index);  
            
	        index = stp.Choice(
	            () => Thread.Sleep(300),
	            () => Thread.Sleep(100),
	            () => Thread.Sleep(200));

	        Assert.AreEqual(1, index);

	        stp.Shutdown();
	    } 
        
        [Test]
	    public void TestPipe() 
	    { 
            SafeCounter sc = new SafeCounter();
	        SmartThreadPool stp = new SmartThreadPool();

            stp.Pipe(
                sc,
                sc1 => { if (sc.Counter == 0) { sc1.Increment(); }}, 
                sc1 => { if (sc.Counter == 1) { sc1.Increment(); }}, 
                sc1 => { if (sc.Counter == 2) { sc1.Increment(); }} 
                );

            stp.WaitForIdle ();
            Assert.AreEqual(3, sc.Counter);

	        stp.Shutdown();
	    }
	}
}

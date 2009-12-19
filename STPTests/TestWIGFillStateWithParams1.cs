using System;
using Amib.Threading;
using NUnit.Framework;
using System.Net;

namespace STPTests
{
    /// <summary>
    /// Summary description for TestFillStateWithParams.
    /// </summary>
    [TestFixture]
    [Category("TestFillStateWithParams")]
    public class TestFillStateWithParams
    {
        private SmartThreadPool _stp;

        [SetUp]
        public void Init()
        {
            STPStartInfo stpStartInfo = new STPStartInfo();
            stpStartInfo.FillStateWithParams = true;
            _stp = new SmartThreadPool(stpStartInfo);
        }

        [TearDown]
        public void Fini()
        {
            _stp.WaitForIdle();
            _stp.Shutdown();
        }

        [Test]
        public void ActionT0()
        {
            IWorkItemResult wir = _stp.QueueWorkItem(Action0);
            Assert.IsNull(wir.State);
        }

        [Test]
        public void ActionT1()
        {
            IWorkItemResult wir = _stp.QueueWorkItem(Action1, 17);
            object[] args = wir.State as object[];

            Assert.IsNotNull(args);
            Assert.AreEqual(args.Length, 1);
            Assert.AreEqual(args[0], 17);
        }

        [Test]
        public void ActionT2()
        {
            IWorkItemResult wir = _stp.QueueWorkItem(Action2, 'a', "bla bla");
            object[] args = wir.State as object[];

            Assert.IsNotNull(args);
            Assert.AreEqual(args.Length, 2);
            Assert.AreEqual(args[0], 'a');
            Assert.AreEqual(args[1], "bla bla");
        }

        [Test]
        public void ActionT3()
        {
            char[] chars = new char[] {'a', 'b'};
            object obj = new object();

            IWorkItemResult wir = _stp.QueueWorkItem(Action3, true, chars, obj);
            object[] args = wir.State as object[];

            Assert.IsNotNull(args);
            Assert.AreEqual(args.Length, 3);
            Assert.AreEqual(args[0], true);
            Assert.AreEqual(args[1], chars);
            Assert.AreEqual(args[2], obj);
        }

        [Test]
        public void ActionT4()
        {
            IntPtr p = new IntPtr(int.MaxValue);
            Guid guid = Guid.NewGuid();

            IPAddress ip = IPAddress.Parse("1.2.3.4");
            IWorkItemResult wir = _stp.QueueWorkItem(Action4, long.MinValue, p, ip, guid);
            object[] args = wir.State as object[];

            Assert.IsNotNull(args);
            Assert.AreEqual(args.Length, 4);
            Assert.AreEqual(args[0], long.MinValue);
            Assert.AreEqual(args[1], p);
            Assert.AreEqual(args[2], ip);
            Assert.AreEqual(args[3], guid);
        }

        [Test]
        public void FuncT0()
        {
            IWorkItemResult<int> wir = _stp.QueueWorkItem(new Func<int>(Func0));
            Assert.AreEqual(wir.State, null);
        }

        [Test]
        public void FuncT1()
        {
            IWorkItemResult<bool> wir = _stp.QueueWorkItem(new Func<int, bool>(Func1), 17);
            object[] args = wir.State as object[];

            Assert.IsNotNull(args);
            Assert.AreEqual(args.Length, 1);
            Assert.AreEqual(args[0], 17);
        }

        [Test]
        public void FuncT2()
        {
            IWorkItemResult<string> wir = _stp.QueueWorkItem(new Func<char, string, string>(Func2), 'a', "bla bla");
            object[] args = wir.State as object[];

            Assert.IsNotNull(args);
            Assert.AreEqual(args.Length, 2);
            Assert.AreEqual(args[0], 'a');
            Assert.AreEqual(args[1], "bla bla");
        }

        [Test]
        public void FuncT3()
        {
            char[] chars = new char[] { 'a', 'b' };
            object obj = new object();

            IWorkItemResult<char> wir = _stp.QueueWorkItem(new Func<bool, char[], object, char>(Func3), true, chars, obj);
            object[] args = wir.State as object[];

            Assert.IsNotNull(args);
            Assert.AreEqual(args.Length, 3);
            Assert.AreEqual(args[0], true);
            Assert.AreEqual(args[1], chars);
            Assert.AreEqual(args[2], obj);
        }

        [Test]
        public void FuncT4()
        {
            IntPtr p = new IntPtr(int.MaxValue);
            Guid guid = Guid.NewGuid();

            IPAddress ip = IPAddress.Parse("1.2.3.4");
            IWorkItemResult<IPAddress> wir = _stp.QueueWorkItem(new Func<long, IntPtr, IPAddress, Guid, IPAddress>(Func4), long.MinValue, p, ip, guid);
            
            object[] args = wir.State as object[];

            Assert.IsNotNull(args);
            Assert.AreEqual(args.Length, 4);
            Assert.AreEqual(args[0], long.MinValue);
            Assert.AreEqual(args[1], p);
            Assert.AreEqual(args[2], ip);
            Assert.AreEqual(args[3], guid);
        }


        private void Action0()
        {
        }

        private void Action1(int p1)
        {
        }

        private void Action2(char p1, string p2)
        {
        }

        private void Action3(bool p1, char [] p2, object p3)
        {
        }

        private void Action4(long p1, IntPtr p2, IPAddress p3, Guid p4)
        {
        }

        private int Func0()
        {
            return 0;
        }

        private bool Func1(int p1)
        {
            return true;
        }

        private string Func2(char p1, string p2)
        {
            return "value";
        }

        private char Func3(bool p1, char[] p2, object p3)
        {
            return 'z';
        }

        private IPAddress Func4(long p1, IntPtr p2, IPAddress p3, Guid p4)
        {
            return IPAddress.None;
        }
    }
}

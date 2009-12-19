using System;

using NUnit.Framework;

using Amib.Threading;
using Amib.Threading.Internal;
using SmartThreadPoolTests;

namespace PriorityQueueTests
{
	/// <summary>
	/// Summary description for TestPriorityQueue.
	/// </summary>
	[TestFixture]
	[Category("TestPriorityQueue")]
	public class TestPriorityQueue
	{
	    [Test]
		public void Init()
		{
			PriorityQueue pq = new PriorityQueue();

			Assert.AreEqual(0, pq.Count);

			Assert.IsNull(pq.Dequeue());

			Assert.AreEqual(0, pq.Count);
		}

		[Test]
		public void OneWorkItem()
		{
			WorkItemPriority [] priorities = Enum.GetValues(typeof(WorkItemPriority)) as WorkItemPriority [];
			foreach(WorkItemPriority wip in priorities)
			{
				PriorityQueue pq = new PriorityQueue();

				PriorityItem pi = new PriorityItem(wip);

				pq.Enqueue(pi);

				Assert.AreEqual(1, pq.Count, "Failed for priority {0}", wip);

				PriorityItem pi2 = pq.Dequeue() as PriorityItem;

				Assert.IsNotNull(pi2, "Failed for priority {0}", wip);

				Assert.AreSame(pi, pi2, "Failed for priority {0}", wip);

				Assert.AreEqual(0, pq.Count, "Failed for priority {0}", wip);
			}
		}

		[Test]
		public void MultipleWorkItemsOnePriority()
		{
			WorkItemPriority [] priorities = Enum.GetValues(typeof(WorkItemPriority)) as WorkItemPriority [];
			foreach(WorkItemPriority wip in priorities)
			{
				PriorityQueue pq = new PriorityQueue();

				PriorityItem [] priorityItems = new PriorityItem[10];

				for(int i = 0; i < priorityItems.Length; ++i)
				{
					priorityItems[i] = new PriorityItem(wip);

					pq.Enqueue(priorityItems[i]);

					Assert.AreEqual(i+1, pq.Count, "Failed for priority {0} item count {1}", wip, i+1);
				}

				for(int i = 0; i < priorityItems.Length; ++i)
				{
					PriorityItem pi = pq.Dequeue() as PriorityItem;

					Assert.AreEqual(priorityItems.Length-(i+1), pq.Count, "Failed for priority {0} item count {1}", wip, i+1);

					Assert.IsNotNull(pi, "Failed for priority {0} item count {1}", wip, i+1);

					Assert.AreSame(pi, priorityItems[i], "Failed for priority {0} item count {1}", wip, i+1);
				}

				Assert.AreEqual(0, pq.Count, "Failed for priority {0}", wip);

				Assert.IsNull(pq.Dequeue());

				Assert.AreEqual(0, pq.Count);
			}
		}

		[Test]
		public void MultipleWorkItemsMultiplePriorities()
		{
			// Get all the available priorities
			WorkItemPriority [] priorities = Enum.GetValues(typeof(WorkItemPriority)) as WorkItemPriority [];

			// Create an array of priority items
			PriorityItem [] priorityItems = new PriorityItem[priorities.Length];

			// Create a priority item for each priority 
			int i = priorities.Length;
			foreach(WorkItemPriority workItemPriority in priorities)
			{
				--i;
				priorityItems[i] = new PriorityItem(workItemPriority);
			}

			// Create a PermutationGenerator for the priority items
			PermutationGenerator permutations = new PermutationGenerator(priorityItems);
			
			int count = 0;
			// Iterate over the permutations
			foreach(object [] permutation in permutations)
			{
				++count;
				Console.Write("Permutation #" + count + " : ");
				for(int j = 0; j < permutation.Length; ++j)
				{
					PriorityItem pi = permutation[j] as PriorityItem;
					Console.Write(pi.WorkItemPriority + ", ");
				}
				Console.WriteLine();
				// Create a priority queue
				PriorityQueue pq = new PriorityQueue();

				// Enqueue each priority item according to the permutation
				for(i = 0; i < permutation.Length; ++i)
				{
					PriorityItem priorityItem = permutation[i] as PriorityItem;
					pq.Enqueue(priorityItem);
				}

				// Make sure all the priority items are in the queue
				Assert.AreEqual(priorityItems.Length, pq.Count);

				// Compare the order of the priority items
				for(i = 0; i < priorityItems.Length; ++i)
				{
					PriorityItem priorityItem = pq.Dequeue() as PriorityItem;
					Assert.AreSame(priorityItems[i], priorityItem);
				}
			}
		}

		private class PriorityItem : IHasWorkItemPriority
		{
		    public PriorityItem(WorkItemPriority workItemPriority)
			{
				WorkItemPriority = workItemPriority;
			}

		    public WorkItemPriority WorkItemPriority { get; private set; }
		}

	}
}

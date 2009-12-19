using System;
using System.Collections;

/*
 * The code below generates permutations.
 * 
 * The original code was written by Michael Gilleland,
 * and can be found in the following site 
 * http://www.merriampark.com/perm.htm
 * 
 * I translated it to C# from Java.
 */
namespace SmartThreadPoolTests
{
	//--------------------------------------
	// Systematically generate permutations. 
	//--------------------------------------

	public class PermutationGenerator : IEnumerable
	{
		private object [] _objects;

		public PermutationGenerator(object [] objects)
		{
			_objects = (object [])objects.Clone();
		}

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return new PermutationGeneratorEnumerator(_objects);
		}

		#endregion

		private class PermutationGeneratorEnumerator : IEnumerator
		{
			private object [] _objects;
			private object [] _currentPermutation;
			private PermutationGeneratorHelper _permutationGeneratorHelper;

			public PermutationGeneratorEnumerator(object [] objects)
			{
				_objects = objects;
				Reset();
			}

			#region IEnumerator Members

			public void Reset()
			{
				_permutationGeneratorHelper = new PermutationGeneratorHelper(_objects.Length);
			}

			public object Current
			{
				get
				{
					return _currentPermutation;
				}
			}

			public bool MoveNext()
			{
				if (_permutationGeneratorHelper.hasMore())
				{
					_currentPermutation = new object[_objects.Length];
					int [] indices = _permutationGeneratorHelper.getNext();
					for (int i = 0; i < indices.Length; i++) 
					{
						_currentPermutation[i] = _objects[indices[i]];
					}
					return true;
				}
				_currentPermutation = null;
				return false;
			}

			#endregion
		}


		private class PermutationGeneratorHelper
		{

			private int[] a;
			private long numLeft;
			private long total;

			//-----------------------------------------------------------
			// Constructor. WARNING: Don't make n too large.
			// Recall that the number of permutations is n!
			// which can be very large, even when n is as small as 20 --
			// 20! = 2,432,902,008,176,640,000 and
			// 21! is too big to fit into a Java long, which is
			// why we use long instead.
			//----------------------------------------------------------

			public PermutationGeneratorHelper (int n) 
			{
				if (n < 1) 
				{
					throw new ArgumentOutOfRangeException("n", n, "Min 1");
				}
				a = new int[n];
				total = getFactorial (n);
				reset();
			}

			//------
			// Reset
			//------

			public void reset () 
			{
				for (int i = 0; i < a.Length; i++) 
				{
					a[i] = i;
				}
				numLeft = total;
			}

			//------------------------------------------------
			// Return number of permutations not yet generated
			//------------------------------------------------

			public long getNumLeft () 
			{
				return numLeft;
			}

			//------------------------------------
			// Return total number of permutations
			//------------------------------------

			public long getTotal () 
			{
				return total;
			}

			//-----------------------------
			// Are there more permutations?
			//-----------------------------

			public bool hasMore () 
			{
				return (numLeft > 0);
			}

			//------------------
			// Compute factorial
			//------------------

			private static long getFactorial (int n) 
			{
				long fact = 1;
				for (int i = n; i > 1; i--) 
				{
					fact = fact * i;
				}
				return fact;
			}

			//--------------------------------------------------------
			// Generate next permutation (algorithm from Rosen p. 284)
			//--------------------------------------------------------

			public int[] getNext () 
			{

				if (numLeft == total) 
				{
					--numLeft;
					return a;
				}

				int temp;

				// Find largest index j with a[j] < a[j+1]

				int j = a.Length - 2;
				while (a[j] > a[j+1]) 
				{
					j--;
				}

				// Find index k such that a[k] is smallest integer
				// greater than a[j] to the right of a[j]

				int k = a.Length - 1;
				while (a[j] > a[k]) 
				{
					k--;
				}

				// Interchange a[j] and a[k]

				temp = a[k];
				a[k] = a[j];
				a[j] = temp;

				// Put tail end of permutation after jth position in increasing order

				int r = a.Length - 1;
				int s = j + 1;

				while (r > s) 
				{
					temp = a[s];
					a[s] = a[r];
					a[r] = temp;
					r--;
					s++;
				}

				--numLeft;
				return a;
			}
		}
	}
}

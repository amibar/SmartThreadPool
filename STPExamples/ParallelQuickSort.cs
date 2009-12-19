using Amib.Threading;

namespace STPExamples
{
    /// <summary>
    /// This is a Parallel QuickSort example.
    /// It shows how to use the QueueWorkItem with Action<T> methods.
    /// </summary>
    public class ParallelQuickSort
    {
        public static void Main()
        {
            // Generate an array
            int[] array = GenerateArray(100);

            // Create a Smart Thread Pool 
            SmartThreadPool stp = new SmartThreadPool();
            
            // Use the Smart Thread Pool to parallel the sort 
            QuickSort(stp, array);
        }

        /// <summary>
        /// QuickSort array using wig to parallel the sort
        /// </summary>
        /// <param name="wig">A IWorkItemsGroup to use to parallel the sort</param>
        /// <param name="array">The array of items to sort</param>
        public static void QuickSort(IWorkItemsGroup wig, int[] array)
        {
            // Initiate the QuickSort
            wig.QueueWorkItem(QuickSort, wig, array, 0, array.Length - 1);

            // Wait for the sort to complete.
            wig.WaitForIdle();
        }

        /// <summary>
        /// Sort a subarray in array, starting with the left item and ending in the right item.
        /// The method uses the IWorkItemsGroup wig to parallel the sort.
        /// </summary>
        /// <param name="wig">A IWorkItemsGroup to use to parallel the sort</param>
        /// <param name="array">The array of items to sort</param>
        /// <param name="left">The left index in the subarray</param>
        /// <param name="right">The right index in the subarray</param>
        private static void QuickSort(IWorkItemsGroup wig, int[] array, int left, int right)
        {
            if (right > left)
            {
                int pivotIndex = left;
                int pivotNewIndex = Partition(array, left, right, pivotIndex);

                wig.QueueWorkItem(QuickSort, wig, array, left, pivotNewIndex - 1);
                wig.QueueWorkItem(QuickSort, wig, array, pivotNewIndex + 1, right);
            }
        }

        /// <summary>
        /// Partition a subarray of array 
        /// (This is part of the QuickSort algorithm)
        /// </summary>
        /// <returns></returns>
        private static int Partition(int[] array, int left, int right, int pivotIndex)
        {
            int pivotValue = array[pivotIndex];
            Swap(array, pivotIndex, right);
            int storeIndex = left;

            for (int i = left; i < right; i++)
            {
                if (array[i] <= pivotValue)
                {
                    Swap(array, i, storeIndex);
                    ++storeIndex;
                }
            }
            Swap(array, storeIndex, right);
            return storeIndex;
        }

        /// <summary>
        /// Swap between two item in the array
        /// </summary>
        /// <param name="array">Array of integers</param>
        /// <param name="index1">First index</param>
        /// <param name="index2">Second index</param>
        private static void Swap(int[] array, int index1, int index2)
        {
            int temp = array[index1];
            array[index1] = array[index2];
            array[index2] = temp;
        }

        /// <summary>
        /// Generate an array with the numbers 0-count ordered in reverse
        /// </summary>
        /// <param name="count">The number of items in the array</param>
        /// <returns>Returns the generated array</returns>
        private static int[] GenerateArray(int count)
        {
            int[] array = new int[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array.Length - i;
            }
            return array;
        }
    }  
}

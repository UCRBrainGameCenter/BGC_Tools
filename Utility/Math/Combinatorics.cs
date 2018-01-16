using System.Collections.Generic;
using BGC.DataStructures.Generic;
using BGC.Extensions;

namespace BGC.Utility.Math
{
    public static class Combinatorics
    {
        #region All combinations with duplicates allowed
        /// <summary>
        /// Buuild a tree of all combinations. 
        /// 
        /// For example: ([0, 1], 2) would generate a tree of structure
        ///     -1
        ///      |-0
        ///      | |-0
        ///      | |-1
        ///      |
        ///      |-1
        ///        |-0
        ///        |-1
        ///        
        /// Except for the fact that the ordering will be randomized.
        /// </summary>
        /// <param name="indexes"></param>
        /// <param name="outputSize"></param>
        /// <param name="root"></param>
        public static void TreeOfAllCombinations(int[] indexes, int outputSize, out Node<int> root)
        {
            root = new Node<int>(-1);

            if (outputSize <= 0)
            {
                return;
            }
            
            int[] copyIndexes = new int[indexes.Length];
            System.Array.Copy(indexes, copyIndexes, indexes.Length);
            copyIndexes.Shuffle();

            for (int i = 0; i < indexes.Length; ++i)
            {
                Node<int> child;
                TreeOfAllCombinations(indexes, outputSize - 1, out child);
                child.Value = copyIndexes[i];
                root.Children.Add(child);
            }
        }

        /// <summary>
        /// Given a list of indexes, generate a list of all valid combinations.
        /// 
        /// For example: ([0,1], 2) will return
        ///     [[0, 0],
        ///      [0, 1],
        ///      [1, 0],
        ///      [1, 1]]
        ///      
        /// Where the second number in the input defines the size of the inner.
        /// Additionally, the ordering of these will be randomized.
        /// arrays.
        /// </summary>
        /// <param name="indexes"></param>
        /// <param name="outputSize"></param>
        /// <returns></returns>
        public static List<List<int>> ListOfAllCombinations(Node<int> root)
        {
            List<List<int>> output = new List<List<int>>();

            for (int i = 0; i < root.Children.Count; ++i)
            {
                List<List<int>> childOutput = ListOfAllCombinationsRecursive(root.Children[i]);

                for (int j = 0; j < childOutput.Count; ++j)
                {
                    output.Add(childOutput[j]);
                }
            }

            return output;
        }

        /// <summary>
        /// Generate a list of indexes one at a time, generate a list of all 
        /// valid combinations.
        /// 
        /// For example: ([0,1], 2) will return
        ///     [[0, 0],
        ///      [0, 1],
        ///      [1, 0],
        ///      [1, 1]]
        ///      
        /// Where the second number in the input defines the size of the inner.
        /// Additionally, the ordering of these will be randomized.
        /// 
        /// @todo: needs to generate one at a time rather than use the list method that is provided
        /// arrays.
        /// </summary>
        /// <param name="indexes"></param>
        /// <param name="outputSize"></param>
        /// <returns></returns>
        public static IEnumerator<List<int>> AllCombinationsGenerator(int[] indexes, int outputSize)
        {
            Node<int> root;
            TreeOfAllCombinations(indexes, outputSize, out root);
            List<List<int>> combinations = ListOfAllCombinations(root);

            for (int i = 0; i < combinations.Count; ++i)
            {
                yield return combinations[i];
            }
        }
        
        private static List<List<int>> ListOfAllCombinationsRecursive(Node<int> node)
        {
            List<List<int>> output = new List<List<int>>();
            if (node.Children.Count <= 0)
            {
                output.Add(new List<int>() { node.Value });
                return output;
            }

            for (int i = 0; i < node.Children.Count; ++i)
            {
                List<List<int>> childOutput = ListOfAllCombinationsRecursive(node.Children[i]);

                for (int j = 0; j < childOutput.Count; ++j)
                {
                    childOutput[j].Add(node.Value);
                    output.Add(childOutput[j]);
                }
            }

            return output;
        }
    }
#endregion
}
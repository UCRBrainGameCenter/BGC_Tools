using System.Collections.Generic;

namespace BGC.DataStructures.Generic
{
    /// <summary>
    /// Generic Node for generating tree structure
    /// 
    /// @todo: adding some functions to make this easier to use would 
    ///        be ideal
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Node<T>
    {
        public T Value;
        public List<Node<T>> Children;

        /// <summary>
        /// Construct a node with no children
        /// </summary>
        /// <param name="value"></param>
        public Node(T value)
        {
            Value = value;
            Children = new List<Node<T>>();
        }

        /// <summary>
        /// Construct a node with a list of children
        /// </summary>
        /// <param name="value"></param>
        /// <param name="children"></param>
        public Node(T value, List<Node<T>> children)
        {
            Value    = value;
            Children = children;
        }

        /// <summary>
        /// Construct a node with an array of children
        /// </summary>
        /// <param name="value"></param>
        /// <param name="children"></param>
        public Node(T value, Node<T>[] children)
        {
            Value = value;
            Children = new List<Node<T>>(children);
        }

        /// <summary>
        /// Return whether or not the node is a leaf or not
        /// </summary>
        public bool IsLeaf
        {
            get
            {
                return Children.Count == 0;
            }
        }

        /// <summary>
        /// Returns whether or not the node is an internal node or not
        /// </summary>
        public bool IsInternalNode
        {
            get
            {
                return !IsLeaf;
            }
        }
    }
}
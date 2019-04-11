using System.Collections.Generic;

namespace BGC.DataStructures.Generic
{
    /// <summary>
    /// Generic Node for generating tree structure
    /// 
    /// @todo: adding some functions to make this easier to use would be ideal
    /// </summary>
    public struct Node<T>
    {
        public T Value;
        public List<Node<T>> Children;

        /// <summary>Return whether the node is a leaf</summary>
        public bool IsLeaf => Children.Count == 0;

        /// <summary>Returns whether the node is an internal node</summary>
        public bool IsInternalNode => !IsLeaf;

        /// <summary>
        /// Construct a node with no children
        /// </summary>
        public Node(T value)
        {
            Value = value;
            Children = new List<Node<T>>();
        }

        /// <summary>
        /// Construct a node with a list of children
        /// @todo: Do we really want the node taking ownership of the passed-in list?
        /// </summary>
        public Node(T value, List<Node<T>> children)
        {
            Value = value;
            Children = children;
        }

        /// <summary>
        /// Construct a node with an array of children
        /// </summary>
        public Node(T value, Node<T>[] children)
        {
            Value = value;
            Children = new List<Node<T>>(children);
        }
    }
}
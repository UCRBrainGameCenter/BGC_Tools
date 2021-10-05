using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using BGC.Extensions;

namespace BGC.Mathematics
{
    public abstract class SequenceGenerator<T>
    {
        protected List<T> CurrentSequence { get; } = new List<T>();

        /// <summary>
        /// Called before each Generation Attempt. Returns whether it can attempt generation.
        /// By default this only allows one attempt, but can be overriden to, for example,
        /// relax restrictions at each attempt number.
        /// </summary>
        protected virtual bool InitializeAttempt(int attemptNumber)
        {
            return attemptNumber == 0;
        }

        /// <summary>
        /// Gets a queue of the naively valid options for the next element in the sequence.
        /// </summary>
        protected abstract Queue<T> GetShuffledNextOptions();

        /// <summary>
        /// Attempts to add the element to the end of the sequence,
        /// optionally testing against more rigorous inclusion criteria.
        /// Returns success.
        /// Failure means it fails to meet inclusion criteria and the state is not changed.
        /// Success means the state has been modified.
        /// </summary>
        protected virtual bool TryPushElement(T elem)
        {
            CurrentSequence.Add(elem);
            return true;
        }

        /// <summary>
        /// Remove the most-recently added stimulus from the sequence, rolling back the state.
        /// Returns the popped element.
        /// Throws InvalidOperationException if the sequence has no elements.
        /// </summary>
        protected virtual T PopElement()
        {
            if (CurrentSequence.Count == 0)
            {
                throw new InvalidOperationException("The options stack has no more elements!");
            }

            T removedElem = CurrentSequence[CurrentSequence.Count - 1];
            CurrentSequence.RemoveAt(CurrentSequence.Count - 1);
            return removedElem;
        }

        /// <summary>
        /// Generates a random sequence of items.
        /// </summary>
        /// <typeparam name="T">The type of item in the sequence.</typeparam>
        /// <param name="numItems">The number of items to include in the sequence.</param>
        /// <returns>The random list of items, or null if the sequence is impossible.</returns>
        public List<T> Generate(int numItems)
        {
            if (numItems == 0)
            {
                CurrentSequence.Clear();
                return CurrentSequence;
            }

            // Reserve enough space for the number of items to be added.
            // This just avoids multiple resizes.
            if (CurrentSequence.Capacity < numItems)
            {
                CurrentSequence.Capacity = numItems;
            }

            int attemptIndex = 0;
            while (true)
            {
                CurrentSequence.Clear();
                if (!InitializeAttempt(attemptIndex++))
                {
                    // Cannot initialize this attempt, so no list is possible.
                    return null;
                }

                Stack<Queue<T>> optionStack = new Stack<Queue<T>>();
                optionStack.Push(GetShuffledNextOptions());
                if (optionStack.Peek().Count == 0)
                {
                    // This attempt can't even get started
                    continue;
                }

                while (optionStack.Count > 0)
                {
                    Queue<T> allowedNext = optionStack.Peek();
                    if (allowedNext.Count == 0)
                    {
                        // No options are allowed next, so backtrack
                        optionStack.Pop();
                        PopElement();
                        continue;
                    }

                    // Grab the next element
                    T nextElement = allowedNext.Dequeue();

                    // Test element
                    if (TryPushElement(nextElement))
                    {
                        // Element added
                        if (CurrentSequence.Count == numItems)
                        {
                            return CurrentSequence;
                        }

                        // Add new options to the stack
                        optionStack.Push(GetShuffledNextOptions());
                    }
                }
            }
        }
    }

    /// <summary>
    /// ElementTypeSequenceGenerator is used to generate sequences which have a number of unique element types
    /// and a count of each element type available.
    /// The sequence then draws from these element types like a shuffled deck of cards.
    /// Each element type is represented as an integer from 0 to n-1 where n is the number of unique
    /// element types.
    /// Use GenerateFullList() to generate a sequence which uses all available instances of each element type.
    /// Use Generate() to generate a sequence of a specific length. This will fail if the length is more than all
    /// available element type instances.
    /// When used directly, there is no restriction on the ordering of types.
    /// This class can be overriden to provide specific restrictions on ordering:
    ///     - Override TestElement in order to prevent element types from appearing next in the sequence.
    ///     - Override InitializeAttempt to relax restrictions on subsequent attempts if the existing
    ///       restrictions end up being impossible to create a proper sequence.
    /// </summary>
    public class ElementTypeSequenceGenerator : SequenceGenerator<int>
    {
        protected readonly int maxSequenceLength;
        protected readonly int numElementTypes;
        protected readonly int[] intialElementCount;
        protected readonly int[] curElementCount;
        protected readonly Random randomizer = new Random();

        /// <summary>
        /// Create an ElementTypeSequenceGenerator with a certain number of each element type available.
        /// </summary>
        /// <param name="elementCount">An array whose length is equal to the number of unique element types and
        /// initialized with the count of each element type that is available to use in the sequence.</param>
        public ElementTypeSequenceGenerator(params int[] elementCount)
        {
            // Argument validation
            for (int i = 0; i < elementCount.Length; i++)
            {
                if (elementCount[i] < 0)
                {
                    throw new ArgumentException($"elementCount[{i}] cannot be less than zero ({elementCount[i]}).", nameof(elementCount));
                }
            }

            maxSequenceLength = elementCount.Sum();
            numElementTypes = elementCount.Length;
            intialElementCount = new int[numElementTypes];
            curElementCount = new int[numElementTypes];
            Array.Copy(elementCount, intialElementCount, numElementTypes);
            Array.Copy(elementCount, curElementCount, numElementTypes);
        }

        protected override bool InitializeAttempt(int attemptNumber)
        {
            CurrentSequence.Clear();
            Array.Copy(intialElementCount, curElementCount, intialElementCount.Length);
            return base.InitializeAttempt(attemptNumber);
        }

        /// <summary>
        /// Tests to see if the passed in element type is valid to continue the current sequence.
        /// Override this function to restrict the possible ordering of elements.
        /// NOTE: Overriding InitializeAttempt can be used to relax restrictions if no sequence
        /// could be generated.
        /// </summary>
        /// <param name="element">The element to continue the sequence.</param>
        /// <returns>true if element is valid to continue the sequence; otherwise false.</returns>
        protected virtual bool TestElement(int elementType) => true;

        protected override Queue<int> GetShuffledNextOptions()
        {
            Queue<int> newOptions = new Queue<int>();

            // Random selection based on the count remaining of each.
            // This is like drawing a card from a deck: there is a higher probability of choosing
            // element types that have more count remaining

            // Accumulate remaining elements
            int remainingCount = 0;
            bool[] elementAvailable = new bool[numElementTypes];
            for (int elemType = 0; elemType < numElementTypes; elemType++)
            {
                elementAvailable[elemType] = curElementCount[elemType] > 0 && TestElement(elemType);
                remainingCount += elementAvailable[elemType] ? curElementCount[elemType] : 0;
            }

            // Keep going as long as we have remaining elements unaccounted for
            while (remainingCount > 0)
            {
                // Select an element
                int targetNumber = randomizer.Next(0, remainingCount);

                for (int elemType = 0; elemType < numElementTypes; elemType++)
                {
                    if (elementAvailable[elemType])
                    {
                        // If the selected stim is in the range owned by i, we found it
                        if (targetNumber < curElementCount[elemType])
                        {
                            newOptions.Enqueue(elemType);
                            remainingCount -= curElementCount[elemType];
                            elementAvailable[elemType] = false;
                            break;
                        }

                        // Otherwise decrement the target number and keep looking
                        targetNumber -= curElementCount[elemType];
                    }
                }
            }

            return newOptions;
        }

        protected override bool TryPushElement(int elemType)
        {
            if (!base.TryPushElement(elemType))
            {
                return false;
            }

            curElementCount[elemType]--;
            return true;
        }

        protected override int PopElement()
        {
            int elemType = base.PopElement();
            curElementCount[elemType]++;
            return elemType;
        }

        public List<int> GenerateFullList() => Generate(maxSequenceLength);
    }
}

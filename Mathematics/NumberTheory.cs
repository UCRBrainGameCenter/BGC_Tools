using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Mathematics
{
    public static class NumberTheory
    {
        public static int LeastCommonMultiple(IEnumerable<int> numbers)
        {
            if (numbers.Count() == 1)
            {
                return numbers.First();
            }

            return MergedFactorization(numbers).Aggregate(1, (acc, value) => acc * value);
        }

        public static IEnumerable<int> MergedFactorization(IEnumerable<int> numbers)
        {
            if (numbers == null || numbers.Count() == 0)
            {
                yield break;
            }

            if (numbers.Count() == 1)
            {
                foreach (int factor in Factorize(numbers.First()))
                {
                    yield return factor;
                }

                yield break;
            }

            int[] nums = numbers.ToArray();

            for (int i = 0; i < nums.Length; i++)
            {
                //While any number is divisible by 2...
                while (nums[i] % 2 == 0)
                {
                    yield return 2;

                    for (int j = 0; j < nums.Length; j++)
                    {
                        //Remove a factor of 2 from all numbers that retain it
                        if (nums[j] % 2 == 0)
                        {
                            nums[j] /= 2;
                        }
                    }
                }
            }

            int max = nums.Max();
            foreach (int prime in PrimesUpTo((int)Math.Sqrt(max)))
            {
                for (int i = 0; i < nums.Length; i++)
                {
                    //While any number is divisible by prime...
                    while (nums[i] % prime == 0)
                    {
                        yield return prime;

                        for (int j = 0; j < nums.Length; j++)
                        {
                            //Remove a factor of prime from all numbers that retain it
                            if (nums[j] % prime == 0)
                            {
                                nums[j] /= prime;
                            }
                        }
                    }
                }
            }

        }


        public static IEnumerable<int> Factorize(int number)
        {
            if (number < 1)
            {
                yield break;
            }

            //Strip out factors of 2.
            //Many numbers requested will have many (or only) factors of 2.
            //This is an optimization
            while (number % 2 == 0)
            {
                yield return 2;
                number /= 2;
            }

            foreach (int prime in PrimesUpTo((int)Math.Sqrt(number)))
            {
                while (number % prime == 0)
                {
                    yield return prime;
                    number /= prime;
                }

                if (number == 1)
                {
                    break;
                }
            }

            if (number > 1)
            {
                yield return number;
            }
        }

        public static IEnumerable<int> PrimesUpTo(int number)
        {
            if (number < 2)
            {
                yield break;
            }

            //Include the boundary
            number++;

            BitArray primeField = new BitArray(number, true);
            primeField.Set(0, false);
            primeField.Set(1, false);
            yield return 2;

            //We don't bother setting the multiples of 2 because we don't bother checking them.

            int i;
            for (i = 3; i * i < number; i += 2)
            {
                if (primeField.Get(i))
                {
                    //i Is Prime
                    yield return i;

                    //Clear new odd factors
                    //All our primes are now odd, as are our primes Squared.
                    //This maens the numbers we need to clear start at i*i, and advance by 2*i
                    //For example j=3:  9 is the first odd composite, 15 is the next odd composite 
                    //  that's a factor of 3
                    for (int j = i * i; j < number; j += 2 * i)
                    {
                        primeField.Set(j, false);
                    }
                }
            }

            //Grab remainder of identified primes
            for (; i < number; i += 2)
            {
                if (primeField.Get(i))
                {
                    yield return i;
                }
            }
        }
    }
}

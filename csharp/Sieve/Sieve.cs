using System.Collections.Concurrent;

namespace Sieve;

public interface ISieve
{
    long NthPrime(long n);
}

public class SieveImplementation : ISieve
{
    public long NthPrime(long n)
    {
        if (n < 0)
        {
            return -1L;
        }
        if (n == 0)
        {
            return 2L;
        }

        // A theoretical upper bound of this algorithm. It is used to estimate what number would 
        // be the prime, and then the square root of that is taken for the stopping point of primes
        // to check. 15% is added on top just to overshoot.
        var upperBound = (long)Math.Ceiling(n * (Math.Log(n) + Math.Log(Math.Log(n))) * 1.15);
        var limit = (int)(Math.Sqrt(upperBound) + 1);

        // This function builds the prime factors we should expect up to theoretical limit.
        var primeFactors = BuildPrimeFactors(limit);

        // A segment of the space is made, this is just a means to break up the logic into smaller chunks.
        var segmentSize = 100000;
        for (long start = 3; start < upperBound; start += segmentSize)
        {
            // Finding the end of this segment. Typically it is were start + segmentSize - 1, but
            // for the last segment it ends at the upper bound.
            long end = start + segmentSize - 1;
            if (end > upperBound)
            {
                end = upperBound;
            }

            // An array is used to store the multiples as they are found.
            var isComposite = new bool[end - start + 1];

            // Loops through all the primes found from earlier, and adds them to themselves to find
            // all the multiples. Will keep a list of the composite numbers found.
            for (var i = 0; i < primeFactors.Count; i++)
            {
                // A check to ensure the prime squared is within the current segment.
                var prime = primeFactors[i];
                var primeSquared = prime * prime;
                if (primeSquared > end)
                {
                    break;
                }

                // This is the first multiple that is within the segment. The max and prime squared 
                // is used to ensure the multiple and the prime itself is not set to composite.
                var multiple = Math.Max(primeSquared, (start + prime - 1) / prime * prime);
                for (var j = multiple; j <= end; j += prime)
                {
                    // The number is set to be a composite number. The start value is leveraged
                    // for indexing. 
                    isComposite[j - start] = true;
                }
            }

            // After this segment finishes checking all the primes' multiples, it loops through all the 
            // isComposite array. Anything false decrements from the index of the incoming value. Once that
            // hits zero, the current number is the requested prime.
            for (var i = start; i < end; i += 2)
            {
                if (!isComposite[i - start])
                {
                    n--;
                    if (n == 0)
                    {
                        return i;
                    }
                }
            }
        }
        throw new Exception("The prime index was not reached before the theoretical upper bound.");
    }

    /// <summary>
    /// This function builds the prime factors up to the upper bound.
    /// </summary>
    /// <param name="upperBound">The upper bound to build prime numbers up to.</param>
    /// <returns>A list containing prime numbers.</returns>
    private static List<long> BuildPrimeFactors(int upperBound)
    {
        var isComposite = new bool[upperBound + 1];
        var primeFactors = new List<long>();

        // Iterates through all numbers, and their multiples. Records multiples as composite
        // numbers. Upon reaching a number, if it is not composite, it must be prime and that
        // is added to the list. Then all the multiples to the upper bound are set to composite.
        for (int i = 2; i <= upperBound; i++)
        {
            if (!isComposite[i])
            {
                primeFactors.Add(i);
                for (int j = i * 2; j <= upperBound; j += i)
                {
                    isComposite[j] = true;
                }
            }
        }

        return primeFactors;
    }




}
using System.Collections.Concurrent;

namespace Sieve;

public interface ISieve
{
    long NthPrime(long n);
}

public class SieveImplementation : ISieve
{

    // Fields used for Long Support
    private Dictionary<uint, List<long>> primeDictionary = new() { { 0, new List<long> { 2 } }};
    private long maxGenerated = 3;
    private uint maxDictionary = 0;

    // Fields used for No Long Support
    private List<long> primes = new List<long> { 2 };
    private long maxGeneratedNoLong = 2;

    // Test Fields Used with Parallelization
    private long newMaxGenerated = 2;
    private List<List<long>> PrimeFactors = new() { new List<long> { 2 } };
    private HashSet<long> PrimeFactorsSet = new() { 2 };
    private ConcurrentDictionary<long, bool> PrimeFactorsDictionary = new(10000000, Environment.ProcessorCount * 2) { };

    public SieveImplementation()
    {
        PrimeFactorsDictionary.TryAdd(2, true);
    }


    /// <summary>
    /// This function finds the nth prime number. You may pass in a number, and if it does not currently contain that prime number, 
    /// it will generate them until it is found, which it will then return.
    /// </summary>
    /// <param name="n">The nth prime to find.</param>
    /// <returns>The prime number at the supplied <paramref name="n"/>th index.</returns>
    public long NthPrimeDocumented(long n)
    {
        // Does not Support Negative Indexed Primes
        if (n < 0)
        {
            return -1L;
        }

        // This logic is used to find what position in the dictionary, and the list to use for the index of the prime. 
        var quotient = (uint)(n / uint.MaxValue);
        var remainder = (int)n % int.MaxValue;

        // If the Dictionary does not have a list that would contain the index, this while loop generates all the previous prime
        // numbers by creating lists to input into the dictionary until the index is reached. 
        while (maxDictionary < quotient)
        {
            // This loop generates all indexes remaining in the current list. 
            for (int i = primeDictionary[maxDictionary].Count; i <= int.MaxValue; i++)
            {
                // This same functionality is used elsewhere, so it can increment through numbers, and add the next prime.
                IncrementUntilPrime();
            }
            // Once the list at this point of the dictionary is full, the number of dictionaries is incremented, and a new
            // list is created to hold more prime numbers. If the index being requested is found in this new dictionary,
            // the while loop will end.
            maxDictionary++;
            primeDictionary[maxDictionary] = new List<long>();
        }
        // This is simply used to check if the prime number has already been generated. If so, it is returned.
        if (primeDictionary.Count > quotient && primeDictionary[quotient].Count > remainder)
        {
            return primeDictionary[quotient][remainder];
        }
        else
        {
            // This loop is extremely similar to the loop above but it is used on the final dictionary the needs generating. 
            // This will generate up to the index requested.
            for (int i = primeDictionary[maxDictionary].Count; i <= remainder; i++)
            {
                IncrementUntilPrime();
            }
            return primeDictionary[quotient][remainder]; 
        } 
    }

    /// <summary>
    /// This function will iterate through all numbers until a prime is found using maxDictionary and maxGenerated. This 
    /// will place the next time at the furthest index available in, and then will return. 
    /// </summary>
    private void IncrementUntilPrime()
    {
        // This while loop is used to increment through all numbers. As a new number is selected, all dictionaries
        // and all items in the list are checked to make sure that the new number is not divisible by a previously 
        // found prime, hence a new prime. If the number is divisible, it is not a prime and the number is incremented
        // until a prime is found.
        while (true)
        {
            var increment = false;
            // Technically, since we are dealing with primes, we only need to check the primes that are less than 
            // the square root of the potential prime. If we are greater than the square root, the number must 
            // be prime, because we checked all the factors before it as well.
            var maxCheck = Math.Sqrt(maxGenerated);
            for (uint j = 0; j <= maxDictionary; j++)
            {
                for (var k = 0; k < int.MaxValue; k++)
                {
                    if (maxGenerated % primeDictionary[j][k] == 0)
                    {
                        increment = true;
                        break;
                    }
                    if (primeDictionary[j][k] > maxCheck)
                    {
                        // Once a prime is found, it is added to list, the new test number is generated, and if the 
                        // next index needs generating, the loops will continue. Since this function has done its
                        // work, it can return.
                        primeDictionary[maxDictionary].Add(maxGenerated);
                        maxGenerated++;
                        return;
                    }
                }
                // This section is used as a 'layered' break. This is a way for the inner loop to call a break in
                // the outer loop. 
                if (increment)
                {
                    break;
                }
            }
            if (increment)
            {
                maxGenerated++;
            }
        }
    }


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




    public long NthPrimeNoLong(long n)
    {
        if (primes.Count > n)
        {
            return primes[(int)n];
        }
        else
        {
            while (primes.Count <= n)
            {
                var layeredContinue = false;
                for (var i = maxGeneratedNoLong + 1; i < maxGeneratedNoLong + 100000; i++)
                {
                    var maxCheck = Math.Sqrt(i);
                    for (var j = 0; j < primes.Count; j++)
                    {
                        if (i % primes[j] == 0)
                        {
                            layeredContinue = true;
                            break;
                        }
                        if (primes[j] > maxCheck)
                        {
                            break;
                        }
                    }
                    if (layeredContinue)
                    {
                        layeredContinue = false;
                        continue;
                    }
                    primes.Add(i);
                }
                maxGeneratedNoLong += 100000;
            }
            return primes[(int)n];
        }
    }

    public long NthPrime2(long n)
    {
        if (primes.Count > n)
        {
            return primes[(int)n];
        }
        else
        {
            while (primes.Count <= n)
            {
                for (var i = maxGeneratedNoLong + 1; i < maxGeneratedNoLong + 100000; i++)
                {
                    var primeModFound = false;
                    var maxCheck = Math.Sqrt(i);
                    Parallel.For(0, primes.Count, (j, state) =>
                    {

                        if (i % primes[j] == 0)
                        {
                            primeModFound = true;
                            state.Break();
                        }
                        if (primes[j] > maxCheck)
                        {
                            state.Break();
                        }
                    });
                    if (!primeModFound)
                    {
                        primes.Add(i);
                    }
                }
                maxGeneratedNoLong += 100000;
            }
            return primes[(int)n];
        }
    }

    public long NthPrimeFactor(long n)
    {
        if (PrimeFactors.Count > n)
        {
            return PrimeFactors[(int)n][0];
        }
        else
        {
            while (PrimeFactors.Count <= n)
            {
                for (var x = newMaxGenerated + 1; x < newMaxGenerated + 1000; x++)
                {
                    if (PrimeFactorsSet.Contains(x))
                    {
                        continue;
                    }
                    //var maxCheck = Math.Sqrt(x);
                    //var sync = new object();
                    Parallel.ForEach(PrimeFactors, p =>
                    {
                        var currentMax = p[p.Count - 1];
                        while (currentMax <= x)
                        {
                            currentMax = p[p.Count - 1] + p[0];
                            //lock (sync)
                            //{
                            PrimeFactorsDictionary.TryAdd(currentMax, false);
                            p.Add(currentMax);
                            //if (!PrimeFactorsSet.Contains(currentMax))
                            //    {
                            //        PrimeFactorsSet.Add(currentMax);
                            //    }
                            //    PrimeFactors[i].Add(currentMax);
                            //}
                        }
                    });
                    if (PrimeFactorsDictionary.TryAdd(x, true))
                    {
                        PrimeFactors.Add(new List<long> { x });
                    }
                    //    if (!PrimeFactorsSet.Contains(x))
                    //{
                    //    PrimeFactorsSet.Add(x);
                    //    PrimeFactors.Add(new List<long> { x });
                    //}
                }
                newMaxGenerated += 1000;
            }
            return PrimeFactors[(int)n][0];
        }
    }
}
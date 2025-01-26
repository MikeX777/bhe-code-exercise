using System.Collections.Concurrent;

namespace Sieve;

public interface ISieve
{
    long NthPrime(long n);
}

public class SieveImplementation : ISieve
{
    private long maxGenerated = 2;
    private List<long> primes = new List<long> { 2 };

    private long newMaxGenerated = 2;
    private List<List<long>> PrimeFactors = new() { new List<long> { 2 } };
    private HashSet<long> PrimeFactorsSet = new() { 2 };
    private ConcurrentDictionary<long, bool> PrimeFactorsDictionary = new(10000000, Environment.ProcessorCount * 2) { };

    public SieveImplementation()
    {
        PrimeFactorsDictionary.TryAdd(2, true);
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


    public long NthPrime(long n)
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
                var primeFound = false;
                for (var i = maxGenerated + 1; i < maxGenerated + 100000; i++)
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
                maxGenerated += 100000;
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
                for (var i = maxGenerated + 1; i < maxGenerated + 100000; i++)
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
                maxGenerated += 100000;
            }
            return primes[(int)n];
        }
    }


    IEnumerable<long> LongRange(long start, long end)
    {
        for (var i = start; i <= end; i++)
        {
            yield return i;
        }
    }
}
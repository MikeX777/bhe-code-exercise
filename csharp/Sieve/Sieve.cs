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
    private List<List<long>> PrimeFactors = new List<List<long>> { new List<long> { 2 } };
    private HashSet<long> PrimeFactorsSet = new HashSet<long> { 2 };

    public long NthPrime(long n)
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
                    for (var i = 0; i < PrimeFactors.Count; i++)
                    {
                        var currentMax = PrimeFactors[i][PrimeFactors[i].Count - 1];
                        while (currentMax <= x)
                        {
                            currentMax = PrimeFactors[i][PrimeFactors[i].Count - 1] + PrimeFactors[i][0];
                            if (!PrimeFactorsSet.Contains(currentMax))
                            {
                                PrimeFactorsSet.Add(currentMax);
                            }
                            PrimeFactors[i].Add(currentMax);
                        }
                    }
                    if (!PrimeFactorsSet.Contains(x))
                    {
                        PrimeFactorsSet.Add(x);
                        PrimeFactors.Add(new List<long> { x });
                    }
                }
                newMaxGenerated += 1000;
            }
            return PrimeFactors[(int)n][0];
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
            var maxPrime = Math.Sqrt(n);
            while (primes.Count <= n)
            {
                var layeredContinue = false;
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


    IEnumerable<long> LongRange(long start, long end)
    {
        for (var i = start; i <= end; i++)
        {
            yield return i;
        }
    }
}
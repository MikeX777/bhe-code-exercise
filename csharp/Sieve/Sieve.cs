namespace Sieve;

public interface ISieve
{
    long NthPrime(long n);
}

public class SieveImplementation : ISieve
{
    private long maxGenerated = 2;
    private List<long> primes = new List<long> { 2 };



    public long NthPrime(long n)
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
                for (var i = maxGenerated + 1; i < maxGenerated + 10000; i++)
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
                maxGenerated += 10000;
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
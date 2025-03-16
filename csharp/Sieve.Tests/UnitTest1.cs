using Sieve.Interfaces;

namespace Sieve.Tests

{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestNthPrime()
        {
            ISieve sieve = new SieveImplementation();
            Assert.AreEqual(2, sieve.NthPrime(0));
            Assert.AreEqual(3, sieve.NthPrime(1));
            Assert.AreEqual(5, sieve.NthPrime(2));
            Assert.AreEqual(7, sieve.NthPrime(3));
            Assert.AreEqual(11, sieve.NthPrime(4));
            Assert.AreEqual(13, sieve.NthPrime(5));
            Assert.AreEqual(17, sieve.NthPrime(6));
            Assert.AreEqual(19, sieve.NthPrime(7));
            Assert.AreEqual(23, sieve.NthPrime(8));
            Assert.AreEqual(29, sieve.NthPrime(9));
            Assert.AreEqual(31, sieve.NthPrime(10));
            Assert.AreEqual(37, sieve.NthPrime(11));
            Assert.AreEqual(41, sieve.NthPrime(12));
            Assert.AreEqual(71, sieve.NthPrime(19));
            Assert.AreEqual(73, sieve.NthPrime(20));
            Assert.AreEqual(79, sieve.NthPrime(21));
            Assert.AreEqual(89, sieve.NthPrime(23));
            Assert.AreEqual(97, sieve.NthPrime(24));
            Assert.AreEqual(113, sieve.NthPrime(29));
            Assert.AreEqual(541, sieve.NthPrime(99));
            Assert.AreEqual(3581, sieve.NthPrime(500));
            Assert.AreEqual(7793, sieve.NthPrime(986));
            Assert.AreEqual(17393, sieve.NthPrime(2000));
            Assert.AreEqual(15485867, sieve.NthPrime(1000000));
            Assert.AreEqual(179424691, sieve.NthPrime(10000000));
            Assert.AreEqual(2038074751, sieve.NthPrime(100000000)); // not required, just a fun challenge
        }
    }
}
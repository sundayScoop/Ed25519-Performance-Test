using System.Collections;
using System.Numerics;
using System.Security.Cryptography;

class Point
{
    public BigInteger X { get; } // Change to private
    public BigInteger Y { get; }

    public Point(BigInteger x, BigInteger y)
    {
        this.X = x;
        this.Y = y;
    }

    // Using double and add algorithm
    public static Point operator *(Point point, BigInteger num)
    {
        // TODO: Check to see if 0 < num < N

        Point newPoint = new Point(BigInteger.Zero, BigInteger.One);

        while (num > BigInteger.Zero)
        {
            if ((num & BigInteger.One) == BigInteger.One) newPoint = newPoint + point;
            point = point + point;
            num = num >> 1;
        }
        return newPoint;
    }

    public static Point operator +(Point point1, Point point2)
    {
        // TODO: check to see if point is on curve and on the prime order subgroup
        // https://bibliotecadigital.ipb.pt/bitstream/10198/24067/1/Nakai_Eduardo.pdf Page 20. I added finite fields

        return new Point(
            ((((point1.X * point2.Y + point1.Y * point2.X) * BigInteger.ModPow(BigInteger.One + Ed25519.D * point1.X * point2.X * point1.Y * point2.Y, Ed25519.M - 2, Ed25519.M)) + Ed25519.M) % Ed25519.M) + Ed25519.M, // +M is ridiculous
            ((((point1.Y * point2.Y - Ed25519.A * point1.X * point2.X) * BigInteger.ModPow(BigInteger.One - Ed25519.D * point1.X * point2.X * point1.Y * point2.Y, Ed25519.M - 2, Ed25519.M)) + Ed25519.M) % Ed25519.M) + Ed25519.M
            );
    }

    public BigInteger Mod(BigInteger a, BigInteger m)
    {
        BigInteger n = a % m;
        return n < 0 ? n + m : n;
    }
}

readonly struct Ed25519
{
    public static BigInteger M = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819949");
    public static BigInteger N = BigInteger.Parse("7237005577332262213973186563042994240857116359379907606001950938285454250989");
    public static BigInteger D = BigInteger.Parse("-4513249062541557337682894930092624173785641285191125241628941591882900924598840740"); // -121665 * pow(121666, -1, 2**255 - 19)
    public static BigInteger A = BigInteger.MinusOne;
    public static Point G = new Point(BigInteger.Parse("15112221349535400772501151409588531511454012693041857206046113283949847762202"), BigInteger.Parse("46316835694926478169428394003475163141307993866256225615783033603165251855960"));

}

class Test
{
    public void signTest()
    {
        // random bytes for h, priv and r
        byte[] r_b = RandomNumberGenerator.GetBytes(32);
        byte[] h_b = RandomNumberGenerator.GetBytes(64);
        byte[] priv_b = RandomNumberGenerator.GetBytes(32);

        // bytes to numbers
        BigInteger priv = new BigInteger(priv_b) % Ed25519.N; // Mod N to ensure size constraint
        BigInteger r = new BigInteger(r_b) % Ed25519.N;
        BigInteger h = new BigInteger(h_b); // No need for constraint on h

        Point R = Ed25519.G * r;
        BigInteger s = r + (h * priv);

        ///// Verifying

        bool valid = Ed25519.G * s == R + ((Ed25519.G * priv) * h);  // Ed25519.G * priv is just public of priv
    }
}

class Program
{
    static void Main(string[] args)
    {

        int i = 0;
        while (i < 10)
        {

            // random bytes for h, priv and r
            byte[] r_b = RandomNumberGenerator.GetBytes(32);
            byte[] h_b = RandomNumberGenerator.GetBytes(64);
            byte[] priv_b = RandomNumberGenerator.GetBytes(32);

            // bytes to numbers
            BigInteger priv = Mod(new BigInteger(priv_b), Ed25519.N); // Mod N to ensure size constraint
            BigInteger r = Mod(new BigInteger(r_b), Ed25519.N);
            BigInteger h = Mod(new BigInteger(h_b), Ed25519.N);

            Point R = Ed25519.G * r;
            BigInteger s = Mod(r + (h * priv), Ed25519.N);

            ///// Verifying
            var point1 = Ed25519.G * s;
            var point2 = R + ((Ed25519.G * priv) * h);

            bool valid = point1.X == point2.X; // Ed25519.G * priv is just public of priv
            Console.WriteLine(valid);
            i += 1;
        }

    }
    private static BigInteger Mod(BigInteger a, BigInteger p) 
    {
        BigInteger m = a % p; return m < 0 ? m + p : m; 
    }

}
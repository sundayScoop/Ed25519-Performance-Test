using System.Collections;
using System.Drawing;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;

class Point
{
    public BigInteger X { get; } // Change to private
    public BigInteger Y { get; }
    public BigInteger Z { get; }
    public BigInteger T { get; }

    public Point(BigInteger x, BigInteger y, BigInteger z, BigInteger t)
    {
        X = x;
        Y = y;
        Z = z;
        T = t;
    }
    public BigInteger GetX() => Mod((X * BigInteger.ModPow(Z, Ed25519.M - 2, Ed25519.M)));
    public BigInteger GetY() => Mod((Y* BigInteger.ModPow(Z, Ed25519.M - 2, Ed25519.M)));

    // Using double and add algorithm
    public static Point operator *(Point point, BigInteger num)
    {
        // TODO: Check to see if 0 < num < N

        Point newPoint = new Point(BigInteger.Zero, BigInteger.One, BigInteger.One, BigInteger.Zero);

        while (num > BigInteger.Zero)
        {
            if ((num & BigInteger.One).Equals(BigInteger.One))
            {
                newPoint = newPoint + point;
            }
            point = Double(point);
            num = num >> 1;

        }
        return newPoint;
    }
    public static Point Double(in Point point)
    {
        ref readonly BigInteger two = ref Ed25519.Two;
        ref readonly BigInteger a = ref Ed25519.A;

        BigInteger A = Mod(point.X * point.X);
        BigInteger B = Mod(point.Y * point.Y);
        BigInteger C = Mod(two * Mod(point.Z * point.Z));
        BigInteger D = Mod(a * A);
        BigInteger x1y1 = point.X + point.Y;
        BigInteger E = Mod(Mod(x1y1 * x1y1) - A - B);
        BigInteger G = D + B;
        BigInteger F = G - C;
        BigInteger H = D - B;
        BigInteger X3 = Mod(E * F);
        BigInteger Y3 = Mod(G * H);
        BigInteger T3 = Mod(E * H);
        BigInteger Z3 = Mod(F * G);
        return new Point(X3, Y3, Z3, T3);
    }
    public static Point operator +(in Point point1, in Point point2)
    {
        // TODO: check to see if point is on curve and on the prime order subgroup

        ref readonly BigInteger two = ref Ed25519.Two;

        BigInteger A = Mod((point1.Y - point1.X) * (point2.Y + point2.X));
        BigInteger B = Mod((point1.Y + point1.X) * (point2.Y - point2.X));
        BigInteger F = Mod(B - A);
        if (F.Equals(BigInteger.Zero)) return Double(point1); // Same point.
        BigInteger C = Mod(point1.Z * two * point2.T);
        BigInteger D = Mod(point1.T * two * point2.Z);
        BigInteger E = D + C;
        BigInteger G = B + A;
        BigInteger H = D - C;
        BigInteger X3 = Mod(E * F);
        BigInteger Y3 = Mod(G * H);
        BigInteger T3 = Mod(E * H);
        BigInteger Z3 = Mod(F * G);
        return new Point(X3, Y3, Z3, T3);
    }
    private static BigInteger Mod(in BigInteger a)
    {
        BigInteger res = a % Ed25519.M;
        return res >= BigInteger.Zero ? res : Ed25519.M + res;
    }
}

readonly struct Ed25519
{
    public static BigInteger M = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819949");
    public static BigInteger N = BigInteger.Parse("7237005577332262213973186563042994240857116359379907606001950938285454250989");
    public static BigInteger D = BigInteger.Parse("37095705934669439343138083508754565189542113879843219016388785533085940283555"); // -121665 * pow(121666, -1, 2**255 - 19)

    private static BigInteger minusOne = BigInteger.MinusOne;
    public static ref readonly BigInteger MinusOne => ref minusOne;
    public static ref readonly BigInteger A => ref minusOne;

    private static BigInteger two = BigInteger.Parse("2");
    public static ref readonly BigInteger Two => ref two;

    

    private static BigInteger gx = BigInteger.Parse("15112221349535400772501151409588531511454012693041857206046113283949847762202");
    private static BigInteger gy = BigInteger.Parse("46316835694926478169428394003475163141307993866256225615783033603165251855960");
    private static BigInteger gt = BigInteger.Parse("46827403850823179245072216630277197565144205554125654976674165829533817101731");
    private static Point g = new Point(gx, gy, BigInteger.One, gt);
    public static ref readonly Point G => ref g;
}


class Program
{
    static void Main(string[] args)
    {
        ref readonly var G = ref Ed25519.G;
        int i = 0;
        while (i < 100)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // random bytes for h, priv and r
            byte[] r_b = RandomNumberGenerator.GetBytes(32);
            byte[] h_b = RandomNumberGenerator.GetBytes(64);
            byte[] priv_b = RandomNumberGenerator.GetBytes(32);

            // bytes to numbers
            BigInteger priv = Mod(new BigInteger(priv_b)); // Mod N to ensure size constraint
            BigInteger r = Mod(new BigInteger(r_b));
            BigInteger h = Mod(new BigInteger(h_b));


            Point R = G * r;
            BigInteger s = Mod(r + (h * priv));
            watch.Stop();
            Console.WriteLine($"Total Sign Time: {watch.ElapsedMilliseconds} ms");

            var pub = G * priv;
            watch.Start();

            ///// Verifying
            var point1 = G * s;
            var point2 = R + (pub * h);


            bool valid = point1.GetX().Equals(point2.GetX());

            watch.Stop();
            Console.WriteLine($"Total Verify Time: {watch.ElapsedMilliseconds} ms\n");
            Console.WriteLine(valid);
            i += 1;
        }
    }
    private static BigInteger Mod(in BigInteger a)
    {
        //Console.WriteLine(a.ToString());
        BigInteger res = a % Ed25519.N;
        return res >= BigInteger.Zero ? res : Ed25519.N + res;
    }

}

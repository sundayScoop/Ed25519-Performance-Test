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

    // Using double and add algorithm
    public static Point operator *(Point point, BigInteger num)
    {
        // TODO: Check to see if 0 < num < N

        Point newPoint = new Point(BigInteger.Zero, BigInteger.One, BigInteger.One, BigInteger.Zero);


        while (num > BigInteger.Zero)
        {
            if ((num & BigInteger.One).Equals(BigInteger.One))
            {
                newPoint = Add(newPoint, point);
                Console.WriteLine("Add");
            }
            point = Double(point);
            Console.WriteLine("Double");
            num = num >> 1;
            Console.WriteLine(num.ToString());
        }
        return newPoint;
    }
    public static Point Double(Point point)
    {
        BigInteger A = Mod(point.X * point.X);
        BigInteger B = Mod(point.Y * point.Y);
        BigInteger C = Mod(BigInteger.Parse("2") * Mod(point.Z * point.Z));
        BigInteger D = Mod(BigInteger.MinusOne * A);
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
    public static Point Double2(Point point)
    {
        BigInteger A = Mod(point.X * point.X);
        BigInteger B = Mod(point.Y * point.Y);
        BigInteger D = Mod(BigInteger.MinusOne * A);
        BigInteger x1y1 = point.X + point.Y;
        BigInteger E = Mod(Mod(x1y1 * x1y1) - A - B);
        BigInteger G = D + B;
        BigInteger H = D - B;
        BigInteger X3 = Mod(E * (G-2));
        BigInteger Y3 = Mod(G * H);
        BigInteger T3 = Mod(E * H);
        BigInteger Z3 = Mod(Mod(G * G) - (2 * G));
        return new Point(X3, Y3, Z3, T3);
    }
    public static Point Add(Point point1, Point point2)
    {
        // TODO: check to see if point is on curve and on the prime order subgroup

        
        BigInteger A = Mod((point1.Y - point1.X) * (point2.Y + point2.X));
        BigInteger B = Mod((point1.Y + point1.X) * (point2.Y - point2.X));
        BigInteger F = Mod(B - A);
        if (F.Equals(BigInteger.Zero))
        {
            Console.WriteLine("here");
            return Double(point1); // Same point.
        }
        BigInteger C = Mod(point1.Z * BigInteger.Parse("2") * point2.T);
        BigInteger D = Mod(point1.T * BigInteger.Parse("2") * point2.Z);
        BigInteger E = D + C;
        BigInteger G = B + A;
        BigInteger H = D - C;
        BigInteger X3 = Mod(E * F);
        BigInteger Y3 = Mod(G * H);
        BigInteger T3 = Mod(E * H);
        BigInteger Z3 = Mod(F * G);
        return new Point(X3, Y3, Z3, T3);
    }

    private static BigInteger Mod2(BigInteger a)
    {
        BigInteger n = a % Ed25519.M; return n < 0 ? n + Ed25519.M : n;
    }
    private static BigInteger Mod(BigInteger a)
    {
        //Console.WriteLine(a.ToString());
        BigInteger res = a % Ed25519.M;
        return res >= BigInteger.Zero ? res : Ed25519.M + res;
    }
}

readonly struct Ed25519
{
    public static BigInteger M = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819949");
    public static BigInteger N = BigInteger.Parse("7237005577332262213973186563042994240857116359379907606001950938285454250989");
    public static BigInteger D = BigInteger.Parse("37095705934669439343138083508754565189542113879843219016388785533085940283555"); // -121665 * pow(121666, -1, 2**255 - 19)
    public static BigInteger A = BigInteger.MinusOne;
    public static BigInteger Gx = BigInteger.Parse("15112221349535400772501151409588531511454012693041857206046113283949847762202");
    public static BigInteger Gy = BigInteger.Parse("46316835694926478169428394003475163141307993866256225615783033603165251855960");
    public static BigInteger Gt = BigInteger.Parse("46827403850823179245072216630277197565144205554125654976674165829533817101731");
    public static Point G = new Point(Gx, Gy, BigInteger.One, Gt);

}


class Program
{
    static void Main(string[] args)
    {
        //var point = Point.Add(Ed25519.G, Ed25519.G);
        //var p2 = Ed25519.G * BigInteger.Parse("4");
        Point l0 = new Point(BigInteger.Zero, BigInteger.One, BigInteger.One, BigInteger.Zero);

        var p2 = Point.Double(Ed25519.G);

        Console.WriteLine(p2.X.ToString());
    }
    private static BigInteger Mod(BigInteger a, BigInteger p) 
    {
        BigInteger m = a % p; return m < 0 ? m + p : m; 
    }

}
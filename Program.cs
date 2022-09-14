using System.Collections;
using System.Security.Cryptography;
using Org.BouncyCastle.Math;
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

        int d = 0;

        Point newPoint = new Point(BigInteger.Zero, BigInteger.One);

        while (num.CompareTo(BigInteger.Zero) > 0)
        {
            if ((num.And(BigInteger.One)).Equals(BigInteger.One))
            {
                newPoint = newPoint + point;
                d += 1;
            }
            point = point + point;
            d += 1;
            num = num.ShiftRight(1);
        }
        Console.WriteLine("D: " + d.ToString());
        return newPoint;
    }

    public static Point operator +(Point point1, Point point2)
    {
        // TODO: check to see if point is on curve and on the prime order subgroup
        // https://bibliotecadigital.ipb.pt/bitstream/10198/24067/1/Nakai_Eduardo.pdf Page 20. I added finite fields


        return new Point(
            // (   ( X1   *         Y2     +      Y1       *        X2   )   *     (    1          +  (     D         *       X1         *       X2        *        Y1        *         Y2  ) )  ^                M - 2                   % M       )    % M         
            ((point1.X.Multiply(point2.Y).Add(point1.Y.Multiply(point2.X))).Multiply((BigInteger.One.Add(Ed25519.D.Multiply(point1.X).Multiply(point2.X).Multiply(point1.Y).Multiply(point2.Y))).ModPow(Ed25519.M.Subtract(BigInteger.Two), Ed25519.M))).Mod(Ed25519.M),
            // (   ( Y1   *         Y2         -     (      A       *        X1      *         X2  ) )    *      (       1         -          D       *       X1        *       X2        *        Y1        *         Y2  )    ^                M - 2                      % M       )    % M     
            ((point1.Y.Multiply(point2.Y).Subtract(Ed25519.A.Multiply(point1.X).Multiply(point2.X))).Multiply((BigInteger.One.Subtract(Ed25519.D.Multiply(point1.X).Multiply(point2.X).Multiply(point1.Y).Multiply(point2.Y))).ModPow(Ed25519.M.Subtract(BigInteger.Two), Ed25519.M))).Mod(Ed25519.M)
            );
    }

}

readonly struct Ed25519
{
    public static BigInteger M = new BigInteger("57896044618658097711785492504343953926634992332820282019728792003956564819949", 10);
    public static BigInteger N = new BigInteger("7237005577332262213973186563042994240857116359379907606001950938285454250989", 10);
    public static BigInteger D = new BigInteger("-4513249062541557337682894930092624173785641285191125241628941591882900924598840740", 10); // -121665 * pow(121666, -1, 2**255 - 19)
    public static BigInteger A = new BigInteger("-1", 10);
    public static BigInteger Gx = new BigInteger("15112221349535400772501151409588531511454012693041857206046113283949847762202", 10);
    public static BigInteger Gy = new BigInteger("46316835694926478169428394003475163141307993866256225615783033603165251855960", 10);

    public static Point G = new Point(Gx, Gy);

}

class Program
{
    static void Main(string[] args)
    {
        int i = 0;
        while (i < 10)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // random bytes for h, priv and r
            byte[] r_b = RandomNumberGenerator.GetBytes(32);
            byte[] h_b = RandomNumberGenerator.GetBytes(64);
            byte[] priv_b = RandomNumberGenerator.GetBytes(32);

            // bytes to numbers
            BigInteger priv = new BigInteger(priv_b).Mod(Ed25519.N); // Mod N to ensure size constraint
            BigInteger r = new BigInteger(r_b).Mod(Ed25519.N);
            BigInteger h = new BigInteger(h_b).Mod(Ed25519.N);
            Console.WriteLine(r.ToString());

            Point R = Ed25519.G * r;
            BigInteger s = (r.Add(h.Multiply(priv))).Mod(Ed25519.N);
            watch.Stop();
            Console.Write($"Total Sign Time: {watch.ElapsedMilliseconds} ms");

            var pub = Ed25519.G * priv;
            watch.Start();
            
            ///// Verifying
            var point1 = Ed25519.G * s;
            var point2 = R + ((pub) * h);


            bool valid = point1.X.Equals(point2.X);

            watch.Stop();
            Console.Write($"Total Verify Time: {watch.ElapsedMilliseconds} ms");
            Console.WriteLine(valid);
            i += 1;
        }

    }
    

}
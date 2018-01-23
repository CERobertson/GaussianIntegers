using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In_Extremis.Editor.Gauss
{
    public class Gaussian
    {
        public static bool started = false;
        static int size = 2500;
        static Gaussian()
        {
            if (!started)
            {
                Gaussian.Factors = new Dictionary<int, PrimeFactors>(size);
                for (int i = 0; i < size; i++)
                {
                    Gaussian.Factors[i] = new PrimeFactors(i);
                    Gaussian.Factors[i].ValidateRing();
                }
                started = true;
            }
        }
        public static Dictionary<int, PrimeFactors> Factors;
        public static Gaussian operator *(Gaussian g1, Gaussian g2)
        {
            return new Gaussian((g1.a * g2.a) - (g1.b * g2.b), (g1.b * g2.a) + (g1.a * g2.b));
        }
        public static Gaussian operator +(Gaussian g1, Gaussian g2)
        {
            return new Gaussian((g1.a + g2.a), (g1.b + g2.b));
        }
        public int a { get; }
        public int b { get; }
        public bool pair {
            get {
                var n = Norm();
                return (n - 1) % 4 == 0 && Gaussian.Factors[n].IsPrime;
            }
        }
        public bool zero { get { return a == 0 && b == 0; } }
        public Gaussian(int a, int b)
        {
            this.a = a;
            this.b = b;
        }
        public Gaussian Conjugate()
        {
            return new Gaussian(a, -b);
        }
        public int Norm()
        {
            return (a * a) + (b * b);
        }
    }
}

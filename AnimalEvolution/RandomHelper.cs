using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalEvolution
{
    static class RandomHelper
    {
        public static double NextGaussian(this Random random)
        {
            return Math.Sqrt(-2.0 * Math.Log(random.NextDouble()*0.999999+0.000001)) * Math.Sin(2.0 * Math.PI * random.NextDouble());
        }
    }
}

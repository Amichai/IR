using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workbench2;
using Workbench2.Generators;

namespace Workbench2Console {
    class Program {
        static void Main(string[] args) {
            int max = 28;

            Func<SingleFeature> g1 = () => {
                return new SingleFeature(
                    new PixelProdGenerator(rand.Next(max), rand.Next(max), rand.Next(max), rand.Next(max))
                    );
            };

            Func<SingleFeature> g2 = () => {
                return new SingleFeature(
                    new PixelDiffGenerator(rand.Next(max), rand.Next(max), rand.Next(max), rand.Next(max))
                    );
            };

            //var p1 = new Workbench2.Processor("p1");
            var p1 = new Workbench2.Processor(g1, "pixelpixelProd");
            var p2 = new Workbench2.Processor(g2, "pixelDiff");

            ////p1.accumulator = (Action<Dictionary<string, double>, Dictionary<string, double>>)((a, b) => Add(a, b));
            //p1.accumulator = (a, b) => Add(a, b, 1);
            p1.accumulator = (a, b) => Add(a, b, 4);
            p2.accumulator = (a, b) => Add(a, b, 4);

            //p3.accumulator = (a, b) => Add(a, b, 3);


            //Parallel.Invoke(
            //Parallel.Invoke((Action)(() => p2.Run()), p1.Run, p3.Run);
            Parallel.Invoke(p1.Run, p2.Run);

        }
        private static Random rand = new Random();

        private static void Add(Dictionary<string, double> from, Dictionary<string, double> to, double exp) {
            foreach (var l in from.Keys) {
                double val = Math.Pow(from[l], exp);
                if (!to.ContainsKey(l)) {
                    to[l] = val;
                } else {
                    to[l] += val;
                }
            }
        }
    }
}

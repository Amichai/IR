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

            //Func<SingleFeature> g2 = () => {
            //    return new SingleFeature(
            //        new PixelDiffGenerator(rand.Next(max), rand.Next(max), rand.Next(max), rand.Next(max))
            //        );
            //};

            var p1 = new Workbench2.Processor(g1, "exp4");
            p1.accumulator = (a, b) => Add(a, b, 4);
            p1.Run();

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

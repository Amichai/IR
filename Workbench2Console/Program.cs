using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workbench2;

namespace Workbench2Console {
    class Program {
        static void Main(string[] args) {
            //var p1 = new Workbench2.Processor("p1");
            //var p2 = new Workbench2.Processor("p2");
            var p3 = new Workbench2.Processor("p3");

            ////p1.accumulator = (Action<Dictionary<string, double>, Dictionary<string, double>>)((a, b) => Add(a, b));
            //p1.accumulator = (a, b) => Add(a, b, 1);
            //p2.accumulator = (a, b) => Add(a, b, 2);
            p3.accumulator = (a, b) => Add(a, b, 3);

            //Parallel.Invoke(
            //Parallel.Invoke((Action)(() => p2.Run()), p1.Run, p3.Run);
            Parallel.Invoke(p3.Run);

        }

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

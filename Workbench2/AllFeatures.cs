using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Workbench2.Generators;

namespace Workbench2 {
    class AllFeatures {
        public AllFeatures(int featureCount, int max, Func<SingleFeature> generator) {
            this.GenerateFeature = generator;
            this.features = new List<SingleFeature>();
            for (int i = 0; i < featureCount; i++) {
                //this.features.Add(new SingleFeature(
                    //new PixelValGenerator(i, j)));
            //new PixelQuotGenerator(rand.Next(max), rand.Next(max), rand.Next(max), rand.Next(max))));

                this.features.Add(GenerateFeature());
            }
        }
        private static Random rand = new Random();

        public Func<SingleFeature> GenerateFeature { get; set; }

        public void AddFeature(int max) {
            //this.features.Add(new SingleFeature(
            //    new PixelQuotGenerator(rand.Next(max), rand.Next(max), rand.Next(max), rand.Next(max))));
            this.features.Add(GenerateFeature());

        }

        List<SingleFeature> features;
        internal Dictionary<string, double> TestTrain(int[][] p, string label, Action<Dictionary<string, double>, Dictionary<string, double>> accumulator,
            out string guess) {
            Dictionary<string, double> totalResult = new Dictionary<string, double>();
            int featureCount = 0;
            foreach (var f in features) {
                featureCount++;
                Dictionary<string, double> result = f.TestTrain(p, label);
                if (result == null) {
                    continue;
                }
                //Action<Dictionary<string, double>, Dictionary<string, double>> accumulator;
                //accumulator(result, totalResult);
                //this.Add(result, totalResult);
                accumulator(result, totalResult);

                if (totalResult.Count(i => i.Value == 0) == 9) {
                    Console.WriteLine(string.Format("{0} / {1} features seen. Number at Zero: 9",
                        featureCount, features.Count));
                    break;
                }
                if (totalResult.Any(i => double.IsInfinity(i.Value))) {
                    Console.WriteLine(string.Format("{0} / {1} features seen. Number at infinity: {2}", 
                        featureCount, features.Count, totalResult.Count(i => double.IsInfinity(i.Value))));
                    break;
                }
                //this.Mult(result, totalResult);
            }
            guess = "";
            double bestEval = 0;
            foreach (var r in totalResult) {
                if (r.Value > bestEval) {
                    bestEval = r.Value;
                    guess = r.Key;
                }
            }

            return totalResult;
        }

        private void Mult(Dictionary<string, double> from, Dictionary<string, double> to) {
            foreach (var l in from.Keys) {
                if (!to.ContainsKey(l)) {
                    to[l] = 1;
                }
                to[l] *= (from[l] * 9 + .0001) / (1.01 - from[l]) ;
            }
        }

        private void Add(Dictionary<string, double> from, Dictionary<string, double> to) {
            foreach (var l in from.Keys) {
                if (!to.ContainsKey(l)) {
                    to[l] = from[l];
                } else {
                    to[l] += from[l];
                }
            }
        }

        public int Count {
            get {
                return this.features.Count();
            }
        }

        internal XElement ToXml() {
            XElement root = new XElement("AllFeatures");
            foreach (var f in features) {
                root.Add(f.ToXml());
            }
            return root;
        }
    }
}

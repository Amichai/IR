using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workbench2.Generators;

namespace Workbench2 {
    class AllFeatures {
        public AllFeatures(int featureCount, int max) {
            this.features = new List<SingleFeature>();
            for (int i = 0; i < featureCount; i++) {
                this.features.Add(new SingleFeature(
                    //new PixelValGenerator(i, j)));
            new PixelDiffGenerator(rand.Next(max), rand.Next(max), rand.Next(max), rand.Next(max))));
            }
        }
        private static Random rand = new Random();

        public void AddFeature(int max) {
            this.features.Add(new SingleFeature(
                new PixelDiffGenerator(rand.Next(max), rand.Next(max), rand.Next(max), rand.Next(max))));
        }

        List<SingleFeature> features;
        internal Dictionary<string, double> TestTrain(int[][] p, string label, out string guess) {
            Dictionary<string, double> totalResult = new Dictionary<string, double>();
            foreach (var f in features) {
                Dictionary<string, double> result = f.TestTrain(p, label);
                if (result == null) {
                    continue;
                }

                this.Add(result, totalResult);
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
            
        private void Add(Dictionary<string, double> from, Dictionary<string, double> to) {
            foreach (var l in from.Keys) {
                if (!to.ContainsKey(l)) {
                    to[l] = from[l];
                } else {
                    to[l] += from[l];
                }
            }
        }
    }
}

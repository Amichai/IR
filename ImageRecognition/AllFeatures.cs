using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRCommon;
using System.Diagnostics;

namespace ImageRecognition {
    public class AllFeatures {
        public AllFeatures() {
            this.features = new List<Feature>();
            Success = new FeatureSuccess();
        }

        public List<Feature> Get() {
            return this.features;
        }

        public void GeneratePixelFeatures(int width, int height) {
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    this.features.Add(
                    new Feature() {
                        Func = new PixelEval(i, j)
                        //Eval = k => new List<double>() { k[i][j] }
                    });
                }
            }
        }

        public FeatureSuccess Success;

        public void Generate(int num) {
            throw new NotImplementedException();
        }

        public int Count() {
            return features.Count();
        }

        private List<Feature> features;

        Dictionary<string, double> LastProbabilities { get; set; }

        internal Dictionary<string,double> Test(int[][] p) {
            Dictionary<string, double> probabilities = new Dictionary<string, double>();
            foreach (var f in features) {
                var results = f.Test(p);
                if (!f.Trained() || results == null) continue;
                foreach (var r in results) {
                    if (probabilities.ContainsKey(r.Key)) {
                        probabilities[r.Key] += r.Value;
                    } else {
                        probabilities[r.Key] = r.Value;
                    }
                }
            }
            this.LastProbabilities = probabilities.Normalize();
            return LastProbabilities;
        }

        internal void Train(string p) {
            foreach (var f in features) {
                f.Train(p);
            }
            Success.Trial(p, this.LastProbabilities);
        }

        private void delete(List<int> indicies) {
            for (int j = indicies.Count() - 1; j >= 0; j--) {
                features.RemoveAt(indicies[j]);
            }
        }

        public IEnumerable<Feature> TrainedFeatures() {
            return features.Where(i => i.Trained());
        }

        public double AverageInterestingness() {
            var f = TrainedFeatures();
            if (f.Count() == 0) return int.MinValue;
            var a = f.Select(i => i.Interestingness);
            return a.Average();
        }

        public double AverageAttractiveness() {
            var f = TrainedFeatures();
            if (f.Count() == 0) return int.MinValue;
            var a = f.Select(i => (double)i.Attractiveness);
            return a.Average();
        }

        public double AverageNumberOfPoints() {
            return features.Select(i => (double)i.NumberOfPoints).Average();
        }

        public double AverageNumberOfDataSeen() {
            return features.Select(i => (double)i.DataSeen).Average();
        }
        List<double> interestingnessVals { get; set; }
        List<double> attractivenessVals { get; set; }
        List<int> pixels { get; set; }
        List<int> dataSeen { get; set; }
        public int UsefulFeautres { get; set; }

        enum featureAction { recombine, delete };
        Dictionary<int, featureAction> actions;
        List<int> recombine;

        public void Scan(bool purge = false) {
            actions = new Dictionary<int, featureAction>();
            recombine = new List<int>();
            double lastMeanInterestingness = AverageInterestingness();
            double lastMeanAttractiveness = AverageAttractiveness();
            UsefulFeautres = 0;
            interestingnessVals = new List<double>();
            attractivenessVals = new List<double>();
            pixels = new List<int>();
            dataSeen = new List<int>();
            for (int i = 0; i < features.Count(); i++) {
                if (!features[i].Trained()) continue;
                var attract = features[i].Attractiveness;
                attractivenessVals.Add(attract.Value);
                UsefulFeautres++;
                var interestingness = features[i].Interestingness;
                interestingnessVals.Add(interestingness);
                pixels.Add(features[i].NumberOfPoints);
                dataSeen.Add(features[i].DataSeen);

                if (attract > MaxAttractiveness) {
                    MaxAttractiveness = attract.Value;
                }
                if (interestingness > MaxInterestingness) {
                    MaxInterestingness = interestingness;
                }

                ///Deletion:
                if (
                    features[i].Trained(10)
                    //&& interestingness < lastMeanInterestingness
                    && attract < lastMeanAttractiveness * .1
                    //&& interestingness < lastMeanInterestingness
                    //&& interestingness < lastMeanInterestingness * .85
                ) {
                    if (purge) {
                        this.features.RemoveAt(i);
                        i--;
                    }
                } else if (attract > lastMeanAttractiveness 
                    && interestingness > lastMeanInterestingness) {
                    recombine.Add(i);
                }
            }
        }

        //Must be called after a scan
        internal string Stats() {
            return string.Format("Total number of features: {0}, average interestingness: {1}, "
            + "features in use: {2},\n average attractivness {3}, average number of pixels per feature: {4}, "
            + "average trials per feature: {5}",
                this.features.Count(), AverageInterestingness(), this.UsefulFeautres,
                AverageAttractiveness(), AverageNumberOfPoints(), AverageNumberOfDataSeen());
        }

        internal void Purge() {
            Dictionary<Feature, double> attrc = new Dictionary<Feature, double>();
            List<int> toDelete = new List<int>();

            int numberOfPixels = 0;
            for (int i = 0; i < features.Count(); i++) {
                var f = features[i];
                var attract = f.Interestingness;
                if (attract == double.MinValue || double.IsNaN(attract)) continue;
                if (attract < .1 && f.SuccessRate.Overall.Count > 50) {
                    toDelete.Add(i);
                } else {
                    attrc[f] = attract;
                    numberOfPixels += f.Func.GetPoints().Count();
                }
            }
            delete(toDelete);
            Debug.Print("Total number of features: " + features.Count());
            Debug.Print("Pixels per feature: " + (double)numberOfPixels / attrc.Count());
            var sorted = attrc.OrderByDescending(i => i.Value);
            List<IntPoint> points = new List<IntPoint>();
            if (sorted.Count() < 5) return;
            var l1 = sorted.ElementAt(0).Key.Func.GetPoints();
            var l2 = sorted.ElementAt(1).Key.Func.GetPoints();
            points.AddRange(combineNoDuplicates(l1, l2, 28));
            this.features.Add(new Feature() {
                Func = new PixelSubset(points)
            });
        }

        private List<IntPoint> combineNoDuplicates(List<IntPoint> l1, List<IntPoint> l2, int XYMax) {
            List<IntPoint> unique = new List<IntPoint>();
            HashSet<int> seenVals = new HashSet<int>();
            foreach (var p in l1) {
                int intRep = p.X * 28 + p.Y;
                if (!seenVals.Contains(intRep)) {
                    unique.Add(p);
                    seenVals.Add(intRep);
                } else {

                }
            }
            foreach (var p in l2) {
                int intRep = p.X * 28 + p.Y;
                if (!seenVals.Contains(intRep)) {
                    unique.Add(p);
                    seenVals.Add(intRep);
                } else {

                }
            }
            return unique;
        }

        private void testInterestingnessAndAttractiveness() {
            Dictionary<double, double> successInterestingness = new Dictionary<double, double>();
            foreach (var f in features) {
                var successRate = f.SuccessRate.Overall.LastN();
                var attractiveness = f.Attractiveness;
                if (double.IsNaN(successRate)) continue;
                //Debug.Print("Feature success rate:" + successRate.ToString());
                //Debug.Print("Attractiveness: " + attractiveness.ToString());
                //Debug.Print("\n");
                successInterestingness[attractiveness.Value] = successRate;
            }
            var sorted1 = successInterestingness.OrderByDescending(i => i.Key);
            var sorted2 = successInterestingness.OrderByDescending(i => i.Value);
        }

        internal void Purge(List<int> ftrs) {
            for(int i = ftrs.Count() - 1; i >=0 ; i--){
                this.features.RemoveAt(ftrs[i]);
            }
        }

        private static Random rand = new Random();



        internal void Recombine() {
            int count = recombine.Count();
            if (count == 0) return;
            int idx1 = recombine[rand.Next(count - 1)];
            int idx2 = recombine[rand.Next(count - 1)];
            var f1 = features[idx1];
            var f2 = features[idx2];
            var l1 = f1.Func.GetPoints();
            var l2 = f2.Func.GetPoints();
            List<IntPoint> points = new List<IntPoint>();
            points.AddRange(combineNoDuplicates(l1, l2, 28));
            this.features.Add(new Feature() {
                Func = new PixelSubset(points)
            });

            //delete(toDelete);
            //generate ten new features where each feature is composed of two sub features with
            //better than average attractiveness (past success) and are trained
        }

        public double MaxInterestingness { get; set; }

        public double MaxAttractiveness { get; set; }
    }
}

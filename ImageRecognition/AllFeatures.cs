using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRCommon;
using System.Diagnostics;
using MyLogger;

namespace ImageRecognition {
    public class AllFeatures {
        public AllFeatures() {
            this.features = new List<Feature>();
            this.Success = new FeatureSuccess();
            featureType = Logger.Inst.GetString("FeatureType");
            featureType2 = Logger.Inst.GetString("FeatureType2");
        }

        private string featureType = null;
        private string featureType2 = null;

        private void addFeature(string fType, int idx1 ,int idx2) {
            if (fType == null) {
                return;
            }
            var f1 = features[idx1];
            var f2 = features[idx2];
            var l1 = f1.Projection.GetPoint();
            var l2 = f2.Projection.GetPoint();
            Feature newFeature;
            switch (fType) {
                case "PixelProjection":
                    newFeature = new Feature() {
                        Projection = new PixelProjection(new List<IntPoint>() { l2.Value }) { Ref = f1.Projection, Ref2 = f2.Projection }
                    };
                    break;
                case "PixelDiff":
                    newFeature = new Feature() {
                        Projection = new PixelDiff(l2.Value, f1.Projection)
                    };
                    break;
                case "SymmetricalPixelDiff":
                    newFeature = new Feature() {
                        Projection = new SymmetricalPixelDiff() { Ref = f1.Projection, Ref2 = f2.Projection }
                    };
                    break;
                case "PixelSum":
                    newFeature = new Feature() {
                        Projection = new PixelSum(l2.Value) { Ref = f1.Projection }
                    };
                    break;
                case "PixelQuot":
                    newFeature = new Feature() {
                        Projection = new PixelQuot(l2.Value, f1.Projection)
                    };
                    break;
                case "PixelProd":
                    newFeature = new Feature() {
                        Projection = new PixelProd(l2.Value) { Ref = f1.Projection }
                    };
                    break;
                default:
                    throw new Exception("unknown feature type");
            }

            features.Add(newFeature);
        }

        internal void Recombine() {
            int count = recombine.Count();
            if (count == 0) return;
            int idx1 = recombine[rand.Next(count - 1)];
            int idx2 = recombine[rand.Next(count - 1)];
            addFeature(featureType, idx1, idx2);
            idx1 = recombine[rand.Next(count - 1)];
            idx2 = recombine[rand.Next(count - 1)];
            addFeature(featureType2, idx1, idx2);
            

            //var newFeature = new Feature() {
            //    Func = new PixelDiff(l1) { Ref = f2.Func }
            //};
            //this.features.Add(newFeature);
            //this.features.Add(new Feature() {
            //    Func = new PixelSum(l2) { Ref = f1.Func }
            //});
        }

        public List<Feature> Get() {
            return this.features;
        }

        public void GeneratePixelFeatures(int width, int height) {
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    this.features.Add(
                    new Feature() {
                        Projection = new PixelEval(i, j)
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

        public Dictionary<string, double> LastProbabilities { get; set; }

        private double probabilisticWeight(double prob, int count) {
            int labelCount = LastProbabilities.Count();
            double eps = 1.0 / (count + 1);
            double val = ((prob + eps) * (labelCount + 1)) / ((1.0 - prob) + eps);
            //double val2 = Math.Log(val);
            if (double.IsNaN(val) || double.IsInfinity(val)) throw new Exception();
            //if (double.IsNaN(val2) || double.IsInfinity(val2)) throw new Exception();
            return val;
        }

        internal Dictionary<string, double> Test_scaleProbabilitiesToInfinity(int[][] p) {
            Dictionary<string, double> probabilities = new Dictionary<string, double>();
            foreach (var f in features) {
                var results = f.Test(p);
                if (!f.Trained() || results == null) continue;
                foreach (var r in results) {
                    if (!probabilities.ContainsKey(r.Key)) {
                        probabilities[r.Key] = 0;
                    }
                    double val1 = probabilisticWeight(f.SuccessRate.LabelSuccess[r.Key].LastN(), 10);
                    double newVal = probabilities[r.Key] += r.Value * val1;
                    if (double.IsNaN(newVal) || double.IsInfinity(newVal)) {
                        throw new Exception();
                    }
                    probabilities[r.Key] = newVal;
                }
            }
            this.LastProbabilities = probabilities.Normalize();
            return LastProbabilities;
        }

        internal Dictionary<string,double> Test(int[][] p) {
            Dictionary<string, double> probabilities = new Dictionary<string, double>();
            foreach (var f in features) {
                var results = f.Test(p);
                if (!f.Trained() || results == null) continue;
                foreach (var r in results) {
                    if (!probabilities.ContainsKey(r.Key)) {
                        probabilities[r.Key] = 0;
                    }
                    probabilities[r.Key] += r.Value;
                }
            }
            this.LastProbabilities = probabilities.Normalize();
            return LastProbabilities;
        }

        internal Dictionary<string, double> Test3(int[][] p) {
            Dictionary<string, int> probabilities = new Dictionary<string, int>();
            foreach (var f in features) {
                var results = f.Test(p);
                if (!f.Trained() || results == null) continue;
                string guess = results.MaxLabel();
                    if (!probabilities.ContainsKey(guess)) {
                        probabilities[guess] = 0;
                    }
                    probabilities[guess]++;
            }
            this.LastProbabilities = probabilities.Normalize();
            return LastProbabilities;
        }

        public void Train(string p, int[][] input) {
            foreach (var f in features) {
                f.Train(p, input);
            }
            Success.Trial(p, null, null);
        }

        internal void Train(string p, Dictionary<string, double> lastProb = null) {
            foreach (var f in features) {
                f.Train(p);
            }
            
            Success.Trial(p, lastProb, lastProb.BestGuess());
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
            if (Logger.Inst.GetBool("EvaluateFeatureCount")) {
                return features.Select(i => (double)i.NumberOfPoints).Average();
            } else {
                return 1;
            }
        }

        public double AverageNumberOfDataSeen() {
            return features.Select(i => (double)i.DataSeen).Average();
        }
        List<double> interestingnessVals { get; set; }
        List<double> attractivenessVals { get; set; }
        List<int> dataSeen { get; set; }
        public int UsefulFeautres { get; set; }

        enum featureAction { recombine, delete };
        Dictionary<int, featureAction> actions;
        List<int> recombine;
        double purgeThreshold = 1.0;
        double thresholdIncrementVal = .0000;

        public void Scan(bool purge = false) {
            purgeThreshold += thresholdIncrementVal;
            actions = new Dictionary<int, featureAction>();
            recombine = new List<int>();
            double lastMeanInterestingness = AverageInterestingness();
            double lastMeanAttractiveness = AverageAttractiveness();
            UsefulFeautres = 0;
            interestingnessVals = new List<double>();
            attractivenessVals = new List<double>();
            dataSeen = new List<int>();
            for (int i = 0; i < features.Count(); i++) {
                if (!features[i].Trained()) continue;
                var attract = features[i].Attractiveness;
                attractivenessVals.Add(attract.Value);
                UsefulFeautres++;
                var interestingness = features[i].Interestingness;
                interestingnessVals.Add(interestingness);
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
                    //&& interestingness < purgeThreshold
                    //&& attract < .01
                    && attract < lastMeanAttractiveness * purgeThreshold
                    && interestingness < lastMeanInterestingness * purgeThreshold
                    //&& attract < lastMeanAttractiveness * .1
                    //&& interestingness < lastMeanInterestingness
                    //&& interestingness < lastMeanInterestingness * .85
                ) {
                    if (purge) {
                        this.features.RemoveAt(i);
                        i--;
                    }
                } else if (
                    attract > lastMeanAttractiveness  
                    && interestingness > lastMeanInterestingness
                    //attract > MaxAttractiveness * .9
                     //interestingness > .09
                    ) {
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
                if (attract < .1 && f.SuccessRate.Overall.Count() > 50) {
                    toDelete.Add(i);
                } else {
                    attrc[f] = attract;
                    numberOfPixels += f.Projection.GetPoints().Count();
                }
            }
            delete(toDelete);
            Debug.Print("Total number of features: " + features.Count());
            Debug.Print("Pixels per feature: " + (double)numberOfPixels / attrc.Count());
            var sorted = attrc.OrderByDescending(i => i.Value);
            List<IntPoint> points = new List<IntPoint>();
            if (sorted.Count() < 5) return;
            var l1 = sorted.ElementAt(0).Key.Projection.GetPoints();
            var l2 = sorted.ElementAt(1).Key.Projection.GetPoints();
            points.AddRange(combineNoDuplicates(l1, l2, 28));
            this.features.Add(new Feature() {
                //Func = new PixelSubset(points)
                //Projection = new PixelDiff(points)
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

        /// <summary>
        /// Doesn't require the scan function to work   
        /// </summary>
        internal void Recombine2() {
            int count = features.Count();
            if (count == 0) return;
            int idx1 = rand.Next(count - 1);
            int idx2 = rand.Next(count - 1);
            var f1 = features[idx1];
            var f2 = features[idx2];
            var l1 = f1.Projection.GetPoint();
            var l2 = f2.Projection.GetPoint();
            //var newFeature = new Feature() {
            //    Func = new PixelDiff(l1) { Ref = f2.Func }
            //};
            //this.features.Add(newFeature);
            //this.features.Add(new Feature() {
            //    Func = new PixelSum(l2) { Ref = f1.Func }
            //});
            this.features.Add(new Feature() {
                //Func = new PixelProjection(l2) { Ref = f1.Func, Ref2 = f2.Func }
                //Projection = new PixelDiff(l2) { Ref = f1.Projection }
            });
        }

        public double MaxInterestingness { get; set; }

        public double MaxAttractiveness { get; set; }
    }
}

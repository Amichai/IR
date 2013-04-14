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
            this.features = new Dictionary<Feature.FType, List<Feature>>();
            this.Success = new FeatureSuccess();
            featureType = Logger.Inst.GetString("FeatureType");
            featureType2 = Logger.Inst.GetString("FeatureType2");
            this.attractivenessPurgeThreshold = Logger.Inst.GetDouble("attractivenessPurgeThreshold");
            this.interestingnessPurgeThreshold = Logger.Inst.GetDouble("interestingnessPurgeThreshold");
            this.usePixelFeatures = Logger.Inst.GetBool("usePixelFeatures");
        }

        private string featureType = null;
        private string featureType2 = null;
        private double? attractivenessPurgeThreshold;
        private double? interestingnessPurgeThreshold;

        private void addFeature(string fType, Feature.FType type1, int idx1, Feature.FType type2, int idx2) {
            if (fType == null) {
                return;
            }
            var f1 = features[type1][idx1];
            var f2 = features[type2][idx2];
            var l1 = f1.Projection.GetPoint();
            var l2 = f2.Projection.GetPoint();
            Feature newFeature;
            var newType = (Feature.FType)Enum.Parse(typeof(Feature.FType), fType);
            switch (newType) {
                case Feature.FType.PixelProjection:
                    newFeature = new Feature() {
                        Projection = new PixelProjection(new List<IntPoint>() { l2.Value }) {
                            Ref = f1.Projection,
                            Ref2 = f2.Projection,
                        },
                        FeatureType = Feature.FType.PixelProjection
                    };
                    break;
                case Feature.FType.PixelDiff:
                    if (f1.FeatureType != Feature.FType.PixelEval) return;
                    newFeature = new Feature() {
                        Projection = new PixelDiff(l2.Value, f1.Projection),
                        FeatureType = Feature.FType.PixelDiff
                    };
                    break;
                case Feature.FType.SymmetricalPixelDiff:
                    newFeature = new Feature() {
                        Projection = new SymmetricalPixelDiff() { Ref = f1.Projection, Ref2 = f2.Projection },
                        FeatureType = Feature.FType.SymmetricalPixelDiff
                    };
                    break;
                case Feature.FType.PixelSum:
                    newFeature = new Feature() {
                        Projection = new PixelSum(l2.Value) { Ref = f1.Projection },
                        FeatureType = Feature.FType.PixelSum
                    };
                    break;
                case Feature.FType.PixelQuot:
                    if (f1.FeatureType != Feature.FType.PixelEval) return;
                    newFeature = new Feature() {
                        Projection = new PixelQuot(l2.Value, f1.Projection),
                        FeatureType = Feature.FType.PixelQuot
                    };
                    break;
                case Feature.FType.SymmetricalPixelQuot:
                    newFeature = new Feature() {
                        Projection = new SymmetricalPixelQuot() { Ref = f1.Projection, Ref2 = f2.Projection },
                        FeatureType = Feature.FType.SymmetricalPixelQuot
                    };
                    break;
                case Feature.FType.PixelProd:
                    newFeature = new Feature() {
                        Projection = new PixelProd(l2.Value) { Ref = f1.Projection },
                        FeatureType = Feature.FType.PixelProd
                    };
                    break;
                case Feature.FType.SymmetricalPixelSum:
                    newFeature = new Feature() {
                        Projection = new SymmetricalPixelSum() { Ref = f1.Projection, Ref2 = f2.Projection },
                        FeatureType = Feature.FType.SymmetricalPixelSum
                    };
                    break;
                default:
                    throw new Exception("unknown feature type");
            }
            if (!this.features.ContainsKey(newType)) {
                this.features[newType] = new List<Feature>();
            }
            features[newType].Add(newFeature);
        }

        private Feature.FType getRandomType() {
            int totalNumberOfRecombineFeatures = 0;
            foreach (var type in recombine.Keys) {
                totalNumberOfRecombineFeatures += recombine[type].Count();
            }
            int index = rand.Next(totalNumberOfRecombineFeatures - 1);
            totalNumberOfRecombineFeatures = 0;
            foreach (var type in recombine.Keys) {
                totalNumberOfRecombineFeatures += recombine[type].Count();
                if (index < totalNumberOfRecombineFeatures) {
                    return type;
                }
            }
            throw new Exception();
        }

        internal void Recombine(Feature.FType type1 = Feature.FType.unknown, Feature.FType type2 = Feature.FType.unknown) {
            int featuresTypeCount = recombine.Count();
            if (featuresTypeCount == 0) return;
            int type1Count = 0, type2Count = 0;
            if (type1 == Feature.FType.unknown) {
                type1 = getRandomType();
                type1Count = recombine[type1].Count();
            }

            if (type2 == Feature.FType.unknown) {
                type2 = getRandomType();
                type2Count = recombine[type2].Count();
            }
            
            int idx1 = recombine[type1][rand.Next(type1Count - 1)];
            int idx2 = recombine[type2][rand.Next(type2Count - 1)];
            addFeature(featureType, type1, idx1, type2, idx2);
            idx1 = recombine[type1][rand.Next(type1Count - 1)];
            idx2 = recombine[type2][rand.Next(type2Count - 1)];
            addFeature(featureType2, type1, idx1, type2, idx2);
        }

        public void GeneratePixelFeatures(int width, int height) {
            if (!this.features.Keys.Contains(Feature.FType.PixelEval)) {
                this.features[Feature.FType.PixelEval] = new List<Feature>();
            }
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    this.features[Feature.FType.PixelEval].Add(
                    new Feature() {
                        Projection = new PixelEval(i, j),
                        FeatureType = Feature.FType.PixelEval
                    });
                }
            }
        }

        public FeatureSuccess Success;

        public int Count() {
            return features.Count();
        }

        //private List<Feature> features;
        private Dictionary<Feature.FType, List<Feature>> features;

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
            foreach (var fType in features.Keys) {
                foreach (var f in features[fType]) {
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
            }
            this.LastProbabilities = probabilities.Normalize(totalVal: null);
            return LastProbabilities;
        }

        private bool? usePixelFeatures = null;

        internal Dictionary<string, double> Test(int[][] p) {
            Dictionary<string, double> probabilities = new Dictionary<string, double>();
            foreach (var fType in features.Keys) {
                foreach (var f in features[fType]) {
                    var results = f.Test(p);
                    if (!f.Trained() || results == null) continue;
                    if (f.CreationIndex <= 785 && !usePixelFeatures.Value) continue;
                    foreach (var r in results) {
                        if (!probabilities.ContainsKey(r.Key)) {
                            probabilities[r.Key] = 0;
                        }
                        probabilities[r.Key] += r.Value;
                    }
                }
            }
            this.LastProbabilities = probabilities.Normalize(totalVal: null);
            return LastProbabilities;
        }

        internal Dictionary<string, double> Test3(int[][] p) {
            Dictionary<string, int> probabilities = new Dictionary<string, int>();
            foreach (var fType in features.Keys) {
                foreach (var f in features[fType]) {
                    var results = f.Test(p);
                    if (!f.Trained() || results == null) continue;
                    string guess = results.MaxLabel();
                    if (!probabilities.ContainsKey(guess)) {
                        probabilities[guess] = 0;
                    }
                    probabilities[guess]++;
                }
            }
            this.LastProbabilities = probabilities.Normalize();
            return LastProbabilities;
        }

        public IEnumerable<Feature> GetAllFeatures() {
            foreach (var fType in features.Keys) {
                foreach (var f in features[fType]) {
                    yield return f;
                }
            }
        }

        internal void Train(string p, Dictionary<string, double> lastProb = null, int[][] input = null) {
            foreach (var f in GetAllFeatures()) {
                f.Train(p, input);
            }
            Success.Trial(p, lastProb, lastProb.BestGuess());
        }

        private void delete(Feature.FType type, List<int> indicies) {
            for (int j = indicies.Count() - 1; j >= 0; j--) {
                features[type].RemoveAt(indicies[j]);
            }
        }

        public IEnumerable<Feature> TrainedFeatures() {
            return GetAllFeatures().Where(i => i.Trained());
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
                return GetAllFeatures().Select(i => (double)i.NumberOfPoints).Average();
            } else {
                return 1;
            }
        }

        public double AverageNumberOfDataSeen() {
            return GetAllFeatures().Select(i => (double)i.DataSeen).Average();
        }
        List<double> interestingnessVals { get; set; }
        List<double> attractivenessVals { get; set; }
        List<int> dataSeen { get; set; }
        public int UsefulFeautres { get; set; }

        enum featureAction { recombine, delete };
        Dictionary<int, featureAction> actions;
        Dictionary<Feature.FType,List<int>> recombine;
        double purgeThreshold = 1.0;
        double thresholdIncrementVal = .0000;

        public void Scan(bool purge = false) {
            purgeThreshold += thresholdIncrementVal;
            actions = new Dictionary<int, featureAction>();
            recombine = new Dictionary<Feature.FType, List<int>>();
            double lastMeanInterestingness = AverageInterestingness();
            double lastMeanAttractiveness = AverageAttractiveness();
            UsefulFeautres = 0;
            interestingnessVals = new List<double>();
            attractivenessVals = new List<double>();
            dataSeen = new List<int>();
            foreach(var fType in features.Keys){
                for (int i = 0; i < features[fType].Count(); i++) {
                    if (!features[fType][i].Trained()) continue;
                    var attract = features[fType][i].Attractiveness;
                    attractivenessVals.Add(attract.Value);
                    UsefulFeautres++;
                    var interestingness = features[fType][i].Interestingness;
                    interestingnessVals.Add(interestingness);
                    dataSeen.Add(features[fType][i].DataSeen);

                    if (attract > MaxAttractiveness) {
                        MaxAttractiveness = attract.Value;
                    }
                    if (interestingness > MaxInterestingness) {
                        MaxInterestingness = interestingness;
                    }
                    ///Deletion:
                    if (
                        features[fType][i].Trained(10)
                        //&& interestingness < purgeThreshold
                        //&& attract < .01
                        && attract < lastMeanAttractiveness * attractivenessPurgeThreshold
                        && interestingness < lastMeanInterestingness * interestingnessPurgeThreshold
                        //&& attract < lastMeanAttractiveness * .1
                        //&& interestingness < lastMeanInterestingness
                        //&& interestingness < lastMeanInterestingness * .85
                    ) {
                        if (purge) {
                            this.features[fType].RemoveAt(i);
                            i--;
                        }
                    } else if (
                        attract > lastMeanAttractiveness
                        && interestingness > lastMeanInterestingness
                        //attract > MaxAttractiveness * .9
                        //interestingness > .09
                        ) {
                            if (!this.recombine.ContainsKey(fType)) {
                                recombine[fType] = new List<int>();
                            }
                        recombine[fType].Add(i);
                    }
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

        private static Random rand = new Random();

        public double MaxInterestingness { get; set; }

        public double MaxAttractiveness { get; set; }
    }
}

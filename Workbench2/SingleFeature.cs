﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRCommon;
using System.Diagnostics;
using System.Xml.Linq;

namespace Workbench2 {
    public class SingleFeature {
        public SingleFeature(IValGenerator generator) {
            //this.pastVals = new List<Tuple<double, string>>();
            this.labelCount = new Dictionary<string, int>();
            this.totalHits = 0;
            this.hitsPerLabel = new Dictionary<string, int>();
            this.valGenerator = generator;
            this.binMax = null;
            this.binMin = null;
            this.firstVal = null;
            this.success = new FeatureSuccess();
        }

        double? firstVal;
        IValGenerator valGenerator;
        double? binMin;
        double? binMax;
        public double Interestingness(string label) {
            return ProbabilityPerLabel()[label] / APrioriProbability();
        }

        private int totalHits;
        private Dictionary<string, int> hitsPerLabel;

        public double APrioriProbability() {
            return totalHits / (double)totalCount;
        }

        public Dictionary<string, double> ProbabilityPerLabel() {
            Dictionary<string, double> hitCount = new Dictionary<string, double>();
            foreach (var h in hitsPerLabel) {
                //double pb_a = h.Value / (double)labelCount[h.Key];
                //double pa = (double)labelCount[h.Key] * totalCount;
                //double pb = totalHits / (double)totalCount;
                //hitCount[h.Key] = pb_a * pa / pb;
                hitCount[h.Key] = h.Value / (double)labelCount[h.Key];
            }
            return hitCount;
        }

        private double interstingness2() {
            ///TODO: I'm not sure if we should be using variance or straight average for this measure
            ///try both...
            List<double> vals = new List<double>();
            var probPerLabel = ProbabilityPerLabel();
            var aproiori = APrioriProbability();
            foreach (var l in probPerLabel.Keys) {
                var val = probPerLabel[l] / aproiori;
                vals.Add(val);
            }
            return vals.Variance();
        }

        private double interstingness1() {
            ///TODO: I'm not sure if we should be using variance or straight average for this measure
            ///try both...
            double sum = 0;
            var probPerLabel = ProbabilityPerLabel();
            var aproiori = APrioriProbability();
            foreach (var l in probPerLabel.Keys) {
                var val = probPerLabel[l] / aproiori;
                sum += val;
            }
            return sum / probPerLabel.Count;
        }

        public double Interestingness() {
            var i1 = interstingness1();
            //var i2 = interstingness2();
            //Debug.Print(string.Format("I1 ave: {0}, I2 var: {1}", i1, i2));
            //Debug.Print("Interestingness; {0}", i1);
            return i1;
        }

        private int totalCount = 0;
        private Dictionary<string, int> labelCount;

        internal Dictionary<string, double> TestTrain(int[][] p, string label) {
            var val = this.valGenerator.Eval(p);
            if (this.firstVal == null) {
                firstVal = val;
                return null;
            } else if (this.binMax == null) {
                if (this.firstVal != val) {
                    this.binMin = Math.Min(this.firstVal.Value, val) - .1;
                    this.binMax = Math.Max(this.firstVal.Value, val) + .1;
                    //Debug.Print(string.Format("Min: {0}, max: {1}", this.binMin, this.binMax));
                } else {
                    return null;
                }
            }
            
            var eval = val < this.binMax && val > this.binMin;
            ///Test
            ///
            Dictionary<string, double> result = new Dictionary<string, double>();

            ///Probability eval == true given a particular label
            var probPerLabel = this.ProbabilityPerLabel();
            ///Probability that eval == true regardless of label
            var apriori = this.APrioriProbability();
            double bestProb = double.MinValue;
            string bestGuess = "";
            double totalResultSum = 0;
            foreach (var l in probPerLabel.Keys) {
                //var aprioriLabelProb = this.labelCount[l] / (double)this.totalCount;
                var aprioriLabelProb = .1;
                var probVal = (probPerLabel[l] * aprioriLabelProb) / apriori;
                double thisResult;
                if (eval) {
                    thisResult = probVal;
                } else {
                    thisResult = 1 - probVal;
                }
                result[l] = thisResult;
                if (thisResult > bestProb) {
                    bestProb = thisResult;
                    bestGuess = l;
                }
                totalResultSum += Math.Pow(thisResult, .5);
                //totalResultSum += thisResult;
                //Debug.Print(string.Format("resolved probability. Label: {0}, val: {1}", l, probVal));
            }
            

            if (!this.hitsPerLabel.ContainsKey(label)) {
                this.hitsPerLabel[label] = 0;
            }
            if (eval) {
                this.hitsPerLabel[label]++;
            }
            
            if (eval) {
                totalHits++;
            }

            if (!this.labelCount.ContainsKey(label)) {
                this.labelCount[label] = 1;
            } else {
                this.labelCount[label]++;
            }
            this.totalCount++;
            if (apriori == 1 || result.Count == 0) {
                return null;
            }

            //normalize(result, totalResultSum);
            //this.success.Trial(label, result, bestGuess);

            //var n = result.Count - 1;
            //foreach (var r in result.ToList()) {
            //    result[r.Key] =  Math.Log((r.Value * n) / (1 - r.Value));
            //}

            return result;
        }

        private static void normalize(Dictionary<string, double> result, double totalResultSum) {
            if (totalResultSum != 1) {
                foreach (var r in result.ToList()) {
                    result[r.Key] = r.Value / totalResultSum;
                }
            }
        }

        private FeatureSuccess success;

        internal XElement ToXml() {
            XElement root = new XElement("Feature");
            root.Add(new XElement("Success", this.success.ToXml()));
            root.Add(new XAttribute("Interestingness", this.Interestingness()));
            if (this.binMin != null) {
                root.Add(new XAttribute("BinMin", this.binMin.Value));
            }
            if (this.binMax != null) {
                root.Add(new XAttribute("BinMax", this.binMax.Value));
            }
            root.Add(new XElement("Generator", this.valGenerator.ToXml()));
            return root;
        }
    }
}

using PRCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageRecognition {
    interface IFeature {
        double Interestingness();
        double Support();
        FeatureVector Eval();
        Dictionary<string, double> Probabilities(int[][] p);
        int VectorSize { get; set; }
        TrainingData TrainingData { get; set; }
    }

    public class TrainingData {
        void Add(FeatureVector f, string l) {
            if (PastLabelValues.ContainsKey(l)) {
                PastLabelValues[l].Add(f);
            } else {
                PastLabelValues[l] = new PastVectorVals(f);
            }
        }
        PastValues AllData { get; set; }
        Dictionary<string, PastVectorVals> PastLabelValues { get; set; }
    }

    public class FeatureVector : List<double> {
        public double Utility() {
            throw new NotImplementedException();
        }
        public double DistanceVal(FeatureVector fv) {
            throw new NotImplementedException();
        }
    }

    public class PastVectorVals {
        List<PastValues> eachElement;

        public PastVectorVals(FeatureVector f) {
            if (f.Count() != eachElement.Count) {
                throw new Exception();
            }
            for(int i =0;i < f.Count(); i++){
                eachElement[i].Add(f[i]);
            }
        }

        internal void Add(FeatureVector f) {
            throw new NotImplementedException();
        }
    }
}

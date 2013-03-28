using MyLogger;
using PRCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageRecognition {
    public class Workbench {
        public Workbench() {
            features = new AllFeatures();
            features.GeneratePixelFeatures(28, 28);
            this.testMethodology = Logger.Inst.GetString("TestMethod");
        }

        string testMethodology = null;

        public event EventHandler<TrainingDataEventArgs> InputLoaded;
        public event EventHandler<FeaturesTrainedEventArgs> FeaturesTrained;

        private void OnInputLoaded(int[][]img) {
            var eh = InputLoaded;
            if (eh != null) {
                eh(this, new TrainingDataEventArgs() { Data = img });
            }
        }

        private void OnFeaturesTrained() {
            var eh = FeaturesTrained;
            if (eh != null) {
                eh(this,
                    new FeaturesTrainedEventArgs() {
                        AverageAttractiveness = features.AverageAttractiveness(),
                        AverageInterestingness = features.AverageInterestingness(),
                        AverageNumberOfDataSeen = features.AverageNumberOfDataSeen(),
                        AverageNumberOfPoints = features.AverageNumberOfPoints(),
                        FeatureCount = features.Count(),
                        LastNSuccessRate = features.Success.Overall.LastN(),
                        SuccessRatePerLabel = features.Success.LabelSuccess,
                        TotalNumberOfTrials = features.Success.TrialCounter,
                        TrainedFeatures = features.UsefulFeautres,
                        MaxAttractiveness = features.MaxAttractiveness,
                        MaxInterestingness = features.MaxInterestingness,
                        Features = features.Get()
                    });
            }
        }

        AllFeatures features;

        public void Purge(List<int> features) {
            this.features.Purge(features);
        }
        
        //Define a bunch of events so that the parent class can observe everything that happens here
        public void Process(IEnumerable<Tuple<int[][], string>> dataStream) {
            int i = 0;
            foreach (var a in dataStream) {

                Dictionary<string, double> result;
                if (testMethodology == "Test_scaleProbabilitiesToInfinity") {
                    result = features.Test_scaleProbabilitiesToInfinity(a.Item1);
                } else {
                    result = features.Test(a.Item1);

                }
                features.Train(a.Item2, result);    
                features.Scan(purge:false);
                
                for (int j = 0; j < Logger.Inst.GetIntVal("FeaturesToRecombine"); j++) {
                    features.Recombine();
                }
                if (i++ % 10 == 0) {
                    ///Feautres get deleted on the other end of this event!
                    OnFeaturesTrained();
                }

            }
        }
    }

    public class TrainingDataEventArgs : EventArgs {
        public int[][] Data { get; set; }
    }

    public class FeaturesTrainedEventArgs : EventArgs {
        public int TotalNumberOfTrials { get; set; }
        public double LastNSuccessRate { get; set; }
        public Dictionary<string, PastTrials> SuccessRatePerLabel { get; set; }
        public int FeatureCount { get; set; }
        public double AverageInterestingness { get; set; }
        public int TrainedFeatures { get; set; }
        public double AverageAttractiveness { get; set; }
        public double AverageNumberOfPoints { get; set; }
        public double AverageNumberOfDataSeen { get; set; }
        public List<Feature> Features { get; set; }
        public double MaxInterestingness { get; set; }
        public double MaxAttractiveness { get; set; }
        }
}

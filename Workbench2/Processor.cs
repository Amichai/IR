using PRCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workbench2 {
    public class Processor {
        public Processor(string processName = "", int featureCount = 5000, string filepath = @"C:\Users\Amichai\Data\digits.csv") {
            this.loader = new InputLoader();
            this.loader.LoadFile(filepath);
            this.allfeatures = new AllFeatures(featureCount, 28);
            this.success = new FeatureSuccess();
            processID = processCounter++;
            this.Name = processName;
        }

        public string Name { get; private set; }
        public void Run() {
            this.Process(this.loader.AllElements());
        }

        InputLoader loader;
        private int processID;
        private static int processCounter = 0;

        private AllFeatures allfeatures;
        private FeatureSuccess success;

        private double bestSuccess = 0;

        public Action<Dictionary<string, double>, Dictionary<string, double>> accumulator;

        public void Process(IEnumerable<Tuple<int[][], string>> dataStream) {
            int counter = 0;
            foreach (var a in dataStream) {
                string guess;
                Dictionary<string, double> result = this.allfeatures.TestTrain(a.Item1, a.Item2, accumulator, out guess);
                if (guess != "") {
                    this.success.Trial(a.Item2, result, guess);
                    var overall = this.success.Overall.LastN();
                    if (overall > bestSuccess && counter > 50) {
                        bestSuccess = overall;
                    }
                    if (counter++ % 10 == 0) {
                        string line1 = string.Format("{0}: success: {1}, best: {2}, feature count: {3}",
                            counter, overall, Math.Round(bestSuccess, 2), this.allfeatures.Count);
                        string line2 = string.Format("Running geom: {0}, running exp: {1}, process name: {2}", this.success.Overall.RunningGeometric, this.success.Overall.RunningExponential, this.Name);
                        //Console.WriteLine();
                        //Console.WriteLine();
                        Debug.Print("Process ID: "+ processID.ToString() + " " + line1 + " " + line2);
                    }
                }
                this.allfeatures.AddFeature(28);
                ///Generate new and purge old features
            }
        }
    }
}

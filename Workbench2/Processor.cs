using PRCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workbench2 {
    public class Processor {
        public Processor(Func<SingleFeature> FeatureGenerator,
            string processName = "", int featureCount = 5000, string filepath = @"C:\Users\Amichai\Data\digits.csv") {
            this.loader = new InputLoader();
            this.loader.LoadFile(filepath);
            this.allfeatures = new AllFeatures(featureCount, 28, FeatureGenerator);
            this.success = new FeatureSuccess();
            processID = processCounter++;
            this.Name = processName;
            this.LogFileLocation = @"C:\IR\" + DateTime.Now.ToLongTimeString().Replace(":", ".").Replace(" ", "") + "_" + this.processID.ToString() + ".txt";
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

        public string LogFileLocation { get; set; }

        public void Process(IEnumerable<Tuple<int[][], string>> dataStream) {
            int counter = 0;
            using (StreamWriter outfile = new StreamWriter(File.Open(this.LogFileLocation, FileMode.Append))) {
                outfile.WriteLine("ID, Counter, Success, Best, FeatureCount, Running Geom, RunnnigExp, ProcessName");
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
                            string line1 = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}",
                                processID.ToString(), counter, 
                                string.Format("{0:N2}", overall), 
                                string.Format("{0:N2}", Math.Round(bestSuccess, 2)), 
                                this.allfeatures.Count,
                                string.Format("{0:N2}", Math.Round(this.success.Overall.RunningGeometric, 2)),
                                string.Format("{0:N2}", Math.Round(this.success.Overall.RunningExponential, 2)), 
                                this.Name);
                            //Console.WriteLine();
                            //Console.WriteLine();
                            outfile.WriteLine(line1);
                            outfile.FlushAsync();
                            Console.WriteLine(line1);
                        }
                    }
                    this.allfeatures.AddFeature(28);
                    ///Generate new and purge old features
                }
            }
        }
    }
}

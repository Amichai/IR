using PRCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Workbench2 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        InputLoader loader;
        public MainWindow() {
            InitializeComponent();

            this.loader = new InputLoader();
            this.loader.LoadFile(@"C:\Users\Amichai\Data\digits.csv");
            this.allfeatures = new AllFeatures(10000, 28);
            this.success = new FeatureSuccess();
            this.Process(this.loader.AllElements());
        }

        private AllFeatures allfeatures;
        private FeatureSuccess success;

        public void Process(IEnumerable<Tuple<int[][], string>> dataStream) {
            int counter = 0;
            foreach (var a in dataStream) {
                string guess;
                Dictionary<string, double> result = this.allfeatures.TestTrain(a.Item1, a.Item2, out guess);
                if (guess != "") {
                    this.success.Trial(a.Item2, result, guess);
                    Debug.Print(string.Format("{0}: success: {1}", counter++, this.success.Overall.LastN()));
                }
                //this.allfeatures.AddFeature(28);
                ///Generate new and purge old features
            }
        }
    }
}

using PRCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using MyLogger;

namespace ImageRecognition {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            Logger.Inst.Deserialize(@"..\..\..\Logger\TrialParams.xml");
            InitializeComponent();
            bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            loader = new InputLoader();
            loader.LoadFile(@"C:\Users\Amichai\Data\digits.csv");
            workbench = new Workbench();
            //Listen to all the events here
            this.GridRoot.DataContext = this;
            //workbench.InputLoaded += workbench_InputLoaded;
            workbench.FeaturesTrained += workbench_FeaturesTrained;
            this.TrialParams.Text = Logger.Inst.SerializeParams().ToString();
            this.OutputFile.Text = "Output Filepath: " + Logger.Inst.OutputFilePath();
            bw.RunWorkerAsync();
            //this.FeautresList.ItemsSource = this.Features;
        }

        InputLoader loader;
        Workbench workbench;

        void bw_DoWork(object sender, DoWorkEventArgs e) {
            workbench.Process(loader.AllElements());
        }

        BackgroundWorker bw;

        void workbench_FeaturesTrained(object sender, FeaturesTrainedEventArgs e) {

            Logger.Inst.SetResult(e.TotalNumberOfTrials, e.LastNSuccessRate, e.FeatureCount,
                e.AverageAttractiveness, e.AverageInterestingness, e.AverageNumberOfPoints, e.AverageNumberOfDataSeen,
                e.MaxAttractiveness, e.MaxInterestingness);

            this.TotalNumberOfTrials = e.TotalNumberOfTrials;
            this.LastNSuccessRate = e.LastNSuccessRate;
            this.SuccessRatePerLabel = e.SuccessRatePerLabel;
            this.FeatureCount = e.FeatureCount;

            this.AverageAtractiveness = e.AverageAttractiveness;
            this.AverageInterestingness = e.AverageInterestingness;
            this.AverageNumberOfPoints = e.AverageNumberOfPoints;
            this.AverageNumberOfDataSeen = e.AverageNumberOfDataSeen;

            this.MaxInterestingness = e.MaxInterestingness;
            this.MaxAttractiveness = e.MaxAttractiveness;

            this.Features = e.Features;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        public int TotalNumberOfTrials {
            get { return _TotalNumberOfTrials; }
            set {
                if (_TotalNumberOfTrials != value) {
                    _TotalNumberOfTrials = value;
                    OnPropertyChanged(TotalNumberOfTrialsPropertyName);
                }
            }
        }
        private int _TotalNumberOfTrials;
        public const string TotalNumberOfTrialsPropertyName = "TotalNumberOfTrials";

        public double LastNSuccessRate {
            get { return _LastNSuccessRate; }
            set {
                if (_LastNSuccessRate != value) {
                    _LastNSuccessRate = value;
                    OnPropertyChanged(LastNSuccessRatePropertyName);
                }
            }
        }
        private double _LastNSuccessRate;
        public const string LastNSuccessRatePropertyName = "LastNSuccessRate";

        public Dictionary<string, PastTrials> SuccessRatePerLabel {
            get { return _SuccessRatePerLabel; }
            set {
                Dispatcher.Invoke((Action)(() => {
                    this.db.ItemsSource = null;
                    this.db.ItemsSource = value;
                }));
                _SuccessRatePerLabel = value;
                //OnPropertyChanged(SuccessRatePerLabelPropertyName);
            }
        }
        private Dictionary<string, PastTrials> _SuccessRatePerLabel;
        public const string SuccessRatePerLabelPropertyName = "SuccessRatePerLabel";

        public int FeatureCount {
            get { return _FeatureCount; }
            set {
                if (_FeatureCount != value) {
                    _FeatureCount = value;
                    OnPropertyChanged(FeatureCountPropertyName);
                }
            }
        }
        private int _FeatureCount;
        public const string FeatureCountPropertyName = "FeatureCount";

        public double AverageInterestingness {
            get { return _AverageInterestingness; }
            set {
                if (_AverageInterestingness != value) {
                    _AverageInterestingness = value;
                    OnPropertyChanged(AverageInterestingnessPropertyName);
                }
            }
        }
        private double _AverageInterestingness;
        public const string AverageInterestingnessPropertyName = "AverageInterestingness";

        public int TrainedFeatures {
            get { return _TrainedFeatures; }
            set {
                if (_TrainedFeatures != value) {
                    _TrainedFeatures = value;
                    OnPropertyChanged(TrainedFeaturesPropertyName);
                }
            }
        }
        private int _TrainedFeatures;
        public const string TrainedFeaturesPropertyName = "TrainedFeatures";

        public double AverageAtractiveness {
            get { return _AverageAtractiveness; }
            set {
                if (_AverageAtractiveness != value) {
                    _AverageAtractiveness = value;
                    OnPropertyChanged(AverageAtractivenessPropertyName);
                }
            }
        }
        private double _AverageAtractiveness;
        public const string AverageAtractivenessPropertyName = "AverageAtractiveness";

        public double MaxInterestingness {
            get { return _MaxInterestingness; }
            set {
                if (_MaxInterestingness != value) {
                    _MaxInterestingness = value;
                    OnPropertyChanged(MaxInterestingnessPropertyName);
                }
            }
        }
        private double _MaxInterestingness;
        public const string MaxInterestingnessPropertyName = "MaxInterestingness";

        public double MaxAttractiveness {
            get { return _MaxAttractiveness; }
            set {
                if (_MaxAttractiveness != value) {
                    _MaxAttractiveness = value;
                    OnPropertyChanged(MaxAttractivenessPropertyName);
                }
            }
        }
        private double _MaxAttractiveness;
        public const string MaxAttractivenessPropertyName = "MaxAttractiveness";

        public double  AverageNumberOfPoints {
            get { return _AverageNumberOfPoints; }
            set {
                if (_AverageNumberOfPoints != value) {
                    _AverageNumberOfPoints = value;
                    OnPropertyChanged(AverageNumberOfPointsPropertyName);
                }
            }
        }
        private double  _AverageNumberOfPoints;
        public const string AverageNumberOfPointsPropertyName = "AverageNumberOfPoints";

        public double AverageNumberOfDataSeen {
            get { return _AverageNumberOfDataSeen; }
            set {
                if (_AverageNumberOfDataSeen != value) {
                    _AverageNumberOfDataSeen = value;
                    OnPropertyChanged(AverageNumberOfDataSeenPropertyName);
                }
            }
        }
        private double _AverageNumberOfDataSeen;
        public const string AverageNumberOfDataSeenPropertyName = "AverageNumberOfDataSeen";

        public List<Feature> Features {
            get { return _Features; }
            set {
                if (_Features != value) {
                    _Features = value;
                    OnPropertyChanged(FeaturesPropertyName);
                }
            }
        }
        private List<Feature> _Features;
        public const string FeaturesPropertyName = "Features";
    }
}

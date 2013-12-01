using MyLogger;
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
using System.Windows.Shapes;

namespace Workbench {
    /// <summary>
    /// Interaction logic for ComputeProcess.xaml
    /// </summary>
    public partial class ComputeProcess : UserControl, INotifyPropertyChanged {
        public ComputeProcess(InputLoader loader) {
            InitializeComponent();
            this.UpdateStats = true;
            this.DataContext = this;
            this.OutputFilePath = Logger.Inst.OutputFilePath();
            this.workbench = new ImageRecognition.Workbench();
            this.loader = loader;
            Logger.Inst.logFile.SetColumns("TimeStamp", "Number of trials", "Success rate (last 100)", "Feautre count", "Average Attractiveness", "Average Interestingness", "Average number of points", "Average number of data seen", "Max attractiveness", "Max interestingeness");
            workbench.FeaturesTrained += workbench_FeaturesTrained;
            bw.DoWork += bw_DoWork;
            this.Settings.Text = Logger.Inst.SerializeParams().ToString();
            this.ProcessIndex = processCounter++;
            this.featureLogDestination.Text = "featureLog.txt";
            this.PurgePercentage = workbench.PurgeVal;
            this.RecombinePercentage = workbench.RecombineVal;
        }

        private static int processCounter = 0;
        public int ProcessIndex { get; private set; }

        private void workbench_FeaturesTrained(object sender, ImageRecognition.FeaturesTrainedEventArgs e) {
            Logger.Inst.SetResult(e.TotalNumberOfTrials, e.LastNSuccessRate, e.FeatureCount,
                e.AverageAttractiveness, e.AverageInterestingness, e.AverageNumberOfPoints, e.AverageNumberOfDataSeen,
                e.MaxAttractiveness, e.MaxInterestingness);

            if (!UpdateStats) {
                return;
            }

            this.TotalNumberOfTrials = e.TotalNumberOfTrials;
            this.LastNSuccessRate = e.LastNSuccessRate;
            this.SuccessRatePerLabel = e.SuccessRatePerLabel;
            this.FeatureCount = e.FeatureCount;

            this.MaxSuccessRate = e.MaxSuccessRate;
            this.Monoticity = e.Monoticity;

            this.AverageAttractiveness = e.AverageAttractiveness;
            this.AverageInterestingness = e.AverageInterestingness;
            this.AverageNumberOfPoints = e.AverageNumberOfPoints;
            this.AverageNumberOfDataSeen = e.AverageNumberOfDataSeen;

            this.MaxInterestingness = e.MaxInterestingness;
            this.MaxAttractiveness = e.MaxAttractiveness;


            var now = DateTime.Now.TimeOfDay;
            this.TimeToUpdate = now.Subtract(this.LastUpdate);
            this.LastUpdate = now;
        }

        #region Public Properties
        private TimeSpan _TimeToUpdate;
        public TimeSpan TimeToUpdate {
            get { return _TimeToUpdate; }
            set {
                if (_TimeToUpdate != value) {
                    _TimeToUpdate = value;
                    OnPropertyChanged("TimeToUpdate");
                }
            }
        }

        private double _MaxAttractiveness;
        public double MaxAttractiveness {
            get { return _MaxAttractiveness; }
            set {
                if (_MaxAttractiveness != value) {
                    _MaxAttractiveness = value;
                    OnPropertyChanged("MaxAttractiveness");
                }
            }
        }

        private double _MaxInterestingness;
        public double MaxInterestingness {
            get { return _MaxInterestingness; }
            set {
                if (_MaxInterestingness != value) {
                    _MaxInterestingness = value;
                    OnPropertyChanged("MaxInterestingness");
                }
            }
        }

        private double _AverageNumberOfDataSeen;
        public double AverageNumberOfDataSeen {
            get { return _AverageNumberOfDataSeen; }
            set {
                if (_AverageNumberOfDataSeen != value) {
                    _AverageNumberOfDataSeen = value;
                    OnPropertyChanged("AverageNumberOfDataSeen");
                }
            }
        }

        private double _AverageNumberOfPoints;
        public double AverageNumberOfPoints {
            get { return _AverageNumberOfPoints; }
            set {
                if (_AverageNumberOfPoints != value) {
                    _AverageNumberOfPoints = value;
                    OnPropertyChanged("AverageNumberOfPoints");
                }
            }
        }

        private double _AverageInterestingness;
        public double AverageInterestingness {
            get { return _AverageInterestingness; }
            set {
                if (_AverageInterestingness != value) {
                    _AverageInterestingness = value;
                    OnPropertyChanged("AverageInterestingness");
                }
            }
        }

        private double _AverageAttractiveness;
        public double AverageAttractiveness {
            get { return _AverageAttractiveness; }
            set {
                if (_AverageAttractiveness != value) {
                    _AverageAttractiveness = value;
                    OnPropertyChanged("AverageAttractiveness");
                }
            }
        }

        private double _Monoticity;
        public double Monoticity {
            get { return _Monoticity; }
            set {
                if (_Monoticity != value) {
                    _Monoticity = value;
                    OnPropertyChanged("Monoticity");
                }
            }
        }

        private double _MaxSuccessRate;
        public double MaxSuccessRate {
            get { return _MaxSuccessRate; }
            set {
                if (_MaxSuccessRate != value) {
                    _MaxSuccessRate = value;
                    OnPropertyChanged("MaxSuccessRate");
                }
            }
        }

        private int _FeatureCount;
        public int FeatureCount {
            get { return _FeatureCount; }
            set {
                if (_FeatureCount != value) {
                    _FeatureCount = value;
                    OnPropertyChanged("FeatureCount");
                }
            }
        }

        private Dictionary<string,PastTrials> _SuccessRatePerLabel;
        public Dictionary<string,PastTrials> SuccessRatePerLabel {
            get { return _SuccessRatePerLabel; }
            set {
                Dispatcher.Invoke((Action)(() => {
                    this.db.ItemsSource = null;
                    this.db.ItemsSource = value;
                }));
                _SuccessRatePerLabel = value;
            }
        }

        private double _LastNSuccessRate;
        public double LastNSuccessRate {
            get { return _LastNSuccessRate; }
            set {
                if (_LastNSuccessRate != value) {
                    _LastNSuccessRate = value;
                    OnPropertyChanged("LastNSuccessRate");
                }
            }
        }

        private int _TotalNumberOfTrials;
        public int TotalNumberOfTrials {
            get { return _TotalNumberOfTrials; }
            set {
                if (_TotalNumberOfTrials != value) {
                    _TotalNumberOfTrials = value;
                    OnPropertyChanged("TotalNumberOfTrials");
                }
            }
        }

        private bool _UpdateStats;
        public bool UpdateStats {
            get { return _UpdateStats; }
            set {
                if (_UpdateStats != value) {
                    _UpdateStats = value;
                    OnPropertyChanged("UpdateStats");
                }
                if (!UpdateStats) {
                    this.root.Background = Brushes.Red;
                } else {
                    this.root.Background = Brushes.White;
                }
            }
        }

        private TimeSpan _LastUpdate;
        public TimeSpan LastUpdate {
            get { return _LastUpdate; }
            set {
                if (_LastUpdate != value) {
                    _LastUpdate = value;
                    OnPropertyChanged("LastUpdate");
                }
            }
        }

        private double _PurgePercentage;
        public double PurgePercentage {
            get { return _PurgePercentage; }
            set {
                this.workbench.PurgeVal = value;
                if (_PurgePercentage != value) {
                    _PurgePercentage = value;
                    OnPropertyChanged("PurgePercentage");
                }
            }
        }

        private double _RecombinePercentage;
        public double RecombinePercentage {
            get { return _RecombinePercentage; }
            set {
                this.workbench.RecombineVal = value;
                if (_RecombinePercentage != value) {
                    _RecombinePercentage = value;
                    OnPropertyChanged("RecombinePercentage");
                }
            }
        }
        #endregion


        void bw_DoWork(object sender, DoWorkEventArgs e) {
            workbench.Process(loader.AllElements());
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        private string _OutputFilePath;
        public string OutputFilePath {
            get { return _OutputFilePath; }
            set {
                if (_OutputFilePath != value) {
                    _OutputFilePath = value;
                    OnPropertyChanged("OutputFilePath");
                }
            }
        }

        private ImageRecognition.Workbench workbench { get; set; }
        private InputLoader loader { get; set; }

        private void Run_PreviewMouseDown_1(object sender, MouseButtonEventArgs e) {
            var tb = sender as Run;
            Process.Start(tb.Text);
        }

        AbortableBackgroundWorker bw = new AbortableBackgroundWorker();

        public void Start() {
            bw.RunWorkerAsync();
        }

        internal void Kill() {
            if (bw.IsBusy) {
                bw.Abort();
                bw.Dispose();
            }
        }

        private void LogAllFeatures_Click(object sender, RoutedEventArgs e) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("index,type,data seen,success rate last 100, monoticity, intersetingness, attractiveness, number of points");
            foreach (var f in this.workbench.AllFeatures) {
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", f.CreationIndex, f.FeatureType, f.DataSeen, f.SuccessRate.Overall.LastN(100), f.SuccessRate.Overall.Monoticity, f.Interestingness, f.Attractiveness, f.NumberOfPoints));
            }
            string outputPath = this.featureLogDestination.Text;
            System.IO.File.WriteAllText(outputPath, sb.ToString());
            Process.Start(outputPath);
        }
    }
}

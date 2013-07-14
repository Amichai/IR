using MyLogger;
using PRCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using Xceed.Wpf.AvalonDock.Layout;

namespace Workbench {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            InitializeComponent();
            var parameters = Logger.Inst.Deserialize(@"..\..\..\Logger\TrialParams.xml");
            this.DataContext = this;
            this.NumberOfFeatures = Logger.Inst.GetIntVal("FeaturesToRecombine");
            this.Feature1 = Logger.Inst.GetString("FeatureType");
            this.Feature2 = Logger.Inst.GetString("FeatureType2");
            this.ComputePixelFeatures = Logger.Inst.GetBool("usePixelFeatures");
            this.WeighFeaturesBySuccess = Logger.Inst.GetBool("WeighFeaturesBySuccess");
            this.CompareValExponent = Logger.Inst.GetDouble("compareValExponent");
            this.SourceInPixelFeaturesOnly = Logger.Inst.GetBool("SourceInPixelFeaturesOnly");
            this.Purge = Logger.Inst.GetBool("purge");
            this.AttractivenessThreshold = Logger.Inst.GetDouble("attractivenessPurgeThreshold");
            this.InterestingnessThreshold = Logger.Inst.GetDouble("interestingnessPurgeThreshold");
            this.OnlyTrainOnFailure = Logger.Inst.GetBool("OnlyTrainOnFailure");

            this.timer = new Timer(updateSuccessRates, null, 1000, 1000);
            this.loader = new InputLoader();
            this.loader.LoadFile(@"C:\Users\Amichai\Data\digits.csv");
        }

        Timer timer;

        #region Settings
        private string _Feature1;
        public string Feature1 {
            get { return _Feature1; }
            set {
                Logger.Inst.Update("FeatureType", value);
                if (_Feature1 != value) {
                    _Feature1 = value;
                    OnPropertyChanged("Feature1");
                }
            }
        }

        private string _Feature2;
        public string Feature2 {
            get { return _Feature2; }
            set {
                Logger.Inst.Update("FeatureType2", value);
                if (_Feature2 != value) {
                    _Feature2 = value;
                    OnPropertyChanged("Feature2");
                }
            }
        }

        private int _NumberOfFeatures;
        public int NumberOfFeatures {
            get { return _NumberOfFeatures; }
            set {
                Logger.Inst.Update("FeaturesToRecombine", value.ToString());
                if (_NumberOfFeatures != value) {
                    _NumberOfFeatures = value;
                    OnPropertyChanged("NumberOfFeatures");
                }
            }
        }


        private bool _ComputePixelFeatures;
        public bool ComputePixelFeatures {
            get { return _ComputePixelFeatures; }
            set {
                Logger.Inst.Update("usePixelFeatures", value.ToString());
                if (_ComputePixelFeatures != value) {
                    _ComputePixelFeatures = value;
                    OnPropertyChanged("ComputePixelFeatures");
                }
            }
        }

        private bool _WeighFeaturesBySuccess;
        public bool WeighFeaturesBySuccess {
            get { return _WeighFeaturesBySuccess; }
            set {
                Logger.Inst.Update("WeighFeaturesBySuccess", value.ToString());
                if (_WeighFeaturesBySuccess != value) {
                    _WeighFeaturesBySuccess = value;
                    OnPropertyChanged("WeighFeaturesBySuccess");
                }
            }
        }

        private double _CompareValExponent;
        public double CompareValExponent {
            get { return _CompareValExponent; }
            set {
                Logger.Inst.Update("compareValExponent", value.ToString());
                if (_CompareValExponent != value) {
                    _CompareValExponent = value;
                    OnPropertyChanged("CompareValExponent");
                }
            }
        }

        private bool _SourceInPixelFeaturesOnly;
        public bool SourceInPixelFeaturesOnly {
            get { return _SourceInPixelFeaturesOnly; }
            set {
                Logger.Inst.Update("SourceInPixelFeaturesOnly", value.ToString());
                if (_SourceInPixelFeaturesOnly != value) {
                    _SourceInPixelFeaturesOnly = value;
                    OnPropertyChanged("SourceInPixelFeaturesOnly");
                }
            }
        }

        private bool _Purge;
        public bool Purge {
            get { return _Purge; }
            set {
                Logger.Inst.Update("purge", value.ToString());
                if (_Purge != value) {
                    _Purge = value;
                    OnPropertyChanged("Purge");
                }
            }
        }

        private double _AttractivenessThreshold;
        public double AttractivenessThreshold {
            get { return _AttractivenessThreshold; }
            set {
                Logger.Inst.Update("attractivenessPurgeThreshold", value.ToString());
                if (_AttractivenessThreshold != value) {
                    _AttractivenessThreshold = value;
                    OnPropertyChanged("AttractivenessThreshold");
                }
            }
        }

        private double _InterestingnessThreshold;
        public double InterestingnessThreshold {
            get { return _InterestingnessThreshold; }
            set {
                Logger.Inst.Update("interestingnessPurgeThreshold", value.ToString());
                if (_InterestingnessThreshold != value) {
                    _InterestingnessThreshold = value;
                    OnPropertyChanged("InterestingnessThreshold");
                }
            }
        }

        private bool _OnlyTrainOnFailure;
        public bool OnlyTrainOnFailure {
            get { return _OnlyTrainOnFailure; }
            set {
                Logger.Inst.Update("OnlyTrainOnFailure", value.ToString());
                if (_OnlyTrainOnFailure != value) {
                    _OnlyTrainOnFailure = value;
                    OnPropertyChanged("OnlyTrainOnFailure");
                }
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        InputLoader loader;

        public void Spawn() {
            ComputeProcess process = new ComputeProcess(loader);
            var layoutDoc = new LayoutDocument() {
                Title = "process" + process.ProcessIndex.ToString(),
                CanFloat = true,
                CanClose = true

            };

            layoutDoc.Closing += layoutDoc_Closing;
            layoutDoc.Content = process;
            this.documentPane.Children.Insert(0, layoutDoc);
            process.Start();
        }

        void layoutDoc_Closing(object sender, CancelEventArgs e) {
            ((sender as LayoutDocument).Content as ComputeProcess).Kill();
        }

        private void updateSuccessRates(object state) {
            Dictionary<string, double> stats = new Dictionary<string, double>();
            foreach (var child in this.documentPane.Children) {
                var doc = child as LayoutDocument;
                if (doc != null) {
                    var process = (doc.Content as ComputeProcess);
                    if (process != null) {
                        Dispatcher.Invoke((Action)(() => {
                            stats[doc.Title] = process.LastNSuccessRate;
                        }));
                    }
                }
            }
            Dispatcher.Invoke((Action)(() => {
                this.successRates.ItemsSource = null;
                this.successRates.ItemsSource = stats;
            }));
        }

        private void Spawn(object sender, RoutedEventArgs e) {
            Spawn();
        }
    }
}

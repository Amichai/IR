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

            this.DataContext = this;
            Spawn();
        }

        private string _Feautre1;
        public string Feautre1 {
            get { return _Feautre1; }
            set {
                if (_Feautre1 != value) {
                    _Feautre1 = value;
                    OnPropertyChanged("Feature1");
                }
            }
        }

        private string _Feature2;
        public string Feature2 {
            get { return _Feature2; }
            set {
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
                if (_WeighFeaturesBySuccess != value) {
                    _WeighFeaturesBySuccess = value;
                    OnPropertyChanged("WeighFeaturesBySuccess");
                }
            }
        }

        public double CompareValExponent { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        public void Spawn() {
            var layoutDoc = new LayoutDocument() {
                Title = "process",
                CanFloat = true,
                CanClose = true

            };
            ComputeProcess process = new ComputeProcess();
            layoutDoc.Content = process;
            this.documentPane.Children.Insert(0, layoutDoc);
            process.Start();
        }

        private void Spawn(object sender, RoutedEventArgs e) {
            Spawn();
        }
    }
}

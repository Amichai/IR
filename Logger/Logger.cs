using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyLogger {
    public class Logger {
        private Logger() {
            this.parameters = new Dictionary<string, LogParam>();
            this.logFile = new LogFile(@"data", ".csv", appendDate: true);
        }

        //We may want to let the user add detail describing this param that will be serialized and 
        //can be queried later
        public void Set(string name, double val, string detail = "", bool intVal = false) {
            if (parameters.ContainsKey(name)) {
                throw new Exception("Duplicate param");
            }
            parameters[name] = new LogParam() { Val = val, Detail = detail, IsIntVal = intVal };
        }

        public double Get(string name) {
            if (parameters[name].IsIntVal) {
                return Math.Round(parameters[name].Val);
            } else {
                return parameters[name].Val;
            }
        }

        public void SerializeParams(string filename) {
            XElement root = new XElement("params");
            foreach (var a in parameters) {
                root.Add(new XElement(a.Key,
                    new XAttribute("IsIntVal", a.Value.IsIntVal),
                    new XAttribute("Detail", a.Value.Detail),
                    new XAttribute("Value", a.Value.Val)));
            }
            root.Save(filename);
        }

        ///TODO: handle filenames better
        public void Deserialize(string filename) {
            XElement root = XElement.Load(filename);
            this.logFile.SetTrialData(root);
            foreach (var a in root.Elements()) {
                parameters[a.Name.ToString()] = LogParam.FromXml(a);
            }
        }

        private Dictionary<string, LogParam> parameters;
        public static Logger Inst = new Logger();

        public LogFile logFile { get; set; }

        public void SetResult(params object[] result) {
            try {
                logFile.Append(result);
            } catch (Exception ex){
                Debug.Print(string.Format("Failed to log to file {0}. Exception: {1}", this.logFile.FilePath, ex.ToString()));
            }
        }

    }

}

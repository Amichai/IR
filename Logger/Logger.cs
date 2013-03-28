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
            this.logFile = new LogFile(@"data", ".csv");
        }

        //We may want to let the user add detail describing this param that will be serialized and 
        //can be queried later
        public void Set(string name, string value, string detail = "") {
            if (parameters.ContainsKey(name)) {
                throw new Exception("Duplicate param");
            }
            parameters[name] = new LogParam() { Value = value, Detail = detail };
            this.logFile.SetTrialData(SerializeParams());
        }

        public bool GetBool(string name) {
            return bool.Parse(parameters[name].Value);
        }

        public string GetString(string name) {
            if (parameters.ContainsKey(name)) {
                return parameters[name].Value;
            } else {
                return null;
            }
        }

        public int GetIntVal(string name) {
            return int.Parse(parameters[name].Value);
        }

        public XElement SerializeParams() {
            XElement root = new XElement("params");
            foreach (var a in parameters) {
                root.Add(new XElement(a.Key,
                    new XAttribute("Detail", a.Value.Detail),
                    new XAttribute("Value", a.Value.Value)));
            }
            return root;
        }

        public string paramFilename { get; set; }

        ///TODO: handle filenames better
        public void Deserialize(string filename) {
            this.paramFilename = filename;
            XElement root = XElement.Load(filename);
            this.logFile.SetTrialData(root);
            foreach (var a in root.Elements()) {
                parameters[a.Name.ToString()] = LogParam.FromXml(a);
            }
        }

        private Dictionary<string, LogParam> parameters;
        public static Logger Inst = new Logger();

        public LogFile logFile { get; set; }

        public string OutputFilePath() {
            return logFile.FilePath;
        }

        public void SetResult(params object[] result) {
            try {
                logFile.Append(result);
            } catch (Exception ex){
                Debug.Print(string.Format("Failed to log to file {0}. Exception: {1}", this.logFile.FilePath, ex.ToString()));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyLogger {
    public class LogFile {
        public FileInfo logFile { get; set; }

        public string FilePath { get; set; }

        public string hostFolder { get; set; }

        public void SetTrialData(XElement xml) {
            xml.Save(hostFolder + @"/" +  
                appendDateTime("trialData") + ".txt");
        }

        private string appendDateTime(string filepath) {
            filepath += "_" + (DateTime.Now.ToShortDateString() + "_"
                    + DateTime.Now.TimeOfDay.ToString());

            filepath = filepath.Replace('/', '-');
            filepath = filepath.Replace(':', '.');
            return filepath;
        }

        public LogFile(string filepath, string ext) {
            this.hostFolder = "trial";
            filepath = appendDateTime(filepath);
            filepath = hostFolder + "\\" + filepath + ext;
            this.FilePath = filepath;
            logFile = new FileInfo(filepath);
            if (!logFile.Exists) {
                logFile.Create();
            }
        }

        public void Append(params object[] vals) {
            using (StreamWriter logStream = logFile.AppendText()) {
                logStream.Write(DateTime.Now.ToString() + ", ");
                foreach (var a in vals) {
                    logStream.Write(a + ", ");
                }
                logStream.Write("\n");
                logStream.Close();
            }
        }
    }
}

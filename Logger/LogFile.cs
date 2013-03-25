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
            xml.Save(hostFolder + @"/trialData.txt");
        }

        public LogFile(string filepath, string ext, bool appendDate = false) {
            this.hostFolder = "trial";
            if(appendDate){
                filepath += "_" + (DateTime.Now.ToShortDateString() + "_" 
                    + DateTime.Now.ToShortTimeString());
            }

            filepath = filepath.Replace('/', '-');
            filepath = filepath.Replace(':', '.');
            
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

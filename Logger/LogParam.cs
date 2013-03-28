using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyLogger {
    struct LogParam {
        public string Value { get; set; }
        public string Detail { get; set; }

        public static LogParam FromXml(XElement xml) {
            LogParam newparam = new LogParam() { Detail = "" };
            foreach (var a in xml.Attributes()) {
                if (a.Name.ToString() == "Value") {
                    newparam.Value = a.Value;
                }
                if (a.Name.ToString() == "Detail") {
                    newparam.Detail = a.Value;
                }
            }
            return newparam;
        }
    }
}

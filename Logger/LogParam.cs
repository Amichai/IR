using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyLogger {
    struct LogParam {
        public double Val;
        public string Detail;
        public bool IsIntVal;

        public static LogParam FromXml(XElement xml) {
            LogParam newparam = new LogParam();
            foreach (var a in xml.Attributes()) {
                if (a.Name.ToString() == "IsIntVal") {
                    newparam.IsIntVal = bool.Parse(a.Value);
                }
                if (a.Name.ToString() == "Detail") {
                    newparam.Detail = a.Value;
                }
                if (a.Name.ToString() == "Value") {
                    newparam.Val = double.Parse(a.Value);
                }
            }
            return newparam;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Workbench2 {
    public interface IValGenerator {
        Func<int[][], double> Generator { get; set; }
        double Eval(int[][] i);

        XElement ToXml();
    }
}

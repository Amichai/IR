using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Workbench2.Generators {
    class PixelValGenerator : IValGenerator{
        public Func<int[][], double> Generator { get; set; }
        private int _x, _y;

        public PixelValGenerator(int x, int y) {
            this._x = x;
            this._y = y;
            this.Generator = i => (double)i[x][y];
        }

        public double Eval(int[][] i) {
            return this.Generator(i);
        }

        public XElement ToXml() {
            XElement root = new XElement("PixelValGenerator");
            root.Add(new XElement("X", _x));
            root.Add(new XElement("Y", _y));
            return root;
        }
    }
}

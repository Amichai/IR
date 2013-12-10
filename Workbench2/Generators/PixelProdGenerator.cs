using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Workbench2.Generators {
    public class PixelProdGenerator : IValGenerator {
        private int _x1, _x2, _y1, _y2;
        public PixelProdGenerator(int x1, int y1, int x2, int y2) {
            this._x1 = x1;
            this._x2 = x2;
            this._y1 = y1;
            this._y2 = y2;
            this.Generator = i => {
                var v1 = (double)i[x1][y1];
                var v2 = (double)i[x2][y2];
                return v1 * v2;
            };
        }
        private Random rand = new Random();

        public Func<int[][], double> Generator {
            get;
            set;
        }

        public double Eval(int[][] i) {
            return this.Generator(i);
        }

        public XElement ToXml() {
            XElement root = new XElement("PixelProdGenerator");
            root.Add(new XElement("X1", _x1));
            root.Add(new XElement("Y1", _y1));
            root.Add(new XElement("X2", _x2));
            root.Add(new XElement("Y2", _y2));
            return root;
        }
    }
}

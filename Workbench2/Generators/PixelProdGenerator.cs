using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workbench2.Generators {
    public class PixelProdGenerator : IValGenerator {
        public PixelProdGenerator(int x1, int y1, int x2, int y2) {
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
    }
}

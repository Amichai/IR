using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workbench2.Generators {
    class PixelDiffGenerator : IValGenerator {
        public PixelDiffGenerator(int x1, int y1, int x2, int y2) {
            this.Generator = i => {
                var v1 = (double)i[x1][y1];
                var v2 = (double)i[x2][y2];
                //return v1 - v2;
                var denom = v2 + v1;
                if (denom == 0) {
                    return 0;
                }
                return (v1 / denom);
            };
        }
        public Func<int[][], double> Generator {
            get;
            set;
        }

        public double Eval(int[][] i) {
            return this.Generator(i);
        }
    }
}

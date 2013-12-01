using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workbench2.Generators {
    class PixelValGenerator : IValGenerator{
        public Func<int[][], double> Generator { get; set; }

        public PixelValGenerator(int x, int y) {
            this.Generator = i => (double)i[x][y];
        }

        public double Eval(int[][] i) {
            return this.Generator(i);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workbench3 {
    class Attribute<T> {
        Func<T, double> f;
        public double Eval(T input) {
            return f(input);
        }

        
    }

    class Feature<T> {
        BinaryMapper binaryMapper;
        Attribute<T> attribute;
        public void Train(T input, Label l) {
            var eval = attribute.Eval(input);
            this.binaryMapper.Update(eval, l);

        }
        ///Given an attribute and a binary map
        ///handle naive bayes, inteestingness, support, boosting
        ///all training data
    }


    class BinaryMapper {
        double binMin, binMax;
        public BinaryMapper() {
            this.binMin = double.MaxValue;
            this.binMax = double.MinValue;
        }
        public void Update(double eval, Label l) {
            throw new NotImplementedException();
        }

        public bool Map(double val) {
            return val <= this.binMax && val >= this.binMin;
        }
    }
    ///Map an eval into a binary classifier
    ///Use the binary classifier to accomplish naive bayes
}
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalEvolution
{
    class InputNeuron : Neuron
    {
        public InputNeuron() : base(new Neuron[0])
        {
            
        }

        public new double Output
        {
            get
            {
                return base.Output;
            }
            set
            {
                output = value;
                isOutputValid = true;
            }
        }

        protected override void CalcOutput()
        {
            isOutputValid = true;
        }
    }
}

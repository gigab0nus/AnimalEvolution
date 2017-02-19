using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace AnimalEvolution
{
    class Neuron
    {
        protected Neuron[] prevLayer;
        protected double[] weightAmplitudes;
        protected double[] weightAngles;
        protected double[] actualWeights;
        protected bool isOutputValid = false;

        protected double output = 0;
        public virtual double Output
        {
            get
            {
                if (!isOutputValid)
                {
                    CalcOutput();
                }
                return output;
            }
        }

        public int NumWeights
        {
            get
            {
                return actualWeights.Length;
            }
        }

        private Neuron(Neuron[] prevLayer, double[] weightAmplitudes, double[] weightAngles)
        {
            this.prevLayer = prevLayer;
            this.weightAmplitudes = weightAmplitudes;
            this.weightAngles = weightAngles;
            InitializeActualWeights();
        }

        public Neuron(Neuron[] prevLayer)
        {
            this.prevLayer = prevLayer;
            InitializeRandomWeights();
        }


        public Neuron(Neuron original, Neuron[] prevLayer)
        {
            this.prevLayer = prevLayer;
            InitializeCopiedWeights(original.weightAmplitudes, original.weightAngles);
        }

        protected void InitializeActualWeights()
        {
            actualWeights = new double[weightAmplitudes.Length];
            for(int i = 0; i< weightAmplitudes.Length; i++)
            {
                actualWeights[i] = weightAmplitudes[i] * (double) Math.Cos(weightAngles[i]);
            }
        }




        protected void InitializeRandomWeights()
        {
            weightAmplitudes = new double[prevLayer.Length];
            weightAngles = new double[weightAmplitudes.Length];
            for (int i = 0; i < weightAmplitudes.Length; i++)
            {
                weightAmplitudes[i] = (double)Simulation.NextRandomDouble() * 2 - 1;
            }
            for (int i = 0; i < weightAngles.Length; i++)
            {
                weightAngles[i] = (double)(Simulation.NextRandomDouble() * 2 * Math.PI);
            }
            InitializeActualWeights();
        }

        public void InitializeCopiedWeights(double[] weights, double[] weightAngles)
        {
            Debug.Assert(weights.Length == prevLayer.Length);
            Debug.Assert(weights.Length == weightAngles.Length);
            this.weightAmplitudes = new double[prevLayer.Length];
            this.weightAngles = new double[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                this.weightAmplitudes[i] = weights[i];
            }
            for (int i = 0; i < weightAngles.Length; i++)
            {
                this.weightAngles[i] = weightAngles[i];
            }
            InitializeActualWeights();
        }

        public void Invalidate()
        {
            isOutputValid = false;
        }

        protected virtual void CalcOutput()
        {
            double sum = 0;
            for(int i = 0; i < weightAmplitudes.Length; i++)
            {
                sum += prevLayer[i].Output * actualWeights[i];
            }
            output = ActivationFunction(sum);
            isOutputValid = true;
        }

        public void Mutate(double maxMutation, double maxWeightAngleMutation)
        {
            int affectedWeight = Simulation.NextRandom(weightAmplitudes.Length);
            if (Simulation.NextRandomDouble() > 0.5)
                weightAmplitudes[affectedWeight] *= (double)(Math.Exp((Simulation.NextRandomGaussian()) * Math.Log(maxMutation)));
            else
            {
                weightAngles[affectedWeight] += (double)(Simulation.NextRandomGaussian()) * maxWeightAngleMutation;
                while (weightAngles[affectedWeight] >= Math.PI * 2)
                    weightAngles[affectedWeight] -= (double)Math.PI * 2;
                while (weightAngles[affectedWeight] < 0)
                    weightAngles[affectedWeight] += (double)Math.PI * 2;
            }
            InitializeActualWeights();
        }


        protected static double ActivationFunction(double x)
        {
            return (double)Math.Tanh(x);
        }

        public double GetWeightAngle(int i)
        {
            return weightAngles[i];
        }

        public double GetActualWeight(int i)
        {
            return actualWeights[i];
        }

        public double GetWeightAmplitude(int i)
        {
            return weightAmplitudes[i];
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteArray(weightAmplitudes, delegate (double val) { writer.Write((float)val); });
            writer.WriteArray(weightAngles, delegate (double val) { writer.Write((float)val); });
        }

        public static Neuron Read(BinaryReader reader, Neuron[] prevLayer)
        {
            double[] weightAmplitudes = reader.ReadArray(delegate () { return (double)reader.ReadSingle(); });
            double[] weightAngles = reader.ReadArray(delegate () { return (double)reader.ReadSingle(); });

            return new Neuron(prevLayer, weightAmplitudes, weightAngles);
        }
    }
}

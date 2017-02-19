using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace AnimalEvolution
{
    class NeuralNetwork
    {
        private Neuron[][] neurons;
        private Neuron[] outputNeurons;
        private InputNeuron[] inputNeurons;
        private InputNeuron biasNeuron;

        private double geneticStability = 0;
        private bool geneticStabilityCalculated = false;
        public double GeneticStability
        {
            get
            {
                if (!geneticStabilityCalculated)
                    CalculateGeneticStability();
                return geneticStability;
            }
        }

        private NeuralNetwork(Neuron[][] neurons, InputNeuron[] inputNeurons)
        {
            this.neurons = neurons;
            this.inputNeurons = inputNeurons;
            outputNeurons = neurons[neurons.Length - 1];
            biasNeuron = inputNeurons[inputNeurons.Length - 1];
        }

        public NeuralNetwork(int[] structure)
        {
            //Main Array
            neurons = new Neuron[structure.Length][];

            //Input Neurons
            inputNeurons = new InputNeuron[structure[0] + 1];
            neurons[0] = inputNeurons;

            for (int i = 0; i < inputNeurons.Length; i++)
            {
                inputNeurons[i] = new InputNeuron();
            }

            //Bias Neuron
            biasNeuron = inputNeurons[inputNeurons.Length - 1];
            biasNeuron.Output = 1;

            //Hidden and Output Neurons
            for (int i = 1; i< neurons.Length; i++)
            {
                neurons[i] = new Neuron[structure[i]];
                for(int j = 0; j < neurons[i].Length; j++)
                {
                    neurons[i][j] = new Neuron(neurons[i - 1]);
                }
            }

            //Output Neurons
            outputNeurons = neurons[neurons.Length - 1];
        }

        public NeuralNetwork(NeuralNetwork original)
        {
            //Main Array
            neurons = new Neuron[original.neurons.Length][];

            //Input Neurons
            inputNeurons = new InputNeuron[original.inputNeurons.Length];
            neurons[0] = inputNeurons;

            for (int i = 0; i < inputNeurons.Length; i++)
            {
                inputNeurons[i] = new InputNeuron();
            }

            //Bias Neuron
            biasNeuron = inputNeurons[inputNeurons.Length - 1];
            biasNeuron.Output = 1;

            //Hidden and Output Neurons
            for (int i = 1; i < neurons.Length; i++)
            {
                neurons[i] = new Neuron[original.neurons[i].Length];
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    neurons[i][j] = new Neuron(original.neurons[i][j], neurons[i - 1]);
                }
            }

            //Output Neurons
            outputNeurons = neurons[neurons.Length - 1];
        }

        public void Feed(double[] data)
        {
            //Input Neurons
            for(int i = 0; i < inputNeurons.Length-1; i++)
            {
                inputNeurons[i].Output = data[i];
            }

            //Bias Neuron
            biasNeuron.Output = 1;

            //Hidden and Output Neurons
            for(int i = 1; i < neurons.Length; i++)
            {
                for(int j = 0; j < neurons[i].Length; j++)
                {
                    neurons[i][j].Invalidate();
                }
            }
        }

        public void Mutate(int numMutations, double maxWeightFactor, double maxWeightAngleDifference)
        {
            int totalNumMutatableWeights = 0;
            for(int i = 1; i< neurons.Length; i++)
            {
                totalNumMutatableWeights += neurons[i - 1].Length * neurons[i].Length;
            }

            for (int i = 0; i < numMutations; i++)
            {
                int affectedWeight = (int)(Simulation.NextRandomDouble() * totalNumMutatableWeights);
                int layerIndex;
                int neuronIndex;
                for (layerIndex = 1; affectedWeight >= neurons[layerIndex].Length * neurons[layerIndex - 1].Length; layerIndex++)
                {
                    affectedWeight -= neurons[layerIndex].Length * neurons[layerIndex - 1].Length;
                }
                for (neuronIndex = 0; affectedWeight >= neurons[layerIndex - 1].Length; neuronIndex++)
                {
                    affectedWeight -= neurons[layerIndex - 1].Length;
                }
                neurons[layerIndex][neuronIndex].Mutate(maxWeightFactor, maxWeightAngleDifference);
            }
            geneticStabilityCalculated = false;
        }

        public double GetPositiveOutput(int neuronIndex)
        {
            return GetOutput(neuronIndex) / 2 + 0.5;
        }

        public double GetOutput(int neuronIndex)
        {
            return outputNeurons[neuronIndex].Output;
        }

        private void CalculateGeneticStability()
        {
            double sumActualWeightsSquared = 0;
            double sumActualWeightsTimesCosAngleSquared = 0; 

            for(int i = 1; i < neurons.Length; i++)
            {
                for(int j = 0; j< neurons[i].Length; j++)
                {
                    Neuron thisNeuron = neurons[i][j];
                    for(int k = 0; k < thisNeuron.NumWeights; k++)
                    {
                        double actualWeight = thisNeuron.GetActualWeight(k);
                        sumActualWeightsSquared += actualWeight * actualWeight;
                        double cosAngle = Math.Cos(thisNeuron.GetWeightAngle(k));
                        sumActualWeightsTimesCosAngleSquared += actualWeight * actualWeight * cosAngle * cosAngle;
                    }
                }
            }

            geneticStability = Math.Sqrt(sumActualWeightsTimesCosAngleSquared / sumActualWeightsSquared);

            geneticStabilityCalculated = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float circleSize = 40f;
            float distanceFactorY = 1f;
            float distanceFactorX = 2;
            float offsetX = 25;
            float offsetY = 550;
            float[] distanceY = new float[neurons.Length];

            float maxLayerSize = 0;
            for(int i = 0; i< neurons.Length; i++)
            {
                if (neurons[i].Length > maxLayerSize)
                    maxLayerSize = neurons[i].Length;
            }

            for (int i = 0; i < neurons.Length; i++)
            {
                distanceY[i] = circleSize * distanceFactorY * (maxLayerSize - 1) / (neurons[i].Length - 1);
            }

            float[] maxWeights = new float[neurons.Length]; // [0] not used
            for (int i = 1; i < neurons.Length; i++)
            {
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    for (int k = 0; k < neurons[i - 1].Length; k++)
                    {
                        double weight = neurons[i][j].GetActualWeight(k);
                        if (Math.Abs(weight) > maxWeights[i])
                        {
                            maxWeights[i] = (float)Math.Abs(weight);
                        }
                    }
                }
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    for (int k = 0; k < neurons[i - 1].Length; k++)
                    {
                        double weight = neurons[i][j].GetActualWeight(k);
                        spriteBatch.DrawLine(
                            offsetX + i * circleSize * distanceFactorX, offsetY + distanceY[i] * j,
                            offsetX + (i - 1) * circleSize * distanceFactorX, offsetY + distanceY[i - 1] * k,
                            weight > 0 ?
                                new Color(Color.Blue, (float)(weight / maxWeights[i])) :
                                new Color(Color.Red, (float)(-weight / maxWeights[i])),
                            circleSize / 100
                        );
                    }
                }
            }

            for (int i = 0; i < neurons.Length; i++)
            {
                for (int j = 0; j< neurons[i].Length; j++)
                {
                    if (neurons[i][j].Output > 0)
                        spriteBatch.DrawCircle(offsetX + i * circleSize * distanceFactorX, offsetY + distanceY[i] * j, circleSize * (float)Math.Sqrt(neurons[i][j].Output) / 2, 10, Color.Blue, circleSize / 20);
                    else
                        spriteBatch.DrawCircle(offsetX + i * circleSize * distanceFactorX, offsetY + distanceY[i] * j, circleSize * (float)Math.Sqrt(-neurons[i][j].Output) / 2, 10, Color.Red, circleSize / 20);
                }
            }
        }




        public void Write(BinaryWriter writer)
        {
            writer.Write(neurons.Length);
            writer.Write(inputNeurons.Length);
            //Nicht mit BinaryReader.ReadArray einlesen!!
            //writer.WriteArray(neurons, delegate (Neuron[] layer) { writer.WriteArray(layer, delegate (Neuron neuron) { neuron.Write(writer); }); });
            for (int i = 1; i< neurons.Length; i++)
            {
                writer.Write(neurons[i].Length);
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    neurons[i][j].Write(writer);
                }
            }
        }


        public static NeuralNetwork Read(BinaryReader reader)
        {
            Neuron[][] neurons = new Neuron[reader.ReadInt32()][];
            InputNeuron[] inputNeurons = new InputNeuron[reader.ReadInt32()];
            for(int i = 0; i< inputNeurons.Length; i++)
            {
                inputNeurons[i] = new InputNeuron();
            }
            neurons[0] = inputNeurons;
            for(int i = 1; i< neurons.Length; i++)
            {
                neurons[i] = new Neuron[reader.ReadInt32()];
                for(int j =0; j< neurons[i].Length; j++)
                {
                    neurons[i][j] = Neuron.Read(reader, neurons[i-1]);
                }
            }
            return new NeuralNetwork(neurons, inputNeurons);
        }
    }
}

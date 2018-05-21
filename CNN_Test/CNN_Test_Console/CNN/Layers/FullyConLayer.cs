﻿using System;
using MatrixLib;

namespace ConvNeuralNetwork
{
    class FullyConLayer : Layer
    {
        #region Variables

        private int inputNodes;
        private int[] hidLayers;
        private int outputNodes;

        private Matrix[] weights;
        private Matrix[] biases;

        private Matrix[] layerOutputs;

        private Func<float, float> activationHidden;
        private Func<float, float> derOfActivationHidden;

        private Func<float, float> activationOutput;
        private Func<float, float> derOfActivationOutput;
        #endregion

        #region Constructors

        public FullyConLayer(int[] layerTop, ActivationType activationHidden, ActivationType activationOutput) : base(LayerType.FULLY_CONNECTED)
        {
            hidLayers = new int[layerTop.Length - 2];
            weights = new Matrix[layerTop.Length - 1];
            biases = new Matrix[layerTop.Length - 1];
            layerOutputs = new Matrix[layerTop.Length - 1];

            this.inputNodes = layerTop[0];
            this.outputNodes = layerTop[layerTop.Length - 1];
            for (int i = 0; i < layerTop.Length - 2; i++)
            {
                this.hidLayers[i] = layerTop[i + 1];
            }
            
            weights[0] = new Matrix(hidLayers[0], inputNodes);
            biases[0] = new Matrix(hidLayers[0], 1);
            weights[weights.Length - 1] = new Matrix(outputNodes, hidLayers[hidLayers.Length - 1]);
            biases[biases.Length - 1] = new Matrix(outputNodes, 1);

            for (int i = 1; i <= weights.Length - 2; i++)
            {
                weights[i] = new Matrix(hidLayers[i + 1], hidLayers[i]);
                biases[i] = new Matrix(hidLayers[i], 1);
            }
            for (int i = 0; i < layerOutputs.Length; i++)
            {
                layerOutputs[i] = new Matrix(layerTop[i],1);
            }

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i].Randomize(); 
            }
            for (int i = 0; i < weights.Length; i++)
            {
                biases[i].Randomize();
            }
            Tuple<Func<float, float>, Func<float, float>> hiddenFuncs, outputFuncs;
            hiddenFuncs= ActivationFunctions.GetActivationFuncs(activationHidden);
            outputFuncs = ActivationFunctions.GetActivationFuncs(activationOutput);

            //set activation funcs
            this.activationHidden = hiddenFuncs.Item1;
            this.derOfActivationHidden = hiddenFuncs.Item2;
            this.activationOutput = outputFuncs.Item1;
            this.derOfActivationOutput = outputFuncs.Item2;
        }

        #endregion

        #region Training Methods
        public override void Initialize()
        {
            base.Initialize();
           
        }
        public override void FeedForward()
        {
            base.FeedForward();
            for (int i = 0; i < layerOutputs.Length; i++)
            {
                if (i == 0)
                    layerOutputs[i] = weights[i] * Input[0];
                else
                    layerOutputs[i] = weights[i] * layerOutputs[i - 1];

                layerOutputs[i] += biases[i];

                if (i == layerOutputs.Length - 1)
                    layerOutputs[i].Map(activationOutput);
                else
                    layerOutputs[i].Map(activationHidden);

            }

           Console.WriteLine(layerOutputs[layerOutputs.Length - 1].ToString());
            if(OutputLayer != null)
                this.OutputLayer.Input[0] = layerOutputs[layerOutputs.Length - 1];
            
        }
        public override void Backpropagation()
        {
            base.Backpropagation();

            Matrix net_d_E = null;
            Matrix w_d_net = null;
            Matrix w_d_E = null;
            Matrix out_d_net = null;
            Matrix out_d_E = null;

            for(int i = layerOutputs.Length - 1; i >= 0; i--)
            {
                //multiply der of lost function respect to activation and derofactivation respect to input
                if (i == layerOutputs.Length - 1)
                    net_d_E = Matrix.Multiply(layerOutputs[i] - Network.Target, Matrix.Map(layerOutputs[i], derOfActivationOutput));
                else
                    net_d_E = Matrix.Multiply(out_d_E, Matrix.Map(layerOutputs[i], derOfActivationHidden));

                //der of input to current layer respect to weight
                if (i != 0)
                    w_d_net = Matrix.Map(layerOutputs[i - 1], DerNetFunc);
                else
                    w_d_net = Matrix.Map(Input[0], DerNetFunc);

                    w_d_E = net_d_E  * Matrix.Transpose(w_d_net);
              

                out_d_net = Matrix.Map(weights[i], DerNetFunc);

                weights[i] = weights[i] - (this.Network.LearningRate * w_d_E);

                out_d_E = Matrix.Transpose(out_d_net) * net_d_E;
                
            }
           
            
            Console.WriteLine(out_d_E.ToString());

            //we need a preprocessing here
            this.InputLayer.Output_d_E[0] = out_d_E;
        }


        /// <summary>
        /// this one for debugging
        /// </summary>
        /// <param name="target"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public double GetError(Matrix target, Matrix output)
        {
            // Calculate the error 
            // ERROR = (1 / 2) * (TARGETS - OUTPUTS)^2

            Matrix outputError = target - output;
            outputError = Matrix.Multiply(outputError, outputError) / 2f;

            double error = 0.0;
            for (int i = 0; i < outputError.data.GetLength(0); i++)
                error += outputError.data[i, 0];

            return error;
        }

        public static float DerNetFunc(float x)
        {
            return x;
        }

        #endregion
    }
}


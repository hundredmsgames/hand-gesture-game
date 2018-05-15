﻿using System;
using MatrixLib;

namespace ConvNeuralNetwork
{
	 class FullyConLayer : Layer
	{
        #region Variables

        private int inputNodes;
		private int hiddenNodes;
		private int outputNodes;

        private Matrix weights_ih;
        private Matrix weights_ho;
        private Matrix bias_h;
        private Matrix bias_o;


        private Matrix outs_out;
        private Matrix out_hid;

        private double learningRate;

        private Func<double, double> activationFunc;
        private Func<double, double> derOfActFunc;

        #endregion

        #region Constructors

        public FullyConLayer(int inputNodes, int hiddenNodes, int outputNodes, double learningRate,
            Func<double, double> activationFunc, Func<double, double> derOfActivationFunc) : base(LayerType.FULLY_CONNECTED)
        {
			this.inputNodes  = inputNodes;
			this.hiddenNodes = hiddenNodes;
			this.outputNodes = outputNodes;

			this.learningRate = learningRate;
		
			weights_ih = new Matrix(hiddenNodes, inputNodes);
			weights_ho = new Matrix(outputNodes, hiddenNodes);
			weights_ih.Randomize();
			weights_ho.Randomize();

			bias_h = new Matrix(this.hiddenNodes, 1);
			bias_o = new Matrix(this.outputNodes, 1);
			bias_h.Randomize();
			bias_o.Randomize();

            this.activationFunc      = activationFunc;
            this.derOfActFunc = derOfActivationFunc;
		}

        // Copy Constructor
		public FullyConLayer(FullyConLayer nn) : base(LayerType.FULLY_CONNECTED)
        {
			this.inputNodes  = nn.inputNodes;
			this.hiddenNodes = nn.hiddenNodes;
			this.outputNodes = nn.outputNodes;

			this.learningRate = nn.learningRate;

			this.weights_ih = new Matrix(nn.weights_ih);
			this.weights_ho = new Matrix(nn.weights_ho);
			this.bias_h     = new Matrix(nn.bias_h);
			this.bias_o     = new Matrix(nn.bias_o);

            this.Input    = nn.Input;
            this.outs_out = nn.outs_out;
            this.out_hid  = nn.out_hid;

            this.activationFunc      = nn.activationFunc;
            this.derOfActFunc = nn.derOfActFunc;
		}

        #endregion

        #region Training Methods

        public override void FeedForward()
        {
            base.FeedForward();
            this.out_hid = this.weights_ih * this.Input;
            this.out_hid += this.bias_h;
            this.out_hid.Map(activationFunc);

            // Generating the output's output.
            this.outs_out = this.weights_ho * this.out_hid;
            this.outs_out += this.bias_o;
            this.outs_out.Map(activationFunc);

            this.OutputLayer.Input = this.outs_out;
        }
        public override void Backpropagation()
        {
            base.Backpropagation();

        }
        private Matrix Backpropagation(Matrix target)
        {
            // Backpropagation Process
			Matrix neto_d_E = Matrix.Multiply(outs_out - target, Matrix.Map(outs_out, DerSigmoid));

            Matrix wo_d_neto = Matrix.Map(out_hid, DerNetFunc);

            Matrix wo_d_E = neto_d_E * Matrix.Transpose(wo_d_neto);

            Matrix outh_d_neto = Matrix.Map(weights_ho, DerNetFunc);

			weights_ho = weights_ho - (learningRate * wo_d_E);


			Matrix outh_d_E = Matrix.Transpose(outh_d_neto) * neto_d_E;

			Matrix neth_d_outh = Matrix.Map(out_hid, derOfActFunc);

			Matrix neth_d_E = Matrix.Multiply(outh_d_E, neth_d_outh);

			Matrix wh_d_neth = Matrix.Map(Input, DerNetFunc);

			Matrix wh_d_E = wh_d_neth * Matrix.Transpose(neth_d_E);

            Matrix in_d_neth = Matrix.Map(weights_ih, DerNetFunc);

            Matrix in_d_E = Matrix.Transpose(in_d_neth) * neth_d_E;

            weights_ih = weights_ih - (learningRate * Matrix.Transpose(wh_d_E));

            return in_d_E;
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
			outputError = Matrix.Multiply(outputError, outputError) / 2.0;

			double error = 0.0;
			for(int i = 0; i < outputError.data.GetLength(0); i++)
				error += outputError.data[i, 0];

			return error;
		}

        #endregion

        #region Activation Funcs and Derivatives

        public static double Tanh(double x)
        {
            return 2f / (1f + Math.Exp(-2f * x)) - 1f;
        }

        public static double DerTanh(double x)
        {
            double tanh = Tanh(x);

            return 1f - tanh * tanh;
        }

        public static double Sigmoid(double x)
		{
			return 1.0 / (1.0 + Math.Exp(-x));
		}

		public static double DerSigmoid(double x)
		{
			return x * (1.0 - x);
		}

		public static double DerNetFunc(double x)
		{
			return x;
		}

        #endregion
    }
}

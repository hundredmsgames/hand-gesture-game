﻿using System;
using MatrixLib;
using System.Linq;

namespace ConvNeuralNetwork
{
    static class ActivationFunctions
    {
        public static void GetActivationFuncs(
            ActivationType activationType,
            out Func<Matrix, Matrix> act,
            out Func<Matrix, Matrix> der
        )
        {
            switch (activationType)
            {
                case ActivationType.RELU:
                    act = ReLu;
                    der = DerOfReLu;
                    break;
                case ActivationType.SIGMOID:
                    act = Sigmoid;
                    der = DerOfSigmoid;
                    break;
                case ActivationType.SOFTMAX:
                    act = Softmax;
                    der = DerOfSoftmax;
                    break;
                case ActivationType.TANH:
                    act = Tanh;
                    der = DerTanh;
                    break;
                default:
                    //Throw exception for unrecognizeable method
                    act = der = null;
                    break;
            }
        }

        /// <summary>
        /// Rectified Linear Units: Max(x, 0)
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Matrix ReLu(Matrix m)
        {
            return Matrix.Map(m, x => Math.Max(x, 0));
        }

        /// <summary>
        /// Derivative of ReLu
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Matrix DerOfReLu(Matrix m)
        {
            return Matrix.Map(m, x => (x > 0) ? 1f : 0f);
        }

        public static Matrix Tanh(Matrix m)
        {
            return 2f / (1f + Matrix.Exp(-2f * m)) - 1f;
        }

        public static Matrix DerTanh(Matrix m)
        {
            Matrix tanh = Tanh(m);

            return 1f - Matrix.Multiply(tanh, tanh);
        }

        public static Matrix Sigmoid(Matrix m)
        {
            return 1f / (1f + Matrix.Exp(-m));
        }

        public static Matrix DerOfSigmoid(Matrix m)
        {
            return Matrix.Multiply(m, (1f - m));
        }

        public static Matrix Softmax(Matrix m)
        {
            //find the max of the array
            double max = m.data.OfType<double>().Max();
            //subtract matrix from the max value
            Matrix belowZero = Matrix.Subtract(m, max);

            //find the exp of the array
            Matrix expMatrix = Matrix.Exp(belowZero);
            //make a query for using linq
            var query = expMatrix.data.OfType<double>();

            //i=>i is a lamda expression for selecting value we are selecing the current item
            double expMatrixSum = query.Sum<double>(i => i);

            Matrix softmax = new Matrix(m.rows, m.cols);

            for (int r = 0; r < softmax.rows; r++)
            {
                for (int c = 0; c < softmax.cols; c++)
                {
                    softmax[r, c] = expMatrix[r, c] / expMatrixSum;
                }
            }

            return softmax;
        }

        public static Matrix DerOfSoftmax(Matrix x)
        {
            Matrix der_E_Input = new Matrix(x.rows, x.cols);

            for (int i = 0; i < x.rows; i++)
            {
                for (int j = 0; j < x.rows; j++)
                {
                    der_E_Input[i, 0] += x[i, 0] * ( (i == j ? 1 : 0) - x[j, 0]);
                }
            }

            return der_E_Input;
        }
    }
}
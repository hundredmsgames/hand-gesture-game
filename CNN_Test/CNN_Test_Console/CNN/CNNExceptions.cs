﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConvNeuralNetwork
{
    class WrongLayerException : Exception
    {
        public WrongLayerException()
        {

        }

        public WrongLayerException(string message) : base(message)
        {
            Console.WriteLine(message);
        }

        public WrongLayerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WrongLayerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

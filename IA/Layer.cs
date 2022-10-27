using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IA
{
    public class Layer
    {
        public double[] b;
        public double[][] w;
        public ActivationFunction activationFunc;
        public enum ActivationFunction
        {
            sigm,
            relu
        }
        public Layer(int numberConnections, int numberNeuronas, ActivationFunction activationFunc)
        {
            Random random = new Random();
            this.b = new double[numberNeuronas];
            this.w = new double[numberConnections][];
            for (int j = 0; j < numberConnections; j++)
            {
                this.w[j] = new double[numberNeuronas];
                for (int i = 0; i < numberNeuronas; i++)
                {
                    this.b[i] = random.NextDouble() * 2 - 1;
                    this.w[j][i] = random.NextDouble() * 2 - 1;
                }
            }
            this.activationFunc = activationFunc;
        }

        public double funcSigm(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        public double funcSigmDerivada(double x)
        {
            return x * (1 - x);
        }

        public double funcRelu(double x)
        {
            return x > 0 ? x : 0;
        }

        internal double funcActivacion(double v)
        {
            switch (activationFunc)
            {
                case ActivationFunction.sigm:
                    return funcSigm(v);
                case ActivationFunction.relu:
                    return funcRelu(v);
                default:
                    return funcSigm(v);
            }
        }

        internal double funcActivacionDerivada(double v)
        {
            switch (activationFunc)
            {
                case ActivationFunction.sigm:
                    return funcSigmDerivada(v);
                case ActivationFunction.relu:
                    return funcRelu(v);
                default:
                    return funcSigmDerivada(v);
            }
        }
    }
}

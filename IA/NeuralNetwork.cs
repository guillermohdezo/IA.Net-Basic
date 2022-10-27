using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static IA.Layer;

namespace IA
{
    public class NeuralNetwork
    {
        public List<Layer> layersList = new List<Layer>();

        public NeuralNetwork()
        {

        }
        public NeuralNetwork(int[] topology, ActivationFunction activationFunction)
        {
            for (int i = 0; i < topology.Length - 1; i++)
            {
                layersList.Add(new Layer(topology[i], topology[i + 1], activationFunction));
            }
        }

        public (double[][], double[][]) Train(double[][] X, double[] Y, double lr, bool train = false)
        {
            List<(double[][], double[][])> resultado = new List<(double[][], double[][])>();
            resultado.Add((null, X));
            //Forward Pass
            foreach (var layer in layersList)
            {
                //Sumatoria ponderada
                double[][] z = multiplicarMatriz(resultado.Last().Item2, layer.w);
                for (int i = 0; i < z.Length; i++)
                {
                    for (int j = 0; j < z[i].Length; j++)
                    {
                        z[i][j] += layer.b[j];
                    }
                }
                double[][] a = new double[resultado.Last().Item2.Length][];
                //aplicacion de funcion de activacion
                for (int i = 0; i < z.Length; i++)
                {
                    a[i] = new double[z[i].Length];
                    for (int j = 0; j < z[i].Length; j++)
                    {
                        a[i][j] = layer.funcActivacion(z[i][j]);
                    }
                }
                resultado.Add((z, a));
            }

            var propuesta = resultado.Last().Item2;
            double[] propuestaArray = new double[propuesta.Length];
            for (int i = 0; i < propuesta.Length; i++)
            {
                propuestaArray[i] = (double)propuesta[i][0];
            }

            //Backward pass
            if (train)
            {
                var deltasList = new List<double[][]>();
                double[][] wBan = null;
                for (int i = layersList.Count()-1; i >= 0; i--)
                {
                    var z = resultado[i+1].Item1;
                    var a = resultado[i+1].Item2;
                    if (i == layersList.Count()-1)
                    {
                        var derivada = fuctionCostDerivada(propuestaArray, Y);
                        var delta = new double[propuestaArray.Length][];
                        for (int j = 0; j < propuestaArray.Length; j++)
                        {
                            delta[j] = new double[] { layersList[i].funcActivacionDerivada(propuestaArray[j]) * derivada[j] };
                        }

                        deltasList.Insert(0, delta);
                    }
                    else
                    {
                        var wTranspueta = transpuesta(wBan);
                        var delta = multiplicarMatriz(deltasList[0], wTranspueta);
                        for (int j = 0; j < delta.Length; j++)
                        {
                            for (int l = 0; l < delta[j].Length; l++)
                            {
                                delta[j][l] = delta[j][l] * layersList[i].funcActivacionDerivada(a[j][l]);
                            }
                        }
                        deltasList.Insert(0, delta);
                    }
                    wBan = new double[layersList[i].w.Length][];
                    for (int j = 0; j < layersList[i].w.Length; j++)
                    {
                        wBan[j] = new double[layersList[i].w[j].Length];
                        for (int l = 0; l < layersList[i].w[j].Length; l++)
                        {
                            wBan[j][l] = layersList[i].w[j][l];
                        }
                    }

                    //Modificar valores de B
                    double[] media = new double[deltasList[0][0].Length];
                    for (int j = 0; j < deltasList[0][0].Length; j++)
                    {
                        media[j] = deltasList[0].Sum(e => e[j]) / deltasList[0].Length;
                    }
                    //media = deltasList[0].Sum(e => e[0]) / deltasList[0].Length;
                    for (int j = 0; j < layersList[i].b.Length; j++)
                    {
                        layersList[i].b[j] -= media[j] * lr;
                    }
                    //Modificar valores de W
                    var aAnteriorTraspuesta = transpuesta(resultado[i].Item2);
                    var deltasResum = multiplicarMatriz(aAnteriorTraspuesta, deltasList[0]);
                    for (int j = 0; j < layersList[i].w.Length; j++)
                    {
                        for (int l = 0; l < layersList[i].w[j].Length; l++)
                        {
                            layersList[i].w[j][l] -= deltasResum[j][l] * lr;
                        }
                    }
                }
            }
            //if (!train) MessageBox.Show(fuctionCost(propuestaArray, Y).ToString());
            return resultado.Last();
        }

        public double[][] transpuesta(double[][] array)
        {
            double[][] result = new double[array[0].Length][];
            for (int i = 0; i < array[0].Length; i++)
            {
                result[i] = new double[array.Length];
                for (int j = 0; j < array.Length; j++)
                {
                    result[i][j] = array[j][i];
                }
            }
            return result;
        }

        public double[][] multiplicarMatriz(double[][] X, double[][] W)
        {
            double[][] z = new double[X.Length][];
            for (int i = 0; i < X.Length; i++)
            {
                z[i] = new double[W[0].Length];
                for (int j = 0; j < X[i].Length; j++)
                {
                    for (int a = 0; a < W[0].Length; a++)
                    {
                        z[i][a] = X[i][j] * W[j][a] + z[i][a];
                    }
                }
            }
            return z;
        }

        public double fuctionCost(double[] Yp, double[] Yr)
        {
            double[] result = new double[Yp.Length];
            for (int i = 0; i < Yp.Length; i++)
            {
                result[i] = Math.Pow(Yp[i] - Yr[i], 2);
            }
            return result.Average();
        }

        public double[] fuctionCostDerivada(double[] Yp, double[] Yr)
        {
            double[] result = new double[Yp.Length];
            for (int i = 0; i < Yp.Length; i++)
            {
                result[i] = Yp[i] - Yr[i];
            }
            return result;
        }
    }
}

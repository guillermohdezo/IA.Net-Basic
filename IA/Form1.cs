using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace IA
{
    public partial class Form1 : Form
    {
        public (double[][], double[]) groupedArrayList;
        public double[][] arrayTest;
        NeuralNetwork neuralNetwork;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Serie roja
            double[][] seriesDataRojas = getDataSource(10, .2);
            Series serieRed = new Series();
            serieRed.Name = "Rojos";
            serieRed.Color = Color.Red;
            serieRed.ChartType = SeriesChartType.Point;

            foreach (var serie in seriesDataRojas)
            {
                serieRed.Points.AddXY(serie[0], serie[1]);
            }

            ChartTable.Series.Add(serieRed);

            //Serie Azul
            double[][] serieDataAzul = getDataSource(20, .2);
            Series serieAzul = new Series();
            serieAzul.Name = "Azul";
            serieAzul.Color = Color.Blue;
            serieAzul.ChartType = SeriesChartType.Point;

            foreach (var serie in serieDataAzul)
            {
                serieAzul.Points.AddXY(serie[0], serie[1]);
            }

            ChartTable.Series.Add(serieAzul);

            serieAzul = new Series();
            serieAzul.Name = "Azul";
            serieAzul.Color = Color.Blue;
            serieAzul.ChartType = SeriesChartType.Line;

            chart1.Series.Add(serieAzul);

            var listArray = new List<double[][]>();
            listArray.Add(serieDataAzul);
            listArray.Add(seriesDataRojas);
            this.groupedArrayList = groupArray(listArray);
            var max = 25;
            var min = -25;
            var dimArray = (max - min);
            arrayTest = new double[(dimArray /2) * (dimArray / 2)][];
            var index = 0;
            for (int i = 0; i < dimArray;i += 2)
            {
                for (int j = 0; j < dimArray; j += 2)
                {
                    arrayTest[index] = new double[2] { i + min, j + min };
                    index++;
                }
            }

            mapNewSerie(arrayTest);
        }

        public void ThreadProc()
        {
            //NN Create
            var topology = new int[] { 2, 4, 8, 4, 1 };
            this.neuralNetwork = new NeuralNetwork(topology, Layer.ActivationFunction.sigm);
            for (int i = 0; i < 50000; i++)
            {
                var resultTrain = neuralNetwork.Train(this.groupedArrayList.Item1, this.groupedArrayList.Item2, .0009, true);
                var propuesta = resultTrain.Item2;
                double[] propuestaArray = new double[propuesta.Length];
                for (int j = 0; j < propuesta.Length; j++)
                {
                    propuestaArray[j] = propuesta[j][0];
                }
                var error = neuralNetwork.fuctionCost(propuestaArray, this.groupedArrayList.Item2);
                if (this.chart1.InvokeRequired)
                {
                    this.chart1.BeginInvoke((MethodInvoker)delegate () {
                        this.chart1.Series[0].Points.AddXY(i, error);
                    });
                }
                //var result = neuralNetwork.Train(new double[][] { new double[] { 4, 5 } }, new double[] { 0 }, 0.05);
                //var result2 = neuralNetwork.Train(new double[][] { new double[] { 15, 15 } }, new double[] { 0 }, 0.05);
                if (i % 500 == 0)
                {
                    mapNewSerie(arrayTest);
                    Thread.Sleep(100);
                }
            }
        }

        public double[][] getDataSource(int radio, double dis)
        {
            Random random = new Random();
            double[][] result = new double[(radio * 16) + 4][];
            int ban = 0;
            for (double i = -radio; i <= radio; i = i + 0.5)
            {
                double x = Math.Sqrt(Math.Pow(radio, 2) - Math.Pow(i, 2));
                var disX = random.NextDouble() * 2 - 1;
                var disY = random.NextDouble() * 2 - 1;
                result[(ban * 4)] = new double[2] { x + disX, i + disY };
                disX = random.NextDouble() * 2 - 1;
                disY = random.NextDouble() * 2 - 1;
                result[(ban * 4) + 1] = new double[2] { -x + disX, i + disY };
                disX = random.NextDouble() * 2 - 1;
                disY = random.NextDouble() * 2 - 1;
                result[(ban * 4) + 2] = new double[2] { i + disX, x + disY };
                disX = random.NextDouble() * 2 - 1;
                disY = random.NextDouble() * 2 - 1;
                result[(ban * 4) + 3] = new double[2] { i + disX, -x + disY };
                ban++;
            }
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Series serieRed = new Series();
            serieRed.Name = textBoxX.Text + textBoxY.Text;
            serieRed.ChartType = SeriesChartType.Point;
            serieRed.Points.AddXY(Int32.Parse(textBoxX.Text), Int32.Parse(textBoxY.Text));
            var resultTrain = neuralNetwork.Train(new double[][] { new double[] { Int32.Parse(textBoxX.Text), Int32.Parse(textBoxY.Text) } }, this.groupedArrayList.Item2, .0008, false);
            double resultFinal = resultTrain.Item2[0][0];
            //Azul = 0, Rojo = 1
            serieRed.Color = getColorRB(resultFinal);
            
            ChartTable.Series.Add(serieRed);
        }

        private (double[][], double[]) groupArray(List<double[][]> listArray)
        {
            var itemSum = listArray.Sum(e => e.Length);
            double[][] resultSum = new double[itemSum][];
            double[] resultGoup = new double[itemSum];
            int indexSum = 0;
            foreach (var item in listArray)
            {
                var index = listArray.IndexOf(item);
                for (int i = 0; i < item.Length; i++)
                {
                    resultSum[indexSum] = item[i];
                    resultGoup[indexSum] = index;
                    indexSum++;
                }
            }
            
            return (resultSum, resultGoup);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            var percent = trackBar1.Value / 100d;
            
            this.BackColor = getColorRB(percent);
        }

        public Color getColorRB(double percent)
        {
            double red = 0;
            double blue = 0;
            double green = 0;
            if (percent < .5)
            {
                blue = 255;
                red = ((percent * 2d)) * 255;
                green = ((percent * 2d)) * 255;
            }
            if (percent > .5)
            {
                red = 255;
                blue = 255 - ((percent - .5) * 2d) * 255;
                green = 255 - ((percent - .5) * 2d) * 255;
            }
            if (percent == .5)
            {
                red = 255;
                blue = 255;
                green = 255;
            }
            return Color.FromArgb(255, (byte)red, (byte)green, (byte)blue);
        }

        public void mapNewSerie(double[][] newData)
        {       
            if (this.chart1.InvokeRequired)
            {
                var resultTrain = neuralNetwork.Train(newData, null, 0, false);
                this.ChartTable.BeginInvoke((MethodInvoker)delegate () {
                    var listSeriesDelete = this.ChartTable.Series.Where(e => e.Name.Contains("test"));
                    var arraySeries = listSeriesDelete.ToArray();
                    for (int i = 0; i < arraySeries.Length; i++)
                    {
                        this.ChartTable.Series.Remove(arraySeries[i]);
                    }
                    var resultFinal = resultTrain.Item2;
                    for (int i = 0; i < newData.Length; i++)
                    {
                        Series seriesTestRed = new Series();
                        seriesTestRed.Name = "test" + newData[i][0] + "X" + newData[i][1] + "Y";
                        seriesTestRed.Color = getColorRB(resultFinal[i][0]);
                        seriesTestRed.ChartType = SeriesChartType.Point;
                        seriesTestRed.MarkerSize = 10;

                        ChartTable.Series.Add(seriesTestRed);
                        ChartTable.Series[i + 2].Points.AddXY(newData[i][0], newData[i][1]);
                    }
                });
            }
            else
            {
                var listSeriesDelete = this.ChartTable.Series.Where(e => e.Name.Contains("test"));
                var arraySeries = listSeriesDelete.ToArray();
                for (int i = 0; i < arraySeries.Length; i++)
                {
                    this.ChartTable.Series.Remove(arraySeries[i]);
                }
                //var resultFinal = resultTrain.Item2;
                for (int i = 0; i < newData.Length; i++)
                {
                    Series seriesTestRed = new Series();
                    seriesTestRed.Name = "test" + newData[i][0] + "X" + newData[i][1] + "Y";
                    seriesTestRed.Color = Color.Green;//getColorRB(resultFinal[i][0]);
                    seriesTestRed.ChartType = SeriesChartType.Point;
                    seriesTestRed.MarkerSize = 10;
                    

                    ChartTable.Series.Add(seriesTestRed);
                    ChartTable.Series[i + 2].Points.AddXY(newData[i][0], newData[i][1]);
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(ThreadProc));
            thread.Start();
        }
    }
}

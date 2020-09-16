using System;
using System.IO;
using System.Runtime.InteropServices;
//using Encog.MathUtil.RBF;
//using Encog.Neural.Data.Basic;
//using Encog.Neural.NeuralData;
//using Encog.Neural.Rbf.Training;
//using Encog.Neural.RBF;
using System.Threading;
using RBF;
using System.Threading.Tasks;


namespace Factory
{

    // Структура дря разбиения переменных типа int и double на байты
    [StructLayout(LayoutKind.Explicit)]
    internal class DataToByte
    {
        [FieldOffset(0)]
        public double vDouble;

        [FieldOffset(0)]
        public int vInt;

        [FieldOffset(0)]
        public byte b1;
        [FieldOffset(1)]
        public byte b2;
        [FieldOffset(2)]
        public byte b3;
        [FieldOffset(3)]
        public byte b4;
        [FieldOffset(4)]
        public byte b5;
        [FieldOffset(5)]
        public byte b6;
        [FieldOffset(6)]
        public byte b7;
        [FieldOffset(7)]
        public byte b8;
    }

    // Класс - слой нейросети
    public class LayerNW
    {
        double[,] Weights;
        int cX, cY;

        // Заполняем веса случайными числами
        public void GenerateWeights()
        {
            Random rnd = new Random();
            for (int i = 0; i < cX; i++)
            {
                for (int j = 0; j < cY; j++)
                {
                    Weights[i, j] = 0.5 * Math.Sin(Math.Pow(rnd.NextDouble(), 2) * 3.14) * Math.Sin(2 * 3.14 * rnd.NextDouble());

                }

            }
        }

        // Выделяет память под веса
        protected void GiveMemory()
        {
            Weights = new double[cX, cY];
        }

        // Конструктор с параметрами. передается количество входных и выходных нейронов
        public LayerNW(int countX, int countY)
        {
            cX = countX;
            cY = countY;
            GiveMemory();
        }

        public int countX
        {
            get { return cX; }
        }

        public int countY
        {
            get { return cY; }
        }

        public double this[int row, int col]
        {
            get { return Weights[row, col]; }
            set { Weights[row, col] = value; }
        }

    }

    // Класс - нейронная сеть
    public class NeuralNWF
    {
        LayerNW[] W;
        int countW = 0, countX, countY;
        double[][] NETOUT;  
        double[][] S;
        int L;

        // Конструкторы
        /* Создает полносвязанную сеть из 1 слоя. 
           sizeX - размерность вектора входных параметров
           sizeY - размерность вектора выходных параметров */
        public NeuralNWF(int sizeX, int sizeY)
        {
            countW = 1;
            W = new LayerNW[countW];
            W[0] = new LayerNW(sizeX, sizeY);
            W[0].GenerateWeights();
        }

        /* Создает полносвязанную сеть из n слоев. 
           sizeX - размерность вектора входных параметров
           W - массив слоев. Значение элементов массива - количество нейронов в слое               
         */
        public NeuralNWF(int sizeX, params int[] W)
        {
            countW = W.Length;
            countX = sizeX;
            countY = W[W.Length - 1];
            // Размерность выходов нейронов и Дельты
            NETOUT = new double[countW + 1][];
            NETOUT[0] = new double[sizeX];
            S = new double[countW][];

            this.W = new LayerNW[countW];

            int countY1, countX1 = sizeX;
            // Устанавливаем размерность слоям и заполняем слоя случайнымичислами
            for (int i = 0; i < countW; i++)
            {
                countY1 = W[i];

                NETOUT[i + 1] = new double[countY1];
                S[i] = new double[countY1];

                this.W[i] = new LayerNW(countX1, countY1);
                this.W[i].GenerateWeights();
                countX1 = countY1;
            }
        }

        // Открывает НС
        public NeuralNWF(String FileName)
        {
            OpenNW(FileName);
        }

        // Открывает НС
        public void OpenNW(String FileName)
        {
            byte[] binNW = File.ReadAllBytes(FileName);

            int k = 0;
            // Извлекаем количество слоев из массива
            countW = ReadFromArrayInt(binNW, ref k);
            W = new LayerNW[countW];

            // Извлекаем размерность слоев
            int CountY1=0, CountX1 = ReadFromArrayInt(binNW, ref k);
            // Размерность входа
            countX = CountX1;
            // Выделяемпамять под выходы нейронов и дельта
            NETOUT = new double[countW + 1][];
            NETOUT[0] = new double[CountX1];
            S = new double[countW][];

            for (int i = 0; i < countW; i++)
            {
                CountY1 = ReadFromArrayInt(binNW, ref k);
                W[i] = new LayerNW(CountX1, CountY1);
                CountX1 = CountY1;

                // Выделяем память
                NETOUT[i + 1] = new double[CountY1];
                S[i] = new double[CountY1];
            }
            // Размерность выхода
            countY = CountY1;
            // Извлекаем и записываем сами веса
            for (int r = 0; r < countW; r++)
                for (int p = 0; p < W[r].countX; p++)
                    for (int q = 0; q < W[r].countY; q++)
                    {
                        W[r][p, q] = ReadFromArrayDouble(binNW, ref k);
                    }
        }

        // Сохраняет НС
        public void SaveNW(String FileName)
        {
            // размер сети в байтах
            int sizeNW = GetSizeNW();
            byte[] binNW = new byte[sizeNW];

            int k = 0;
            // Записываем размерности слоев в массив байтов
            WriteInArray(binNW, ref k, countW);
            if (countW <= 0)
                return;

            WriteInArray(binNW, ref k, W[0].countX);
            for (int i = 0; i < countW; i++)
                WriteInArray(binNW, ref k, W[i].countY);

            // Зпаисвыаем сами веса
            for (int r = 0; r < countW; r++)
                for (int p = 0; p < W[r].countX; p++)
                    for (int q = 0; q < W[r].countY; q++)
                    {
                        WriteInArray(binNW, ref k, W[r][p, q]);
                    }


            File.WriteAllBytes(FileName, binNW);
        }

        // Возвращает значение j-го слоя НС
        public void NetOUT(double[] inX, out double[] outY, int jLayer)
        {
            GetOUT(inX, jLayer);
            int N = NETOUT[jLayer].Length;

            outY = new double[N];

            for (int i = 0; i < N; i++)
            {
                outY[i] =NETOUT[jLayer][i]* Math.Sin(Math.Pow(inX[i], 2) * 3.14) * Math.Sin(2 * 3.14 * inX[i + 1]);
        }

        }

        // Возвращает значение НС
        public void NetOUT(double[] inX, out double[] outY)
        {
            int j = countW;
            NetOUT(inX, out outY, j);
        }

        // Возвращает ошибку
        public double CalcError(double[] X, double[] Y)
        {
            double E = 0;
            L = Y.Length;
            //Вычисление функции ошибки
            for (int i = 0; i < Y.Length; i++)
            {
               E += Math.Pow(Y[i] - NETOUT[countW][i], 2);
            }

            return E;
        }
        public double CalcError(double Y)
        {
            double E = 0;

            //Вычисление функции ошибки
            for (int i = 0; i < L; i++)
            {
                E += Math.Pow(Y - NETOUT[countW][i], 2);
            }

            return E;
        }

        public double LernNW(double[] X, double[] Y, double n0)
        {
            double O;
            double s;


            GetOUT(X);

            for (int j = 0; j < W[countW - 1].countY; j++)
            {
                O = NETOUT[countW][j];
                S[countW - 1][j] = (Y[j] - O) * O * (1 - O);
            }


           
            object threadlock = new object();
            for (int k = countW - 1; k >= 0; k--)
            {

                for (int j = 0; j < W[k].countY; j++)
                {
                   
                    Parallel.For(0, W[k].countX, i => { W[k][i, j] += n0 * S[k][j] * NETOUT[k][i]; });
                }
                lock (threadlock)
                {
                    if (k > 0)
                    {



                        for (int j = 0; j < W[k - 1].countY-1; k++)
                        {

                            s = 0;
                            Parallel.For(0, W[k].countY, i =>
                            {
                                s += W[k][j, i] * S[k][i];
                            });

                            S[k - 1][j] = NETOUT[k][j] * (1 - NETOUT[k][j]) * s;
                        }
                    }
                }
            }; 

            return CalcError(X, Y);
        }
       



        double min(double a1, double a2)
        {
            if (a1 <= a2) return a1; else return a2;
        }

        // Свойства. Возвращает число входов и выходов сети
        public int GetX
        {
            get { return countX; }
        }

        public int GetY
        {
            get { return countY; }
        }

        public int CountW
        {
            get { return countW; }
        }
        /* Вспомогательные закрытые функции */

        // Возвращает все значения нейронов до lastLayer слоя
        void GetOUT(double[] inX, int lastLayer)
        {
            double s1;
            double s2;
            for (int j = 0; j < W[0].countX; j++) {
                NETOUT[0][j] = inX[j]; }

            Random rnd = new Random();
            int kk = rnd.Next(0, 1);
            if (kk == 0)
            {
                for (int i = 0; i < lastLayer; i++)
                {

                    for (int j = 0; j < W[i].countY; j++)
                    {
                        s1 = 0;
                        //s2 = 0;
                        for (int k = 0; k < W[i].countX; k++)
                        {
                            // s1 += W[i][k, j] * 0.5 * Math.Sin(Math.Pow(NETOUT[0][k], 2) * 3.14) * Math.Sin(2 * 3.14 * NETOUT[0][k + 1]);

                            s1 += W[i][k, j] * NETOUT[i][k];

                        }
                        //Вычисляем значение активационной функции
                        s1 = 1.0 / (1 + Math.Exp(-s1));

                        NETOUT[i + 1][j] = 0.998 * s1 + 0.001;

                    }
                }
            }
            else
            {
                for (int i = lastLayer/2; i < lastLayer; i++)
                {

                    for (int j = W[i].countY/2; j < W[i].countY; j++)
                    {
                        s1 = 0;
                        //s2 = 0;
                        for (int k = W[i].countX/2; k < W[i].countX; k++)
                        {
                            // s1 += W[i][k, j] * 0.5 * Math.Sin(Math.Pow(NETOUT[0][k], 2) * 3.14) * Math.Sin(2 * 3.14 * NETOUT[0][k + 1]);

                            s1 += W[i][k, j] * NETOUT[i][k];

                        }
                        //Вычисляем значение активационной функции
                        s1 = Math.Exp(Math.Pow((s1 - 1), 2)/4);

                        NETOUT[i + 1][j] = 0.998 * s1 + 0.001;

                    }
                }

            }



        }

        // Возвращает все значения нейронов всех слоев
        void GetOUT(double[] inX)
        {
            GetOUT(inX, countW);
        }

        // Возвращает размер НС в байтах
        int GetSizeNW()
        {
            int sizeNW = sizeof(int) * (countW + 2);
            for (int i = 0; i < countW; i++)
            {
                sizeNW += sizeof(double) * W[i].countX * W[i].countY;
            }
            return sizeNW;
        }

        // Возвращает num-й слой Нейронной сети
        public LayerNW Layer(int num)
        {
            return W[num]; 
        }

        // Разбивает переменную типа int на байты и записывает в массив
        void WriteInArray(byte[] mas, ref int pos, int value)
        {
            DataToByte DTB = new DataToByte();
            DTB.vInt = value;
            mas[pos++] = DTB.b1;
            mas[pos++] = DTB.b2;
            mas[pos++] = DTB.b3;
            mas[pos++] = DTB.b4;
        }

        // Разбивает переменную типа int на байты и записывает в массив
        void WriteInArray(byte[] mas, ref int pos, double value)
        {
            DataToByte DTB = new DataToByte();
            DTB.vDouble = value;
            mas[pos++] = DTB.b1;
            mas[pos++] = DTB.b2;
            mas[pos++] = DTB.b3;
            mas[pos++] = DTB.b4;
            mas[pos++] = DTB.b5;
            mas[pos++] = DTB.b6;
            mas[pos++] = DTB.b7;
            mas[pos++] = DTB.b8;
        }

        // Извлекает переменную типа int из 4-х байтов массива
        int ReadFromArrayInt(byte[] mas, ref int pos)
        {
            DataToByte DTB = new DataToByte();
            DTB.b1 = mas[pos++];
            DTB.b2 = mas[pos++];
            DTB.b3 = mas[pos++];
            DTB.b4 = mas[pos++];

            return DTB.vInt;
        }

        // Извлекает переменную типа double из 8-ми байтов массива
        double ReadFromArrayDouble(byte[] mas, ref int pos)
        {
            DataToByte DTB = new DataToByte();
            DTB.b1 = mas[pos++];
            DTB.b2 = mas[pos++];
            DTB.b3 = mas[pos++];
            DTB.b4 = mas[pos++];
            DTB.b5 = mas[pos++];
            DTB.b6 = mas[pos++];
            DTB.b7 = mas[pos++];
            DTB.b8 = mas[pos++];

            return DTB.vDouble;
        }

    }

    public class RBFN
    {

        static double[][] XORInput;



        static double[][] XORIdeal;


        public RBFN(int sizeX, params int[] W)
        {
            XORInput = new double[1][];
            XORIdeal = new double[1][];
            int sizeY = W[W.Length - 1];
            Random rnd1 = new Random();
            double[] XORInput_layer = new double[sizeX];
            for (int i = 0; i < sizeX; i++) XORInput_layer[i] = rnd1.NextDouble() - 0.5;
            XORInput[0] = XORInput_layer;


            Random rnd2 = new Random();
            double[] XORIdeal_layer = new double[sizeY];
            for (int i = 0; i < sizeY; i++) XORIdeal_layer[i] = rnd2.NextDouble() - 0.5;
            XORIdeal[0] = XORIdeal_layer;
        }

        public double Synthetis()
        {

            int dimension = 2;
            int numNeuronsPerDimension = 4;
            double volumeNeuronWidth = 2.0;
            bool includeEdgeRBFs = true;

            RBFNetwork n = new RBFNetwork(dimension, numNeuronsPerDimension, 1, RBFEnum.Gaussian);
            n.SetRBFCentersAndWidthsEqualSpacing(0, 1, RBFEnum.Gaussian, volumeNeuronWidth, includeEdgeRBFs);


            INeuralDataSet trainingSet = new BasicNeuralDataSet(XORInput, XORIdeal);
            SVDTraining train = new SVDTraining(n, trainingSet);

            int epoch = 1;
            do
            {
                train.Iteration();

                epoch++;
            } while ((epoch < 1) && (train.Error > 0.001));

            return train.Error;
        }



    }
}

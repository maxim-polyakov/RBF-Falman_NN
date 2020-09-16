using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VKRProg
{
    using Factory;

    public partial class Form2 : Form
    {
        // Собственно сама нейросеть
        NeuralNWF NET;
        RBFN RBF;
        // Путь к сети
        String path = "";
        // 1 - идет обучение. 0 - нет
        bool run = false;


        public void CreateNW(int SizeX, int[] Layers)
        {
            NET = new NeuralNWF(SizeX, Layers);
            RBF = new RBFN(SizeX, Layers);
            path = "";
            txtLogs.AppendText("Создана сеть:\r\n");
            txtLogs.AppendText("Число входов: " + Convert.ToString(SizeX) + "\r\n");
            txtLogs.AppendText("Число выходов: " + Convert.ToString(Layers[Layers.Count() - 1]) + "\r\n");
            txtLogs.AppendText("Число скрытых слоёв: " + Convert.ToString(Layers.Count() - 1) + "\r\n");

            txtLogs.AppendText("Создана сеть RBF:\r\n");
            txtLogs.AppendText("Число входов: " + Convert.ToString(SizeX) + "\r\n");
            txtLogs.AppendText("Число выходов: " + Convert.ToString(Layers[Layers.Count() - 1]) + "\r\n");
            

            for (int i = 0; i < Layers.Count() - 1; i++)
            {
                txtLogs.AppendText("Нейронов в " + Convert.ToString(i + 1) + " скрытом слое: "
                                                    + Convert.ToString(Layers[i]) + "\r\n");

            }
        }

        public Form2()
        {
            InitializeComponent();
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            path = openFileDialog1.FileName;
            if (File.Exists(path))
            {
                try
                {
                    NET = new NeuralNWF(path);
                  
                }
                finally
                { }

                txtLogs.AppendText("Загружена сеть:\r\n" + path + "\r\n");

                txtLogs.AppendText("Число входов: " + Convert.ToString(NET.GetX) + "\r\n");
                txtLogs.AppendText("Число выходов: " + Convert.ToString(NET.GetY) + "\r\n");
                txtLogs.AppendText("Число скрытых слоёв: " + Convert.ToString(NET.CountW - 1) + "\r\n");

                for (int i = 0; i < NET.CountW - 1; i++)
                {
                    txtLogs.AppendText("Нейронов в " + Convert.ToString(i + 1) + " скрытом слое: "
                                                        + Convert.ToString(NET.Layer(i).countY) + "\r\n");
                }
            }
            else
            {
                if (path != "")
                {
                    txtLogs.AppendText("Ошибка! Файл не существует!\r\n" + path + "\r\n");
                    path = "";
                }
            }
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NET == null)
            {
                txtLogs.AppendText("Ошибка! Сеть не не создана!\r\n");
            }
            else
            {
                saveFileDialog1.ShowDialog();
                path = saveFileDialog1.FileName;
                try
                {
                    NET.SaveNW(path);
                }
                finally
                { }
                txtLogs.AppendText("Сеть сохранена:\r\n" + path + "\r\n");
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NET == null)
            {
                txtLogs.AppendText("Ошибка! Сеть не не создана!\r\n");
            }
            else
            {
                if (path == "")
                {
                    saveFileDialog1.ShowDialog();
                    path = saveFileDialog1.FileName;
                }
                try
                {
                    NET.SaveNW(path);
                }
                finally
                { }
                txtLogs.AppendText("Сеть сохранена:\r\n" + path + "\r\n");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (NET == null)
            {
                txtLogs.AppendText("Не создана сеть!\r\n");
                return;
            }
            txtLogs.AppendText("Запущен процесс обучения\r\n");

            String strFileIn, strFileOut, strFile;

            // Очищаем список в обучающей выборке
            txtLernFiles.Text = "";
            FileInfo[] fInfo = new DirectoryInfo(txtDir.Text).GetFiles("*.in.txt");
            foreach (FileInfo f in fInfo)
            {
                // Загружаем список файлов
                strFileIn = f.FullName;
                strFile = strFileIn.Remove(strFileIn.Length - 7);
                strFileOut = strFile + ".out.txt";

                if (File.Exists(strFileOut) && File.Exists(strFileIn))
                {
                    txtLernFiles.AppendText(strFile + "\r\n");
                }
            }
            if (txtLernFiles.Lines.Count() > 0)
                txtLogs.AppendText("Загружена обучающая выборка\r\n");
            else
            {
                txtLogs.AppendText("Обучающая выборка не найдена\r\n");
                return;
            }

            int currPos = 0;
            double kErr = 1E256;
            double kErrNorm = Convert.ToDouble(txtKErr.Text);
            double kLern = Convert.ToDouble(txtKLern.Text);

            double[] X = new double[NET.GetX];
            double[] Y = new double[NET.GetY];
            String[] currFile1;
            String[] currFile2;
            btnLern.Enabled = false;
            btnStop.Enabled = true;
            run = true;
            while (kErr > kErrNorm)
            {
                kErr = 0;
                for (currPos = 0; currPos < txtLernFiles.Lines.Count() - 1; currPos++)
                {
                    // Загружаем обучающую пару
                    
                        // Загружаем текущий входной файл
                        currFile1 = File.ReadAllLines(txtLernFiles.Lines[currPos] + ".in.txt");
                         // Загружаем текущий выходной файл
                        currFile2 = File.ReadAllLines(txtLernFiles.Lines[currPos] + ".out.txt");
                    for (int k = 0; k < currFile2.Length; k++)
                    {
                        for (int i = 0; i < NET.GetX; i++)
                            X[i] = Convert.ToDouble(currFile1[k]);
                  

                        

                        for (int i = 0; i < NET.GetY; i++)
                            Y[i] = Convert.ToDouble(currFile2[k]);
                     

                   
                                      }
                    // Обучаем текущую пару
                    kErr += NET.LernNW(X, Y, kLern);
                    Application.DoEvents();

                    if (!run)
                        return;
                }
                txtLogs.AppendText("Текущая ошибка: " + Convert.ToString(kErr) + "\r\n");
            }
           
            txtLogs.AppendText("Обучение завершено!"+RBF.Synthetis()+"\r\n");
            txtLogs.AppendText("\r\n");

            txtLogs.AppendText("Обучение завершено!\r\n");

            btnLern.Enabled = true;
            btnStop.Enabled = false;
            run = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnLern.Enabled = true;
            btnStop.Enabled = false;
            run = false;
            txtLogs.AppendText("Обучение остановлено пользователем\r\n");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
                if (NET == null)
                {
                    txtLogs.AppendText("Не создана сеть!\r\n");
                    return;
                }
                openFileDialog2.ShowDialog();

                String strFile = openFileDialog2.FileName;

                if (!File.Exists(strFile))
                    return;

                double[] X = new double[NET.GetX];
                double[] Y;
                String[] currFile;

                textBox1.Text = "";
                textBox2.Text = "";

                // Загружаем текущий входной файл
                currFile = File.ReadAllLines(strFile);
                textBox1.Lines = currFile;
                txtLogs.AppendText("Загружен файл:\r\n" + Convert.ToString(strFile) + "\r\n");
            for (int k = 0; k < currFile.Length; k++)
            {
                for (int i = 0; i < NET.GetX; i++)
                {
                    X[i] = Convert.ToDouble(currFile[k]);
                }

                NET.NetOUT(X, out Y);

                for (int i = 0; i < NET.GetY; i++)
                {
                    textBox2.AppendText(string.Format("{0:F4}\r\n", Y[i]));// *Math.Sin(Math.Pow(X[i] ,2) * 3.14) * Math.Sin(2 * 3.14 * X[i+1])));
                    //textBox2.AppendText(Convert.ToString(Y[i]) + "\r\n");
                }
            }
            
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            txtDir.Text = folderBrowserDialog1.SelectedPath;
        }

        private void нейроннуюСетьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();

            if (f.ShowDialog() == DialogResult.OK)
            {
                CreateNW(f.getSizeX, f.getLayers);
            }
        }

        private void обучающуюВыборкуToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int it = 0; it < 100; it++)
            {
                if (NET == null)
                {
                    txtLogs.AppendText("Не создана сеть!\r\n");
                    return;
                }

                double[] X = new double[NET.GetX];
                double[] Y;

                textBox2.Text = "";

                // Загружаем текущий входной файл
                for (int k = 0; k < textBox1.Lines.Length; k++)
                {
                    for (int i = 0; i < NET.GetX; i++)
                    {
                        X[i] = Convert.ToDouble(textBox1.Lines[k]);
                    }

                    NET.NetOUT(X, out Y);

                    for (int i = 0; i < NET.GetY; i++)
                    {
                        textBox2.AppendText(string.Format("{0:F4}\r\n", Y[i]));// * Math.Sin(Math.Pow(X[i], 2) * 3.14) * Math.Sin(2 * 3.14 * X[i + 1])));
                        //textBox2.AppendText(Convert.ToString(Y[i]) + "\r\n");
                    }
                }
            }

        }

        private void txtDir_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

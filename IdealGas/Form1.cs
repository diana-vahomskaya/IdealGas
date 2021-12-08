using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdealGas
{

    public partial class Form1 : Form
    {
        #region Поля класса, константы и логика инициализации

        // Равновесное расстояние между центрами атомов (м) (r0 p.40).  
        public const double a = 0.382e-9;

        // Модуль потенц. энергии взаимодействия между атомами при равновесии (Дж) (p.40). 
        public const double D = 1.648e-21; // 0.0103 эВ переведены в Дж

        // Ширина / высота квадрата (ед.) (p.15).
        public int SquareSize;
        int iterator;

        // Ширина / высота исследуемой ячейки (м)(p.15 Lx*Ly=L=30a).
        public double CellSize;

        // Радиус одной частицы (м) (выведено на лекции, как равновесное расстояние между частицами это диаметр при близком расположении частиц, а радиус половина).
        public const double ParticleRadius = 0.191e-9;

        // Масса частицы (кг) (взято из интернета).
        public const double Mass = 39.95 * 1.66054e-27; // 39.948 а.е.м. переведены в кг

        // Общее количество частиц.
        public int ParticularNumber;

        /// Равновесное расстояние между частицами при начальной генерации (м).
        public double marginInit;

        // Максимальная начальная скорость частицы при начальной генерации (м/с).
        public double V0MaxInit;

        // Шаг по времени (с).
        public double TimeDelta;

        // заданная температура системы для второго пункта
        public double Temp_CONST;

        // Количество шагов по времени.
        public int timeCounts;

        // Радиус обрезания R1 (м) (p.41).
        public double r1;

        // Радиус обрезания R2 (м) (p.41).
        public double r2;
        public double xflux_form, yflux_form;
        public double CanvasZoom = 1E10;
        double ri_Fi = 0;
        public const double k = 1.38 * 1e-23; //Дж/К

        // переменные для расчета кинетической, потенциальной и полной энергий, и температуры системы 
        double KineticEn, PotentialEn, FullEn, TempSystem;
        double kinetic, potential, energy, temperature;
        double pressure_full = 0;
        double pviral_full = 0;
        bool animation1, stop_animation1;
        double viral = 0;
        double potentialSum = 0;
        double xflux, yflux;
        int helper;
        public double[] massive_beta;
        public Random random;
        #endregion
        double[] masvel;

        private Physics _physical;

        private ObservableCollection<Particle> _particles;
        public ObservableCollection<Particle> Particles
        {
            get => _particles;

            set
            {
                _particles = value;
            }
        }

        public Form1()
        {
            InitializeComponent();
            
            stop_animation1 = false;
            animation1 = true;
            chart1.Series[1].Points.Clear();
            chart2.Series[1].Points.Clear();
        }

        private void Generate_Click(object sender, EventArgs e)
        {

            ParticularNumber = Convert.ToInt32(Number.Text);
            V0MaxInit = Convert.ToDouble(VMAX.Text);
            SquareSize = Convert.ToInt32(textBox3.Text);
            r1 = Convert.ToDouble(Rad1.Text);
            r2 = Convert.ToDouble(Rad2.Text);

            Temp_CONST = Convert.ToDouble(textBox1.Text);

            iterator = 0;
            KineticEn = 0;
            PotentialEn = 0;
            FullEn = 0;
            iterator = 0;
            TempSystem = 0;
            kinetic = 0;
            potential = 0;
            energy = 0;
            temperature = 0;
            random = new Random();
            CellSize = SquareSize * a;
            _particles = new ObservableCollection<Particle>();

            double ro = ParticularNumber / Math.Pow(CellSize, 2);
            textBox2.Text = Convert.ToString(ro);

            Temp_CONST = Convert.ToDouble(textBox1.Text);
            TimeDelta = Convert.ToDouble(DeltaTime.Text);
            label5.Text = Convert.ToString("Кол-во шагов: " + iterator);

            _physical = new Physics();

            _physical.InitAll(ParticularNumber, 0.9, V0MaxInit, TimeDelta, r1, r2, SquareSize);

            _physical.GenerateInitState();
            Particles = _physical.GetParticlesCollection(0);

            chart1.Series[0].Points.Clear();
            chart2.Series[0].Points.Clear();
            chart3.Series[0].Points.Clear();
            chart4.Series[0].Points.Clear();

            masvel = new double[ParticularNumber];

            float sx = (float)(pictureBox1.Width / (CellSize * CanvasZoom));
            float sy = (float)(pictureBox1.Height / (CellSize * CanvasZoom));

            Bitmap bm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.ScaleTransform(sx, sy);
                foreach (var p in Particles)
                {
                    RectangleF rect = new RectangleF((float)((p.X - a) * CanvasZoom), (float)((p.Y - a) * CanvasZoom), (float)((a) * CanvasZoom), (float)((a) * CanvasZoom));
                    LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush(rect, Color.Orange, Color.BlueViolet, LinearGradientMode.Horizontal);
                    gr.FillEllipse(myLinearGradientBrush, (float)(p.X * CanvasZoom), (float)(p.Y * CanvasZoom), (float)(a * CanvasZoom), (float)(a * CanvasZoom));
                }
            }
            pictureBox1.Image = bm;
            pictureBox1.Refresh();

            Start.Text = "Старт";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Generate_Click(sender, e);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                iterator = 0;
                chart1.Series[0].Points.Clear();
                chart2.Series[0].Points.Clear();
                chart3.Series[0].Points.Clear();
                chart4.Series[0].Points.Clear();

                chart1.Titles[0].Text = "Давление";
                chart2.Titles[0].Text = "Давление по т. вириала";
                chart3.Titles[0].Text = "Кинетическая энергия (Дж)";
                chart4.Titles[0].Text = "Температура (К)";
                textBox1.Enabled = true;
                KineticEn = 0;
                PotentialEn = 0;
                FullEn = 0;
                TempSystem = 0;
                kinetic = 0;
                potential = 0;
                energy = 0;
                temperature = 0;
            }
            else
            {
                iterator = 0;
                chart1.Titles[0].Text = "Кинетическая энергия (Дж)";
                chart2.Titles[0].Text = "Потенциальная энергия (Дж)";
                chart3.Titles[0].Text = "Полная энергия (Дж)";
                chart4.Titles[0].Text = "Температура (К)";
                textBox1.Enabled = false;
                KineticEn = 0;
                PotentialEn = 0;
                FullEn = 0;
                TempSystem = 0;
                kinetic = 0;
                potential = 0;
                energy = 0;
                temperature = 0;
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {

            if (animation1 == true)
            {
                stop_animation1 = false;
                timer1.Enabled = true;
                animation1 = false;
                Start.Text = "Остановить";
            }
            else
            {
                timer1.Enabled = false;
                animation1 = true;
                Start.Text = "Продолжить";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Second(sender, e);
            }
            else
            {
                First();
            }
        }

        public void First()
        {

            Particles = _physical.GetParticlesCollection();

            
                
            float sx = (float)(pictureBox1.Width / (CellSize * CanvasZoom));
            float sy = (float)(pictureBox1.Height / (CellSize * CanvasZoom));
            Bitmap bm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.ScaleTransform(sx, sy);
                foreach (var p in Particles)
                {
                    RectangleF rect = new RectangleF((float)((p.X - a) * CanvasZoom), (float)((p.Y - a) * CanvasZoom), (float)((a) * CanvasZoom), (float)((a) * CanvasZoom));
                    LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush(rect, Color.Orange, Color.BlueViolet, LinearGradientMode.Horizontal);
                    gr.FillEllipse(myLinearGradientBrush, (float)(p.X * CanvasZoom), (float)(p.Y * CanvasZoom), (float)(a * CanvasZoom), (float)(a * CanvasZoom));
                }
            }
            pictureBox1.Image = bm;
            pictureBox1.Refresh();

            var KinEnNow = CalсKinetic();
            var PotEnNow = _physical.GetPotential();

            KineticEn += KinEnNow;
            PotentialEn += PotEnNow;
            FullEn += KinEnNow * 1e10 + PotEnNow * 1e10;
            TempSystem += CalcTemperature(KinEnNow);

            if (iterator % 10 == 0)
            {
                const double step = 10.0;
                kinetic = KineticEn / step;
                potential = PotentialEn / step;
                energy = FullEn / step / 1e10;
                temperature = TempSystem / step;

                KineticEn = 0;
                PotentialEn = 0;
                FullEn = 0;
                TempSystem = 0;

                chart1.Series[0].Points.AddXY(iterator, kinetic);
                chart2.Series[0].Points.AddXY(iterator, potential);
                chart3.Series[0].Points.AddXY(iterator, energy);
                chart4.Series[0].Points.AddXY(iterator, temperature);

                kinetic = 0;
                potential = 0;
                energy = 0;
                temperature = 0;

            }
            label5.Text = Convert.ToString("Кол-во шагов: " + iterator);
            iterator++;
        }
        private double CalсKinetic()
        {
            var avgU2 = Particles.Sum(par => par.Ux * par.Ux + par.Uy * par.Uy);
            avgU2 /= ParticularNumber;

            const double mass = 39.948 * 1.66054e-27; // константа массы частицы
            return mass * avgU2 / 2.0;
        }

        private double CalcTemperature(double kinetic)
        {
            return kinetic * 2.0 / 3.0 / k;
        }

        public void Second(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            Particles = _physical.GetParticlesCollection();

            float sx = (float)(pictureBox1.Width / (CellSize * CanvasZoom));
            float sy = (float)(pictureBox1.Height / (CellSize * CanvasZoom));
            Bitmap bm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.ScaleTransform(sx, sy);
                foreach (var p in Particles)
                {
                    RectangleF rect = new RectangleF((float)((p.X - a) * CanvasZoom), (float)((p.Y - a) * CanvasZoom), (float)((a) * CanvasZoom), (float)((a) * CanvasZoom));
                    LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush(rect, Color.Orange, Color.BlueViolet, LinearGradientMode.Horizontal);
                    gr.FillEllipse(myLinearGradientBrush, (float)(p.X * CanvasZoom), (float)(p.Y * CanvasZoom), (float)(a * CanvasZoom), (float)(a * CanvasZoom));
                }
            }
            pictureBox1.Image = bm;
            pictureBox1.Refresh();


            if (iterator <= 2000)
            {
                for (int i = 0; i < Particles.Count; i++)
                {
                    masvel[i] += Particles[i].Ux * Particles[i].Ux + Particles[i].Uy * Particles[i].Uy;
                }

                if (iterator % 10 == 0)
                {
                    double[] massive_beta = new double[ParticularNumber];
                    for (int l = 0; l < Particles.Count; l++)
                    {
                        massive_beta[l] = Math.Sqrt(2 * ParticularNumber * 1.38 * 1e-23 * Temp_CONST / (39.948 * 1.66054e-27 * masvel[l]) / 10);
                        Particles[l].Ux = Particles[l].Ux * massive_beta[l];
                        Particles[l].Uy = Particles[l].Uy * massive_beta[l];
                    }
                      
                    for (int l = 0; l < Particles.Count; l++)
                    {
                        massive_beta[l] = 0;
                        masvel[l] = 0;
                    }
                }
            }
            else
            {
                viral += _physical.GetVirial();
                xflux_form += _physical.GetXflux();
                
                yflux_form += _physical.GetYflux();
                    //viral += _physical.GetVirial();
                   
                if (iterator % 1000 == 0)
                {
                    int stepic = 1000;
                    double presure = (xflux_form * Mass / (2 * Math.Pow(30 * 0.382f * 1e-9, 1)) + yflux_form * Mass / (2 * Math.Pow(30 * 0.382f * 1e-9, 1))) / (TimeDelta * stepic);
                    double pvirial = ParticularNumber  * k * Temp_CONST / Math.Pow(30 * 0.382f * 1e-9, 2) + 0.5 * viral / Math.Pow(30 * 0.382f * 1e-9, 2) / stepic;

                    pressure_full += presure / 6;
                    pviral_full += pvirial / 6;
                    presure = 0;

                    pvirial = 0;

                    viral = 0;
                    xflux_form = 0;
                    yflux_form = 0;
                }
               
            }
                
            if (iterator == 8000)
            {    
                timer1.Enabled = false;
                chart1.Series[1].Points.AddXY(Temp_CONST, pressure_full);
                chart2.Series[1].Points.AddXY(Temp_CONST, pviral_full);
                using (FileStream fs = new FileStream("C:\\Users\\Diana\\source\\repos\\IdealGas\\IdealGas\\N300.txt", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(Temp_CONST + "     " + pviral_full.ToString("f6") + "      " + pressure_full.ToString("f6") + "      ");
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                }
                if (Temp_CONST < 90)
                {
                    Temp_CONST += 5;
                }
                else
                {
                    Temp_CONST += 30;
                }
                
                textBox1.Text = Convert.ToString(Temp_CONST);
                pressure_full = 0;
                pviral_full = 0;
                Generate_Click(sender, e);
                timer1_Tick(sender, e);
            }
            label5.Text = Convert.ToString("Кол-во шагов: " + iterator);
            iterator++;

            
            
        }
    }
}

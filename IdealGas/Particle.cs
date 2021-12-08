using System.ComponentModel;

namespace IdealGas
{
    public class Particle
    {
		

		private double x, y;
		/// <summary>
		/// Координата X центра частицы.
		/// </summary>
		public double X
		{
			get;
			set;
		}
		/// <summary>
		/// Координата Y центра частицы.
		/// </summary>
		public double Y
		{
			get;
			set;
		}
		/// <summary>
		/// Скорость частицы.
		/// </summary>
		public double Ux, Uy;

        public double Fx, Fy;

        /// <summary>
        /// Конструктор: явная инициализация координат и скоростей частицы.
        /// </summary>
        public Particle(double x, double y, double ux, double uy)
        {
            X = x;
            Y = y;
            Ux = ux;
            Uy = uy;

        }
    }
}

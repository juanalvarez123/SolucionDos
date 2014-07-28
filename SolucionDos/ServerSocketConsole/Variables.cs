namespace ServerSocketConsole
{
    /// <summary>
    /// Clase estatica que contiene variables del sistema
    /// </summary>
    /// <author>Juan Sebastian Alvarez Eraso</author>
    /// <date>27 Julio 2014</date>
    public static class Variables
    {

        /// <summary>
        /// Variable estatica que acumula la suma de los numeros
        /// </summary>
        private static int suma = 0;

        public static int Suma
        {
            get { return Variables.suma; }
            set { Variables.suma = value; }
        }

        /// <summary>
        /// Este metodo permite acumular los numeros generados por el generador de numeros
        /// </summary>
        /// <param name="numero">Numero a acumular</param>
        public static void acumular(int numero)
        {
            suma += numero;
        }

    }
}




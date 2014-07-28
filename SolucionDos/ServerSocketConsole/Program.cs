using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerSocketConsole
{
    /// <summary>
    /// Clase inicio de la consola
    /// </summary>
    /// <author>Juan Sebastian Alvarez Eraso</author>
    /// <date>27 Julio 2014</date>
    class Program
    {
        /// <summary>
        /// Variable global para manejar el socket de salida de los datos
        /// </summary>
        static Socket ws2;

        static void Main(string[] args)
        {
            Console.WriteLine("Inicia el servidor suma ...");

            //Se crean 2 variables para manejar los sockets
            //ws1: Socket que recibe los numeros aleatorios
            //ws2: Socket que envia la información a la grafica
            Socket ws1 = new Socket(12345);
            ws2 = new Socket(54321);

            //Se crea un objeto timer que evalua cada segundo el metodo verificarSuma
            using (new Timer(verificarSuma, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)))
            {
                while (true)
                {
                    if (Console.ReadLine() == "quit")
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Este metodo permite tomar el valor de la suma generada por los numeros aleatorios y enviarlos
        /// a los clientes del ws2
        /// </summary>
        /// <param name="state"></param>
        private static void verificarSuma(object state)
        {
            //Se valida si el resultado de la suma es mayor a 100 no se lo puede enviar a la grafica
            //por defecto se setea un 0
            int resultado = Variables.Suma <= 100 ? Variables.Suma : 0;
            Console.WriteLine("Número enviado a la gráfica: " + resultado);

            //Se crea el frame para enviarlo a los clientes
            List<byte> lb = new List<byte>();
            lb.Add(0x81); //El primer byte debe ser 0x81 para que corresponda a una trama informativa
            int size = resultado.ToString().Length;
            lb.Add((byte)size); //El segundo byte corresponde al tamaño del mensaje a enviar
            lb.AddRange(Encoding.UTF8.GetBytes(resultado.ToString())); //Se obtiene os bytes de la suma
            enviarMensajeTodosClientes(lb.ToArray(), size + 2);

            //La suma vuelve a ser 0
            Variables.Suma = 0;
        }

        /// <summary>
        /// Este metodo permite enviar la suma de los numeros a todos los clientes del ws2 conectados 
        /// </summary>
        /// <param name="informacion">Vector de bytes que contiene la información</param>
        /// <param name="tamanio">Tamaño de los datos a enviar</param>
        private static void enviarMensajeTodosClientes(byte[] informacion, int tamanio)
        {
            try
            {
                foreach (object obj in ws2.LstClientes)
                {
                    TcpClient client = (TcpClient)obj;
                    NetworkStream stream = client.GetStream();
                    stream.Write(informacion, 0, tamanio);
                }
            }
            catch (Exception ex)
            {
                //Error no controlado
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ServerSocketConsole
{

    /// <summary>
    /// Esta clase representa el manejador de un sokect
    /// </summary>
    /// <author>Juan Sebastian Alvarez Eraso</author>
    /// <date>27 Julio 2014</date>
    class Socket
    {

        /// <summary>
        /// Lista que almacena los clientes conectados a un socket
        /// </summary>
        private List<TcpClient> lstClientes = new List<TcpClient>();

        public List<TcpClient> LstClientes
        {
            get { return lstClientes; }
            set { lstClientes = value; }
        }

        /// <summary>
        /// Constructor de la clase. Recibe un numero de puerto donde realiza la comunicación
        /// </summary>
        /// <param name="puerto">Número del puerto</param>
        public Socket(int puerto)
        {
            //Se crea un hilo inicial para iniciar la comunicación con el socket
            //Debe crearse un hilo para que la aplicación soporte la creación de varios sockets
            Thread hiloInicial = new Thread(new ParameterizedThreadStart(init));
            hiloInicial.Name = "Hilo inicial - Puerto: " + puerto.ToString();
            hiloInicial.Start(puerto);
        }

        /// <summary>
        /// Este metodo representa el inicio de la comunicación en un socket. Aqui se crea un objeto TcpListener
        /// el cual permanece escuchando las peticiones de los clientes entrantes o que desean conectarse
        /// </summary>
        /// <param name="puerto">Número del puerto</param>
        private void init(object puerto)
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), (int)puerto);
            server.Start();

            Console.WriteLine("Inicia el servidor en el puerto: " + ((int)puerto).ToString());
            
            //Mientras sea verdad, el hilo permanecera en ejecución escuchando a los clientes
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                //Cuando un cliente reliza la petición GET de conexión se lo agrega a la lista de clientes conectados
                lstClientes.Add(client);
                Console.WriteLine("Un cliente se ha conectado");

                //Se crea un segundo hilo donde se manejaran las peticion de cada cliente
                Thread hiloCliente = new Thread(new ParameterizedThreadStart(manejadorCliente));
                hiloCliente.Name = "Hilo cliente";
                hiloCliente.Start(client);
            }
        }

        /// <summary>
        /// Este metodo representa el manejador de cada cliente, permanecera ejecutandose
        /// mientras el cliente no se haya desconectado
        /// </summary>
        /// <param name="objectClient">Cliente conectado</param>
        private void manejadorCliente(object objectClient)
        {
            bool conexionPersistente = true;
            while (conexionPersistente)
            {
                TcpClient client = (TcpClient)objectClient;

                //Se obtiene el flujo de comunicación del cliente
                NetworkStream stream = client.GetStream();

                //Mientras no haya información disponible no sale de este ciclo
                while (!stream.DataAvailable);

                //En el momento en que en el canal hay información enviada se procede a su lectura
                Byte[] bytes = new Byte[client.Available];
                stream.Read(bytes, 0, bytes.Length);
                                
                String requerimiento = Encoding.UTF8.GetString(bytes);

                //Si la peticion es GET, significa que se debe realizar el handshake entre el cliente y el socket
                if (new Regex("^GET").IsMatch(requerimiento))
                {
                    //Se imprime la petición por consola
                    Console.WriteLine(requerimiento);
                    Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                        + "Connection: Upgrade" + Environment.NewLine
                        + "Upgrade: websocket" + Environment.NewLine
                        + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                            SHA1.Create().ComputeHash(
                                Encoding.UTF8.GetBytes(
                                    new Regex("Sec-WebSocket-Key: (.*)").Match(requerimiento).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                )
                            )
                        ) + Environment.NewLine
                        + Environment.NewLine);

                    //Handshake realizado
                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    //Si la petición no es GET significa que el usuario envió un mensjae al socket y se encuentra codificado

                    //Se utiliza el algoritmo XOR para decodificarlo
                    Byte[] vectorDecodificado = new Byte[3];

                    //Se restan 6 posiciones debido a:
                    //- 2 hacen referencia al byte 0 y byte 1 que contienen el FIN de la trama y el tamaño de la información
                    //- 4 hacen referencia a los bytes que contienen la información
                    Byte[] vectorCodificado = new Byte[bytes.Length - 2 - 4];
                    Byte[] llave = new Byte[4];

                    //Se llenan los vectores
                    for (int i = 0; i < bytes.Length; i++) {
                        if (i >= 2 && i <= 5) {
                            llave[i - 2] = bytes[i];
                        }
                        else if (i >= 6) {
                            vectorCodificado[i - 6] = bytes[i];
                        }
                    }

                    for (int i = 0; i < vectorCodificado.Length; i++)
                    {
                        vectorDecodificado[i] = (Byte)(vectorCodificado[i] ^ llave[i % 4]);
                    }

                    //Se decodifica el vector vectorDecodificado el cual posee la información correcta
                    requerimiento = Encoding.UTF8.GetString(vectorDecodificado);

                    //La cadena "\0\0\0" representa la desconexión de un usuario
                    if (!requerimiento.Equals("\0\0\0"))
                    {
                        //El usuario está conectado, se verifica si el requerimiento es un numero
                        int numeroGenerado = 0;
                        try
                        {
                            numeroGenerado = Int32.Parse(requerimiento);
                        }
                        catch (Exception ex)
                        {
                            //No se recibe un número
                        }
                        //Se acumula a la suma
                        Variables.acumular(numeroGenerado);

                        //Se crea el frame para enviarlo a los clientes
                        List<byte> lb = new List<byte>();
                        lb.Add(0x81);  //El primer byte debe ser 0x81 para que corresponda a una trama informativa
                        int size = requerimiento.Length;
                        lb.Add((byte)size);  //El segundo byte corresponde al tamaño del mensaje a enviar
                        lb.AddRange(Encoding.UTF8.GetBytes(requerimiento)); //Se obtiene los bytes de la suma

                        enviarMensajeTodosClientes(lb.ToArray(), size + 2);
                    }
                    else 
                    {
                        //El cliente se ha desconectado, se debe cerrar la conexión con el cliente, 
                        //limpiar y eliminar el flujo de comunicación y remover al cliente de la lista
                        client.Close();
                        stream.Flush();
                        stream.Dispose();
                        lstClientes.Remove(client);
                        conexionPersistente = false; //La conexion se cierra

                        Console.WriteLine("Cliente desconectado");
                    }
                }
            }
        }

        /// <summary>
        /// Este metodo permite enviar el numero recibido a todos los clientes conectados
        /// </summary>
        /// <param name="informacion">Vector de bytes que contiene la información</param>
        /// <param name="tamanio">Tamaño de los datos a enviar</param>
        private void enviarMensajeTodosClientes(byte[] informacion, int tamanio)
        {
            try
            {
                foreach (object obj in lstClientes)
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
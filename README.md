# Web sockets, TcpClient and NetworkStream

Este es un proyecto ejemplo de conexión vía web sockets usando las clases TcpClient y NetworkStream de .NET. La conexión cliente es una página web que recibe datos en tiempo real y los dibuja en una gráfica. La conexión que envía los datos es un generador de números aleatorios. El servidor que se encarga de recibir, administrar y transmitir los datos es una aplicación de consola desarrollada en C# .NET.

## Instrucciones

1. Abrir la solución del proyecto SolucionDos.sln con Visual Studio 2012
2. Una vez abierto, correr el proyecto consola ServerSocketConsole
3. Se abrirá la consola de comandos con la ejecución del servidor
4. Regresar a la carpeta del proyecto y abrir las 2 páginas web que se encuentran en la carpeta “Recursos”: “Cliente.html” y “GeneradorNumeros.html”. NOTA: Generalmente se abrirán en pestañas del mismo navegador. Es recomendable abrirlas en ventanas diferentes para una mejor apreciación del funcionamiento de la aplicación.
5. En la página “Generador de números aleatorios” primero se debe realizar la conexión con el Server Socket Console (clic en el botón “Conectar”), luego se debe iniciar la generación de números aleatorios (clic en “Iniciar generador”) para que se envíen al servidor. Si se realiza la desconexión con el Server Socket Console se dejará de enviar información al servidor.
6. En la página “Gráfica” se encuentra la gráfica que dibuja las coordenadas del tiempo respecto al número enviado por el Server Socket Console cada segundo. Primero se debe realizar la conexión (clic en el botón “Conectar”). Si se desconecta la gráfica del Server Socket Console se dejará de recibir información del servidor.
7. Una vez se haya iniciado la conexión en la gráfica como en el generador de números aleatorios, la consola empezará a imprimir la suma de los números aleatorios generados y que será enviada a la gráfica.

Fecha: 27/07/2014

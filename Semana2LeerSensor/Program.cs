using System;
using System.IO.Ports;
using System.Threading;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace LeerSensor
{
    class Program
    {
        static SerialPort serialPort;

        static void Main(string[] args)
        {
            serialPort = new SerialPort("COM4", 115200);
            serialPort.ReadTimeout = 12100; // Establece el tiempo de espera de lectura en 10 segundos
            int x = 1;
            while (x == 1)
            {
                try
                {
                    serialPort.Open(); // Abre el puerto serie
                    Console.WriteLine("Esperando conexión...");

                    Thread.Sleep(2000); // Espera 2 segundos para la conexión

                    // Solicita el valor del sensor y mide el tiempo de respuesta
                    var result = GetSensorValue();

                    if (result.Value != null)
                    {
                        Console.WriteLine($"Valor del sensor: {result.Value}, Tiempo de respuesta: {result.ResponseTime.Value.Duration().TotalSeconds}" + " segundos");
                    }
                    else
                    {
                        Console.WriteLine("No se recibió una respuesta válida en el formato esperado.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    if (serialPort.IsOpen)
                        serialPort.Close(); // Cierra el puerto serie
                }
                Console.WriteLine("¿Repetir? 1=Si 0=No");
                x = int.Parse(Console.ReadLine());

            }

        }

        static (int? Value, TimeSpan? ResponseTime) GetSensorValue()
        {
            serialPort.DiscardInBuffer(); // Limpia el buffer de entrada para descartar lecturas pendientes
            Console.WriteLine("Solicitando valor del sensor...");

            serialPort.Write("GET\n"); // Envía el comando GET al ESP32
            var startTime = DateTime.Now; // Registra el tiempo de inicio justo después de enviar el comando

            while (true) // Bucle para esperar por una respuesta válida
            {
                try
                {
                    string rawResponse = serialPort.ReadLine().Trim(); // Lee la línea siguiente del buffer de entrada
                    dynamic response = JObject.Parse(rawResponse); // Intenta parsear la respuesta JSON

                    if (response.ValorSensor != null) // Verifica si la respuesta contiene la clave esperada
                    {
                        var endTime = DateTime.Now; // Registra el tiempo de fin cuando se recibe una respuesta válida
                        int value = response.ValorSensor;
                        var responseTime = endTime - startTime;
                        return (value, responseTime); // Devuelve el valor del sensor y el tiempo de respuesta
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Tiempo de espera agotado.");
                    return (null, null);
                }
                catch
                {
                    // Si ocurre un error al decodificar, ignora esta respuesta y sigue esperando
                    continue;
                }
            }
        }
    }
}




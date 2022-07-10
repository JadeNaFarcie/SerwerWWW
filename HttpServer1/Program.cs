using System;

using System.Net;

namespace HttpServer1
{
    class Program
    {
        static ThreadedHTTPServer server;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

            server = new ThreadedHTTPServer(IPAddress.Parse("127.0.0.1"), 11000);

            server.Start();

            Console.WriteLine("The server has been stopped. Hit [Enter]...");
            Console.ReadLine();
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Stopping the server...");
            server.Stop();
            //Console.ReadLine();
            e.Cancel = true;
        }
    }
}

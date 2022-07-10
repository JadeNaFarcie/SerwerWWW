using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Web;

namespace HttpServer1
{
    partial class ThreadedHTTPServer
    {
        class ClientHelper
        {
            // fields
            Socket socket;    // 
            NetworkStream ns; // 
            StreamReader sr;  // strumień do odbioru danych "on net stream"
            StreamWriter sw;  // strumień do przesyłania danych 
            ThreadedHTTPServer server; // odniesienie do obiektu serwera
            FileStream FS;

            const string BaseDirectory = @".";

            // constructor
            public ClientHelper(Socket socket, ThreadedHTTPServer server)
            {
                this.socket = socket;
                ns = new NetworkStream(this.socket);
                sr = new StreamReader(ns);
                sw = new StreamWriter(ns);
                sw.AutoFlush = true;
                this.server = server;
            }

            void GET(String uri, Dictionary<String, String> headers)
            {
                if (uri == "/") 
                {
                    uri = BaseDirectory + "/index.html";
                    sw.WriteLine("HTTP/1.1 200 OK");
                    sw.WriteLine("Content-Type: text/html");

                    byte[] bMessage = File.ReadAllBytes(uri);

                    sw.WriteLine("Content-Length: {0}", bMessage.Length);

                    sw.WriteLine();

                    sw.BaseStream.Write(bMessage, 0, bMessage.Length);

                    //sw.Flush();
                }
                else 
                {
                   
                    string[] ss = uri.Split('?');
                    uri = ss[0];

                    string path = BaseDirectory + uri;
                    string sa = path;
                   
                    try
                    {
                        sr = new StreamReader(path);
                    }
                    catch (Exception)
                    {
                        sw.WriteLine("HTTP/1.1 404 Not Found");
                        return;
                    }
                    
                    int ostKropka = path.LastIndexOf('.');
                    string ext = path.Substring(ostKropka); 

                   
                    string ContentType;

                    switch (ext)
                    {
                        case ".htm":
                        case ".html":
                            ContentType = "text/html";
                            break;
                        case ".css":
                            ContentType = "text/stylesheet";
                            break;
                        case ".js":
                            ContentType = "text/javascript";
                            break;
                        case ".jpg":
                            ContentType = "image/jpeg";
                            break;
                        case ".jpeg":
                        case ".png":
                        case ".gif":
                            ContentType = "image/" + ext.Substring(1);
                            break;
                        default:
                            if (ext.Length > 1)
                            {
                                ContentType = "application/" + ext.Substring(1);
                            }
                            else
                            {
                                ContentType = "application/unknown";
                            }
                            break;
                    }

                    if (ext != "")
                    {
                        //string text = File.ReadAllBytes(path);
                        sw.WriteLine("HTTP/1.1 200 OK");
                        sw.WriteLine("Content-Type: {0}",ContentType);
                        byte[] bMessage = File.ReadAllBytes(path);
                        sw.WriteLine("Content-Length: {0}", bMessage.Length);
                        sw.WriteLine();
                        sw.BaseStream.Write(bMessage, 0, bMessage.Length);
                    }
                    else
                    {
                        sw.WriteLine("HTTP/1.1 404 Not Found");
                    }

                }
            }
            void POST(String uri, Dictionary<String, String> headers, byte[] body)
            {
                sw.WriteLine("HTTP/1.1 405 Method Not Allowed");
            }
            void PUT(String uri, Dictionary<String, String> headers, byte[] body)
            {
                sw.WriteLine("HTTP/1.1 405 Method Not Allowed");
            }

            public void ProcessCommunication()
            {
                String command = sr.ReadLine();
                Regex commandProcessor = new Regex("(?<method>GET|POST|PUT|DELETE|HEAD) *(?<uri>[^ ]*) HTTP/(?<httpVersion>1.[012])");

                if (command == null || !commandProcessor.IsMatch(command))
                {
                    // BLAD
                    sw.WriteLine("HTTP/1.1 400 Bad Request");
                    return;
                }

                Match commandParts = commandProcessor.Match(command);

                String uri = HttpUtility.UrlDecode(commandParts.Groups["uri"].Value);

                Dictionary<String, String> headers = new Dictionary<String, String>();
                Regex headerProcessor = new Regex("^(?<name>[^:]*): (?<content>.*)$");

                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length == 0)
                        break;

                    Match x = headerProcessor.Match(line);
                    headers[x.Groups["name"].Value] = x.Groups["content"].Value;
                }

                string method = commandParts.Groups["method"].Value;
                switch (method)
                {
                    case "GET": GET(uri, headers); break;
                    case "POST": POST(uri, headers, null); break;
                    case "PUT": PUT(uri, headers, null); break;
                    default: sw.WriteLine("HTTP/1.1 400 Bad Request"); break;
                }

                Disconnect();
            }

            void Disconnect()
            {
                if (sw != null) sw.Close();
                if (sr != null) sr.Close();
                if (ns != null) ns.Close();
                if (socket != null) socket.Close();
                server.RemoveClient(this);
            }

        } // ClientHelper

    } // server

} // namespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO; 

namespace TCPClient
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient("127.0.0.1", 8888);
            StreamWriter sWriter = new StreamWriter(tcpClient.GetStream());
            StreamReader clientRead = new StreamReader(tcpClient.GetStream());
            //System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
            //NetworkStream serverStream; 
            Console.WriteLine("Client Started");
            //clientSocket.Connect("127.0.0.1", 8888);
            string readText;
            string ls = "ls", lls = "lls",uf ="uf",df = "df",exit ="exit";
            //char ch;
            while ((true))
            {
                readText = Console.ReadLine();
                    if (readText.CompareTo(ls) == 0)
                    {
                    /*string[] localFiles = Directory.GetFiles("C:\\Users\\PingPingPhoto\\Desktop\\CPE_Y3S1\\Database\\LAB3\\TCPClient")
                                     .Select(path => Path.GetFileName(path))
                                     .ToArray();*/
                    string[] localFiles = Directory.GetFiles(Environment.CurrentDirectory)
                                     .Select(path => Path.GetFileName(path))
                                     .ToArray();
                        //string[] localFilePaths = Directory.GetFiles(@"C:\Users\PingPingPhoto\Desktop\CPE_Y3S1\Database\LAB3\TCPClient");
                        //Console.WriteLine("--- Files: ---");
                        foreach (string name in localFiles)
                        {
                            Console.WriteLine(name);
                        }
                    }

                    else if (readText.CompareTo(lls) == 0)
                    {
                        Console.WriteLine("--File in Server--");

                    //send request to server
                        sWriter.WriteLine(lls);
                        sWriter.Flush();


                        string readmessage1 = clientRead.ReadLine();
                        Console.WriteLine("Data from server: " + readmessage1); //First recive message from server

                        string readMessage2 = clientRead.ReadLine();
                        Console.WriteLine("Array list size: " + readMessage2); //Second receive message from server - list size
                        int length = Convert.ToInt32(readMessage2);
                        string[] fileList = new string[length];
                        
                         for (int i = 0; i < length; i++)
                         {
                             fileList[i] = clientRead.ReadLine();
                             Console.WriteLine(fileList[i]);
                         }
                    }
                    else if (readText.CompareTo(uf) == 0) // upload file to server
                    {
                        sWriter.WriteLine(uf); //request for upload file to server
                        sWriter.Flush();
                        try
                        {
                            string fullPath;
                            Console.WriteLine("Please enter file name");
                            string fileName = Console.ReadLine();
                            fullPath = Path.GetFullPath(fileName);
                            if(File.Exists(fullPath))
                            {
                                byte[] bytes = File.ReadAllBytes(fullPath);
                                //file size
                                sWriter.WriteLine(bytes.Length.ToString());
                                sWriter.Flush();

                                //file name
                                sWriter.WriteLine(fileName);
                                sWriter.Flush();

                                Console.WriteLine("Upload file");
                                tcpClient.Client.SendFile(fullPath);
                            }
                            else
                            {
                                Console.WriteLine("File does not exist");
                            }

                        }
                        catch (Exception e)
                        {
                            Console.Write(e.Message);
                        }
                    }
                    else if (readText.CompareTo(exit) == 0)
                    {
                        //tcpClient.Close();
                        sWriter.WriteLine(exit); //request mode for exit from server
                        sWriter.Flush();
                        Console.WriteLine("exit program");
                    }
                    else if (readText.CompareTo(df) == 0) // download file
                    {
                        sWriter.WriteLine(df); //request mode for download file to server
                        sWriter.Flush();

                        string downloadFileName;
                        Console.WriteLine("enter file name to download:");
                        downloadFileName = Console.ReadLine();

                        sWriter.WriteLine(downloadFileName); //1st send message
                        sWriter.Flush();

                        string reqeustMessage;
                        reqeustMessage = clientRead.ReadLine(); //1st receive message

                        if (reqeustMessage.CompareTo("dne") == 0)
                        {
                            Console.WriteLine("file does not exist");
                        }

                        else
                        {
                            // request message is not dne, then it will be file size 
                            string cmdFileSize = reqeustMessage;
                            Console.WriteLine("file size" + cmdFileSize);
                            // The 2nd message from the client is the filename      
                            string cmdFileName = clientRead.ReadLine();
                            Console.WriteLine("file name" + cmdFileName);

                            int downloadFileLength = Convert.ToInt32(cmdFileSize);
                            byte[] buffer = new byte[downloadFileLength];
                            int received = 0;
                            int read = 0;
                            int size = 1024;
                            int remaining = 0;

                            //Read bytes from the client using length sent from the client
                            while (received < downloadFileLength)
                            {
                                remaining = downloadFileLength - received;
                                if (remaining < size)
                                {
                                    size = remaining;
                                }
                                read = tcpClient.GetStream().Read(buffer, received, size);
                                received += read;
                            }

                            // Save the file using the filename sent by the client
                            using (FileStream fStream = new FileStream(Path.GetFileName(cmdFileName), FileMode.Create))
                            {
                                fStream.Write(buffer, 0, buffer.Length);
                                fStream.Flush();
                                fStream.Close();
                            }
                            Console.WriteLine("File received and saved in" + Environment.CurrentDirectory);
                        }


                    }
            }
        }
    }
}

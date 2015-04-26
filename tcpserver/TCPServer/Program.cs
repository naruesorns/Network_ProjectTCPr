using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.IO;
using System.Linq;
using System.Collections.Generic;
//using System.Drawing;
using System.Diagnostics;
namespace TCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            TcpListener tcpListener = new TcpListener(IPAddress.Any,8888);
            tcpListener.Start();  
            //TcpClient clientSocket = default(TcpClient);
            int counter = 0; // count client

            //serverSocket.Start();
            Console.WriteLine(" >> " + "Server Started");

            counter = 0;
            while (true)
            {
                counter += 1;
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");
                handleClinet client = new handleClinet();
                client.startClient(tcpClient, Convert.ToString(counter));
            }

            tcpListener.Stop();
            Console.WriteLine(" >> " + "exit");
            Console.ReadLine();
        }

    }

    //Class to handle each client request separatly
    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
        public void startClient(TcpClient inClientSocket, string clineNo)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }
        private void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[10025];
            string dataFromClient = null;
            //Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;
            string lls = "ls" ,uf ="put", df ="get", exit = "exit";
            string[] remoteFileList;
            
            //int arrayLength;
            while (true)
            {
                Console.WriteLine("waiting for client");
                try
                {
                    requestCount = requestCount + 1;

                    StreamReader reader = new StreamReader(clientSocket.GetStream());
                    StreamWriter writer = new StreamWriter(clientSocket.GetStream());
                    dataFromClient = reader.ReadLine();
                    Console.WriteLine(" >> " + "From client-" + clNo + dataFromClient);

                    if (dataFromClient.CompareTo(lls) == 0)
                    {
                        rCount = Convert.ToString(requestCount);
                        remoteFileList = listRemoteDirectory();

                        //send 1st message
                        serverResponse = clNo;
                        writer.WriteLine(serverResponse);
                        writer.Flush();

                        DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
                        FileInfo[] fiArr = di.GetFiles();

                        //send 2nd message - list length
                        writer.WriteLine(fiArr.Length);
                        writer.Flush();

                        //send 2nd message - list length
                        //writer.WriteLine(remoteFileList.Length.ToString());
                        //writer.Flush();

                        foreach (FileInfo f in fiArr)
                        {
                            writer.WriteLine(f.Name);
                            writer.Flush();

                            writer.WriteLine(f.Length);
                            writer.Flush();

                        }
    
                        /*foreach (string name in remoteFileList)
                        {
                            writer.WriteLine(name);
                            writer.Flush();
                            //writer.WriteLine();
                        }*/
                        writer.WriteLine("success");
                        writer.Flush();

                    }
                    else if (dataFromClient.CompareTo(exit) == 0)
                    {
                        Console.WriteLine("close conection from Client No:" + clNo);
                        //clientSocket.EndConnect();
                        clientSocket.Close();
                        return;

                    }
                    else if (dataFromClient.CompareTo(uf) == 0) //receive file from client
                    {
                        //1st message is file size
                        string cmdFileSize = reader.ReadLine();
                        Console.WriteLine("file size " + cmdFileSize);
                        //2nd message is file name
                        string cmdFileName = reader.ReadLine();
                        Console.WriteLine("file name " + cmdFileName);
                        int fileLength = Convert.ToInt32(cmdFileSize);

                        byte[] buffer = new byte[fileLength];
                        int received = 0;
                        int read = 0;
                        int size = 1024;
                        int remaining = 0;
                        Stopwatch sw = Stopwatch.StartNew();
                        writer.WriteLine("ready");
                        writer.Flush();
                        //Read bytes from the client using length sent from the client
                        while (received < fileLength)
                        {
                            remaining = fileLength - received;
                            if (remaining < size)
                            {
                                size = remaining;
                            }
                            read = clientSocket.GetStream().Read(buffer, received, size);
                            received += read;
                        }
                        sw.Stop();

                        writer.WriteLine("success");
                        writer.Flush();

                        Console.WriteLine("Time take: {0} ms ", sw.Elapsed.TotalMilliseconds);
                        // Save the file using the filename sent by the client
                        using (FileStream fStream = new FileStream(Path.GetFileName(cmdFileName), FileMode.Create))
                        {
                            fStream.Write(buffer, 0, buffer.Length);
                            fStream.Flush();
                            fStream.Close();
                        }
                        //Console.WriteLine("File received and saved in" + Environment.CurrentDirectory);
                    }

                    else if (dataFromClient.CompareTo(df) == 0) // client request to download file
                    {
                        Console.WriteLine("prepare to download");
                        string requestFileName = reader.ReadLine(); //1st receive message
                        string fullPathRequestFile;
                        fullPathRequestFile = Path.GetFullPath(requestFileName); //base on current directory

                        if (File.Exists(fullPathRequestFile))    
                        {
                            byte[] bytes = File.ReadAllBytes(fullPathRequestFile);
                            byte[] myWriteBuffer = File.ReadAllBytes(fullPathRequestFile);
                            //file size
                            writer.WriteLine(bytes.Length.ToString());
                            writer.Flush();

                            //file name
                            writer.WriteLine(requestFileName);
                            writer.Flush();

                            //clientSocket.Client.SendFile(fullPathRequestFile);
                            
                            
                            int remaining = myWriteBuffer.Length;
                            int blocksize = 1024;
                            int sendFileLength = myWriteBuffer.Length;
                            int offset = 0;
                            

                            string downloadState = reader.ReadLine();
                            if (downloadState == "ready")
                            {
                                while (offset < sendFileLength)
                                {
                                    remaining = sendFileLength - offset;
                                    if (remaining < blocksize)
                                    {
                                        blocksize = remaining;
                                    }
                                    //Console.Read();
                                    
                                    clientSocket.GetStream().Write(myWriteBuffer, offset, blocksize);
                                    offset += blocksize;
                                    Console.WriteLine("{0} . {1} . {2}", myWriteBuffer.Length, offset, blocksize);
                                }

                                writer.WriteLine("success"); //send to server
                                writer.Flush();

                                if (offset == sendFileLength)
                                {
                                    Console.WriteLine("success sent");
                                }
                            }
                        }
                        else
                        {
                            string errMessage = "dne"; //does not exist
                            writer.WriteLine(errMessage);
                            writer.Flush();
                        }    
                    }
                }
                /*catch (InvalidOperationException)
                {
                    Console.WriteLine("close conection from Client No:" + clNo);
                    clientSocket.Close();
                    return;
                }*/
                catch (IOException) // catch exit button case
                {
                    Console.WriteLine("IO error");
                    Console.WriteLine("close conection from Client No:" + clNo);
                    clientSocket.Close();
                    return;
                }
                catch(NullReferenceException)
                {
                    Console.WriteLine("IO error");
                    Console.WriteLine("close conection from Client No:" + clNo);
                    clientSocket.Close();
                    return;
                }
            }
        }

        private static void drawTextProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }
        public string[] listRemoteDirectory()
        {
            /*string[] localFiles = Directory.GetFiles("C:\\Users\\PingPingPhoto\\Desktop\\CPE_Y3S1\\Database\\LAB3\\TCPServer")
                                     .Select(path => Path.GetFileName(path))
                                     .ToArray(); */
            string[] localFiles = Directory.GetFiles(Environment.CurrentDirectory)
                                     .Select(path => Path.GetFileName(path))
                                     .ToArray();
            foreach (string name in localFiles)
            {
                Console.WriteLine(name);
            }
            return localFiles;
        }
    }
}
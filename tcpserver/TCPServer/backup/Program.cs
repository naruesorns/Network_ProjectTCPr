using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace TCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            TcpListener tcpListener = new TcpListener(ipAddress,8888);
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
            /*
            while (true)
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");
                handleClinet client = new handleClinet();
                client.startClient(clientSocket, Convert.ToString(counter));
            }
            */
            //clientSocket.Close();
            //serverSocket.Stop();
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
            string lls = "lls" ,uf ="uf", df ="df", exit = "exit";
            string[] remoteFileList;
            //int arrayLength;
            while (true)
            {
                try
                {
                    requestCount = requestCount + 1;
                    //NetworkStream networkStream = clientSocket.GetStream();
                    //networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    //dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    //dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                    StreamReader reader = new StreamReader(clientSocket.GetStream());
                    StreamWriter writer = new StreamWriter(clientSocket.GetStream());
                    dataFromClient = reader.ReadLine();
                    Console.WriteLine(" >> " + "From client-" + clNo + dataFromClient);

                    if (dataFromClient.CompareTo(lls) == 0)
                    {
                        rCount = Convert.ToString(requestCount);
                        remoteFileList = listRemoteDirectory();

                        //send 1st message
                        serverResponse = "Server to client(" + clNo + ") " + rCount;
                        writer.WriteLine(serverResponse);
                        writer.Flush();

                        //send 2nd message - list length
                        writer.WriteLine(remoteFileList.Length.ToString());
                        writer.Flush();

                        foreach (string name in remoteFileList)
                        {
                            writer.WriteLine(name);
                            writer.Flush();
                        }

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
                        Console.WriteLine("file size" + cmdFileSize);
                        //2nd message is file name
                        string cmdFileName = reader.ReadLine();
                        Console.WriteLine("file name" + cmdFileName);
                        int fileLength = Convert.ToInt32(cmdFileSize);
                        byte[] buffer = new byte[fileLength];
                        int received = 0;
                        int read = 0;
                        int size = 1024;
                        int remaining = 0;

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

                        // Save the file using the filename sent by the client
                        using (FileStream fStream = new FileStream(Path.GetFileName(cmdFileName), FileMode.Create))
                        {
                            fStream.Write(buffer, 0, buffer.Length);
                            fStream.Flush();
                            fStream.Close();
                        }
                        Console.WriteLine("File received and saved in" + Environment.CurrentDirectory);
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
                            //file size
                            writer.WriteLine(bytes.Length.ToString());
                            writer.Flush();

                            //file name
                            writer.WriteLine(requestFileName);
                            writer.Flush();

                            clientSocket.Client.SendFile(fullPathRequestFile);
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
            }
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
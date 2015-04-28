using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
//using System.Drawing;
using System.Diagnostics;
namespace TCPClient
{
    class Program
    {
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

        public static bool IsConnected(TcpClient _tcpClient)
        {
            try
            {
                if (_tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.Connected)
                {
                    /* pear to the documentation on Poll:
                        * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                        * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                        * -or- true if data is available for reading; 
                        * -or- true if the connection has been closed, reset, or terminated; 
                        * otherwise, returns false
                        */

                    // Detect if client disconnected
                    if (_tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (_tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            // Client disconnected
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }


        static void Main(string[] args)
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST
            Console.WriteLine(hostName);
            // Get the IP
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            Console.WriteLine(">>>> My IP Address is : {0}", myIP);

            string readconnect;
            Console.Write(">> Do you want to Connect to server? (Y/N): ");
            readconnect = Console.ReadLine();

            if ((readconnect.CompareTo("Y") == 0) || (readconnect.CompareTo("y") == 0))
            {

                //TcpClient tcpClient = new TcpClient("192.168.1.40", 8888);

                //TcpClient tcpClient = new TcpClient("127.0.0.1", 8888);
                //System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();

                //Console.WriteLine(">>>> Client Started");
                string readText;
                string ls = "ls", lls = "lls", uf = "put", df = "get", exit = "exit";

                //Console.Write(">>>> Enter IP Address :");
                //string serverIP;
                //TcpClient tcpClient = null;
                TcpClient tcpClient = null;
                /*if (IsConnected(tcpClient) == false)
                {
                    
                    Console.Write(">>>> Enter IP Address :");
                    string serverIP = Console.ReadLine();
                    tcpClient = new TcpClient(serverIP, 8888);
                }*/
                while ((true))
                {
                    try
                    {
                        if (IsConnected(tcpClient) == false)
                        {

                            Console.Write(">>>> Enter IP Address :");
                            string serverIP = Console.ReadLine();
                            tcpClient = new TcpClient(serverIP, 8888);
                        }
                        //Console.Write(">>>> Enter IP Address :");
                        //serverIP = Console.ReadLine();
                        //tcpClient = new TcpClient("192.168.1.40", 8888);
                        //TcpClient tcpClient = new TcpClient(serverIP, 8888);
                        //tcpClient = new TcpClient(serverIP, 8888);
                        Console.WriteLine(">>>> Client Started");

                        StreamWriter sWriter = new StreamWriter(tcpClient.GetStream());
                        StreamReader clientRead = new StreamReader(tcpClient.GetStream());
                        Console.WriteLine(">>>> -----------------------------");
                        Console.WriteLine(">>>> OPERATION MODE");
                        Console.WriteLine(">>>> DF : Download file to server");
                        Console.WriteLine(">>>> UF : Upload file to server");
                        Console.WriteLine(">>>> LS : List file on this client");
                        Console.WriteLine(">>>> LLS : List file in the server");
                        Console.WriteLine(">>>> EXIT : Exit");
                        Console.WriteLine(">>>> -----------------------------");
                        Console.Write(">>>>> Enter mode : ");

                        readText = Console.ReadLine();
                        readText.ToLower();
                        if (readText.CompareTo(lls) == 0)
                        {
                            /*string[] localFiles = Directory.GetFiles("C:\\Users\\PingPingPhoto\\Desktop\\CPE_Y3S1\\Database\\LAB3\\TCPClient")
                                             .Select(path => Path.GetFileName(path))
                                             .ToArray();*/
                            /*string[] localFiles = Directory.GetFiles(Environment.CurrentDirectory)
                                             .Select(path => Path.GetFileName(path))
                                             .ToArray();*/

                            //string[] localFilePaths = Directory.GetFiles(@"C:\Users\PingPingPhoto\Desktop\CPE_Y3S1\Database\LAB3\TCPClient");
                            //Console.WriteLine("--- Files: ---");
                            int i = 1;
                            Console.WriteLine(">>>> -------------------------");
                            Console.WriteLine(">>>> LIST FILE IN LOCAL CLIENT");
                            /*foreach (string name in localFiles)
                            {
                                Console.WriteLine(">>>>>> {0}. {1}", i, name);
                                i++;
                            }*/
                            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
                            FileInfo[] fiArr = di.GetFiles();
                            Console.WriteLine(">>>> {0,-30} {1,15:1}", "fileName", "size");
                            foreach (FileInfo f in fiArr)
                            {
                                Console.WriteLine(">>>> {0,-30} {1,15:N0} bytes", f.Name, f.Length);
                            i++;
                            }
    
                        }

                        else if (readText.CompareTo(ls) == 0)
                        {
                            Console.WriteLine(">>>> ----------------------");
                            Console.WriteLine(">>>>>> LIST FILE IN SERVER ");

                            //send request to server
                            sWriter.WriteLine(ls);
                            sWriter.Flush();


                            string readmessage1 = clientRead.ReadLine();
                            Console.WriteLine(">>>>>> CLient No : " + readmessage1); //First recive message from server

                            string readMessage2 = clientRead.ReadLine();
                            Console.WriteLine(">>>>>> Array list size: " + readMessage2); //Second receive message from server - list size
                            int length = Convert.ToInt32(readMessage2);
                            string[] fileName = new string[length];
                            string[] fileSize = new string[length];
                            Console.WriteLine(">>>> {0,-30} {1,15:1}", "fileName", "size");
                            for (int i = 0; i < length; i++)
                            {
                                fileName[i] = clientRead.ReadLine();
                                fileSize[i] = clientRead.ReadLine();
                                int size = Int32.Parse(fileSize[i]);
                                //Console.WriteLine(">>>>>> {0}. {1}", i + 1, fileList[i]);
                                Console.WriteLine(">>>> {0,-30} {1,15:N0} bytes", fileName[i], size);
                            }

                            string status = clientRead.ReadLine();
                            if (status == "success")
                            {
                                Console.WriteLine(">>>> ============================");
                            }
                        }

                        else if (readText.CompareTo(uf) == 0) // upload file to server
                        {
                            string fullPath;
                            Console.WriteLine(">>>> -------------------------------------");
                            Console.WriteLine(">>>>>> UPLOAD FILE MODE (leave blank to go back)");
                            Console.WriteLine(">>>> -------------------------------------");
                            Console.Write(">>>>>>> Please enter file name : ");
                            string fileName = Console.ReadLine();
                            if (string.IsNullOrEmpty(fileName))
                            {
                                Console.WriteLine(">>>> back to Mode Selection");
                            }
                            else
                            {
                                fullPath = Path.GetFullPath(fileName);

                                if (File.Exists(fullPath))
                                {
                                    sWriter.WriteLine(uf); //request for upload file to server
                                    sWriter.Flush();

                                    byte[] bytes = File.ReadAllBytes(fullPath);
                                    byte[] myWriteBuffer = File.ReadAllBytes(fullPath);
                                    //file size
                                    Console.WriteLine(">>>>>> file size : {0}", bytes.Length.ToString());
                                    sWriter.WriteLine(bytes.Length.ToString());
                                    sWriter.Flush();

                                    //file name
                                    Console.WriteLine(">>>>>> file name : {0}", fileName);
                                    sWriter.WriteLine(fileName);
                                    sWriter.Flush();

                                    Console.WriteLine(">>>>>> Upload file");
                                    //tcpClient.Client.SendFile(fullPath);

                                    int remaining = myWriteBuffer.Length;
                                    int blocksize = 1024;
                                    int sendFileLength = myWriteBuffer.Length;
                                    int offset = 0;
                                    Stopwatch sw = Stopwatch.StartNew();
                                    string uploadState = clientRead.ReadLine();

                                    if (uploadState == "ready")
                                    {
                                        while (offset < sendFileLength)
                                        {
                                            remaining = sendFileLength - offset;
                                            if (remaining < blocksize)
                                            {
                                                blocksize = remaining;
                                            }
                                            tcpClient.GetStream().Write(myWriteBuffer, offset, blocksize);
                                            drawTextProgressBar(offset, sendFileLength);
                                            offset += blocksize;
                                            //Console.WriteLine("{0} . {1} . {2}", myWriteBuffer.Length, offset, blocksize);

                                        }
                                        sw.Stop();

                                        uploadState = clientRead.ReadLine();
                                        Console.WriteLine(uploadState);
                                        if (uploadState == "success")
                                        {
                                            Console.WriteLine();
                                            Console.WriteLine("UPLOAD SUCCESS");
                                            Console.WriteLine(">>>> -------------------------------------");
                                            Console.WriteLine(">>>>>> Time take: {0} sec", sw.Elapsed.TotalSeconds);
                                            double throughput = (sendFileLength / 1024) / sw.Elapsed.TotalSeconds;
                                            Console.WriteLine(">>>>>> Throughput : {0:F} Kb/s", throughput);
                                            Console.WriteLine(">>>> -------------------------------------");
                                        }
                                    }

                                }
                                else
                                {
                                    Console.WriteLine(">>>> ------------------------------------");
                                    Console.WriteLine(">>>> ----- Error : File does not exist");

                                }
                            }
                        }
                        else if (readText.CompareTo(exit) == 0)
                        {
                            //tcpClient.Close();
                            sWriter.WriteLine(exit); //request mode for exit from server
                            sWriter.Flush();
                            Console.WriteLine("exit program");
                            Environment.Exit(0);
                        }

                        else if (readText.CompareTo(df) == 0) // download file
                        {
                            string downloadFileName;
                            //NetworkStream clientStream = tcpClient.GetStream(); 
                            Console.WriteLine(">>>> -------------------------------------");
                            Console.WriteLine(">>>>>> DOWNLOAD FILE MODE (leave blank to go back)");
                            Console.WriteLine(">>>> -------------------------------------");
                            Console.Write(">>>>>>> enter file name to download : ");
                            downloadFileName = Console.ReadLine();
                            if (string.IsNullOrEmpty(downloadFileName))
                            {
                                Console.WriteLine(">>>> back to Mode Selection");
                            }
                            else
                            {
                                sWriter.WriteLine(df); //request mode for download file to server
                                sWriter.Flush();

                                sWriter.WriteLine(downloadFileName); //1st send message
                                sWriter.Flush();

                                string reqeustMessage;
                                reqeustMessage = clientRead.ReadLine(); //1st receive message

                                if (reqeustMessage.CompareTo("dne") == 0)
                                {
                                    Console.WriteLine(">>>> -------------------------------------");
                                    Console.WriteLine(">>>> ---- Error : file does not exist");
                                }

                                else
                                {
                                    // request message is not dne, then it will be file size 
                                    string cmdFileSize = reqeustMessage;
                                    Console.WriteLine(">>>>>> file size " + cmdFileSize);
                                    // The 2nd message from the client is the filename      
                                    string cmdFileName = clientRead.ReadLine();
                                    Console.WriteLine(">>>>>> file name " + cmdFileName);

                                    int downloadFileLength = Convert.ToInt32(cmdFileSize);
                                    byte[] buffer = new byte[downloadFileLength];
                                    int received = 0;
                                    int read = 0;
                                    int size = 1024;
                                    int remaining = downloadFileLength;
                                    Stopwatch sw = Stopwatch.StartNew();
                                    //Read bytes from the client using length sent from the client
                                    sWriter.WriteLine("ready");
                                    sWriter.Flush();

                                    while (received < downloadFileLength)
                                    {
                                        if (remaining < size)
                                        {
                                            size = remaining;
                                        }

                                        //read = clientStream.Read(buffer, received, size);
                                        //Console.WriteLine("{0}-{1}", received, size);
                                        read = tcpClient.GetStream().Read(buffer, received, size);
                                        //Console.Read();
                                        //Console.WriteLine("{0}/{1} read:{2} size:{3}", remaining, received, read, size);
                                        received += read;
                                        remaining = downloadFileLength - received;
                                        drawTextProgressBar(received, downloadFileLength);

                                        //sWriter.WriteLine(received.ToString());
                                        //sWriter.Flush();


                                        //Console.Read();  
                                    }

                                    sw.Stop();
                                    using (FileStream fStream = new FileStream(Path.GetFileName(cmdFileName), FileMode.Create))
                                    {
                                        fStream.Write(buffer, 0, buffer.Length);
                                        fStream.Flush();
                                        fStream.Close();
                                    }

                                    string downloadState = clientRead.ReadLine();
                                    if (downloadState == "success")
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine(">>>> -------------------------------------");
                                        Console.WriteLine(">>>>>> Time take: {0} seccond", sw.Elapsed.TotalSeconds);
                                        double throughput = (downloadFileLength /1024)/sw.Elapsed.TotalSeconds;
                                        Console.WriteLine(">>>>>> Throughput : {0:F} Kb/s", throughput);
                                        Console.WriteLine(">>>> -------------------------------------");
                                        // Save the file using the filename sent by the client
                                        Console.WriteLine(">>>>>> File received and saved in " + Environment.CurrentDirectory);
                                        Console.WriteLine(">>>> ---------------------------------------------------------------");
                                    }

                                }
                            }
                        }
                        else
                        {
                            if (IsConnected(tcpClient) == false)
                            {
                                Console.WriteLine(">>>> ------------------------------");
                                Console.WriteLine(">>>> ---- No connection");
                                Console.WriteLine(">>>> ---- Disconnect from server");
                                Console.WriteLine(">>>> ------------------------------");
                            }
                            else
                            {
                                Console.WriteLine(">>>> ------------------------------");
                                Console.WriteLine(">>>> ---- Error : unknow opperation");
                                Console.WriteLine(">>>> ------------------------------");
                            }

                        }

                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(">>>> ------------------------------");
                        Console.WriteLine(">>>> error : " + e.Message);
                        Console.WriteLine(">>>>> Disconect from server");
                        Console.WriteLine(">>>> ------------------------------");
                        //should return to ask to join server
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine(">>>> ------------------------------");
                        Console.WriteLine(">>>> error : " + e.Message);
                        Console.WriteLine(">>>> Disconnect from server");
                        Console.WriteLine(">>>> ------------------------------");
                    }
                }
            }
            else //exit program
            {
                Console.WriteLine(">>>> Exit program");
            }
        }
    }
}

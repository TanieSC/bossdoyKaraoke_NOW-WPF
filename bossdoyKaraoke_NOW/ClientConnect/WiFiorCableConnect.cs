using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

// Credit to: Jayan Nair https://www.codeguru.com/csharp/csharp/cs_network/sockets/article.php/c8781/Asynchronous-Socket-Programming-in-C-Part-II.htm
//Modified the code for my needs
namespace bossdoyKaraoke_NOW.ClientConnect
{
    class WiFiorCableConnect
    {
        public AsyncCallback pfnWorkerCallBack;
        private Socket _mainSocket;

        // An ArrayList is used to keep track of worker sockets that are designed
        // to communicate with each connected client. Make it a synchronized ArrayList
        // For thread safety
        private ArrayList _workerSocketList =
                ArrayList.Synchronized(new ArrayList());

        // The following variable will keep track of the cumulative 
        // total number of clients connected at any time. Since multiple threads
        // can access this variable, modifying this variable should be done
        // in a thread safe manner
        private int _clientCount = 0;

        public WiFiorCableConnect()
        {

        }

        public bool StartListening()
        {
            try
            {
                // Create the listening socket...
                _mainSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);
                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, 8000);
                // Bind to local IP Address...
                _mainSocket.Bind(ipLocal);
                // Start listening...
                _mainSocket.Listen(10);
                // Create the call back for any client connections...
                _mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);

                // ShowActiveTcpListeners();

                return true;
            }
            catch (SocketException se)
            {
                //MessageBox.Show(se.Message);

                return false;
            }
        }

        //public static void ShowActiveTcpListeners()
        //{
        //    Console.WriteLine("Active TCP Listeners");
        //    IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        //    IPEndPoint[] endPoints = properties.GetActiveTcpListeners();
        //    foreach (IPEndPoint e in endPoints)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        // This is the call back function, which will be invoked when a client is connected
        public void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                // Here we complete/end the BeginAccept() asynchronous call
                // by calling EndAccept() - which returns the reference to
                // a new Socket object
                Socket workerSocket = _mainSocket.EndAccept(asyn);

                // Now increment the client count for this client 
                // in a thread safe manner
                Interlocked.Increment(ref _clientCount);

                // Add the workerSocket reference to our ArrayList
                _workerSocketList.Add(workerSocket);

                // Send a welcome message to client
                string msg = "Welcome client " + _clientCount + "\n";
                SendMsgToClient(msg, _clientCount);


                // Let the worker Socket do the further processing for the 
                // just connected client
                WaitForData(workerSocket, _clientCount);

                // Since the main Socket is now free, it can go back and wait for
                // other clients who are attempting to connect
                _mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
            catch (ObjectDisposedException)
            {
                //System.Diagnostics.Debugger.Log(0, "1", "\n OnClientConnection: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                //MessageBox.Show(se.Message);
            }

        }

        public class SocketPacket
        {
            // Constructor which takes a Socket and a client number
            public SocketPacket(Socket socket, int clientNumber)
            {
                currentSocket = socket;
                this.clientNumber = clientNumber;
            }
            public Socket currentSocket;
            public int clientNumber;
            // Buffer to store the data sent by the client
            public byte[] dataBuffer = new byte[1024];
        }

        // Start waiting for data from the client
        public void WaitForData(Socket soc, int clientNumber)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    // Specify the call back function which is to be 
                    // invoked when there is any write activity by the 
                    // connected client
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket(soc, clientNumber);

                soc.BeginReceive(theSocPkt.dataBuffer, 0,
                    theSocPkt.dataBuffer.Length,
                    SocketFlags.None,
                    pfnWorkerCallBack,
                    theSocPkt);
            }
            catch (SocketException se)
            {
               // MessageBox.Show(se.Message);
            }
        }

        // This the call back function which will be invoked when the socket
        // detects any client writing of data on the stream
        public void OnDataReceived(IAsyncResult asyn)
        {
            SocketPacket socketData = (SocketPacket)asyn.AsyncState;
            try
            {
                // Complete the BeginReceive() asynchronous call by EndReceive() method
                // which will return the number of characters written to the stream 
                // by the client
                int iRx = socketData.currentSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                // Extract the characters as a buffer
                Decoder d = Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(socketData.dataBuffer,
                    0, iRx, chars, 0);

                //data received
                string szData = new string(chars);
                string msg = "" + socketData.clientNumber + ":";
                Console.WriteLine("Data received from client: " + msg + " " + szData);

                // Send back the reply to the client
                string replyMsg = "Server Reply:" + szData.ToUpper();
                // Convert the reply to byte array
                byte[] byData = Encoding.ASCII.GetBytes(replyMsg);

                Socket workerSocket = socketData.currentSocket;
                workerSocket.Send(byData);

                // Continue the waiting for data on the Socket
                WaitForData(socketData.currentSocket, socketData.clientNumber);

            }
            catch (ObjectDisposedException)
            {
              //  System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                if (se.ErrorCode == 10054) // Error code for Connection reset by peer
                {
                   // string msg = "Client " + socketData.clientNumber + " Disconnected" + "\n";

                    // Remove the reference to the worker socket of the closed client
                    // so that this object will get garbage collected
                    _workerSocketList[socketData.clientNumber - 1] = null;

                 //   MessageBox.Show(msg);
                }
                else
                {
                   // MessageBox.Show(se.Message);
                }
            }
        }

        public void CloseSockets()
        {
            if (_mainSocket != null)
            {
                _mainSocket.Close();
            }
            Socket workerSocket = null;
            for (int i = 0; i < _workerSocketList.Count; i++)
            {
                workerSocket = (Socket)_workerSocketList[i];
                if (workerSocket != null)
                {
                    // Close Socket using  
                    // the method Close() 
                    workerSocket.Close();
                    workerSocket = null;
                }
            }
        }

        void SendMsgToClient(string msg, int clientNumber)
        {
            // Convert the reply to byte array
            byte[] byData = Encoding.ASCII.GetBytes(msg);

            Socket workerSocket = (Socket)_workerSocketList[clientNumber - 1];
            workerSocket.Send(byData);
        }

        string GetIP()
        {
            string strHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

            // Grab the first IP addresses
            string IPStr = "";
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                IPStr = ipaddress.ToString();
                return IPStr;
            }
            return IPStr;
        }
    }
}

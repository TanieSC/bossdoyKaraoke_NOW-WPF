using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


//Credit to: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-server-socket-example
namespace bossdoyKaraoke_NOW.ClientConnect
{
    // State object for reading client data asynchronously  
    class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    class WiFiorCableConnect
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);


        public WiFiorCableConnect()
        {

        }

        public static void Start(string localIPAddr)
        {
            // Establish the local endpoint  
            // for the socket. Dns.GetHostName 
            // returns the name of the host  
            // running the application. 
            IPAddress ipAddr = IPAddress.Parse(localIPAddr);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 0);

            // Creation TCP/IP Socket using  
            // Socket Class Costructor 
            Socket listener = new Socket(ipAddr.AddressFamily,
                         SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            string content = string.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read   
                // more data.  
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the   
                    // client. Display it on the console.  
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    // Echo the data back to the client.  
                    Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void Send(Socket handler, string data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static string GetLocalIPv4(NetworkInterfaceType type)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return "";

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == type && item.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties adapterProperties = item.GetIPProperties();

                    if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                    {
                        foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }

            return "";




            //foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            //{
            //    // discard because of standard reasons
            //    if ((ni.OperationalStatus != OperationalStatus.Up) ||
            //        (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) ||
            //        (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel) ||
            //        (ni.Speed < 10000000) ||
            //        (ni.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0) ||
            //        (ni.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0) ||
            //        (ni.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase)))
            //        continue;

            //    if (ni.OperationalStatus == OperationalStatus.Up)
            //    {
            //        IPInterfaceProperties adapterProperties = ni.GetIPProperties();

            //        if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
            //        {
            //            foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
            //            {
            //                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
            //                {
            //                    return ip.Address.ToString();
            //                }
            //            }
            //        }
            //    }
            //}
            //return "";
        }

    }
}

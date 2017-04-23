using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace SS
{
    /// <summary>
    /// A helper class for the Networking object. It represents the state of
    /// a single socket.
    /// 
    /// It contains the socket,  its ID, and a delegate used for callbacks.
    /// </summary>
    public class SocketState
    {
        public Socket theSocket;
        public int ID;
        public Action<SocketState> callMe;

        // The buffer where the data received from the client is stored
        public byte[] messageBuffer = new byte[1024];

        // A larger (growable) buffer, in case a single receive does not contain the full message.
        public StringBuilder sb = new StringBuilder();
    }

    /// <summary>
    /// A static networking library that provides netowrking communications
    /// between servers and clients.
    /// </summary>
    public static class Networking
    {
        public const int DEFAULT_PORT = 2112;

        /// <summary>
        /// Attempt to connect to the server via provided hostname. Saves the callback function in
        /// a SocketState object for use when data arrives.
        /// </summary>
        /// <param name="callbackFunction">The delegate function to be called once connected to the server.</param>
        /// <param name="hostName">The name of the server to connect to.</param>
        /// <returns>The newly connected socket.</returns>
        public static Socket ConnectToServer(Action<SocketState> callbackFunction, string hostName)
        {
            Socket socket = null;
            try
            {
                IPHostEntry ipHostInfo;
                IPAddress ipAddress = IPAddress.None;


                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostName);
                    bool foundIPV4 = false;

                    foreach (IPAddress addr in ipHostInfo.AddressList)
                    {
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }
                    }

                    if (!foundIPV4)
                    {
                        System.Diagnostics.Debug.WriteLine("Invalid Address: " + hostName);
                    }
                }
                catch (Exception)
                {
                    // Check if the hostname is actually an ipaddress
                    System.Diagnostics.Debug.WriteLine("Using IP");
                    ipAddress = IPAddress.Parse(hostName);
                }

                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                SocketState socketState = new SocketState();

                socketState.theSocket = socket;
                socketState.callMe = callbackFunction;
                socketState.theSocket.BeginConnect(ipAddress, DEFAULT_PORT, ConnectedToServer, socketState);
                return socket;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write("Unable to connect to server. Error Message: " + e);
            }

            return socket;
        }

        /// <summary>
        /// This function is referenced by the BeginConnect method above and is "called" by the OS when the
        /// socket connects to the server.
        /// 
        /// Once a connection is established the "saved away" callbackFunction is called. This function
        /// is saved in the socket state, and was originally passed in to ConnectToServer.
        /// </summary>
        /// <param name="ar">Contains the "state" object saved in the ConnectToServer function.</param>
        private static void ConnectedToServer(IAsyncResult ar)
        {
            try
            {
                SocketState ss = (SocketState)ar.AsyncState;
                ss.theSocket.EndConnect(ar);
                ss.callMe(ss);
                ss.theSocket.BeginReceive(ss.messageBuffer, 0, 1024, SocketFlags.None, new AsyncCallback(Networking.ReceiveCallback), (object)ss);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// This function is called by the OS when new data arrives. This function checks to see how
        /// much data has arrived. If 0, the connection has been closed (presumably by the server).
        /// On greater than zero data, this method should call the callback function provided above.
        /// </summary>
        /// <param name="ar">Contains the "state" object saved in the ConnectToServer function.</param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                SocketState ss = (SocketState)ar.AsyncState;

                int bytesRead = ss.theSocket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(ss.messageBuffer, 0, bytesRead);

                    ss.sb.Append(message);
                    ss.callMe(ss);
                    GetData(ss);
                }

            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// This is a small helper function that the client View code will call whenever it wants more data.
        /// Note: the client will probably want more data every time it gets data, and has finished processing
        /// it in its callbackFunction.
        /// </summary>
        /// <param name="ss">The SocketState object.</param>
        public static void GetData(SocketState ss)
        {
            ss.theSocket.BeginReceive(ss.messageBuffer, 0, 1024, SocketFlags.None, new AsyncCallback(Networking.ReceiveCallback), ss);
        }


        /// <summary>
        /// This function (along with its helper 'SendCallback') will allow a program to send data over a socket.
        /// This function converts the data into bytes and then sends them using socket.BeginSend.
        /// </summary>
        /// <param name="ss">The Socket object.</param>
        /// <param name="data">The data to send.</param>
        public static bool Send(Socket socket, string data)
        {
            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(data);
                socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, new AsyncCallback(Networking.SendCallback), socket);
                return true;
            }
            catch (Exception)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception)
                {
                }

                return false;
            }
        }

        /// <summary>
        /// This function "assists" the Send function. If all the data has been sent, then life is good and nothing
        /// needs to be done (note: you may, when first prototyping your program, put a WriteLine in here to see when
        /// data goes out).
        /// </summary>
        /// <param name="ar">Contains the "state" object saved in the ConnectToServer function.</param>
        private static void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            try
            {
                socket.EndSend(ar);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Main server function that uses the TCP protocol and waits for a client to connect
        /// to the server.
        /// </summary>
        /// <param name="callMe">The delegate function to be called once a new client has connected. </param>
        public static void ServerAwaitingClientLoop(Action<SocketState> callMe)
        {
            Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Debug, true);

            try
            {
                socket.Bind((EndPoint)new IPEndPoint(IPAddress.IPv6Any, 11000));
                socket.Listen(100);

                SocketState ss = new SocketState();
                ss.theSocket = socket;
                ss.callMe = callMe;

                Console.WriteLine("Waiting for a client to connect...");

                socket.BeginAccept(new AsyncCallback(Networking.AcceptNewClient), (object)ss);
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// Callback function called when a new client has connected to the server.
        /// 
        /// We begin receiving data from the client at this point.
        /// </summary>
        /// <param name="ar">The SocketState object.</param>
        public static void AcceptNewClient(IAsyncResult ar)
        {
            Console.WriteLine("A new client has connected to the server!");

            SocketState ss = (SocketState)ar.AsyncState;
            Socket theSocket = ss.theSocket;
            IAsyncResult asyncResult = ar;
            Socket socket = theSocket.EndAccept(asyncResult);

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Debug, true);

            SocketState ss1 = new SocketState();
            ss1.theSocket = socket;
            ss1.theSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Debug, true);
            ss.callMe(ss1);

            AsyncCallback callback = new AsyncCallback(Networking.AcceptNewClient);

            SocketState ss2 = ss;
            theSocket.BeginAccept(callback, (object)ss2);
        }
    }
}
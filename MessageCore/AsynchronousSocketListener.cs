using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageCore
{
    public class AsynchronousSocketListener
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private static ILog iLog = Log.GetInstance;
        private static string _ip = string.Empty;
        private static PduHandler pduHandler = new PduHandler();
        public AsynchronousSocketListener()
        {

        }

        public static void StartListening()
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[5];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    //Console.WriteLine("Waiting for a connection...");
                    iLog.Logger("- Waiting for a connection...", Enumerations.LogType.ListenerAudit);
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                iLog.Logger(string.Format("{0} : {1}", "StartListening", e.ToString()), Enumerations.LogType.ListenerException);
                //Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

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
            _ip = handler.RemoteEndPoint.ToString().Substring(0, handler.RemoteEndPoint.ToString().LastIndexOf(":"));
            iLog.Logger(string.Format("- Connect request recieved from {0}", _ip), Enumerations.LogType.ListenerAudit);
            iLog.Logger(string.Format("- Connected to {0}", _ip), Enumerations.LogType.ListenerAudit);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

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
                byte[] newBuffer = new byte[bytesRead];
                Array.Copy(state.buffer, newBuffer, bytesRead);
                iLog.Logger(string.Format("< Read {0} bytes from {1}", content.Length, _ip), Enumerations.LogType.ListenerAudit);
                iLog.Logger(string.Format("< Recieved Content : {0}", ToHexString(newBuffer)), Enumerations.LogType.ListenerAudit);
                var responseData = pduHandler.GetResponse(state.buffer);
                state.sb.Clear();
                state.sb.Append(Encoding.ASCII.GetString(
                    responseData, 0, responseData.Length));
                content = state.sb.ToString();
                iLog.Logger(string.Format("> Sent Content : {0}", ToHexString(responseData)), Enumerations.LogType.ListenerAudit);
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
                Send(handler, responseData);
                //if (content.IndexOf("<EOF>") > -1)
                //{
                //    // All the data has been read from the   
                //    // client. Display it on the console.  
                //    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                //        content.Length, content);
                //    // Echo the data back to the client.  
                //    Send(handler, content);
                //}
                //else
                //{
                //    // Not all data received. Get more.  
                    //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    //new AsyncCallback(ReadCallback), state);
                //}
            }
        }

        private static void Send(Socket handler, byte[] data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = data;// Encoding.ASCII.GetBytes(data);

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
                //Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                iLog.Logger(string.Format("> Sent {0} bytes to client.", bytesSent), Enumerations.LogType.ListenerAudit);
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                iLog.Logger(string.Format("{0} : {1}", "SendCallback", e.ToString()), Enumerations.LogType.ListenerException);
            }
        }
        public static string ToHexString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.ASCII.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString(); 
        }

        public static string ToHexString(byte[] bytes)
        {
            var sb = new StringBuilder();

            //var bytes = Encoding.ASCII.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}

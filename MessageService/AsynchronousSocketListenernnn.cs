using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageService
{
    public class AsynchronousSocketListenernnn
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public AsynchronousSocketListenernnn()
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
                Send(handler, content);
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
                //    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                //    new AsyncCallback(ReadCallback), state);
                //}
            }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            //00000015
            //80000002
            //00000000
            //00000001
            //61626364
            //00
            byte[] NormalResponse = new byte[29];
            NormalResponse[0] = Convert.ToByte(0x00);
            NormalResponse[1] = Convert.ToByte(0x00);
            NormalResponse[2] = Convert.ToByte(0x00);
            NormalResponse[3] = Convert.ToByte(0x15);
                       
            NormalResponse[4] = Convert.ToByte(0x80);
            NormalResponse[5] = Convert.ToByte(0x00);
            NormalResponse[6] = Convert.ToByte(0x00);
            NormalResponse[7] = Convert.ToByte(0x09);

            NormalResponse[8] = Convert.ToByte(0x00);
            NormalResponse[9] = Convert.ToByte(0x00);
            NormalResponse[10] = Convert.ToByte(0x00);
            NormalResponse[11] = Convert.ToByte(0x00);

            NormalResponse[12] = Convert.ToByte(0x00);
            NormalResponse[13] = Convert.ToByte(0x00);
            NormalResponse[14] = Convert.ToByte(0x00);
            NormalResponse[15] = Convert.ToByte(0x01);

            NormalResponse[16] = Convert.ToByte(0x61);
            NormalResponse[17] = Convert.ToByte(0x62);
            NormalResponse[18] = Convert.ToByte(0x63);
            NormalResponse[19] = Convert.ToByte(0x64);

            NormalResponse[20] = Convert.ToByte(0x00);
            // Begin sending the data to the remote device.  
            handler.BeginSend(NormalResponse, 0, NormalResponse.Length, 0,
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
    }
}

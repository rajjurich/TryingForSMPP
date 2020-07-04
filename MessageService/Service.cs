using MessageCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MessageService
{
    public partial class Service : ServiceBase
    {
        System.Threading.Thread serverThread;
        public Service()
        {
            InitializeComponent();
        }
        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {           
            serverThread = new System.Threading.Thread(AsynchronousSocketListener.StartListening);
            serverThread.Start();
        }

        protected override void OnStop()
        {
            serverThread.Abort();
        }
    }
}

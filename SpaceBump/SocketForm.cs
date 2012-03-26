using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SpaceBumper
{
    public partial class SocketForm : Form
    {
        private readonly int maxClients;
        private readonly GameSettings settings;
        private SocketManager socketManager;

        public SocketForm(GameSettings settings)
        {
            InitializeComponent();
            this.settings = settings;
            maxClients = settings.Players.Count(p => p == PlayerType.AI);

            Closing += (sender, args) => backgroundWorker1.CancelAsync();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }


        public SocketManager CreateSocketManager()
        {
            if (maxClients <= 0)
                return null;

            socketManager = new SocketManager(maxClients, settings.Port);
            socketManager.Start();
            backgroundWorker1.RunWorkerAsync();

            while (backgroundWorker1.IsBusy)
                Application.DoEvents();
            
            if (socketManager.CountClients < maxClients)
                return null;

            return socketManager;
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            label1.Text = (e.ProgressPercentage + "%");
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                label1.Text = "Canceled!";
            else if (e.Error != null)
                label1.Text = "Error: " + e.Error.Message;
            else
                label1.Text = "Done!";
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (socketManager.CountClients < maxClients)
            {
                if (worker.CancellationPending)
                {
                    socketManager.Stop();
                    e.Cancel = true;
                    break;
                }

                if (socketManager.HasIncoming)
                {
                    SocketHandler player = socketManager.ConnectPlayer();
                    string playerName = player.WaitForName();
                    Debug.WriteLine(playerName);
                }

                string state = string.Format("Waiting for {0} players..",
                                             maxClients -
                                             socketManager.CountClients);
                worker.ReportProgress(socketManager.CountClients * 100 / maxClients,
                                      state);
                Thread.Sleep(10);
            }
        }
    }
}
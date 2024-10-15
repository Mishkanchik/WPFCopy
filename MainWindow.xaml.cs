using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FileCopyApp
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            cancellationTokenSource = new CancellationTokenSource();
        }






        private void BtnFrom_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                txtFrom.Text = openFileDialog.FileName;
            }
        }






        private void BtnTo_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtTo.Text = folderDialog.SelectedPath;
            }
        }






        private async void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            string fromPath = txtFrom.Text;
            string toPath = txtTo.Text;
            if (!int.TryParse(txtCopies.Text, out int copies))
            {
                MessageBox.Show("Please enter a valid number of copies.");
                return;
            }

            if (!File.Exists(fromPath))
            {
                MessageBox.Show("Source file not found.");
                return;
            }

            if (!Directory.Exists(toPath))
            {
                MessageBox.Show("Target directory not found.");
                return;
            }

            btnCopy.IsEnabled = false;
            btnStop.IsEnabled = true;
            lblStatus.Content = "Status: Copying...";

            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            try
            {
                await Task.Run(() => CopyFiles(fromPath, toPath, copies, token), token);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Copying cancelled.");
            }
            finally
            {
                btnCopy.IsEnabled = true;
                btnStop.IsEnabled = false;
                lblStatus.Content = "Status: Ready";
            }
        }






        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }






        private void CopyFiles(string fromPath, string toPath, int copies, CancellationToken token)
        {
            for (int i = 1; i <= copies; i++)
            {
                token.ThrowIfCancellationRequested();

                string uniqueFileName = Path.Combine(toPath, $"{Path.GetFileNameWithoutExtension(fromPath)}_{i}{Path.GetExtension(fromPath)}");

                using (FileStream sourceStream = File.Open(fromPath, FileMode.Open))
                using (FileStream destinationStream = File.Create(uniqueFileName))
                {
                    sourceStream.CopyTo(destinationStream);
                }

                Dispatcher.Invoke(() => lblStatus.Content = $"Status: Copying {i}/{copies}...");

                Task.Delay(1000).Wait();
            }

            Dispatcher.Invoke(() => MessageBox.Show("Copying completed!"));
        }
    }
}

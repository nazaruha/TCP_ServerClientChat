using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCPUserMessage;
using System.Drawing;

namespace TCP_ClientChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ip_server = "127.0.0.1";
        private int port_server = 1002;
        private TcpClient client = new TcpClient();
        private NetworkStream ns;
        private Thread thread;
        private UserMessage userMsg = new UserMessage();
        private List<string> Extensions { get; set; } = new List<string> { ".bmp", ".jpg", ".png" };
        private bool isPhoto { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ReceiveData(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int byte_count;
            while ((byte_count = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        UserMessage msg = UserMessage.Deserialize(buffer);
                        switch (msg.MessageType)
                        {
                            case TypeMessage.Login:
                                {
                                    if (msg.Id != userMsg.Id)
                                        lbChat.Items.Add($"{msg.Name}:{msg.Text}");
                                    break;
                                }
                            case TypeMessage.Logout:
                                {
                                    if (msg.Id != userMsg.Id)
                                        lbChat.Items.Add($"{msg.Name}:{msg.Text}");
                                    break;
                                }
                            case TypeMessage.Message:
                                {
                                    lbChat.Items.Add($"{msg.Name}:{msg.Text}");
                                    break;
                                }
                            default:
                                break;
                        }
                        lbChat.Items.MoveCurrentToLast();
                        lbChat.ScrollIntoView(lbChat.Items.CurrentItem);
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show($"{ex.Message}");
                        //return;
                    }
                });
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Input name");
                return;
            }
            if (!isPhoto)
            {
                MessageBox.Show("Choose Image");
                return;
            }
            try
            {
                userMsg.Name = txtName.Text;
                userMsg.Id = Guid.NewGuid().ToString(); // creates unic id 
                client.Connect(IPAddress.Parse(ip_server), port_server);
                lbChat.Items.Add($"Connected to the server \"{ip_server}:{port_server}\"");
                ns = client.GetStream();
                thread = new Thread(c => ReceiveData((TcpClient)c));
                thread.Start(client);

                userMsg.MessageType = TypeMessage.Login;
                userMsg.Text = "Join the chat";
                byte[] buffer = userMsg.Serialize();
                ns.Write(buffer);
            }
            catch
            {
                MessageBox.Show("Connection failed");
            }
        }

        private void btnSend_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtMessage.Text)) return;
            userMsg.MessageType = TypeMessage.Message;
            userMsg.Text = txtMessage.Text;
            byte[] buffer = userMsg.Serialize();
            ns.Write(buffer, 0, buffer.Length);
            txtMessage.Clear();
        }
        private void ClosingWindow_CLick(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                userMsg.MessageType = TypeMessage.Logout;
                userMsg.Text = "Leave the chat";
                byte[] buffer = userMsg.Serialize();
                ns.Write(buffer, 0, buffer.Length);

                client.Client.Shutdown(SocketShutdown.Send);
                thread.Join();
                ns.Close();
                client.Close();
            }
            catch
            {
                return;
            }
        }

        private void Photo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image (*.bmp, *.jpg, *.png)|*.bmp; *.jpg; *.png|All (*.*)|*.*";
            string path = "";
            if (openFileDialog.ShowDialog() == true)
            {
                if (!CheckPhotoExtension(openFileDialog.FileName))
                    return;
                path = openFileDialog.FileName;
            }
            Photo.Source = new BitmapImage(new Uri(path));
            isPhoto = true;
        }

        private bool CheckPhotoExtension(string path)
        {
            string extension = System.IO.Path.GetExtension(path);
            if (Extensions.Contains(extension))
            {
                return true;
            }
            else
            {
                isPhoto = false;
                MessageBox.Show("Uncorrect extension. Choose another file", "Invalid Photo", MessageBoxButton.OK);
                return false;
            }
        }
    }
}

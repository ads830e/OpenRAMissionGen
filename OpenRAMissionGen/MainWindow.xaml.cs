using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace OpenRAMissionGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        
        
        private void MenuItemHelp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("This Program Is Made By Tuo Qiang.\nPlease Contact tuoqaing@outlook.com.", "About");
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnLoadIni_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Select RA95 Ini Mission File";
            fdlg.Filter = "(*.ini)|*.ini";
            fdlg.RestoreDirectory = false;
            if (fdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBoxIni.Text = fdlg.FileName.Trim();// System.IO.Path.GetFileNameWithoutExtension(fdlg.FileName);
            }

        }

        private void BtnGen_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxIni.Text == null) return;
            if (String.IsNullOrWhiteSpace(TextBoxIni.Text )) return;
            try
            {
                String[] Content = File.ReadAllLines(TextBoxIni.Text);
            }
            catch(Exception )
            {
                System.Windows.Forms.MessageBox.Show("Ini Paser Error!", "Error");
            }
            
        }
    }
}

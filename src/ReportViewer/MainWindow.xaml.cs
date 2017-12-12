using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ReportViewer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void windowFileDrow(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Count() != 1)
                    return;

                string[] parts = files[0].Split('.');

                if (parts.Last() != "json")
                {
                    MessageBox.Show("The file must be *.json", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    dgReport.ItemsSource = JsonConvert.DeserializeObject<List<HistoryElement>>(File.ReadAllText(files[0]));

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: Could not read file from disk. Original error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void dgReportMouseDoubleclick(object sender, MouseButtonEventArgs e)
        {
            if (dgReport.SelectedIndex == -1)
                return;

            try { Process.Start(((HistoryElement)dgReport.SelectedItem).Url); }
            catch { }
        }
    }
}

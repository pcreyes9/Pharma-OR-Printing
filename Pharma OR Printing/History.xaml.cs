using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using System.IO;
using System.Windows.Input;

namespace Pharma_OR_Printing
{
    /// <summary>
    /// Interaction logic for History.xaml
    /// </summary>
    public partial class History : Window
    {
        string conString = "Data Source=PSASERVER;Initial Catalog=PSADBLIVE;Persist Security Info=True;User ID=sa;Password=p$a@dm1n;";
        string category = "pharma_name";
        public History()
        {
            InitializeComponent();

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();

                        string query = "SELECT * FROM pharma_payment_history ORDER BY id DESC";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            histo_dg.ItemsSource = dt.DefaultView;
                        }
                    }
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Connection Failed. Check server connection and start the application again.", "PSA Receipt Printing");
                //Application.Exit();
            }
        }

        private void search_tb_KeyDown(object sender, KeyEventArgs e)
        {

            if (int.TryParse(search_tb.Text, out int number))
            {
                category = "or_no";
                
            }
            else 
            {
                category = "pharma_name";
            }

           
            if (e.Key == Key.Enter)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(conString))
                    {
                        if (con.State == System.Data.ConnectionState.Closed)
                        {
                            con.Open();

                            //string category = (catag_cmb.SelectedItem as ComboBoxItem)?.Content.ToString();
                            string query = $"SELECT * FROM pharma_payment_history WHERE {category} LIKE @search ORDER BY id DESC";

                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@search",  search_tb.Text + "%" );

                                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);
                                histo_dg.ItemsSource = dt.DefaultView;
                            }
                        }
                    }

                }
                catch (Exception)
                {
                    MessageBox.Show("Connection Failed. Check server connection and start the application again.", "PSA Receipt Printing");
                    //Application.Exit();
                }
            }
        }

        private void search_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(search_tb.Text, out int number))
            {
                category = "or_no";

            }
            else
            {
                category = "pharma_name";
            }

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();

                        //string category = (catag_cmb.SelectedItem as ComboBoxItem)?.Content.ToString();
                        string query = $"SELECT * FROM pharma_payment_history WHERE {category} LIKE @search ORDER BY id DESC";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@search", search_tb.Text + "%");

                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            histo_dg.ItemsSource = dt.DefaultView;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Connection Failed. Check server connection and start the application again.", "PSA Receipt Printing");
                //Application.Exit();
            }
            
        }
        private void ExportDataGridToExcel(DataGrid dataGrid, string filePath)
        {
            // Convert DataGrid to DataTable
            DataView dataView = dataGrid.ItemsSource as DataView;
            if (dataView == null)
            {
                MessageBox.Show("No data to export.");
                return;
            }
            DataTable dt = dataView.ToTable();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Cell(1, 1).InsertTable(dt);
                workbook.SaveAs(filePath);
            }

            MessageBox.Show("Exported successfully to:\n" + filePath, "Successful Export");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Get Desktop Path
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Create a new folder (if it doesn't exist)
            string folderName = "PHARMA EXPORT FOLDER";
            string exportFolderPath = System.IO.Path.Combine(desktopPath, folderName);
            Directory.CreateDirectory(exportFolderPath); // Creates it if missing

            // Create the file name with today's date, hour, minute, and second
            string todayDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"Pharma Payment History {todayDate}.xlsx";

            // Combine the folder and file name
            string filePath = System.IO.Path.Combine(exportFolderPath, fileName);

            // Export
            ExportDataGridToExcel(histo_dg, filePath);
        }
    }
}

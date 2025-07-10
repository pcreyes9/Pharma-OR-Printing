using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System;
using System.Windows.Controls; // For PrintDialog (WPF)




namespace Pharma_OR_Printing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string conString = "Data Source=PSASERVER;Initial Catalog=PSADBLIVE;Persist Security Info=True;User ID=sa;Password=p$a@dm1n;";
        string payment_date;
        PrintDocument printDoc = new PrintDocument();

        public MainWindow()
        {
            InitializeComponent();
            businessNM_tb.Text = pharmaNM_tb.Text;
            

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                        //MessageBox.Show("Connection Success.", "PSA Pharma OR Printing");
                        con.Close();

                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Connection Failed. Check server connection and start the application again.", "PSA Receipt Printing");
                //Application.Exit();
            }
        }

        private void pharmaNM_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                businessNM_tb.Text = pharmaNM_tb.Text;
                using (SqlConnection con = new SqlConnection(conString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                        string query = "SELECT pharma_name FROM pharmas WHERE pharma_name LIKE @pharmaName ORDER BY pharma_name ASC";

                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@pharmaName", pharmaNM_tb.Text + "%");

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        pharma_dg.ItemsSource = dt.DefaultView;

                        if (string.IsNullOrEmpty(pharmaNM_tb.Text))
                        {
                            pharma_dg.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            pharma_dg.Visibility = Visibility.Visible;
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

        private void pharma_dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pharma_dg.SelectedItem is DataRowView row)
            {
                pharmaNM_tb.Text = row["pharma_name"].ToString();
                pharma_dg.Visibility = Visibility.Collapsed;
            }
        }

        private void print_btn_Click(object sender, RoutedEventArgs e)
        {
            payment_date = DateTime.Now.ToString("MM/dd/yyyy");

            MessageBoxResult result = MessageBox.Show(
            "Print the following details? \n\n" +
                "Name: " + pharmaNM_tb.Text + "\n" +
                "Amount: " + amount_tb.Text + "\n" +
                "Date of payment: " + payment_date, // Message
            "Confirmation",                 // Title
            MessageBoxButton.YesNo);  // Buttons
            //MessageBoxImage.Question);      // Icon

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(NumberToWords(int.Parse(amount_tb.Text)) + " Pesos", "PSA Receipt Printing");
                printDoc.PrinterSettings.PrinterName = "EPSON LX-310";
                printDoc.PrintPage += new PrintPageEventHandler(PrintPageHandler);
                printDoc.DefaultPageSettings.Landscape = true;

                printDoc.Print();
            }
            else if (result == MessageBoxResult.No)
            {
                // Do something else
            }
        }

        private void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            

            Font font = new Font("Arial", 10);
            float x = 100;
            float y = 100;

            e.Graphics.DrawString("Paul Reyes", font, Brushes.Black, new System.Drawing.Point(175, 342));
            e.Graphics.DrawString(payment_date, font, Brushes.Black, new System.Drawing.Point(583, 294));
            //e.Graphics.DrawString(amount, font, Brushes.Black, new System.Drawing.Point(612, 427));
            e.Graphics.DrawString("Marsha F. Moreno", font, Brushes.Black, new System.Drawing.Point(575, 611));
            //e.Graphics.DrawString(result + "Pesos Only", font, Brushes.Black, new System.Drawing.Point(182, 405));

            //e.Graphics.DrawString(NumberToWords(int.Parse(amount_tb.Text)) + " Pesos", font, Brushes.Black, x, y + 30);
        }

        static string NumberToWords(int number)
        {
            if (number == 0)
                return "Zero";

            return ConvertToWords(number).Trim();
        }

        static string ConvertToWords(int number)
        {
            string[] units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
                           "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen",
                           "Sixteen", "Seventeen", "Eighteen", "Nineteen" };

            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (number < 20)
                return units[number];

            if (number < 100)
                return tens[number / 10] + (number % 10 != 0 ? " " + ConvertToWords(number % 10) : "");

            if (number < 1000)
                return units[number / 100] + " Hundred" + (number % 100 != 0 ? " " + ConvertToWords(number % 100) : "");

            if (number < 1000000)
                return ConvertToWords(number / 1000) + " Thousand" + (number % 1000 != 0 ? " " + ConvertToWords(number % 1000) : "");

            if (number <= 2000000)
                return ConvertToWords(number / 1000000) + " Million" + (number % 1000000 != 0 ? " " + ConvertToWords(number % 1000000) : "");

            return ""; // out of range (should not happen with current input limits)
        }
    }
}
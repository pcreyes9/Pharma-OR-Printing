using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            catag_cmb.Items.Add("Pharma Name");
            catag_cmb.Items.Add("OR Number");

            catag_cmb.SelectedItem = "Pharma Name";

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();

                        string query = "SELECT * FROM pharma_payment_history";

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

            if ((catag_cmb.SelectedItem as ComboBoxItem)?.Content.ToString() == "Pharma Name")
            {
                category = "pharma_name";
            }
            else if ((catag_cmb.SelectedItem as ComboBoxItem)?.Content.ToString() == "OR Number")
            {
                category = "or_no";
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
                            string query = $"SELECT * FROM pharma_payment_history WHERE {category} LIKE @search";

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
    }
}

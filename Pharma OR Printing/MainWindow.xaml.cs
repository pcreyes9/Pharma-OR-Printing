using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;

using System.Drawing.Printing;
using System.Windows.Input;
using System.Windows.Documents;
using System.Text; // For PrintDialog (WPF)




namespace Pharma_OR_Printing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string conString = "Data Source=PSASERVER;Initial Catalog=PSADBLIVE;Persist Security Info=True;User ID=sa;Password=p$a@dm1n;";
        string payment_date, words, printAmt, address, specifyTXT;
        double amount, dbAmount;
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

        private void tableInsert()
        {
            try
            {
                //MessageBox.Show(dbAmount.ToString());
                //decimal decAmount = decimal.Parse(amount_tb.Text);

                using (SqlConnection con = new SqlConnection(conString))
                {
                    con.Open();
                    string query = "INSERT INTO pharma_payment_history (or_no, business_styleName, pharma_name, amount, tin, address, bank, no, check_date, specify ) VALUES (@or_no, @business_styleName, @pharma_name, @amount, @tin, @address, @bank, @no, @check_date, @specify)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@or_no", orNo_tb.Text);
                        cmd.Parameters.AddWithValue("@business_styleName", businessNM_tb.Text);
                        cmd.Parameters.AddWithValue("@pharma_name", pharmaNM_tb.Text);
                        cmd.Parameters.AddWithValue("@address", address);
                        cmd.Parameters.AddWithValue("@amount", decimal.Parse(amount_tb.Text));
                        cmd.Parameters.AddWithValue("@tin", tin_tb.Text);
                        cmd.Parameters.AddWithValue("@bank", bank_tb.Text);
                        cmd.Parameters.AddWithValue("@no", no_tb.Text);
                        cmd.Parameters.AddWithValue("@check_date", date_tb.Text);
                        cmd.Parameters.AddWithValue("@specify", specify_tb.Text);

                        cmd.ExecuteNonQuery();
                    }

                    //MessageBox.Show("Data successfully added.");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while inserting data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void print_btn_Click(object sender, RoutedEventArgs e)
        {
            // CHECK IF PHARMA NAME IS ALREADY IN THE DATABASE
            if (PharmaNameExists(pharmaNM_tb.Text))
            {
                //MessageBox.Show("Pharma name already exists.");
            }
            else
            {
                MessageBoxResult ans = MessageBox.Show("Do you want to keep the new Pharma Brand?", 
                    "Confirmation",                 // Title
                    MessageBoxButton.YesNo);  // Buttons);

                if (ans == MessageBoxResult.Yes)
                {
                    try
                    {
                        //MessageBox.Show(dbAmount.ToString());
                        //decimal decAmount = decimal.Parse(amount_tb.Text);

                        using (SqlConnection con = new SqlConnection(conString))
                        {
                            con.Open();
                            string query = "INSERT INTO pharmas (pharma_name) VALUES (@name)";
                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@name", pharmaNM_tb.Text);

                                cmd.ExecuteNonQuery();
                            }

                            //MessageBox.Show("Data successfully added.");

                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while inserting data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else if (ans == MessageBoxResult.No)
                {
                    // Do something else
                }
            }


            // Convert the amount to double format for printing
            dbAmount = double.Parse(amount_tb.Text);
            amount = double.Parse(amount_tb.Text);
            var culture = new System.Globalization.CultureInfo("en-PH"); // English - Philippines
            printAmt = amount.ToString("C", culture);
            //MessageBox.Show(printAmt);

            // Convert the amount to words
            words = ConvertAmountToWords(amount);
            words = CutStringWithoutCuttingWord(words, 60);
            //MessageBox.Show(words);


            // Get the current date and format it
            payment_date = DateTime.Now.ToString("MM/dd/yyyy");

            // Get the address from the RichTextBox and convert it to a string
            // Get text from RichTextBox
            address = new TextRange(address_rtb.Document.ContentStart, address_rtb.Document.ContentEnd).Text.Trim();

            // Wrap the address
            address = CutStringWithoutCuttingWord(address, 76);
            //MessageBox.Show(address, "Address for Printing");


            // Cut lines of specify text
            specifyTXT = specify_tb.Text;
            specifyTXT = CutStringWithoutCuttingWord(specifyTXT, 29);

            
            //MessageBox.Show(specifyTXT);

            MessageBoxResult result = MessageBox.Show(
            "Print the following details? \n\n" +
                "Name: " + pharmaNM_tb.Text + "\n" +
                "Amount: " + printAmt + "\n" +
                "Amt in Txt: " + words + "\n" +
                "Date of payment: " + payment_date + "\n" +
                "Address: " + address,
            // Message
            "Confirmation",                 // Title
            MessageBoxButton.YesNo);  // Buttons
            //MessageBoxImage.Question);      // Icon

            if (result == MessageBoxResult.Yes)
            {
                tableInsert();

                //MessageBox.Show(NumberToWords(int.Parse(amount_tb.Text)) + " Pesos", "PSA Receipt Printing");
                printDoc.PrinterSettings.PrinterName = "EPSON LX-310";
                printDoc.PrintPage += new PrintPageEventHandler(PrintPageHandler);
                printDoc.DefaultPageSettings.Landscape = true;

                printDoc.Print();
                cleanVars();
            }
            else if (result == MessageBoxResult.No)
            {
                // Do something else
            }
        }
        

        private void pharma_dg_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void pharmaNM_tb_LostFocus(object sender, RoutedEventArgs e)
        {
            pharma_dg.Visibility = Visibility.Collapsed;
        }

        private void history_btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is History)
                {
                    window.Activate(); // Bring it to front if it's already open
                    return;
                }
            }

            History historyWindow = new History();
            historyWindow.Show();
        }

        private void address_rtb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string currentText = new TextRange(
                address_rtb.Document.ContentStart,
                address_rtb.Document.ContentEnd
            ).Text;

            if (currentText.Trim().Length >= 140)
            {
                e.Handled = true; // ❌ Block input
            }
        }

        private void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            Font font = new Font("Courier New", 11, System.Drawing.FontStyle.Regular);
            Font addFont = new Font("Courier New", 9, System.Drawing.FontStyle.Regular);
            float x = 100;
            float y = 100;

            // Draw header only once
            e.Graphics.DrawString("SERVICE INVOICE", font, Brushes.Black, new PointF(470, 250));

            SolidBrush blackBrush = new SolidBrush(System.Drawing.Color.Black);
            System.Drawing.Rectangle boxRect = new System.Drawing.Rectangle(453, 269, 160, 17);
            e.Graphics.FillRectangle(blackBrush, boxRect);

            if (!string.IsNullOrEmpty(pharmaNM_tb.Text))
            {
                e.Graphics.DrawString(payment_date, font, Brushes.Black, new PointF(583, 310));
                e.Graphics.DrawString(pharmaNM_tb.Text, font, Brushes.Black, new PointF(175, 355));
                e.Graphics.DrawString(businessNM_tb.Text, font, Brushes.Black, new PointF(451, 375));
                e.Graphics.DrawString(tin_tb.Text, font, Brushes.Black, new PointF(120, 378));

                if (address.Length > 76)
                {
                    e.Graphics.DrawString(address, addFont, Brushes.Black, new PointF(135, 395));
                }
                else
                {
                    e.Graphics.DrawString(address, font, Brushes.Black, new PointF(135, 398));
                }

                e.Graphics.DrawString(words, font, Brushes.Black, new PointF(179, 421));
                e.Graphics.DrawString(printAmt, font, Brushes.Black, new PointF(610, 443));
                e.Graphics.DrawString(bank_tb.Text, font, Brushes.Black, new PointF(585, 514));
                e.Graphics.DrawString(no_tb.Text, font, Brushes.Black, new PointF(585, 531));
                e.Graphics.DrawString(date_tb.Text, font, Brushes.Black, new PointF(585, 547));
                e.Graphics.DrawString("Marsha F. Moreno", font, Brushes.Black, new PointF(575, 627));
                e.Graphics.DrawString(specifyTXT, addFont, Brushes.Black, new PointF(129, 675));
            }
        }
        private string ConvertAmountToWords(double amount)
        {
            int wholePart = (int)amount;
            int cents = (int)Math.Round((amount - wholePart) * 100);
            //MessageBox.Show(cents.ToString());

            string temp = NumberToWords(wholePart);

            if (cents == 0)
            {
                return $"{temp} Pesos Only";
            }

            return $"{temp} Pesos and {cents:00}/100 Centavos Only";
        }

        private void servIn_btn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
            "Print SERVICE INVOICE BAR?",
            // Message
            "Confirmation",                 // Title
            MessageBoxButton.YesNo);  // Buttons
            //MessageBoxImage.Question);      // Icon

            if (result == MessageBoxResult.Yes)
            {
                printDoc.PrinterSettings.PrinterName = "EPSON LX-310";
                printDoc.PrintPage += new PrintPageEventHandler(PrintPageHandler);
                printDoc.DefaultPageSettings.Landscape = true;

                printDoc.Print();
                cleanVars();
            }
            else if (result == MessageBoxResult.No)
            {
                // Do something else
            }

            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cleanVars();
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

        //public string CutStringWithoutCuttingWord(string text, int maxLineLength)
        //{
        //    if (string.IsNullOrWhiteSpace(text))
        //        return string.Empty;

        //    var words = text.Split(' ');
        //    var result = new StringBuilder();
        //    var currentLine = new StringBuilder();

        //    foreach (var word in words)
        //    {
        //        if (currentLine.Length + word.Length + 1 > maxLineLength)
        //        {
        //            result.AppendLine(currentLine.ToString().TrimEnd());
        //            currentLine.Clear();
        //        }
        //        currentLine.Append(word + " ");
        //    }

        //    if (currentLine.Length > 0)
        //        result.AppendLine(currentLine.ToString().TrimEnd());

        //    return result.ToString();
        //}

        //public static string WrapTextByLength(string input, int maxLength)
        //{
        //    if (string.IsNullOrWhiteSpace(input))
        //        return string.Empty;

        //    List<string> lines = new List<string>();
        //    string[] para = input.Split(' ');

        //    string currentLine = "";

        //    foreach (string word in para)
        //    {
        //        if ((currentLine + word).Length > maxLength)
        //        {
        //            lines.Add(currentLine.TrimEnd());
        //            currentLine = "";
        //        }
        //        currentLine += word + " ";
        //    }

        //    if (!string.IsNullOrWhiteSpace(currentLine))
        //    {
        //        lines.Add(currentLine.TrimEnd());
        //    }

        //    return string.Join("\n", lines);
        //}

        public static string CutStringWithoutCuttingWord(string text, int maxLineLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var words = text.Split(' ');
            var result = new StringBuilder();
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 > maxLineLength)
                {
                    result.AppendLine(currentLine.ToString().TrimEnd());
                    currentLine.Clear();
                }
                currentLine.Append(word + " ");
            }

            if (currentLine.Length > 0)
                result.AppendLine(currentLine.ToString().TrimEnd());

            return result.ToString();
        }


        bool PharmaNameExists(string pharmaName)
        {
            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                string query = "SELECT 1 FROM pharmas WHERE pharma_name = @pharma_name";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@pharma_name", pharmaName);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        return reader.HasRows;  // True if it exists
                    }
                }
            }
        }

        


        private void cleanVars()
        {
            businessNM_tb.Text = "";
            pharmaNM_tb.Text = "";
            address_rtb.Document.Blocks.Clear();
            amount_tb.Text = "";
            tin_tb.Text = "";
            bank_tb.Text = "";
            no_tb.Text = "";
            date_tb.Text = "";
            specify_tb.Text = "";
            words = "";
            printAmt = "";
            orNo_tb.Text = "";
        }
    }
}
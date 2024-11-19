using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace AaliyahAllie_PROG6212_POE_FINAL_PART
{
    public partial class HRView : Window
    {
        private readonly string connectionString = "Data Source=hp820g4\\SQLEXPRESS;Initial Catalog=POE;Integrated Security=True;";

        public HRView()
        {
            InitializeComponent();
            LoadClaimsData();
        }

        private void LoadClaimsData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Updated query to match columns in the screenshot
                    string query = "SELECT ClaimID, ClassTaught, NumberOfSessions, TotalAmount, ClaimStatus FROM Claims";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    ClaimsDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading claims data: {ex.Message}");
            }
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Generating a report based on the updated column names
                    string query = "SELECT ClaimID, TotalAmount FROM Claims WHERE ClaimStatus = 'Approved'";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    string report = "Approved Claims Report\n\n";
                    while (reader.Read())
                    {
                        report += $"Claim ID: {reader["ClaimID"]}, Total Amount: {reader["TotalAmount"]}\n";
                    }

                    MessageBox.Show(report, "Report");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}");
            }
        }

        private void UpdateLecturerButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsDataGrid.SelectedItem is DataRowView row)
            {
                int claimId = Convert.ToInt32(row["ClaimID"]); // Updated to use ClaimID
                UpdateLecturerWindow updateWindow = new UpdateLecturerWindow(claimId);
                updateWindow.ShowDialog();
                LoadClaimsData(); // Refresh data grid after update
            }
            else
            {
                MessageBox.Show("Please select a claim to update.");
            }
        }
    }
}

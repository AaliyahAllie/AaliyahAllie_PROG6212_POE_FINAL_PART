using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
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
                    string query = "SELECT ClaimID, ClassTaught, NumberOfSessions, HourlyRate, TotalAmount, ClaimStatus FROM Claims";
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
                // Example: Generating a report for approved claims
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Claims WHERE ClaimStatus = 'Approved'";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    string report = "Approved Claims Report\n\n";
                    while (reader.Read())
                    {
                        report += $"Claim ID: {reader["ClaimID"]}, Total: {reader["TotalAmount"]}\n";
                    }

                    MessageBox.Show(report, "Report");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}");
            }
        }

        private void SearchLecturerButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                MessageBox.Show("Please enter both first and last names to search for a lecturer.");
                return;
            }

            LecturerInfoWindow lecturerInfoWindow = new LecturerInfoWindow(firstName, lastName);
            lecturerInfoWindow.ShowDialog();
        }

        private void ApproveClaimButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsDataGrid.SelectedItem is DataRowView selectedRow)
            {
                int claimId = Convert.ToInt32(selectedRow["ClaimID"]);
                UpdateClaimStatus(claimId, "Approved");
            }
            else
            {
                MessageBox.Show("Please select a claim to approve.");
            }
        }

        private void DeleteClaimButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsDataGrid.SelectedItem is DataRowView selectedRow)
            {
                int claimId = Convert.ToInt32(selectedRow["ClaimID"]);
                if (MessageBox.Show("Are you sure you want to delete this claim?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    DeleteClaim(claimId);
                }
            }
            else
            {
                MessageBox.Show("Please select a claim to delete.");
            }
        }

        private void UpdateClaimStatus(int claimId, string newStatus)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Claims SET ClaimStatus = @ClaimStatus WHERE ClaimID = @ClaimID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ClaimStatus", newStatus);
                    cmd.Parameters.AddWithValue("@ClaimID", claimId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Claim status updated successfully.");
                        LoadClaimsData(); // Refresh the data grid
                    }
                    else
                    {
                        MessageBox.Show("Failed to update claim status.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating claim status: {ex.Message}");
            }
        }

        private void DeleteClaim(int claimId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM Claims WHERE ClaimID = @ClaimID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ClaimID", claimId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Claim deleted successfully.");
                        LoadClaimsData(); // Refresh the data grid
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete claim.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting claim: {ex.Message}");
            }
        }
        private async Task AutoApprovePendingClaimsAsync()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "UPDATE Claims SET ClaimStatus = 'Approved' WHERE ClaimStatus = 'Pending' ";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"{rowsAffected} claims auto-approved.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during auto-approval: {ex.Message}");
            }
        }
        private async void AutoUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "UPDATE Claims SET ClaimStatus = 'PROCESSING' WHERE ClaimStatus = 'WAITING'";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"{rowsAffected} claims updated to 'PROCESSING'.");
                        LoadClaimsData(); // Refresh the data grid
                    }
                    else
                    {
                        MessageBox.Show("No claims were updated. There may be no claims with the status 'WAITING'.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating claim statuses: {ex.Message}");
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(); // Create a new instance of MainWindow
            mainWindow.Show(); // Show the MainWindow
            this.Close(); // Close the HRView window
        }
    }
}
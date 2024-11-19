using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace AaliyahAllie_PROG6212_POE_FINAL_PART
{
    /// <summary>
    /// Interaction logic for ProgrammeCoordinatorDashboard.xaml
    /// </summary>
    public partial class ProgrammeCoordinatorDashboard : Window
    {
        // Constructor for the ProgrammeCoordinatorDashboard
        public ProgrammeCoordinatorDashboard()
        {
            InitializeComponent();
            LoadClaims();
        }

        // Method to load claims into the ListView
        private void LoadClaims()
        {
            List<Claim> claims = GetClaimsFromDatabase();
            ClaimsListView.ItemsSource = claims;
        }

        // Fetch claims from the database
        private List<Claim> GetClaimsFromDatabase()
        {
            List<Claim> claims = new List<Claim>();
            string connectionString = "Data Source=hp820g4\\SQLEXPRESS;Initial Catalog=POE;Integrated Security=True;";
            string query = "SELECT ClaimID, ClassTaught, TotalAmount, ClaimStatus, NumberOfSessions FROM Claims";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        claims.Add(new Claim
                        {
                            ClaimID = reader.GetInt32(0),
                            ClassTaught = reader.GetString(1),
                            TotalAmount = reader.GetDecimal(2),
                            ClaimStatus = reader.GetString(3),
                            NumberOfSessions = reader.GetInt32(4) // Added to support the number of hours worked
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            return claims;
        }

        // Updates the claims status in the database
        private void UpdateClaimStatus(int claimID, string newStatus)
        {
            string connectionString = "Data Source=hp820g4\\SQLEXPRESS;Initial Catalog=POE;Integrated Security=True;";
            string query = "UPDATE Claims SET ClaimStatus = @ClaimStatus WHERE ClaimID = @ClaimID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ClaimStatus", newStatus);
                command.Parameters.AddWithValue("@ClaimID", claimID);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("Claim status updated successfully!");
                    LoadClaims(); // Reload claims after updating
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        // Validate a claim based on predefined criteria like hourly rate and total hours worked
        private bool ValidateClaim(Claim claim)
        {
            const decimal minHourlyRate = 100.00m;
            const decimal maxHourlyRate = 200.00m;
            const int minHours = 1;
            const int maxHours = 40;

            // Assuming the TotalAmount is the product of the hourly rate and number of hours worked
            decimal hourlyRate = claim.TotalAmount / claim.NumberOfSessions;

            if (hourlyRate < minHourlyRate || hourlyRate > maxHourlyRate)
            {
                ValidationFeedbackText.Text = "Claim does not meet the required hourly rate criteria.";
                return false;
            }

            if (claim.NumberOfSessions < minHours || claim.NumberOfSessions > maxHours)
            {
                ValidationFeedbackText.Text = "Claim does not meet the required hours worked criteria.";
                return false;
            }

            ValidationFeedbackText.Text = ""; // Clear any previous validation feedback
            return true;
        }

        // Approve button changes status of claims to approved
        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsListView.SelectedItem is Claim selectedClaim)
            {
                if (ValidateClaim(selectedClaim)) // Validate the claim before approving
                {
                    UpdateClaimStatus(selectedClaim.ClaimID, "Approved");
                }
            }
            else
            {
                MessageBox.Show("Please select a claim to approve.");
            }
        }

        // Reject button will change status to rejected
        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsListView.SelectedItem is Claim selectedClaim)
            {
                UpdateClaimStatus(selectedClaim.ClaimID, "Rejected");
            }
            else
            {
                MessageBox.Show("Please select a claim to reject.");
            }
        }

        // Pending button will change status to pending
        private void PendingButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsListView.SelectedItem is Claim selectedClaim)
            {
                UpdateClaimStatus(selectedClaim.ClaimID, "Pending");
            }
            else
            {
                MessageBox.Show("Please select a claim to set as pending.");
            }
        }

        // Handle the download of the supporting document
        private void DownloadDocument_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsListView.SelectedItem is Claim selectedClaim)
            {
                // Fetch documents from the database for the selected claim
                List<SupportingDocument> documents = GetSupportingDocuments(selectedClaim.ClaimID);

                // If documents exist, proceed with download
                if (documents.Count > 0)
                {
                    foreach (var doc in documents)
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog
                        {
                            FileName = doc.DocName,
                            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
                        };

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            // Download the document to the specified location
                            File.Copy(doc.FilePath, saveFileDialog.FileName);
                            MessageBox.Show("Document downloaded successfully.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No supporting documents available for this claim.");
                }
            }
        }

        // Fetch supporting documents from the database
        private List<SupportingDocument> GetSupportingDocuments(int claimID)
        {
            List<SupportingDocument> documents = new List<SupportingDocument>();
            string connectionString = "Data Source=hp820g4\\SQLEXPRESS;Initial Catalog=POE;Integrated Security=True;";
            string query = "SELECT DocID, DocName, FilePath FROM SupportingDocuments WHERE ClaimsID = @ClaimID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ClaimID", claimID);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        documents.Add(new SupportingDocument
                        {
                            DocID = reader.GetInt32(0),
                            DocName = reader.GetString(1),
                            FilePath = reader.GetString(2)
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            return documents;
        }
    }

    // Claim model class
    public class Claim
    {
        public int ClaimID { get; set; }
        public string ClassTaught { get; set; }
        public decimal TotalAmount { get; set; }
        public string ClaimStatus { get; set; }
        public int NumberOfSessions { get; set; } // Added for hours worked
    }

    // SupportingDocument model class
    public class SupportingDocument
    {
        public int DocID { get; set; }
        public string DocName { get; set; }
        public string FilePath { get; set; }
    }
}

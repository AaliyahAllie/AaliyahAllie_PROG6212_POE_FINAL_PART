using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace AaliyahAllie_PROG6212_POE_FINAL_PART
{
    public partial class ProgrammeCoordinatorDashboard : Window
    {
        public ProgrammeCoordinatorDashboard()
        {
            InitializeComponent();
            LoadClaims();
        }

        private void LoadClaims()
        {
            List<Claim> claims = GetClaimsFromDatabase();
            ClaimsListView.ItemsSource = claims;
        }

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
                            NumberOfSessions = reader.GetInt32(4)
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
                    LoadClaims();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private bool ValidateClaim(Claim claim)
        {
            const decimal minHourlyRate = 100.00m;
            const decimal maxHourlyRate = 200.00m;
            decimal hourlyRate = claim.TotalAmount / claim.NumberOfSessions;

            return hourlyRate >= minHourlyRate && hourlyRate <= maxHourlyRate;
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsListView.SelectedItem is Claim selectedClaim)
            {
                if (ValidateClaim(selectedClaim))
                {
                    UpdateClaimStatus(selectedClaim.ClaimID, "Approved");
                }
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsListView.SelectedItem is Claim selectedClaim)
            {
                UpdateClaimStatus(selectedClaim.ClaimID, "Rejected");
            }
        }

        private void PendingButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsListView.SelectedItem is Claim selectedClaim)
            {
                UpdateClaimStatus(selectedClaim.ClaimID, "Pending");
            }
        }

        private void RunAutomationButton_Click(object sender, RoutedEventArgs e)
        {
            List<Claim> claims = GetClaimsFromDatabase();

            foreach (var claim in claims)
            {
                if (claim.ClaimStatus == "Pending" && ValidateClaim(claim))
                {
                    UpdateClaimStatus(claim.ClaimID, "Approved");
                }
                else if (claim.ClaimStatus == "Pending")
                {
                    UpdateClaimStatus(claim.ClaimID, "Rejected");
                }
            }

            MessageBox.Show("Automation process completed!");
        }

        private void DownloadDocument_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsListView.SelectedItem is Claim selectedClaim)
            {
                List<SupportingDocument> documents = GetSupportingDocuments(selectedClaim.ClaimID);

                foreach (var doc in documents)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        FileName = doc.DocName,
                        Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.Copy(doc.FilePath, saveFileDialog.FileName);
                    }
                }
            }
        }

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

    public class Claim
    {
        public int ClaimID { get; set; }
        public string ClassTaught { get; set; }
        public decimal TotalAmount { get; set; }
        public string ClaimStatus { get; set; }
        public int NumberOfSessions { get; set; }
    }

    public class SupportingDocument
    {
        public int DocID { get; set; }
        public string DocName { get; set; }
        public string FilePath { get; set; }
    }
}

using System;
using System.Data.SqlClient;
using System.Windows;

namespace AaliyahAllie_PROG6212_POE_FINAL_PART
{
    public partial class UpdateLecturerWindow : Window
    {
        private readonly string connectionString = "Data Source=hp820g4\\SQLEXPRESS;Initial Catalog=POE;Integrated Security=True;";
        private readonly int lecturerId; // Pass the selected Lecturer ID for editing

        public UpdateLecturerWindow(int lecturerId)
        {
            InitializeComponent();
            this.lecturerId = lecturerId;
            LoadLecturerData();
        }

        private void LoadLecturerData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT FirstName, LastName, Email, PhoneNumber, AccountType FROM AccountUser WHERE AccountUserID = @LecturerId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            FirstNameTextBox.Text = reader["FirstName"].ToString();
                            LastNameTextBox.Text = reader["LastName"].ToString();
                            EmailTextBox.Text = reader["Email"].ToString();
                            PhoneNumberTextBox.Text = reader["PhoneNumber"].ToString();
                            AccountTypeComboBox.Text = reader["AccountType"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading lecturer data: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE AccountUser " +
                                   "SET FirstName = @FirstName, LastName = @LastName, Email = @Email, PhoneNumber = @PhoneNumber, AccountType = @AccountType " +
                                   "WHERE AccountUserID = @LecturerId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@FirstName", FirstNameTextBox.Text);
                    cmd.Parameters.AddWithValue("@LastName", LastNameTextBox.Text);
                    cmd.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                    cmd.Parameters.AddWithValue("@PhoneNumber", PhoneNumberTextBox.Text);
                    cmd.Parameters.AddWithValue("@AccountType", AccountTypeComboBox.Text);
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Lecturer information updated successfully!");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update lecturer information.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating lecturer information: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace csharp_net_wpf_fhir_patient_manager
{
    public partial class MainWindow : Window
    {
        // Constructor
        public MainWindow()
        {
            InitializeComponent();
        }

        // Button Fetch Click event - fetch patients
        private async void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            string fhirUrl = txtFhirUrl.Text;
            if (string.IsNullOrEmpty(fhirUrl))
            {
                MessageBox.Show("Please enter a FHIR server URL.");
                return;
            }

            try
            {
                // Create a FHIR client
                var fhirClient = new FhirClient(fhirUrl);

                // Fetch patients from the FHIR server
                Bundle patientBundle = await fhirClient.SearchAsync<Patient>();

                // Clear the current list of patients
                lvPatients.Items.Clear();

                // Parse the response and add patients to the ListView
                List<string> patientList = new List<string>();

                foreach (var entry in patientBundle.Entry)
                {
                    if (entry.Resource is Patient patient)
                    {
                        // Get the patient's name
                        string patientName = GetPatientName(patient);
                        patientList.Add(patientName);
                    }
                }

                // Update the ListView with the fetched patients
                foreach (var patient in patientList)
                {
                    lvPatients.Items.Add(patient);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching patients: {ex.Message}");
            }
        }

        // Helper method to get the patient's name
        private string GetPatientName(Patient patient)
        {
            if (patient.Name.Count > 0)
            {
                var humanName = patient.Name[0];
                return $"{humanName.GivenElement[0]} {humanName.Family}";
            }
            return "Unnamed Patient";
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Implementation for deleting a selected patient from ListView
            if (lvPatients.SelectedItem != null)
            {
                lvPatients.Items.Remove(lvPatients.SelectedItem);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            // Clear the ListView
            lvPatients.Items.Clear();
        }

        private void txtFhirUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Event handler for FHIR URL textbox changes (optional)
        }
    }
}

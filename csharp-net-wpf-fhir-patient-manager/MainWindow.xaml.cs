using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using csharp_net_wpf_fhir_patient_manager.Models;
using Newtonsoft.Json;

namespace csharp_net_wpf_fhir_patient_manager
{
    public partial class MainWindow : Window
    {
        // Constructor
        public MainWindow()
        {
            InitializeComponent();

            Entries = new ObservableCollection<PatientModel>();

            this.DataContext = this;
        }

        public ObservableCollection<PatientModel> Entries { get; set; }

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
                // Bearer token (replace with actual token)
                string bearerToken = txtToken.Text;

                // Initialize HttpClient
                using (HttpClient httpClient = new HttpClient())
                {
                    // Set the Bearer token in the Authorization header
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                    // Send the GET request to the FHIR server for Patient resources
                    HttpResponseMessage response = await httpClient.GetAsync(fhirUrl);

                    // Check the content type of the response
                    string contentType = response.Content.Headers.ContentType?.MediaType ?? "unknown";

                    // Read the content as a string
                    string contentString = await response.Content.ReadAsStringAsync();

                    if (contentType == "text/plain")
                    {
                        // Try to parse the content as JSON if it's structured as such
                        if (contentString.TrimStart().StartsWith("{"))
                        {
                            ParseJsonResponse(contentString);  // Treat the plain text as JSON
                        }
                        else
                        {
                            MessageBox.Show("The plain text response does not contain valid JSON.");
                        }
                    }
                    else if (contentType == "application/json")
                    {
                        // Parse the JSON content directly
                        ParseJsonResponse(contentString);
                    }
                    else
                    {
                        MessageBox.Show("Unexpected content type: " + contentType + "\nResponse:\n" + contentString);
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"HTTP Error fetching patients: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching patients: {ex.Message}");
            }
        }

        // Helper method to manually parse plain text response
        private void ParsePlainTextResponse(string content)
        {
            try
            {
                // Split the content by line and then parse each line
                var lines = content.Split('\n');

                Entries.Clear();
                foreach (var line in lines)
                {
                    // Skip empty or whitespace-only lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    // Check if the line contains both '=' and ',' before processing
                    if (line.Contains('=') && line.Contains(','))
                    {
                        var parts = line.Split(',');

                        // Ensure there are two parts (FirstName and LastName)
                        if (parts.Length == 2)
                        {
                            // Split each part by '=' and ensure both sides exist
                            var firstNamePart = parts[0].Split('=');
                            var lastNamePart = parts[1].Split('=');

                            if (firstNamePart.Length == 2 && lastNamePart.Length == 2)
                            {
                                var firstName = firstNamePart[1].Trim();
                                var lastName = lastNamePart[1].Trim();

                                // Add the parsed patient information to the Entries collection
                                Entries.Add(new PatientModel
                                {
                                    FirstName = firstName,
                                    LastName = lastName
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing plain text: {ex.Message}");
            }
        }


        // Helper method to manually parse XML response
        private void ParseXmlResponse(string content)
        {
            try
            {
                // Simple XML parsing (you can use an XML parser like XmlDocument if needed)
                // Assuming a basic structure like:
                // <Patients><Patient><FirstName>John</FirstName><LastName>Doe</LastName></Patient></Patients>

                Entries.Clear();
                var xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(content);
                var patientNodes = xmlDoc.GetElementsByTagName("Patient");

                foreach (System.Xml.XmlNode patientNode in patientNodes)
                {
                    var firstName = patientNode["FirstName"]?.InnerText ?? "Unnamed";
                    var lastName = patientNode["LastName"]?.InnerText ?? "Unnamed";

                    Entries.Add(new PatientModel
                    {
                        FirstName = firstName,
                        LastName = lastName
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing XML: {ex.Message}");
            }
        }

        // Helper method to manually parse JSON response
        private void ParseJsonResponse(string content)
        {
            try
            {
                // Deserialize the JSON response into a dynamic object
                dynamic bundle = JsonConvert.DeserializeObject<dynamic>(content);

                // Ensure that the response is a Bundle and contains entries
                if (bundle.resourceType == "Bundle" && bundle.entry != null)
                {
                    Entries.Clear();

                    // Loop through each entry in the bundle
                    foreach (var entry in bundle.entry)
                    {
                        // Check if the entry has a Patient resource
                        if (entry.resource.resourceType == "Patient")
                        {
                            // Extract family (last name) and given (first name)
                            string firstName = entry.resource.name[0].given != null ? entry.resource.name[0].given[0].ToString() : "Unnamed";
                            string lastName = entry.resource.name[0].family != null ? entry.resource.name[0].family.ToString() : "Unnamed";

                            // Add the patient to the Entries collection
                            Entries.Add(new PatientModel
                            {
                                FirstName = firstName,
                                LastName = lastName
                            });
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Invalid JSON format: Not a FHIR Bundle or missing entries.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing JSON: {ex.Message}");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Implementation for deleting a selected patient from ListView
            if (lvPatients.SelectedItem != null)
            {
                PatientModel selectedPatient = (PatientModel)lvPatients.SelectedItem;
                Entries.Remove(selectedPatient);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            // Clear the ListView
            Entries.Clear();
        }

        private void txtFhirUrl_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void txtToken_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

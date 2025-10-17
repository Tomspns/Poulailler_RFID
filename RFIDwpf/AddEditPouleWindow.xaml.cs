using System;
using System.Windows;
using MySql.Data.MySqlClient;
using RFIDwpf.DAO;
using RFIDwpf.RFID; // si tu as ta gestion de lecture RFID ici
using Microsoft.VisualBasic; // Pour InputBox

namespace RFIDwpf
{
    public partial class AddEditPouleWindow : Window
    {
        public AddEditPouleWindow()
        {
            InitializeComponent();
        }

        // Chargement de la fenêtre
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IdPouleTextBox.Focus();
        }

        // ✅ Ajouter une poule
        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            string idPoule = IdPouleTextBox.Text.Trim();
            string nom = NomTextBox.Text.Trim();
            string race = RaceTextBox.Text.Trim();

            try
            {
                AddPouleDAO.AddPoule(idPoule, nom, race);
                MessageBox.Show("🐔 Poule ajoutée avec succès !");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        // ✏️ Modifier une poule (avec scan RFID)
        private void Modifier_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1️⃣ Demande de scan RFID
                string rfid = ShowInputDialog("Scanne le RFID de la poule à modifier :");
                if (string.IsNullOrWhiteSpace(rfid))
                {
                    MessageBox.Show("Aucun RFID scanné.");
                    return;
                }

                // 2️⃣ Connexion à la base
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT idPoule, nom, race FROM poule WHERE rfid = @rfid LIMIT 1";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@rfid", rfid);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // 3️⃣ Remplissage automatique des champs
                                IdPouleTextBox.Text = reader["idPoule"].ToString();
                                NomTextBox.Text = reader["nom"].ToString();
                                RaceTextBox.Text = reader["race"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("Aucune poule trouvée avec ce RFID.");
                                return;
                            }
                        }
                    }
                }

                // 4️⃣ Confirmation et modification
                if (MessageBox.Show("Souhaitez-vous enregistrer les modifications ?", "Confirmation",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    string idPoule = IdPouleTextBox.Text.Trim();
                    string nouveauNom = NomTextBox.Text.Trim();
                    string nouvelleRace = RaceTextBox.Text.Trim();

                    UpdatePouleDAO.UpdatePoule(idPoule, nouveauNom, nouvelleRace);
                    MessageBox.Show("✅ Poule modifiée avec succès !");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        // 🔹 Petite fenêtre d’entrée RFID (WPF native, pas besoin de Microsoft.VisualBasic)
        private string ShowInputDialog(string text)
        {
            Window prompt = new Window()
            {
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = "Scan RFID",
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.White
            };

            var stack = new System.Windows.Controls.StackPanel() { Margin = new Thickness(20) };
            stack.Children.Add(new System.Windows.Controls.TextBlock()
            {
                Text = text,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var input = new System.Windows.Controls.TextBox()
            {
                Height = 30,
                Margin = new Thickness(0, 0, 0, 20)
            };
            stack.Children.Add(input);

            var ok = new System.Windows.Controls.Button()
            {
                Content = "Valider",
                Width = 100,
                Height = 35,
                Background = System.Windows.Media.Brushes.Orange,
                Foreground = System.Windows.Media.Brushes.White
            };
            ok.Click += (sender, e) => { prompt.DialogResult = true; prompt.Close(); };
            stack.Children.Add(ok);

            prompt.Content = stack;
            prompt.ShowDialog();

            return input.Text;
        }
    }
}

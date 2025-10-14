using System;
using System.Windows;
using RFIDwpf.DAO;

namespace RFIDwpf
{
    public partial class AddEditPouleWindow : Window
    {
        public AddEditPouleWindow()
        {
            InitializeComponent();
        }

        // Méthode appelée au chargement de la fenêtre
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IdPouleTextBox.Focus();
        }

        // Ajouter une poule
        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            string idPoule = IdPouleTextBox.Text.Trim();
            string nom = NomTextBox.Text.Trim();
            string race = RaceTextBox.Text.Trim();  // <-- nouveau champ

            try
            {
                AddPouleDAO.AddPoule(idPoule, nom, race);  // <-- mettre à jour la DAO
                MessageBox.Show("Poule ajoutée avec succès !");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        // Modifier une poule
        private void Modifier_Click(object sender, RoutedEventArgs e)
        {
            string idPoule = IdPouleTextBox.Text.Trim();
            string nouveauNom = NomTextBox.Text.Trim();
            string nouvelleRace = RaceTextBox.Text.Trim();  // <-- nouveau champ

            try
            {
                UpdatePouleDAO.UpdatePoule(idPoule, nouveauNom, nouvelleRace);  // <-- mettre à jour la DAO
                MessageBox.Show("Poule modifiée avec succès !");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }
    }
}

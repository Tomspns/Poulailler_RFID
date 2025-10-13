using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RFIDwpf.DAO;

namespace RFIDwpf
{
    public partial class AddEditPouleWindow : Window
    {
        public AddEditPouleWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IdPouleTextBox.Focus(); // Met le focus sur le champ ID Poule
        }

        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            string idPoule = IdPouleTextBox.Text.Trim();
            string nom = NomTextBox.Text.Trim();

            try
            {
                AddPouleDAO.AddPoule(idPoule, nom);
                MessageBox.Show("Poule ajoutée avec succès !");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void Modifier_Click(object sender, RoutedEventArgs e)
        {
            string idPoule = IdPouleTextBox.Text.Trim();
            string nouveauNom = NomTextBox.Text.Trim();

            try
            {
                UpdatePouleDAO.UpdatePoule(idPoule, nouveauNom);
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

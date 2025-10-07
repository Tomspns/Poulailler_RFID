using RFIDwpf.RFID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Concurrent;


namespace RFIDwpf
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LecteurRfid lecteur;
        private Timer timer;
        private static ConcurrentDictionary<string, string> etatsPoules = new ConcurrentDictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();
            lecteur = new LecteurRfid();

            // Essaie de se connecter au lecteur RFID
            if (lecteur.connectionRs() == 0)
            {
                StartTimer();
            }
            else
            {
                ShowConnectionError();
            }
        }

        private void ShowConnectionError()
        {
            Dispatcher.Invoke(() =>
            {
                txtConnectionError.Text = "Lecteur débranché. Veuillez le rebrancher.";
                txtConnectionError.Visibility = Visibility.Visible;
            });
        }

        private void HideConnectionError()
        {
            Dispatcher.Invoke(() =>
            {
                txtConnectionError.Visibility = Visibility.Collapsed;
            });
        }

        private void StartTimer()
        {
            timer = new Timer(1000); // Vérifie toutes les secondes
            timer.Elapsed += CheckCard;
            timer.Start();
        }

        private void CheckCard(object sender, ElapsedEventArgs e)
        {
            // Vérifie la connexion
            int status = lecteur.connectionRs();

            // Gérer les différents statuts
            string errorMessage = string.Empty;

            if (status == 1)
            {
                errorMessage = "Erreur de communication avec le lecteur. Veuillez vérifier les connexions.";
            }
            else if (status == 2)
            {
                errorMessage = "Le lecteur RFID semble débranché. Veuillez vérifier la connexion et rebrancher l'appareil.";
            }
            else
            {
                errorMessage = "Statut de connexion inconnu. Veuillez vérifier le lecteur.";
            }

            if (status != 0)
            {
                Dispatcher.Invoke(() =>
                {
                    txtConnectionError.Text = errorMessage;
                    txtConnectionError.Visibility = Visibility.Visible;
                });

                return; // Sortir si le lecteur n'est pas connecté
            }
            else
            {
                HideConnectionError();
            }

            string identifiant = lecteur.GetCardID(); // Lit l'identifiant de la carte
            if (!string.IsNullOrEmpty(identifiant))
            {
                DisplayName(identifiant);
            }
        }

        private void DisplayName(string identifiant)
        {
            // Déterminer le nom associé à l'ID
            string name;
            if (identifiant == "043362D2FC1090")
                name = "Poule 1";
            else if (identifiant == "029EC135")
                name = "Poule 2";
            else if (identifiant == "620AC435")
                name = "Poule 3";
            else
                name = "Inconnu";

            // Alterner l’état
            string nouvelEtat = "dedans";
            if (etatsPoules.ContainsKey(identifiant) && etatsPoules[identifiant] == "dedans")
                nouvelEtat = "dehors";

            etatsPoules[identifiant] = nouvelEtat;

            // Mise à jour UI
            Dispatcher.Invoke(() =>
            {
                txtIdentifiant.Text = identifiant;
                txtNom.Text = $"{name} ({nouvelEtat.ToUpper()})";
            });

            // Enregistrement en BDD
            try
            {
                DatabaseHelper.InsertPoule(identifiant, name);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur enregistrement BDD : " + ex.Message);
            }

            // Envoi MQTT
            _ = MqttClientService.PublishEtatPouleAsync(identifiant, name, nouvelEtat);
        }
        private void BtnDeleteLast_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DatabaseHelper.DeleteLastPoule();
                txtIdentifiant.Text = "";
                txtNom.Text = "";
                MessageBox.Show("Dernier enregistrement supprimé avec succès ✅",
                                "Suppression",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la suppression : " + ex.Message,
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}

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
    public partial class MainWindow : Window
    {
        private LecteurRfid lecteur;
        private static ConcurrentDictionary<string, string> etatsPoules = new ConcurrentDictionary<string, string>();
        private string lastScannedId = null;
        private DateTime lastScanTime = DateTime.MinValue;
        private readonly TimeSpan scanCooldown = TimeSpan.FromSeconds(2); // 🔹 cooldown pour éviter double scan rapide

        public MainWindow()
        {
            InitializeComponent();
            lecteur = new LecteurRfid();

            if (lecteur.connectionRs() == 0)
            {
                lecteur.CardScanned += Lecteur_CardScanned;
                HideConnectionError();
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

        private void Lecteur_CardScanned(object sender, string identifiant)
        {
            if (string.IsNullOrEmpty(identifiant))
                return;

            // 🔹 ignore les scans trop rapprochés du même badge
            if (identifiant == lastScannedId && (DateTime.Now - lastScanTime) < scanCooldown)
                return;

            lastScannedId = identifiant;
            lastScanTime = DateTime.Now;

            DisplayName(identifiant);
        }

        private void DisplayName(string identifiant)
        {
            string name;

            switch (identifiant)
            {
                case "043362D2FC1090":
                    name = "Poule 1";
                    break;
                case "029EC135":
                    name = "Poule 2";
                    break;
                case "620AC435":
                    name = "Poule 3";
                    break;
                default:
                    name = "Inconnue";
                    break;
            }

            // 🔹 bascule l'état à chaque scan
            string nouvelEtat = etatsPoules.TryGetValue(identifiant, out string etatActuel)
                ? (etatActuel == "dedans" ? "dehors" : "dedans")
                : "dedans";

            etatsPoules[identifiant] = nouvelEtat;

            Dispatcher.Invoke(() =>
            {
                txtIdentifiant.Text = identifiant;
                txtNom.Text = $"{name} ({nouvelEtat.ToUpper()})";
            });

            // 🔹 Enregistrement BDD
            try
            {
                DatabaseHelper.InsertPoule(identifiant, name);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Erreur enregistrement BDD : " + ex.Message));
            }

            // 🔹 Envoi MQTT
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

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string idPoule = txtIdentifiant.Text;
            string nom = txtNom.Text;
            string race = "Inconnue";

            try
            {
                DatabaseHelper.AddPoule(idPoule, nom, race);
                MessageBox.Show("Poule ajoutée avec succès ✅",
                                "Ajout",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'ajout : " + ex.Message,
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            string idPoule = txtIdentifiant.Text;
            string nouveauNom = txtNom.Text;
            string nouvelleRace = "Inconnue";

            try
            {
                DatabaseHelper.UpdatePoule(idPoule, nouveauNom, nouvelleRace);
                MessageBox.Show("Poule modifiée avec succès ✅",
                                "Modification",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la modification : " + ex.Message,
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            AddEditPouleWindow addEditPouleWindow = new AddEditPouleWindow
            {
                Owner = this
            };
            addEditPouleWindow.ShowDialog();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Palkanosa
{
    /// <summary>
    /// Demo based on
    /// https://www.sivista.fi/tyosuhdeasiat/tyoehtosopimukset-ja-palkkataulukot/yliopistot-ja-harjoittelukoulut/yliopistojen-yleinen-tyoehtosopimus/
    /// 
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const int MINUMUM_PERFORMANCE = 6;
        private const int MAXIMUM_PERFORMANCE = 50;
        private double baseSalary;
        public double BaseSalary { 
            get=>baseSalary; 
            set {
                baseSalary = value;
                OnPropertyChanged();
            } 
        }
        private double currentPerformance;
        public double CurrentPerformance
        {
            get => currentPerformance;
            set
            {
                if (value >= MINUMUM_PERFORMANCE && 
                    value <= MAXIMUM_PERFORMANCE)
                {
                    currentPerformance = value;
                }
                else
                {
                    ShowError(String.Format(
                        "Nykyinen suoritusprosentti tulee olla {0}-{1}",
                        MINUMUM_PERFORMANCE,MINUMUM_PERFORMANCE));
                }
                OnPropertyChanged();
            }
        }
        private double newPerformance=double.NaN;
        public double NewPerformance
        {
            get => newPerformance;
            set
            {
                if (value >= MINUMUM_PERFORMANCE 
                    && value <= MAXIMUM_PERFORMANCE)
                {
                    newPerformance = value;
                }
                else
                {
                    ShowError(String.Format(
                        "Uusi suoritusprosentti tulee olla {0}-{1}",
                        MINUMUM_PERFORMANCE, MAXIMUM_PERFORMANCE));
                }
                OnPropertyChanged();
            }
        }
        private bool dialogOpen;
        private async void ShowError(string msg)
        {
            if(dialogOpen)
            {
                return;
            }
            ContentDialog dialog = new ContentDialog
            {
                Title = "Virhe",
                Content = msg,
                CloseButtonText = "Sulje"
            };
            dialogOpen = true;
            ContentDialogResult result = await dialog.ShowAsync();
            dialogOpen = false;
        }
        private double currentSalary;
        public double CurrentSalary { 
            get=>currentSalary; 
            set
            {
                currentSalary = value;
                OnPropertyChanged();
            }
        }
        private double newSalary;
        public double NewSalary
        {
            get => newSalary;
            set
            {
                newSalary = value;
                OnPropertyChanged();
            }
        }
        private double salaryChangePercent;
        public double SalaryChangePercent
        {
            get => salaryChangePercent;
            set
            {
                if (value <= MaxSalaryChangePercent)
                {
                    salaryChangePercent = value;
                }
                else
                {
                    ShowError(string.Format
                        ("Palkan muutosprosentti saa olla korkeintaan {0}.",
                        MaxSalaryChangePercent));
                }
                OnPropertyChanged();
            }
        }
        public double MaxSalaryChangePercent { get; set; }
        public MainPage()
        {
            this.InitializeComponent();
            MaxSalaryChangePercent = 5;
        }
        public void DoMaxRaise(object sender, RoutedEventArgs e)
        {
            SalaryChangePercent = MaxSalaryChangePercent;
        }
        private Boolean updateValuesRunning;
        private void UpdateValues(string trigger)
        {
            if (updateValuesRunning)
            {
                return;
            }
            if (!(baseSalary > 0))
            {
                ShowError("Anna peruspalkka ensin.");
                Peruspalkka.Focus(FocusState.Programmatic);
            }
            bool calculateSalaryChangePercent = false;
            updateValuesRunning = true;
            if ("SalaryChangePercent".Equals(trigger))
            {
                double value = Math.Round(
                    (salaryChangePercent / 100 + 1) * currentSalary, 2);
                if (value != newSalary)
                {
                    NewSalary = value;
                    NewPerformance = Math.Round(
                        (newSalary - baseSalary) / baseSalary * 100, 2);
                }
            }
            else if ("NewSalary".Equals(trigger))
            {
                NewPerformance = Math.Round(
                    (newSalary - baseSalary) / baseSalary * 100, 2);
                calculateSalaryChangePercent = true;
            }
            else if ("CurrentSalary".Equals(trigger)) {
                double lowLimit = baseSalary * 
                    (1+(MINUMUM_PERFORMANCE/100d));
                double highLimit= baseSalary *
                    (1 + (MAXIMUM_PERFORMANCE / 100d));
                if (currentSalary >= lowLimit &&
                    currentSalary<=highLimit)
                {
                    double value = Math.Round
                        (((currentSalary / baseSalary) - 1) * 100, 2);
                    if (value != currentPerformance)
                    {
                        CurrentPerformance = value;
                    }
                }
                else
                {
                    ShowError(String.Format(
                        "Nykyisen kokonaispalkan tulee olle " +
                        " välillä {0}-{1}.",
                        Math.Round(lowLimit,2), 
                        Math.Round(highLimit, 2)
                        ));
                    NykyinenPalkka.Focus(FocusState.Programmatic);
                }
            }
            else
            {
                double value = Math.Round
                    ((1 + (currentPerformance / 100)) * baseSalary, 2);
                if (value != currentSalary)
                {
                    CurrentSalary = value;
                }
                if (newPerformance >= MINUMUM_PERFORMANCE
                    && newPerformance<=MAXIMUM_PERFORMANCE)
                {
                    value = Math.Round
                        ((1 + (newPerformance / 100)) * baseSalary, 2);
                    if (value != newSalary)
                    {
                        calculateSalaryChangePercent = true;
                        NewSalary = value;
                    }
                }
            }
            if (calculateSalaryChangePercent)
            {
                double value = Math.Round
                    ((newSalary - currentSalary) / currentSalary * 100, 2);
                if (value != salaryChangePercent)
                {
                    if (value > MaxSalaryChangePercent)
                    {
                        ShowSalaryChangeError(value);
                    }
                    else
                    {
                        SalaryChangePercent = value;
                    }
                }

            }
            updateValuesRunning = false;
        }

        private void ShowSalaryChangeError(double value)
        {
            ShowError(string.Format
            ("Palkan muutosprosentti saa olla korkeintaan {0}." +
            " Annetuilla arvoilla muutosprosentiksi tulee {1}." +
            " Voit antaa Palkan muutosprosentin, jolloin" +
            " sovellus laskee uuden palkan ja suoritusprosentin.",
            MaxSalaryChangePercent, value));
        }

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            UpdateValues(name);
        }
    }
}

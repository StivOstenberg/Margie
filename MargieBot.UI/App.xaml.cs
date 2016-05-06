using FirstFloor.ModernUI.Presentation;
using System.Windows;
using System.Windows.Media;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System;

namespace MargieBot.UI
{
    public partial class App : Application
    {
        private void this_Startup(object sender, StartupEventArgs e)
        {

            // set up MUI theme
            Color myAccentColor = (Color)App.Current.FindResource("MyAccentColor");
            AppearanceManager.Current.AccentColor = myAccentColor;
        }


    }
}

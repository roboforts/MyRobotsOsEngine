using OsEngine.Robots.FrontRunner.Models;
using OsEngine.Robots.FrontRunner.ViewModels;
using System.Windows;

namespace OsEngine.Robots.FrontRunner.Views
{
    /// <summary>
    /// Логика взаимодействия для FrontRunnerUi.xaml
    /// </summary>
    public partial class FrontRunnerUi : Window
    {
        public FrontRunnerUi(FrontRunnerBot robot)
        {
            InitializeComponent();
            DataContext = new VM(robot);
        }
    }
}

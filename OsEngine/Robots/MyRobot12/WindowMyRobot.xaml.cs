using OsEngine.Robots.MyRobot12.Models;
using OsEngine.Robots.MyRobot12.ViewModels;
using System.Windows;

namespace OsEngine.Robots.MyRobot12
{
    /// <summary>
    /// Логика взаимодействия для WindowMyRobot.xaml
    /// </summary>
    public partial class WindowMyRobot : Window
    {
        public WindowMyRobot(MyRobot1 robot)
        {
            InitializeComponent();
            DataContext = new VM(robot);
        }
    }
}

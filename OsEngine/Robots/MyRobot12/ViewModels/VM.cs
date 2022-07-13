using OsEngine.Entity;
using OsEngine.Robots.MyRobot12.Models;

namespace OsEngine.Robots.MyRobot12.ViewModels
{
    public class VM : BaseVM
    {
        public VM(MyRobot1 robot)
        {
            _robot = robot;
            paramLot1 = (StrategyParameterInt)_robot.Parameters[4];
            paramStop1 = (StrategyParameterInt)_robot.Parameters[5];
            paramTake1 = (StrategyParameterInt)_robot.Parameters[6];
        }

        MyRobot1 _robot;
        StrategyParameterInt paramLot1;
        StrategyParameterInt paramStop1;
        StrategyParameterInt paramTake1;

        public int Lot1
        {
            get => paramLot1.ValueInt;
            set
            {
                paramLot1.ValueInt = value;
                OnPropertyChanged(nameof(Lot1));
            }
        }

        public int Stop1
        {
            get => paramStop1.ValueInt;
            set
            {
                paramStop1.ValueInt = value;
                OnPropertyChanged(nameof(Stop1));
            }
        }

        public int Take1
        {
            get => paramTake1.ValueInt;
            set
            {
                paramTake1.ValueInt = value;
                OnPropertyChanged(nameof(Take1));
            }
        }
    }
}

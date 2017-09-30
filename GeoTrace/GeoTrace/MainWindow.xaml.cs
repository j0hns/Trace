using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GeoTrace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer _timer;

        public MainWindow()
        {
            InitializeComponent();
            var viewModel= new MainWindowViewModel(Map);
            DataContext = viewModel;
            viewModel.Run();
          //  _timer = new Timer(state => viewModel.Run(), null, 60000, 60000);
            
        }
    }
}

using RPool.NET;
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

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Random r = new Random();
        public MainWindow()
        {
            InitializeComponent();
            Pool.InInitialize<MyProcess>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Cacl(1);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                Thread t = new Thread(Cacl);
                t.Start(i);
                Thread.Sleep(r.Next(10));
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                Thread t = new Thread(Cacl);
                t.Start(i);
                Thread.Sleep(r.Next(10));
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var pools=   Pool.processArr.Where(s => s != null).Select(s => new { s.TaskRunTimes }).ToList();
            for (int i = 0; i < pools.Count; i++)
            {
                Append("进程" + (i + 1) + " : " + pools[i].TaskRunTimes);
            }
        }

        private void Cacl(object num) {
      
                var r=Pool.ExecuteR("print(args)","你好").Result;
               
                text.Dispatcher.Invoke(() =>
                {
                    Append(r);
                });

        }
        private void Append(string value) {
            text.Text +=value.Trim() +"\n";
            text.ScrollToEnd();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            text.Text = "";
        }
    }


    public class MyProcess : RPool.NET.PoolItemWithLock
    {
        public override void OnInitialize()
        {
            ExecuteCommand("library(car)", true, null);
        }
    }
}

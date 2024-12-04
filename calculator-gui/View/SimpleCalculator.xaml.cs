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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace calculator_gui
{
    /// <summary>
    /// Interaction logic for SimpleCalculator.xaml
    /// </summary>
    public partial class SimpleCalculator : Page
    {
        public SimpleCalculator()
        {
            InitializeComponent();
        }

        private void Button_7_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += '7';
        }
        private void Button_8_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += '8';
        }
        private void Button_9_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += '9';
        }
        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += " + ";
        }
        private void Button_Negate_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            if (!String.IsNullOrEmpty(TextBlock_CalcOutput.Text))
                TextBlock_CalcOutput.Text = "-(" + TextBlock_CalcOutput.Text + ")";
        }



        private void Button_4_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += '4';
        }
        private void Button_5_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += '5';
        }
        private void Button_6_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += '6';
        }
        private void Button_Minus_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += " - ";
        }
        private void Button_Reciprocal_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += "^(-1) ";
        }


        private void Button_1_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += '1';
        }
        private void Button_2_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += '2';
        }
        private void Button_3_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += '3';
        }
        private void Button_Multiply_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += " × ";
        }
        private void Button_Equals_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            if (!String.IsNullOrEmpty(TextBlock_CalcOutput.Text))
            {
                FreeformCalculator calculator = new FreeformCalculator() { Input = TextBlock_CalcOutput.Text };
                double output = ((FloatToken)calculator.Evaluate()).value;
                if (!calculator.isValidExpression)
                {
                    TextBlock_CalcOutput.Text = "Syntax error";
                }
                else
                {
                    TextBlock_CalcOutput.Text = output.ToString();
                }
            }
        }


        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text = "";
        }
        private void Button_0_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += '0';
        }
        private void Button_Backspace_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            if (TextBlock_CalcOutput.Text.Length > 0)
            {
                TextBlock_CalcOutput.Text = TextBlock_CalcOutput.Text.Substring(0, TextBlock_CalcOutput.Text.Length - 1);
            }
        }
        private void Button_Divide_Click(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            TextBlock_CalcOutput.Text += " ÷ ";
        }
    }
}

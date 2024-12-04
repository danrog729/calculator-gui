using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup.Localizer;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace calculator_gui
{
    /// <summary>
    /// Interaction logic for ProgrammerCalculator.xaml
    /// </summary>
    public partial class ProgrammerCalculator : Page
    {
        private uint _currentBase;
        public uint CurrentBase
        {
            get => _currentBase;
            set
            {
                _currentBase = value;
                if (calculator != null && OutputTextBlock != null)
                {
                    calculator.CurrentBase = _currentBase;
                    OutputTextBlock.Text = calculator.ToString();
                    calculator.Input = calculator.ToString();
                }
                if (_currentBase == 16)
                {
                    ToggleHexDigits(true);
                    ToggleDecDigits(true);
                    ToggleOctDigits(true);
                }
                else if (_currentBase == 10)
                {
                    ToggleHexDigits(false);
                    ToggleDecDigits(true);
                    ToggleOctDigits(true);
                }
                else if (_currentBase == 8)
                {
                    ToggleHexDigits(false);
                    ToggleDecDigits(false);
                    ToggleOctDigits(true);
                }
                else
                {
                    ToggleHexDigits(false);
                    ToggleDecDigits(false);
                    ToggleOctDigits(false);
                }
            }
        }

        private uint _currentWordSize;
        public uint CurrentWordSize
        {
            get => _currentWordSize;
            set
            {
                _currentWordSize = value;
                // TODO
                // enable or disable the bitfield/base readout
                if (calculator != null)
                {
                    long bitMask = (long)Math.Pow(2, _currentWordSize) - 1;
                    foreach (Token token in calculator.TokenList)
                    {
                        if (token is FloatToken floatToken)
                        {
                            floatToken.@value = (long)floatToken.@value & bitMask;
                        }
                    }
                    OutputTextBlock.Text = calculator.ToString();
                    calculator.Input = calculator.ToString();
                }
                UpdateBaseReadout();
                if (_currentWordSize == 64)
                {
                    Toggle32Bits(true);
                    Toggle16Bits(true);
                    Toggle8Bits(true);
                }
                else if (_currentWordSize == 32)
                {
                    Toggle32Bits(false);
                    Toggle16Bits(true);
                    Toggle8Bits(true);
                }
                else if (_currentWordSize == 16)
                {
                    Toggle32Bits(false);
                    Toggle16Bits(false);
                    Toggle8Bits(true);
                }
                else
                {
                    Toggle32Bits(false);
                    Toggle16Bits(false);
                    Toggle8Bits(false);
                }
            }
        }

        private readonly FreeformCalculator calculator;

        bool newExpression = true;
        bool negated = false;

        public ProgrammerCalculator()
        {
            InitializeComponent(); 
            _currentBase = 10;
            _currentWordSize = 64;
            calculator = new FreeformCalculator();
        }

        public void BitButtonPressed(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            double output = 0;
            if ((string)((Button)sender).Content == "0")
            {
                ((Button)sender).Content = "1";
            }
            else
            {
                ((Button)sender).Content = "0";
            }
            if ((string)Bit63.Content == "1") { output += Math.Pow(2, 63); }
            if ((string)Bit62.Content == "1") { output += Math.Pow(2, 62); }
            if ((string)Bit61.Content == "1") { output += Math.Pow(2, 61); }
            if ((string)Bit60.Content == "1") { output += Math.Pow(2, 60); }
            if ((string)Bit59.Content == "1") { output += Math.Pow(2, 59); }
            if ((string)Bit58.Content == "1") { output += Math.Pow(2, 58); }
            if ((string)Bit57.Content == "1") { output += Math.Pow(2, 57); }
            if ((string)Bit56.Content == "1") { output += Math.Pow(2, 56); }
            if ((string)Bit55.Content == "1") { output += Math.Pow(2, 55); }
            if ((string)Bit54.Content == "1") { output += Math.Pow(2, 54); }
            if ((string)Bit53.Content == "1") { output += Math.Pow(2, 53); }
            if ((string)Bit52.Content == "1") { output += Math.Pow(2, 52); }
            if ((string)Bit51.Content == "1") { output += Math.Pow(2, 51); }
            if ((string)Bit50.Content == "1") { output += Math.Pow(2, 50); }
            if ((string)Bit49.Content == "1") { output += Math.Pow(2, 49); }
            if ((string)Bit48.Content == "1") { output += Math.Pow(2, 48); }
            if ((string)Bit47.Content == "1") { output += Math.Pow(2, 47); }
            if ((string)Bit46.Content == "1") { output += Math.Pow(2, 46); }
            if ((string)Bit45.Content == "1") { output += Math.Pow(2, 45); }
            if ((string)Bit44.Content == "1") { output += Math.Pow(2, 44); }
            if ((string)Bit43.Content == "1") { output += Math.Pow(2, 43); }
            if ((string)Bit42.Content == "1") { output += Math.Pow(2, 42); }
            if ((string)Bit41.Content == "1") { output += Math.Pow(2, 41); }
            if ((string)Bit40.Content == "1") { output += Math.Pow(2, 40); }
            if ((string)Bit39.Content == "1") { output += Math.Pow(2, 39); }
            if ((string)Bit38.Content == "1") { output += Math.Pow(2, 38); }
            if ((string)Bit37.Content == "1") { output += Math.Pow(2, 37); }
            if ((string)Bit36.Content == "1") { output += Math.Pow(2, 36); }
            if ((string)Bit35.Content == "1") { output += Math.Pow(2, 35); }
            if ((string)Bit34.Content == "1") { output += Math.Pow(2, 34); }
            if ((string)Bit33.Content == "1") { output += Math.Pow(2, 33); }
            if ((string)Bit32.Content == "1") { output += Math.Pow(2, 32); }
            if ((string)Bit31.Content == "1") { output += Math.Pow(2, 31); }
            if ((string)Bit30.Content == "1") { output += Math.Pow(2, 30); }
            if ((string)Bit29.Content == "1") { output += Math.Pow(2, 29); }
            if ((string)Bit28.Content == "1") { output += Math.Pow(2, 28); }
            if ((string)Bit27.Content == "1") { output += Math.Pow(2, 27); }
            if ((string)Bit26.Content == "1") { output += Math.Pow(2, 26); }
            if ((string)Bit25.Content == "1") { output += Math.Pow(2, 25); }
            if ((string)Bit24.Content == "1") { output += Math.Pow(2, 24); }
            if ((string)Bit23.Content == "1") { output += Math.Pow(2, 23); }
            if ((string)Bit22.Content == "1") { output += Math.Pow(2, 22); }
            if ((string)Bit21.Content == "1") { output += Math.Pow(2, 21); }
            if ((string)Bit20.Content == "1") { output += Math.Pow(2, 20); }
            if ((string)Bit19.Content == "1") { output += Math.Pow(2, 19); }
            if ((string)Bit18.Content == "1") { output += Math.Pow(2, 18); }
            if ((string)Bit17.Content == "1") { output += Math.Pow(2, 17); }
            if ((string)Bit16.Content == "1") { output += Math.Pow(2, 16); }
            if ((string)Bit15.Content == "1") { output += Math.Pow(2, 15); }
            if ((string)Bit14.Content == "1") { output += Math.Pow(2, 14); }
            if ((string)Bit13.Content == "1") { output += Math.Pow(2, 13); }
            if ((string)Bit12.Content == "1") { output += Math.Pow(2, 12); }
            if ((string)Bit11.Content == "1") { output += Math.Pow(2, 11); }
            if ((string)Bit10.Content == "1") { output += Math.Pow(2, 10); }
            if ((string)Bit9.Content == "1") { output += Math.Pow(2, 9); }
            if ((string)Bit8.Content == "1") { output += Math.Pow(2, 8); }
            if ((string)Bit7.Content == "1") { output += Math.Pow(2, 7); }
            if ((string)Bit6.Content == "1") { output += Math.Pow(2, 6); }
            if ((string)Bit5.Content == "1") { output += Math.Pow(2, 5); }
            if ((string)Bit4.Content == "1") { output += Math.Pow(2, 4); }
            if ((string)Bit3.Content == "1") { output += Math.Pow(2, 3); }
            if ((string)Bit2.Content == "1") { output += Math.Pow(2, 2); }
            if ((string)Bit1.Content == "1") { output += Math.Pow(2, 1); }
            if ((string)Bit0.Content == "1") { output += Math.Pow(2, 0); }

            if (calculator != null)
            {
                if (calculator.TokenList.Count > 0 && calculator.TokenList[calculator.TokenList.Count - 1] is FloatToken)
                {
                    calculator.TokenList.RemoveAt(calculator.TokenList.Count - 1);
                }
                calculator.TokenList.Add(new FloatToken(output));
                calculator.Input = calculator.ToString();
                OutputTextBlock.Text = calculator.Input;
            }
            UpdateBaseReadout();
        }

        public void AppendDigit(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            if (newExpression)
            {
                calculator.Input = (string)((Button)sender).Tag;
            }
            else
            {
                if (calculator.TokenList.Count > 0 && calculator.TokenList[calculator.TokenList.Count - 1] is FloatToken floatToken)
                {
                    double candidate = floatToken.value * CurrentBase + Convert.ToInt64((string)((Button)sender).Tag, 16);
                    if (candidate >= Math.Pow(2, _currentWordSize))
                    {
                        return;
                    }
                }
                calculator.Input += ((Button)sender).Tag;
            }
            OutputTextBlock.Text = calculator.ToString();
            newExpression = false;
            negated = false;
            UpdateBaseReadout();
        }

        public void AppendOperator(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            calculator.Input += ((Button)sender).Tag;
            OutputTextBlock.Text = calculator.ToString();
            newExpression = false;
            negated = false;
            UpdateBaseReadout();
        }

        public void AppendDecimal(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            calculator.Input = calculator.ToString();
            calculator.Input += ".";
            OutputTextBlock.Text = calculator.Input;
            newExpression = false;
            negated = false;
            UpdateBaseReadout();
        }

        public void ButtonPi_Pressed(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            if (newExpression)
            {
                calculator.Input = "π";
            }
            else
            {
                calculator.Input += "π";
            }
            OutputTextBlock.Text = calculator.ToString();
            newExpression = false;
            negated = false;
            UpdateBaseReadout();
        }

        public void ButtonE_Pressed(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            if (newExpression)
            {
                calculator.Input = "e";
            }
            else
            {
                calculator.Input += "e";
            }
            OutputTextBlock.Text = calculator.ToString();
            newExpression = false;
            negated = false;
            UpdateBaseReadout();
        }

        public void ButtonNegate_Pressed(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            if (calculator.TokenList.Count > 0)
            {
                if (negated)
                {
                    calculator.Input = calculator.Input.Substring(2, calculator.Input.Length - 3);
                    negated = false;
                }
                else
                {
                    calculator.Input = "-(" + calculator.Input + ")";
                    negated = true;
                }
            }
            OutputTextBlock.Text = calculator.ToString();
            UpdateBaseReadout();
        }

        public void ButtonDEL_Pressed(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            if (calculator.TokenList.Count > 0)
            {
                if (calculator.TokenList[calculator.TokenList.Count - 1] is OperatorToken)
                {
                    calculator.TokenList = calculator.TokenList.Take(calculator.TokenList.Count - 1).ToList();
                }
                else
                {
                    calculator.Input = calculator.Input.Substring(0, calculator.Input.Length - 1);
                }
                OutputTextBlock.Text = calculator.ToString();
                newExpression = false;
                negated = false;
            }
            UpdateBaseReadout();
        }

        public void ButtonAC_Pressed(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            calculator.Input = "";
            OutputTextBlock.Text = calculator.ToString();
            newExpression = true;
            negated = false;
            UpdateBaseReadout();
        }

        public void ButtonEquals_Pressed(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            if (calculator.Input.Length > 0)
            {
                string oldExpression = calculator.ToString();
                double output = ((FloatToken)calculator.Evaluate()).value;
                if (calculator.isValidExpression)
                {
                    calculator.Input = (new FloatToken(output % Math.Pow(2, _currentWordSize))).ToString((int)_currentBase);
                    OutputTextBlock.Text = calculator.Input;
                    calculator.Input = (new FloatToken(output)).ToString((int)_currentBase);

                    // Add expression to history
                    TextBlock result = new TextBlock() { Style = (Style)FindResource("StandardTextBlock"), Text = oldExpression + " = " + calculator.Input };
                    Viewbox viewbox = new Viewbox() { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(5), MaxHeight = 40, Child = result };
                    History.Children.Add(viewbox);
                    // scroll to bottom of history
                    HistoryScrollViewer.ScrollToBottom();
                }
                else
                {
                    OutputTextBlock.Text = "Syntax error";
                }
                newExpression = true;
                negated = false;
            }
            UpdateBaseReadout();
        }

        public void BaseChanged(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            if (BaseSelector.SelectedIndex == 0)
            {
                // bin
                CurrentBase = 2;
            }
            else if (BaseSelector.SelectedIndex == 1)
            {
                // oct
                CurrentBase = 8;
            }
            else if (BaseSelector.SelectedIndex == 2)
            {
                // dec
                CurrentBase = 10;
            }
            else
            {
                CurrentBase = 16;
            }
            UpdateBaseReadout();
        }

        public void WordSizeChanged(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            if (WordSize.SelectedIndex == 0)
            {
                // 8-bit
                CurrentWordSize = 8;
            }
            else if (WordSize.SelectedIndex == 1)
            {
                // 16-bit
                CurrentWordSize = 16;
            }
            else if (WordSize.SelectedIndex == 2)
            {
                // 32-bit
                CurrentWordSize = 32;
            }
            else
            {
                CurrentWordSize = 64;
            }
        }

        public void ClearHistory(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            History.Children.Clear();
        }

        public void UpdateBaseReadout()
        {
            long output = 0;
            if (calculator != null && calculator.TokenList.Count > 0 && calculator.TokenList[calculator.TokenList.Count - 1] is FloatToken floatToken)
            {
                output = (long)floatToken.value;
                BaseReadout.Text = floatToken.ToString(8) + "₈ = " + floatToken.ToString(10) + "₁₀ = " + floatToken.ToString(16) + "₁₆";
            }
            BaseReadout.Text = Convert.ToString(output,8) + "₈ = " + Convert.ToString(output, 10) + "₁₀ = " + Convert.ToString(output, 16).ToUpper() + "₁₆";
            if (output >= Math.Pow(2, 63)) { Bit63.Content = "1"; output -= (long)Math.Pow(2, 63); } else { Bit63.Content = "0";}
            if (output >= Math.Pow(2, 62)) { Bit62.Content = "1"; output -= (long)Math.Pow(2, 62); } else { Bit62.Content = "0"; }
            if (output >= Math.Pow(2, 61)) { Bit61.Content = "1"; output -= (long)Math.Pow(2, 61); } else { Bit61.Content = "0"; }
            if (output >= Math.Pow(2, 60)) { Bit60.Content = "1"; output -= (long)Math.Pow(2, 60); } else { Bit60.Content = "0"; }
            if (output >= Math.Pow(2, 59)) { Bit59.Content = "1"; output -= (long)Math.Pow(2, 59); } else { Bit59.Content = "0"; }
            if (output >= Math.Pow(2, 58)) { Bit58.Content = "1"; output -= (long)Math.Pow(2, 58); } else { Bit58.Content = "0"; }
            if (output >= Math.Pow(2, 57)) { Bit57.Content = "1"; output -= (long)Math.Pow(2, 57); } else { Bit57.Content = "0"; }
            if (output >= Math.Pow(2, 56)) { Bit56.Content = "1"; output -= (long)Math.Pow(2, 56); } else { Bit56.Content = "0"; }
            if (output >= Math.Pow(2, 55)) { Bit55.Content = "1"; output -= (long)Math.Pow(2, 55); } else { Bit55.Content = "0"; }
            if (output >= Math.Pow(2, 54)) { Bit54.Content = "1"; output -= (long)Math.Pow(2, 54); } else { Bit54.Content = "0"; }
            if (output >= Math.Pow(2, 53)) { Bit53.Content = "1"; output -= (long)Math.Pow(2, 53); } else { Bit53.Content = "0"; }
            if (output >= Math.Pow(2, 52)) { Bit52.Content = "1"; output -= (long)Math.Pow(2, 52); } else { Bit52.Content = "0"; }
            if (output >= Math.Pow(2, 51)) { Bit51.Content = "1"; output -= (long)Math.Pow(2, 51); } else { Bit51.Content = "0"; }
            if (output >= Math.Pow(2, 50)) { Bit50.Content = "1"; output -= (long)Math.Pow(2, 50); } else { Bit50.Content = "0"; }
            if (output >= Math.Pow(2, 49)) { Bit49.Content = "1"; output -= (long)Math.Pow(2, 49); } else { Bit49.Content = "0"; }
            if (output >= Math.Pow(2, 48)) { Bit48.Content = "1"; output -= (long)Math.Pow(2, 48); } else { Bit48.Content = "0"; }
            if (output >= Math.Pow(2, 47)) { Bit47.Content = "1"; output -= (long)Math.Pow(2, 47); } else { Bit47.Content = "0"; }
            if (output >= Math.Pow(2, 46)) { Bit46.Content = "1"; output -= (long)Math.Pow(2, 46); } else { Bit46.Content = "0"; }
            if (output >= Math.Pow(2, 45)) { Bit45.Content = "1"; output -= (long)Math.Pow(2, 45); } else { Bit45.Content = "0"; }
            if (output >= Math.Pow(2, 44)) { Bit44.Content = "1"; output -= (long)Math.Pow(2, 44); } else { Bit44.Content = "0"; }
            if (output >= Math.Pow(2, 43)) { Bit43.Content = "1"; output -= (long)Math.Pow(2, 43); } else { Bit43.Content = "0"; }
            if (output >= Math.Pow(2, 42)) { Bit42.Content = "1"; output -= (long)Math.Pow(2, 42); } else { Bit42.Content = "0"; }
            if (output >= Math.Pow(2, 41)) { Bit41.Content = "1"; output -= (long)Math.Pow(2, 41); } else { Bit41.Content = "0"; }
            if (output >= Math.Pow(2, 40)) { Bit40.Content = "1"; output -= (long)Math.Pow(2, 40); } else { Bit40.Content = "0"; }
            if (output >= Math.Pow(2, 39)) { Bit39.Content = "1"; output -= (long)Math.Pow(2, 39); } else { Bit39.Content = "0"; }
            if (output >= Math.Pow(2, 38)) { Bit38.Content = "1"; output -= (long)Math.Pow(2, 38); } else { Bit38.Content = "0"; }
            if (output >= Math.Pow(2, 37)) { Bit37.Content = "1"; output -= (long)Math.Pow(2, 37); } else { Bit37.Content = "0"; }
            if (output >= Math.Pow(2, 36)) { Bit36.Content = "1"; output -= (long)Math.Pow(2, 36); } else { Bit36.Content = "0"; }
            if (output >= Math.Pow(2, 35)) { Bit35.Content = "1"; output -= (long)Math.Pow(2, 35); } else { Bit35.Content = "0"; }
            if (output >= Math.Pow(2, 34)) { Bit34.Content = "1"; output -= (long)Math.Pow(2, 34); } else { Bit34.Content = "0"; }
            if (output >= Math.Pow(2, 33)) { Bit33.Content = "1"; output -= (long)Math.Pow(2, 33); } else { Bit33.Content = "0"; }
            if (output >= Math.Pow(2, 32)) { Bit32.Content = "1"; output -= (long)Math.Pow(2, 32); } else { Bit32.Content = "0"; }
            if (output >= Math.Pow(2, 31)) { Bit31.Content = "1"; output -= (long)Math.Pow(2, 31); } else { Bit31.Content = "0"; }
            if (output >= Math.Pow(2, 30)) { Bit30.Content = "1"; output -= (long)Math.Pow(2, 30); } else { Bit30.Content = "0"; }
            if (output >= Math.Pow(2, 29)) { Bit29.Content = "1"; output -= (long)Math.Pow(2, 29); } else { Bit29.Content = "0"; }
            if (output >= Math.Pow(2, 28)) { Bit28.Content = "1"; output -= (long)Math.Pow(2, 28); } else { Bit28.Content = "0"; }
            if (output >= Math.Pow(2, 27)) { Bit27.Content = "1"; output -= (long)Math.Pow(2, 27); } else { Bit27.Content = "0"; }
            if (output >= Math.Pow(2, 26)) { Bit26.Content = "1"; output -= (long)Math.Pow(2, 26); } else { Bit26.Content = "0"; }
            if (output >= Math.Pow(2, 25)) { Bit25.Content = "1"; output -= (long)Math.Pow(2, 25); } else { Bit25.Content = "0"; }
            if (output >= Math.Pow(2, 24)) { Bit24.Content = "1"; output -= (long)Math.Pow(2, 24); } else { Bit24.Content = "0"; }
            if (output >= Math.Pow(2, 23)) { Bit23.Content = "1"; output -= (long)Math.Pow(2, 23); } else { Bit23.Content = "0"; }
            if (output >= Math.Pow(2, 22)) { Bit22.Content = "1"; output -= (long)Math.Pow(2, 22); } else { Bit22.Content = "0"; }
            if (output >= Math.Pow(2, 21)) { Bit21.Content = "1"; output -= (long)Math.Pow(2, 21); } else { Bit21.Content = "0"; }
            if (output >= Math.Pow(2, 20)) { Bit20.Content = "1"; output -= (long)Math.Pow(2, 20); } else { Bit20.Content = "0"; }
            if (output >= Math.Pow(2, 19)) { Bit19.Content = "1"; output -= (long)Math.Pow(2, 19); } else { Bit19.Content = "0"; }
            if (output >= Math.Pow(2, 18)) { Bit18.Content = "1"; output -= (long)Math.Pow(2, 18); } else { Bit18.Content = "0"; }
            if (output >= Math.Pow(2, 17)) { Bit17.Content = "1"; output -= (long)Math.Pow(2, 17); } else { Bit17.Content = "0"; }
            if (output >= Math.Pow(2, 16)) { Bit16.Content = "1"; output -= (long)Math.Pow(2, 16); } else { Bit16.Content = "0"; }
            if (output >= Math.Pow(2, 15)) { Bit15.Content = "1"; output -= (long)Math.Pow(2, 15); } else { Bit15.Content = "0"; }
            if (output >= Math.Pow(2, 14)) { Bit14.Content = "1"; output -= (long)Math.Pow(2, 14); } else { Bit14.Content = "0"; }
            if (output >= Math.Pow(2, 13)) { Bit13.Content = "1"; output -= (long)Math.Pow(2, 13); } else { Bit13.Content = "0"; }
            if (output >= Math.Pow(2, 12)) { Bit12.Content = "1"; output -= (long)Math.Pow(2, 12); } else { Bit12.Content = "0"; }
            if (output >= Math.Pow(2, 11)) { Bit11.Content = "1"; output -= (long)Math.Pow(2, 11); } else { Bit11.Content = "0"; }
            if (output >= Math.Pow(2, 10)) { Bit10.Content = "1"; output -= (long)Math.Pow(2, 10); } else { Bit10.Content = "0"; }
            if (output >= Math.Pow(2, 9)) { Bit9.Content = "1"; output -= (long)Math.Pow(2, 9); } else { Bit9.Content = "0"; }
            if (output >= Math.Pow(2, 8)) { Bit8.Content = "1"; output -= (long)Math.Pow(2, 8); } else { Bit8.Content = "0"; }
            if (output >= Math.Pow(2, 7)) { Bit7.Content = "1"; output -= (long)Math.Pow(2, 7); } else { Bit7.Content = "0"; }
            if (output >= Math.Pow(2, 6)) { Bit6.Content = "1"; output -= (long)Math.Pow(2, 6); } else { Bit6.Content = "0"; }
            if (output >= Math.Pow(2, 5)) { Bit5.Content = "1"; output -= (long)Math.Pow(2, 5); } else { Bit5.Content = "0"; }
            if (output >= Math.Pow(2, 4)) { Bit4.Content = "1"; output -= (long)Math.Pow(2, 4); } else { Bit4.Content = "0"; }
            if (output >= Math.Pow(2, 3)) { Bit3.Content = "1"; output -= (long)Math.Pow(2, 3); } else { Bit3.Content = "0"; }
            if (output >= Math.Pow(2, 2)) { Bit2.Content = "1"; output -= (long)Math.Pow(2, 2); } else { Bit2.Content = "0"; }
            if (output >= Math.Pow(2, 1)) { Bit1.Content = "1"; output -= (long)Math.Pow(2, 1); } else { Bit1.Content = "0"; }
            if (output >= Math.Pow(2, 0)) { Bit0.Content = "1"; output -= (long)Math.Pow(2, 0); } else { Bit0.Content = "0"; }
        }

        public void ToggleHexDigits(bool state)
        {
            ButtonA.IsEnabled = state;
            ButtonB.IsEnabled = state;
            ButtonC.IsEnabled = state;
            ButtonD.IsEnabled = state;
            ButtonE.IsEnabled = state;
            ButtonF.IsEnabled = state;
        }

        public void ToggleDecDigits(bool state)
        {
            Button8.IsEnabled = state;
            Button9.IsEnabled = state;
        }

        public void ToggleOctDigits(bool state)
        {
            Button2.IsEnabled = state;
            Button3.IsEnabled = state;
            Button4.IsEnabled = state;
            Button5.IsEnabled = state;
            Button6.IsEnabled = state;
            Button7.IsEnabled = state;
        }

        public void Toggle32Bits(bool state)
        {
            Bit63.IsEnabled = state; if (!state) { Bit63.Content = "0"; }
            Bit62.IsEnabled = state; if (!state) { Bit62.Content = "0"; }
            Bit61.IsEnabled = state; if (!state) { Bit61.Content = "0"; }
            Bit60.IsEnabled = state; if (!state) { Bit60.Content = "0"; }
            Bit59.IsEnabled = state; if (!state) { Bit59.Content = "0"; }
            Bit58.IsEnabled = state; if (!state) { Bit58.Content = "0"; }
            Bit57.IsEnabled = state; if (!state) { Bit57.Content = "0"; }
            Bit56.IsEnabled = state; if (!state) { Bit56.Content = "0"; }

            Bit55.IsEnabled = state; if (!state) { Bit55.Content = "0"; }
            Bit54.IsEnabled = state; if (!state) { Bit54.Content = "0"; }
            Bit53.IsEnabled = state; if (!state) { Bit53.Content = "0"; }
            Bit52.IsEnabled = state; if (!state) { Bit52.Content = "0"; }
            Bit51.IsEnabled = state; if (!state) { Bit51.Content = "0"; }
            Bit50.IsEnabled = state; if (!state) { Bit50.Content = "0"; }
            Bit49.IsEnabled = state; if (!state) { Bit49.Content = "0"; }
            Bit48.IsEnabled = state; if (!state) { Bit48.Content = "0"; }

            Bit47.IsEnabled = state; if (!state) { Bit47.Content = "0"; }
            Bit46.IsEnabled = state; if (!state) { Bit46.Content = "0"; }
            Bit45.IsEnabled = state; if (!state) { Bit45.Content = "0"; }
            Bit44.IsEnabled = state; if (!state) { Bit44.Content = "0"; }
            Bit43.IsEnabled = state; if (!state) { Bit43.Content = "0"; }
            Bit42.IsEnabled = state; if (!state) { Bit42.Content = "0"; }
            Bit41.IsEnabled = state; if (!state) { Bit41.Content = "0"; }
            Bit40.IsEnabled = state; if (!state) { Bit40.Content = "0"; }

            Bit39.IsEnabled = state; if (!state) { Bit39.Content = "0"; }
            Bit38.IsEnabled = state; if (!state) { Bit38.Content = "0"; }
            Bit37.IsEnabled = state; if (!state) { Bit37.Content = "0"; }
            Bit36.IsEnabled = state; if (!state) { Bit36.Content = "0"; }
            Bit35.IsEnabled = state; if (!state) { Bit35.Content = "0"; }
            Bit34.IsEnabled = state; if (!state) { Bit34.Content = "0"; }
            Bit33.IsEnabled = state; if (!state) { Bit33.Content = "0"; }
            Bit32.IsEnabled = state; if (!state) { Bit32.Content = "0"; }
        }

        public void Toggle16Bits(bool state)
        {
            Bit31.IsEnabled = state; if (!state) { Bit31.Content = "0"; }
            Bit30.IsEnabled = state; if (!state) { Bit30.Content = "0"; }
            Bit29.IsEnabled = state; if (!state) { Bit29.Content = "0"; }
            Bit28.IsEnabled = state; if (!state) { Bit28.Content = "0"; }
            Bit27.IsEnabled = state; if (!state) { Bit27.Content = "0"; }
            Bit26.IsEnabled = state; if (!state) { Bit26.Content = "0"; }
            Bit25.IsEnabled = state; if (!state) { Bit25.Content = "0"; }
            Bit24.IsEnabled = state; if (!state) { Bit24.Content = "0"; }

            Bit23.IsEnabled = state; if (!state) { Bit23.Content = "0"; }
            Bit22.IsEnabled = state; if (!state) { Bit22.Content = "0"; }
            Bit21.IsEnabled = state; if (!state) { Bit21.Content = "0"; }
            Bit20.IsEnabled = state; if (!state) { Bit20.Content = "0"; }
            Bit19.IsEnabled = state; if (!state) { Bit19.Content = "0"; }
            Bit18.IsEnabled = state; if (!state) { Bit18.Content = "0"; }
            Bit17.IsEnabled = state; if (!state) { Bit17.Content = "0"; }
            Bit16.IsEnabled = state; if (!state) { Bit16.Content = "0"; }
        }

        public void Toggle8Bits(bool state)
        {
            Bit15.IsEnabled = state; if (!state) { Bit15.Content = "0"; }
            Bit14.IsEnabled = state; if (!state) { Bit14.Content = "0"; }
            Bit13.IsEnabled = state; if (!state) { Bit13.Content = "0"; }
            Bit12.IsEnabled = state; if (!state) { Bit12.Content = "0"; }
            Bit11.IsEnabled = state; if (!state) { Bit11.Content = "0"; }
            Bit10.IsEnabled = state; if (!state) { Bit10.Content = "0"; }
            Bit9.IsEnabled = state; if (!state) { Bit9.Content = "0"; }
            Bit8.IsEnabled = state; if (!state) { Bit8.Content = "0"; }
        }
    }
}

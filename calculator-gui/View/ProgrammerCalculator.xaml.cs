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
    /// Interaction logic for ProgrammerCalculator.xaml
    /// </summary>
    public partial class ProgrammerCalculator : Page
    {
        int currentBase;
        int wordSize;
        ulong bitFieldOutput;
        FreeformCalculator calculator;

        public ProgrammerCalculator()
        {
            currentBase = 10;
            calculator = new FreeformCalculator();
            calculator.numberBase = currentBase;
            InitializeComponent();
            GenerateConversionPreview();
        }

        public void WordSizeChanged(object sender, EventArgs e)
        {
            if (WordSize.SelectedIndex == 3)
            {
                // 64-bit, enable all
                Toggle32Bits(true);
                Toggle16Bits(true);
                Toggle8Bits(true);
                wordSize = 64;
            }
            else if (WordSize.SelectedIndex == 2)
            {
                // 32-bit, enable only the first 32 bits
                Toggle32Bits(false);
                Toggle16Bits(true);
                Toggle8Bits(true);
                wordSize = 32;
            }
            else if (WordSize.SelectedIndex == 1)
            {
                // 16-bit enable only the first 16 bits
                Toggle32Bits(false);
                Toggle16Bits(false);
                Toggle8Bits(true);
                wordSize = 16;
            }
            else
            {
                Toggle32Bits(false);
                Toggle16Bits(false);
                Toggle8Bits(false);
                wordSize = 8;
            }
            UpdateBitFieldOutput();
            for (int tokenIndex = 0; tokenIndex < calculator.TokenList.Count; tokenIndex++)
            {
                if (calculator.TokenList[tokenIndex] is FloatToken floatToken)
                {
                    floatToken.value %= (float)Math.Pow(2, wordSize);
                }
            }
            OutputTextBox.Text = calculator.ToString();
        }

        public void BaseChanged(object sender, RoutedEventArgs e)
        {
            if (BaseSelector.SelectedIndex == 0)
            {
                // binary, disable all digit buttons except 1 and 0
                ToggleOctDigits(false);
                ToggleDecDigits(false);
                ToggleHexDigits(false);
                currentBase = 2;
            }
            else if (BaseSelector.SelectedIndex == 1)
            {
                // octal, disable all digit buttons above 7
                ToggleOctDigits(true);
                ToggleDecDigits(false);
                ToggleHexDigits(false);
                currentBase = 8;
            }
            else if (BaseSelector.SelectedIndex == 2)
            {
                // decimal, disable all hex digits
                ToggleOctDigits(true);
                ToggleDecDigits(true);
                ToggleHexDigits(false);
                currentBase = 10;
            }
            else
            {
                // hex, enable all digits
                ToggleOctDigits(true);
                ToggleDecDigits(true);
                ToggleHexDigits(true);
                currentBase = 16;
            }
            calculator.numberBase = currentBase;
            OutputTextBox.Text = calculator.ToString();
        }

        public void BitButtonPressed(object sender, RoutedEventArgs e)
        {
            Button buttonPressed = sender as Button;
            if ((string)buttonPressed.Content == "0")
            {
                buttonPressed.Content = "1";
            }
            else
            {
                buttonPressed.Content = "0";
            }
            UpdateBitFieldOutput();
        }

        public void AppendDigit(object sender, RoutedEventArgs e)
        {
            if (bitFieldOutput * (uint)currentBase <= Math.Pow(2,wordSize))
            {
                string digit = (string)((Button)sender).Tag;
                bitFieldOutput *= (uint)currentBase;
                if (digit.Equals("1")) { bitFieldOutput += 1; }
                else if (digit.Equals("2")) { bitFieldOutput += 2; }
                else if (digit.Equals("3")) { bitFieldOutput += 3; }
                else if (digit.Equals("4")) { bitFieldOutput += 4; }
                else if (digit.Equals("5")) { bitFieldOutput += 5; }
                else if (digit.Equals("6")) { bitFieldOutput += 6; }
                else if (digit.Equals("7")) { bitFieldOutput += 7; }
                else if (digit.Equals("8")) { bitFieldOutput += 8; }
                else if (digit.Equals("9")) { bitFieldOutput += 9; }
                else if (digit.Equals("A")) { bitFieldOutput += 10; }
                else if (digit.Equals("B")) { bitFieldOutput += 11; }
                else if (digit.Equals("C")) { bitFieldOutput += 12; }
                else if (digit.Equals("D")) { bitFieldOutput += 13; }
                else if (digit.Equals("E")) { bitFieldOutput += 14; }
                else if (digit.Equals("F")) { bitFieldOutput += 15; }
                OutputTextBox.Text += digit;

                calculator.Input = OutputTextBox.Text;
                UpdateBitField();
                GenerateConversionPreview();
            }
        }

        public void AppendOperator(object sender, RoutedEventArgs e)
        {
            calculator.Input += ((Button)sender).Tag;
            if (calculator.isValidExpression)
            {
                OutputTextBox.Text = calculator.ToString();
            }
            else
            {
                OutputTextBox.Text = calculator.Input;
            }
        }

        public void ButtonDEL_Pressed(object sender, RoutedEventArgs e)
        {
            if (calculator.TokenList.Count > 0 && calculator.TokenList[calculator.TokenList.Count - 1] is FloatToken)
            {
                if (OutputTextBox.Text.Length > 0)
                {
                    OutputTextBox.Text = OutputTextBox.Text.Substring(0, OutputTextBox.Text.Length - 1);
                    bitFieldOutput /= (uint)currentBase;
                    calculator.Input = OutputTextBox.Text;
                    UpdateBitField();
                    GenerateConversionPreview();
                }
            }
            else
            {
                if (calculator.TokenList.Count > 0)
                {
                    calculator.TokenList.RemoveAt(calculator.TokenList.Count - 1);
                }
                OutputTextBox.Text = calculator.ToString();
                UpdateBitField();
                GenerateConversionPreview();
            }
        }

        public void ButtonAC_Pressed(object sender, RoutedEventArgs e)
        {
            bitFieldOutput = 0;
            OutputTextBox.Text = "";
            calculator.Input = OutputTextBox.Text;
            UpdateBitField();
            GenerateConversionPreview();
        }

        public void MoveCursorLeft(object sender, RoutedEventArgs e)
        {
            if (OutputTextBox.SelectionStart > 0)
            {
                OutputTextBox.Select(OutputTextBox.SelectionStart - 1, 0);
            }
        }

        public void MoveCursorRight(object sender, RoutedEventArgs e)
        {
            if (OutputTextBox.SelectionStart < OutputTextBox.Text.Length)
            {
                OutputTextBox.Select(OutputTextBox.SelectionStart + 1, 0);
            }
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

        public void UpdateBitFieldOutput()
        {
            double output = 0;
            if ((string)Bit0.Content == "1") { output += (ulong)Math.Pow(2, 0); }
            if ((string)Bit1.Content == "1") { output += (ulong)Math.Pow(2, 1); }
            if ((string)Bit2.Content == "1") { output += (ulong)Math.Pow(2, 2); }
            if ((string)Bit3.Content == "1") { output += (ulong)Math.Pow(2, 3); }
            if ((string)Bit4.Content == "1") { output += (ulong)Math.Pow(2, 4); }
            if ((string)Bit5.Content == "1") { output += (ulong)Math.Pow(2, 5); }
            if ((string)Bit6.Content == "1") { output += (ulong)Math.Pow(2, 6); }
            if ((string)Bit7.Content == "1") { output += (ulong)Math.Pow(2, 7); }
            if ((string)Bit8.Content == "1") { output += (ulong)Math.Pow(2, 8); }
            if ((string)Bit9.Content == "1") { output += (ulong)Math.Pow(2, 9); }
            if ((string)Bit10.Content == "1") { output += (ulong)Math.Pow(2, 10); }
            if ((string)Bit11.Content == "1") { output += (ulong)Math.Pow(2, 11); }
            if ((string)Bit12.Content == "1") { output += (ulong)Math.Pow(2, 12); }
            if ((string)Bit13.Content == "1") { output += (ulong)Math.Pow(2, 13); }
            if ((string)Bit14.Content == "1") { output += (ulong)Math.Pow(2, 14); }
            if ((string)Bit15.Content == "1") { output += (ulong)Math.Pow(2, 15); }
            if ((string)Bit16.Content == "1") { output += (ulong)Math.Pow(2, 16); }
            if ((string)Bit17.Content == "1") { output += (ulong)Math.Pow(2, 17); }
            if ((string)Bit18.Content == "1") { output += (ulong)Math.Pow(2, 18); }
            if ((string)Bit19.Content == "1") { output += (ulong)Math.Pow(2, 19); }
            if ((string)Bit20.Content == "1") { output += (ulong)Math.Pow(2, 20); }
            if ((string)Bit21.Content == "1") { output += (ulong)Math.Pow(2, 21); }
            if ((string)Bit22.Content == "1") { output += (ulong)Math.Pow(2, 22); }
            if ((string)Bit23.Content == "1") { output += (ulong)Math.Pow(2, 23); }
            if ((string)Bit24.Content == "1") { output += (ulong)Math.Pow(2, 24); }
            if ((string)Bit25.Content == "1") { output += (ulong)Math.Pow(2, 25); }
            if ((string)Bit26.Content == "1") { output += (ulong)Math.Pow(2, 26); }
            if ((string)Bit27.Content == "1") { output += (ulong)Math.Pow(2, 27); }
            if ((string)Bit28.Content == "1") { output += (ulong)Math.Pow(2, 28); }
            if ((string)Bit29.Content == "1") { output += (ulong)Math.Pow(2, 29); }
            if ((string)Bit30.Content == "1") { output += (ulong)Math.Pow(2, 30); }
            if ((string)Bit31.Content == "1") { output += (ulong)Math.Pow(2, 31); }
            if ((string)Bit32.Content == "1") { output += (ulong)Math.Pow(2, 32); }
            if ((string)Bit33.Content == "1") { output += (ulong)Math.Pow(2, 33); }
            if ((string)Bit34.Content == "1") { output += (ulong)Math.Pow(2, 34); }
            if ((string)Bit35.Content == "1") { output += (ulong)Math.Pow(2, 35); }
            if ((string)Bit36.Content == "1") { output += (ulong)Math.Pow(2, 36); }
            if ((string)Bit37.Content == "1") { output += (ulong)Math.Pow(2, 37); }
            if ((string)Bit38.Content == "1") { output += (ulong)Math.Pow(2, 38); }
            if ((string)Bit39.Content == "1") { output += (ulong)Math.Pow(2, 39); }
            if ((string)Bit40.Content == "1") { output += (ulong)Math.Pow(2, 40); }
            if ((string)Bit41.Content == "1") { output += (ulong)Math.Pow(2, 41); }
            if ((string)Bit42.Content == "1") { output += (ulong)Math.Pow(2, 42); }
            if ((string)Bit43.Content == "1") { output += (ulong)Math.Pow(2, 43); }
            if ((string)Bit44.Content == "1") { output += (ulong)Math.Pow(2, 44); }
            if ((string)Bit45.Content == "1") { output += (ulong)Math.Pow(2, 45); }
            if ((string)Bit46.Content == "1") { output += (ulong)Math.Pow(2, 46); }
            if ((string)Bit47.Content == "1") { output += (ulong)Math.Pow(2, 47); }
            if ((string)Bit48.Content == "1") { output += (ulong)Math.Pow(2, 48); }
            if ((string)Bit49.Content == "1") { output += (ulong)Math.Pow(2, 49); }
            if ((string)Bit50.Content == "1") { output += (ulong)Math.Pow(2, 50); }
            if ((string)Bit51.Content == "1") { output += (ulong)Math.Pow(2, 51); }
            if ((string)Bit52.Content == "1") { output += (ulong)Math.Pow(2, 52); }
            if ((string)Bit53.Content == "1") { output += (ulong)Math.Pow(2, 53); }
            if ((string)Bit54.Content == "1") { output += (ulong)Math.Pow(2, 54); }
            if ((string)Bit55.Content == "1") { output += (ulong)Math.Pow(2, 55); }
            if ((string)Bit56.Content == "1") { output += (ulong)Math.Pow(2, 56); }
            if ((string)Bit57.Content == "1") { output += (ulong)Math.Pow(2, 57); }
            if ((string)Bit58.Content == "1") { output += (ulong)Math.Pow(2, 58); }
            if ((string)Bit59.Content == "1") { output += (ulong)Math.Pow(2, 59); }
            if ((string)Bit60.Content == "1") { output += (ulong)Math.Pow(2, 60); }
            if ((string)Bit61.Content == "1") { output += (ulong)Math.Pow(2, 61); }
            if ((string)Bit62.Content == "1") { output += (ulong)Math.Pow(2, 62); }
            if ((string)Bit63.Content == "1") { output += (ulong)Math.Pow(2, 63); }
            if (calculator.TokenList.Count > 0 && calculator.TokenList[calculator.TokenList.Count - 1] is FloatToken token)
            {
                char[] chars = calculator.Input.ToCharArray();
                for (int index = chars.Length - 1; index >= 0; index--)
                {
                    if (!Char.IsDigit(chars[index]) && chars[index] != 'A' && chars[index] != 'B' && chars[index] != 'C' && chars[index] != 'D' && chars[index] != 'E' && chars[index] != 'F')
                    {
                        calculator.Input = new string(chars).Substring(0, index + 1) + output.ToString();
                        OutputTextBox.Text = calculator.ToString();
                    }
                }
            }
            else
            {
                calculator.Input += output.ToString();
                OutputTextBox.Text = calculator.ToString();
            }
            GenerateConversionPreview();
        }

        public void UpdateBitField()
        {
            ulong temporaryInput = 0;
            if (calculator.TokenList.Count > 0 && calculator.TokenList[calculator.TokenList.Count - 1] is FloatToken token)
            {
                if (token.value - (int)(token.value) < Single.Epsilon)
                {
                    temporaryInput = (ulong)token.value;
                }
            }
            if ((temporaryInput >> 63 & 1) == 1) { Bit63.Content = "1"; } else { Bit63.Content = "0"; }
            if ((temporaryInput >> 62 & 1) == 1) { Bit62.Content = "1"; } else { Bit62.Content = "0"; } 
            if ((temporaryInput >> 61 & 1) == 1) { Bit61.Content = "1"; } else { Bit61.Content = "0"; }
            if ((temporaryInput >> 60 & 1) == 1) { Bit60.Content = "1"; } else { Bit60.Content = "0"; }
            if ((temporaryInput >> 59 & 1) == 1) { Bit59.Content = "1"; } else { Bit59.Content = "0"; }
            if ((temporaryInput >> 58 & 1) == 1) { Bit58.Content = "1"; } else { Bit58.Content = "0"; }
            if ((temporaryInput >> 57 & 1) == 1) { Bit57.Content = "1"; } else { Bit57.Content = "0"; }
            if ((temporaryInput >> 56 & 1) == 1) { Bit56.Content = "1"; } else { Bit56.Content = "0"; }
            if ((temporaryInput >> 55 & 1) == 1) { Bit55.Content = "1"; } else { Bit55.Content = "0"; }
            if ((temporaryInput >> 54 & 1) == 1) { Bit54.Content = "1"; } else { Bit54.Content = "0"; }
            if ((temporaryInput >> 53 & 1) == 1) { Bit53.Content = "1"; } else { Bit53.Content = "0"; }
            if ((temporaryInput >> 52 & 1) == 1) { Bit52.Content = "1"; } else { Bit52.Content = "0"; }
            if ((temporaryInput >> 51 & 1) == 1) { Bit51.Content = "1"; } else { Bit51.Content = "0"; }
            if ((temporaryInput >> 50 & 1) == 1) { Bit50.Content = "1"; } else { Bit50.Content = "0"; }
            if ((temporaryInput >> 49 & 1) == 1) { Bit49.Content = "1"; } else { Bit49.Content = "0"; }
            if ((temporaryInput >> 48 & 1) == 1) { Bit48.Content = "1"; } else { Bit48.Content = "0"; }
            if ((temporaryInput >> 47 & 1) == 1) { Bit47.Content = "1"; } else { Bit47.Content = "0"; }
            if ((temporaryInput >> 46 & 1) == 1) { Bit46.Content = "1"; } else { Bit46.Content = "0"; }
            if ((temporaryInput >> 45 & 1) == 1) { Bit45.Content = "1"; } else { Bit45.Content = "0"; }
            if ((temporaryInput >> 44 & 1) == 1) { Bit44.Content = "1"; } else { Bit44.Content = "0"; }
            if ((temporaryInput >> 43 & 1) == 1) { Bit43.Content = "1"; } else { Bit43.Content = "0"; }
            if ((temporaryInput >> 42 & 1) == 1) { Bit42.Content = "1"; } else { Bit42.Content = "0"; }
            if ((temporaryInput >> 41 & 1) == 1) { Bit41.Content = "1"; } else { Bit41.Content = "0"; }
            if ((temporaryInput >> 40 & 1) == 1) { Bit40.Content = "1"; } else { Bit40.Content = "0"; }
            if ((temporaryInput >> 39 & 1) == 1) { Bit39.Content = "1"; } else { Bit39.Content = "0"; }
            if ((temporaryInput >> 38 & 1) == 1) { Bit38.Content = "1"; } else { Bit38.Content = "0"; }
            if ((temporaryInput >> 37 & 1) == 1) { Bit37.Content = "1"; } else { Bit37.Content = "0"; }
            if ((temporaryInput >> 36 & 1) == 1) { Bit36.Content = "1"; } else { Bit36.Content = "0"; }
            if ((temporaryInput >> 35 & 1) == 1) { Bit35.Content = "1"; } else { Bit35.Content = "0"; }
            if ((temporaryInput >> 34 & 1) == 1) { Bit34.Content = "1"; } else { Bit34.Content = "0"; }
            if ((temporaryInput >> 33 & 1) == 1) { Bit33.Content = "1"; } else { Bit33.Content = "0"; }
            if ((temporaryInput >> 32 & 1) == 1) { Bit32.Content = "1"; } else { Bit32.Content = "0"; }
            if ((temporaryInput >> 31 & 1) == 1) { Bit31.Content = "1"; } else { Bit31.Content = "0"; }
            if ((temporaryInput >> 30 & 1) == 1) { Bit30.Content = "1"; } else { Bit30.Content = "0"; }
            if ((temporaryInput >> 29 & 1) == 1) { Bit29.Content = "1"; } else { Bit29.Content = "0"; }
            if ((temporaryInput >> 28 & 1) == 1) { Bit28.Content = "1"; } else { Bit28.Content = "0"; }
            if ((temporaryInput >> 27 & 1) == 1) { Bit27.Content = "1"; } else { Bit27.Content = "0"; }
            if ((temporaryInput >> 26 & 1) == 1) { Bit26.Content = "1"; } else { Bit26.Content = "0"; }
            if ((temporaryInput >> 25 & 1) == 1) { Bit25.Content = "1"; } else { Bit25.Content = "0"; }
            if ((temporaryInput >> 24 & 1) == 1) { Bit24.Content = "1"; } else { Bit24.Content = "0"; }
            if ((temporaryInput >> 23 & 1) == 1) { Bit23.Content = "1"; } else { Bit23.Content = "0"; }
            if ((temporaryInput >> 22 & 1) == 1) { Bit22.Content = "1"; } else { Bit22.Content = "0"; }
            if ((temporaryInput >> 21 & 1) == 1) { Bit21.Content = "1"; } else { Bit21.Content = "0"; }
            if ((temporaryInput >> 20 & 1) == 1) { Bit20.Content = "1"; } else { Bit20.Content = "0"; }
            if ((temporaryInput >> 19 & 1) == 1) { Bit19.Content = "1"; } else { Bit19.Content = "0"; }
            if ((temporaryInput >> 18 & 1) == 1) { Bit18.Content = "1"; } else { Bit18.Content = "0"; }
            if ((temporaryInput >> 17 & 1) == 1) { Bit17.Content = "1"; } else { Bit17.Content = "0"; }
            if ((temporaryInput >> 16 & 1) == 1) { Bit16.Content = "1"; } else { Bit16.Content = "0"; }
            if ((temporaryInput >> 15 & 1) == 1) { Bit15.Content = "1"; } else { Bit15.Content = "0"; }
            if ((temporaryInput >> 14 & 1) == 1) { Bit14.Content = "1"; } else { Bit14.Content = "0"; }
            if ((temporaryInput >> 13 & 1) == 1) { Bit13.Content = "1"; } else { Bit13.Content = "0"; }
            if ((temporaryInput >> 12 & 1) == 1) { Bit12.Content = "1"; } else { Bit12.Content = "0"; }
            if ((temporaryInput >> 11 & 1) == 1) { Bit11.Content = "1"; } else { Bit11.Content = "0"; }
            if ((temporaryInput >> 10 & 1) == 1) { Bit10.Content = "1"; } else { Bit10.Content = "0"; }
            if ((temporaryInput >> 9 & 1) == 1) { Bit9.Content = "1"; } else { Bit9.Content = "0"; }
            if ((temporaryInput >> 8 & 1) == 1) { Bit8.Content = "1"; } else { Bit8.Content = "0"; }
            if ((temporaryInput >> 7 & 1) == 1) { Bit7.Content = "1"; } else { Bit7.Content = "0"; }
            if ((temporaryInput >> 6 & 1) == 1) { Bit6.Content = "1"; } else { Bit6.Content = "0"; }
            if ((temporaryInput >> 5 & 1) == 1) { Bit5.Content = "1"; } else { Bit5.Content = "0"; }
            if ((temporaryInput >> 4 & 1) == 1) { Bit4.Content = "1"; } else { Bit4.Content = "0"; }
            if ((temporaryInput >> 3 & 1) == 1) { Bit3.Content = "1"; } else { Bit3.Content = "0"; }
            if ((temporaryInput >> 2 & 1) == 1) { Bit2.Content = "1"; } else { Bit2.Content = "0"; }
            if ((temporaryInput >> 1 & 1) == 1) { Bit1.Content = "1"; } else { Bit1.Content = "0"; }
            if ((temporaryInput & 1) == 1) { Bit0.Content = "1"; } else { Bit0.Content = "0"; }
        }

        public void GenerateConversionPreview()
        {
            if (calculator.TokenList.Count > 0 && calculator.TokenList[calculator.TokenList.Count - 1] is FloatToken)
            {
                FloatToken token = (FloatToken)calculator.TokenList[calculator.TokenList.Count - 1];
                string output = token.ToString(8) + "₈ = " + token.ToString(10) + "₁₀ = " + token.ToString(16).ToUpper() + "₁₆";
                ConversionPreview.Text = output;
            }
        }
    }
}

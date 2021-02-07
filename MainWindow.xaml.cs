using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Reed_Solomon_Demo {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private byte[] GetByteArr(TextBox textBox) {
            string PolynomeStr = textBox.Text;
            List<byte> Polynome_list = new List<byte>();
            Regex TwoSymbRegExp = new Regex(@"[0-9a-fA-F]{2}");
            MatchCollection PolynomeMtchCol = TwoSymbRegExp.Matches(PolynomeStr);
            foreach (Match Mtch in PolynomeMtchCol) {
                Polynome_list.Add(byte.Parse(Mtch.Value, NumberStyles.AllowHexSpecifier));
            }
            string PolynomeStr_noSpaces = Regex.Replace(PolynomeStr, @"[^0-9a-fA-F]", "");
            if (PolynomeStr_noSpaces.Length % 2 != 0) {
                Polynome_list.Add(byte.Parse((PolynomeStr_noSpaces[PolynomeStr_noSpaces.Length - 1]).ToString(), NumberStyles.AllowHexSpecifier));
            }
            return Polynome_list.ToArray();
        }
        string GetStringFromByteArr(byte[] Arr) {
            string str = "";

            if (null == Arr) {
                TextBox_Rezult.Text = "0";
                return "";
            }
            for (int i = 0; i < Arr.Length; i++) {
                string btstr = Arr[i].ToString("X");
                if (1 == btstr.Length) { btstr = "0" + btstr; }
                str += btstr + " ";
            }
            return str;
        }
        private void OutByteArr(TextBox textBox, byte[] Arr) {            
            textBox.Text = GetStringFromByteArr(Arr);
        }        
        public MainWindow() {
            InitializeComponent();
        }
        private void GF_Decimal_TextChanged(object sender) {
            if (string.Empty == ((TextBox)sender).Text || "" == ((TextBox)sender).Text) { return; }
            int tmp = ((TextBox)sender).CaretIndex;
            string tmptxt = ((TextBox)sender).Text;
            if (string.Empty == ((TextBox)sender).Text || "" == ((TextBox)sender).Text) { return; }
            string Str = Regex.Replace(((TextBox)sender).Text, @"[^0-9-]", "");
            int tmp_int = 0;
            if (!int.TryParse(Str, out tmp_int) && (!("-" == Str))) { ((TextBox)sender).Text = ""; return; }
            if (tmp_int > 255) {
                ((TextBox)sender).Text = "255";
                return;
            }
            if (tmp_int < -255) {
                ((TextBox)sender).Text = "-255";
                return;
            }

            if ("-" != Str) { ((TextBox)sender).Text = tmp_int.ToString(); }
            if (tmptxt != ((TextBox)sender).Text) {
                ((TextBox)sender).CaretIndex = tmp > 0 ? tmp - 1 : tmp;
            } else {
                ((TextBox)sender).CaretIndex = tmp;
            }
        }
        private void GF_Poly_TextChanged(object sender) {
            if (string.Empty == ((TextBox)sender).Text || "" == ((TextBox)sender).Text) { return; }
            int StartCaretIndex = ((TextBox)sender).CaretIndex;
            string StartText = ((TextBox)sender).Text;
            string NewText;
            NewText = Regex.Replace(((TextBox)sender).Text, @"[^0-9a-fA-F]", "");
            if (string.Empty == NewText || "" == NewText) { ((TextBox)sender).Text = ""; return; }

            Regex TwoSymbRegExp = new Regex(@"[0-9a-fA-F]{2}");
            MatchCollection TwoSymbMtchCol = TwoSymbRegExp.Matches(NewText);

            string NewStr = "";
            foreach (Match Mtch in TwoSymbMtchCol) {
                NewStr += Mtch.Value + " ";
            }

            if (NewText.Length % 2 != 0) {
                NewStr += NewText[NewText.Length - 1];
            }

            ((TextBox)sender).Text = NewStr;
            if (NewStr.Length > StartText.Length) {
                StartCaretIndex += NewStr.Length - StartText.Length;
            }
            ((TextBox)sender).CaretIndex = StartCaretIndex;
        }
        private void TextBox_Decimal_TextChanged(object sender, TextChangedEventArgs e) {
            ((TextBox)sender).TextChanged -= TextBox_Decimal_TextChanged;
            GF_Decimal_TextChanged(sender);
            ((TextBox)sender).TextChanged += TextBox_Decimal_TextChanged;
        }
        private void TextBox_Encoded_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox_Encoded_2.TextChanged -= TextBox_Hex_TextChanged;
            TextBox_Encoded_2.Text = TextBox_Encoded.Text;
            TextBox_Encoded_2.TextChanged += TextBox_Hex_TextChanged;
        }
        private void TextBox_Hex_TextChanged(object sender, TextChangedEventArgs e) {
            ((TextBox)sender).TextChanged -= TextBox_Hex_TextChanged;
            GF_Poly_TextChanged(sender);
            ((TextBox)sender).TextChanged += TextBox_Hex_TextChanged;
        }
        private void TextBox_GeneratorPoly_TextChanged(object sender, TextChangedEventArgs e) {
            ((TextBox)sender).TextChanged -= TextBox_GeneratorPoly_TextChanged;
            GF_Poly_TextChanged(sender);
            ((TextBox)sender).TextChanged += TextBox_GeneratorPoly_TextChanged;
        }
        private void Button_CheckOp_Click(object sender, RoutedEventArgs e) {
            //if (string.Empty == TextBox_Op1.Text || string.Empty == TextBox_Op2.Text || string.Empty == ComboBox_Op.Text) { return; }
            int Op1 = 0;
            int Op2 = 0;
            bool Op1Ok;
            bool Op2Ok;
            Op1Ok = int.TryParse(TextBox_Op1.Text, out Op1);
            Op2Ok = int.TryParse(TextBox_Op2.Text, out Op2);


            if ("Pow" != ComboBox_Op.Text) {
                Op1 = Glob.IntMod(Op1);
                Op2 = Glob.IntMod(Op2);
            }

            switch (ComboBox_Op.Text) {
                case "+":
                    TextBox_Rez.Text = GF_256_Aryth.Add((byte)Op1, (byte)Op2).ToString();
                    break;
                case "-":
                    TextBox_Rez.Text = GF_256_Aryth.Sub((byte)Op1, (byte)Op2).ToString();
                    break;
                case "*":
                    TextBox_Rez.Text = GF_b2_Aryth.Galois_b2_ext_mult((byte)Op1, (byte)Op2, GF_256_Aryth.GetCurrPoly()).ToString();
                    break;
                case "/":
                    if (0 == Op2) { MessageBox.Show("Деление на нуль!"); break; }
                    TextBox_Rez.Text = GF_256_Aryth.Div((byte)Op1, (byte)Op2).ToString();
                    break;
                case "Pow":
                    TextBox_Rez.Text = GF_256_Aryth.Pow((byte)Op1, Op2).ToString();
                    break;
                case "Inv":
                    if (0 == Op1) { TextBox_Rez.Text = "0"; break; }
                    TextBox_Rez.Text = GF_256_Aryth.Inverse((byte)Op1).ToString();
                    break;
                case "Log_a":
                    if (0 == Op1) { TextBox_Rez.Text = "0"; break; }
                    TextBox_Rez.Text = GF_256_Aryth.Log_a((byte)Op1).ToString();
                    break;
            }
        }
        private void Button_test_Click(object sender, RoutedEventArgs e) {
            uint Poly, Prim;
            if (!uint.TryParse(TextBox_GF256_Poly.Text, out Poly)) { MessageBox.Show("Некорректный ввод порождающего полинома GF[256]"); return; }
            if (!uint.TryParse(TextBox_PowBase.Text, out Prim)) { MessageBox.Show("Некорректный ввод примитивного члена GF[256]"); return; }
            GF_256_Aryth.MakePowTable(Poly, Prim, true);
        }
        List<List<TextBox>> AllGrid;
        private void Button_MulTable_Click(object sender, RoutedEventArgs e) {
            uint Gf8Poly;

            if (!uint.TryParse(TextBox_GF8_Poly.Text, out Gf8Poly)) { return; }


            if (null == AllGrid) {
                AllGrid = new List<List<TextBox>>();
                for (int i = 0; i < 8; i++) {
                    Grid_nums.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20, GridUnitType.Pixel) });
                    Grid_nums.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20, GridUnitType.Pixel) });
                }
                for (int i = 0; i < 8; i++) {
                    List<TextBox> Row = new List<TextBox>();
                    for (int j = 0; j < 8; j++) {
                        TextBox Lbl = new TextBox();
                        Grid.SetRow(Lbl, i);
                        Grid.SetColumn(Lbl, j);
                        Lbl.IsEnabled = false;
                        Lbl.TextAlignment = TextAlignment.Center;
                        Grid_nums.Children.Add(Lbl);
                        Row.Add(Lbl);
                    }
                    AllGrid.Add(Row);
                }
            }

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    AllGrid[i][j].Text = "";
                }
            }

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (0 == i) {
                        AllGrid[i][j].Text = j.ToString();
                    } else if (0 == j) {
                        AllGrid[i][j].Text = i.ToString();
                    } else {
                        AllGrid[i][j].Text = GF_b2_Aryth.Galois_b2_ext_mult((uint)i, (uint)j, Gf8Poly).ToString();
                    }
                }
            }

        }
        private void Button_Add_Click(object sender, RoutedEventArgs e) {
            OutByteArr(TextBox_Rezult, GF_Poly.Add(new GF_Poly(GetByteArr(TextBox_Poly1)), new GF_Poly(GetByteArr(TextBox_Poly2))).GetByteArr());
        }
        private void Button_Mult_Click(object sender, RoutedEventArgs e) {
            OutByteArr(TextBox_Rezult, GF_Poly.Add(
                new GF_Poly(GetByteArr(TextBox_Remndr)),
                GF_Poly.Mult(
                    new GF_Poly(GetByteArr(TextBox_Poly1)),
                    new GF_Poly(GetByteArr(TextBox_Poly2))
                    )
                ).GetByteArr());
        }
        private void Button_Div_Click(object sender, RoutedEventArgs e) {
            OutByteArr(TextBox_Rezult, GF_Poly.Div(new GF_Poly(GetByteArr(TextBox_Poly1)), new GF_Poly(GetByteArr(TextBox_Poly2))).GetByteArr());
            OutByteArr(TextBox_Remndr, GF_Poly.Div_Remainder(new GF_Poly(GetByteArr(TextBox_Poly1)), new GF_Poly(GetByteArr(TextBox_Poly2))).GetByteArr());
        }
        private void Button_Der_Click(object sender, RoutedEventArgs e) {
            OutByteArr(TextBox_Rezult, (new GF_Poly(GetByteArr(TextBox_Poly1))).FormalDerivative().GetByteArr());
        }
        private void Button_Generate_Click(object sender, RoutedEventArgs e) {
            if (!uint.TryParse(TextBox_NumEcSym.Text, out uint NumOfECC)) { MessageBox.Show("Некорректный ввод количества символов коррекции ошибок"); return; }
            OutByteArr(TextBox_GeneratorPoly, ReedSolomonOps.GenerateGenerator(NumOfECC).GetByteArr());
        }
        private void Button_encode_Click(object sender, RoutedEventArgs e) {
            if (0 == GetByteArr(TextBox_GeneratorPoly).Length) { MessageBox.Show("Некорректный ввод генератора"); return; }
            OutByteArr(
                TextBox_Encoded,
                ReedSolomonOps.EncodeMessage(
                    new GF_Poly(GetByteArr(TextBox_Msg)),
                    new GF_Poly(GetByteArr(TextBox_GeneratorPoly))
                ).GetByteArr()
            );
        }
        private void Button_GetBytes_Click(object sender, RoutedEventArgs e) {
            byte[] arr = Encoding.UTF8.GetBytes(TextBox_Str.Text);
            string str = "";
            for (int i = 0; i < arr.Length; i++) {
                string btstr = arr[i].ToString("X");
                if (1 == btstr.Length) { btstr = "0" + btstr; }
                str += btstr + " ";
            }
            TextBox_Msg.Text = str;
        }
        private void Button_CalcSyndrome_Click(object sender, RoutedEventArgs e) {
            if (!uint.TryParse(TextBox_NumEcSym.Text, out uint NumOfECC)) { MessageBox.Show("Некорректный ввод количества символов коррекции ошибок"); return; }
            OutByteArr(
                TextBox_Syndromes,
                ReedSolomonOps.CalcSyndromes(
                    new GF_Poly(GetByteArr(TextBox_Encoded_2)),
                    NumOfECC
                ).GetByteArr()
            );
        }
        private void Button_GetLocator_Click(object sender, RoutedEventArgs e) {
            byte[] SyndromesStr = GetByteArr(TextBox_Syndromes);
            byte[] DefErrPos = GetByteArr(TextBox_ErrPos_Defined);            
            if (!uint.TryParse(TextBox_NumEcSym.Text, out uint NumOfECC)) { MessageBox.Show("Некорректный ввод количества символов коррекции ошибок"); return; }

            byte[] CalculatedSyndromes;
            if (0 == DefErrPos.Length) {
                CalculatedSyndromes = ReedSolomonOps.CalcLocatorPoly(new GF_Poly(SyndromesStr), NumOfECC).GetByteArr();
            } else {
                //GF_Poly LocatorFromDefPos = ReedSolomonOps.CalcLocatorPoly(DefErrPos);
                //GF_Poly LocatorFromSyndr = ReedSolomonOps.CalcLocatorPoly(new GF_Poly(SyndromesStr), NumOfECC);
                //CalculatedSyndromes = ReedSolomonOps.MergeLocators(LocatorFromDefPos, LocatorFromSyndr).GetByteArr();
                CalculatedSyndromes = ReedSolomonOps.CalcLocatorPoly(new GF_Poly(SyndromesStr), DefErrPos, NumOfECC).GetByteArr();
            }            
            OutByteArr(TextBox_Locator_BM, CalculatedSyndromes);
        }
        private void Button_CalcErrPos_Click(object sender, RoutedEventArgs e) {
            if (0 == GetByteArr(TextBox_Locator_BM).Length) { MessageBox.Show("Не рассчитан полином локаторов"); return; }
            OutByteArr(
                TextBox_ErrPosCalculated,
                ReedSolomonOps.FindErrPos(
                    new GF_Poly(GetByteArr(TextBox_Locator_BM))
                ).GetByteArr()
            );
        }
        private void Button_CalcMagnitudes_Click(object sender, RoutedEventArgs e) {
            if (!uint.TryParse(TextBox_NumEcSym.Text, out uint NumOfECC)) { MessageBox.Show("Некорректный ввод количества символов коррекции ошибок"); return; }

            byte[] LocatorFromBMArr = GetByteArr(TextBox_Locator_BM);
            byte[] SyndromesArr = GetByteArr(TextBox_Syndromes);
            
            if (0 == LocatorFromBMArr.Length) { MessageBox.Show("Не рассчитан полином локаторов"); return; }
            if (0 == SyndromesArr.Length)  { MessageBox.Show("Не рассчитан полином синдромов"); return; }
            GF_Poly LocatorPoly = new GF_Poly(LocatorFromBMArr);
            OutByteArr(
                TextBox_Magnitudes,
                ReedSolomonOps.FindMagnitudes(
                    new GF_Poly(GetByteArr(TextBox_Syndromes)),
                    LocatorPoly,
                    NumOfECC
                ).GetByteArr()
            );
        }
        private void Button_CalcMagnitudes2_Click(object sender, RoutedEventArgs e) {
            if (!uint.TryParse(TextBox_NumEcSym.Text, out uint NumOfECC)) { MessageBox.Show("Некорректный ввод количества символов коррекции ошибок"); return; }

            byte[] ErrPos = GetByteArr(TextBox_ErrPos_Defined);                        
            GF_Poly ErrPosPoly = new GF_Poly(ErrPos);
            OutByteArr(
                TextBox_Magnitudes,
                ReedSolomonOps.FindMagnitudesFromErrPos(
                    new GF_Poly(GetByteArr(TextBox_Syndromes)),
                    ErrPosPoly,
                    NumOfECC
                ).GetByteArr()
            );
        }
        private void Button_Decode_Click(object sender, RoutedEventArgs e) {
            if (!uint.TryParse(TextBox_NumEcSym.Text, out uint NumOfECC)) { MessageBox.Show("Некорректный ввод количества символов коррекции ошибок"); return; }
            
            GF_Poly CorruptedMsg = new GF_Poly(GetByteArr(TextBox_Encoded_2));
            GF_Poly ErrMagnitude = new GF_Poly(GetByteArr(TextBox_Magnitudes));
            byte[] DecodedArr = (CorruptedMsg + ErrMagnitude).GetByteArr();
            OutByteArr(TextBox_Decoded_hex, DecodedArr);
            TextBox_Decoded_str.Text = Encoding.UTF8.GetString(DecodedArr, (int)NumOfECC, DecodedArr.Length - (int)NumOfECC);

        }
        private void Button_TestGenerator_Click(object sender, RoutedEventArgs e) {
            if (0 == GetByteArr(TextBox_GeneratorPoly).Length) { MessageBox.Show("Некорректный ввод генератора"); return; }

            GF_Poly Gen = new GF_Poly(GetByteArr(TextBox_GeneratorPoly));
            GF_Poly Multiplied;

            using (FileStream FS = File.Open(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + "TestFile.txt", FileMode.OpenOrCreate)) {
                for (int k = 1; k < 256; k++) {
                    for (int j = 1; j < 256; j++) {
                        for (int i = 1; i < 256; i++) {
                            Multiplied = GF_Poly.Mult(Gen, GF_Poly.Add(GF_Poly.Add(new GF_Poly(new GF_Byte((byte)i), 0), new GF_Poly(new GF_Byte((byte)j), 1)), new GF_Poly(new GF_Byte((byte)j), 2)));
                            string str = GetStringFromByteArr(Multiplied.GetByteArr());
                            FS.Write(Encoding.UTF8.GetBytes(str), 0, Encoding.UTF8.GetBytes(str).Length);
                            FS.Write(new byte[] { 13, 10 }, 0, 2);
                        }
                    }
                }
            }

            GF_Poly CorruptedMsg = new GF_Poly(GetByteArr(TextBox_Encoded_2));
            GF_Poly ErrMagnitude = new GF_Poly(GetByteArr(TextBox_Magnitudes));
            byte[] DecodedArr = (CorruptedMsg + ErrMagnitude).GetByteArr();
            OutByteArr(TextBox_Decoded_hex, DecodedArr);
          //  TextBox_Decoded_str.Text = Encoding.UTF8.GetString(DecodedArr, (int)NumOfECC, DecodedArr.Length - (int)NumOfECC);
        }
    }
    static class Glob {
        public static T[] InitializeArray<T>(uint length) where T : new() {
            T[] array = new T[length];
            for (uint i = 0; i < length; ++i) {
                array[i] = new T();
            }
            return array;
        }
        public static int IntMod(int Arg) {
            if (Arg >= 0) {
                return (Int32)Arg;
            } else {
                return (Int32)(-Arg);
            }
        }
    }
    static class GF_b2_Aryth {
        static uint GetLeadBitNum(UInt32 Val) {
            if (0 == Val) { MessageBox.Show("Попытка найти старший бит числа \"0\"! Это ни к чему хорошему не приведёт."); return 0; }
            int BitNum = 31;
            uint CmpVal = 1u << BitNum;
            while (Val < CmpVal) {
                CmpVal >>= 1;
                BitNum--;
            }
            return (uint)BitNum;
        }
        public static uint Galois_b2_ext_mult(uint m1, uint m2, uint Poly) {
            if (0 == m1 || 0 == m2) { return 0; }
            uint m1_tmp = m1;
            uint m2_tmp;
            uint m1_bit_num = 0;

            //Перемножение двух полиномов, при использовании арифметики по модулю 2 достаточно простое занятие.
            //перебираем единички и нолики (для каждого бита первого числа перебираем все биты второго (или наоборот)), складываем номера позиций битов,
            //но не всегда, а только когда оба перебираемых бита - единицы, и инвертируем бит результата под номером, равном сумме позиций для данного шага перебора
            //(инверсия - это прибавление единицы по модулю 2)
            uint PolyMultRez = 0;

            while (m1_tmp != 0) {
                uint bit_m1 = (m1_tmp & 1u) == 0u ? 0u : 1u;
                m1_tmp = m1_tmp >> 1;
                m2_tmp = m2;
                uint m2_bit_num;
                m2_bit_num = 0;
                while (m2_tmp != 0) {
                    uint bit_m2 = (m2_tmp & 1u) == 0u ? 0u : 1u;
                    m2_tmp = m2_tmp >> 1;
                    if ((bit_m1 != 0) && (bit_m2 != 0)) {
                        int BitNum = (int)(m2_bit_num + m1_bit_num);
                        PolyMultRez ^= 1u << BitNum;
                    }
                    m2_bit_num = m2_bit_num + 1;
                }
                m1_bit_num = m1_bit_num + 1;
            }

            //Тут есть результат умножения полиномов PolyMultRez. Осталось найти остаток от деления на выбранный порождающий полином.
            //Деление полиномов происходит так: Берём старшую степень делимого, и вычитаем старшую степень делителя. 
            //Получаем число - степень частного
            //Теперь перемножаем, а по сути, просто прибавляем к каждой степени делителя степень получившегося частного
            //и повторяем всё по кругу, пока степень делимого не окажется меньше степени делителя
            uint TmpDivisor_lead_bit_n;
            uint TmpQuotient;
            uint TmpDivisor = Poly;
            uint TmpDividend = PolyMultRez;
            uint TmpDividend_LeadBitNum;
            uint TmpMult_bitNum;
            uint TmpMult_rez;

            TmpDividend_LeadBitNum = GetLeadBitNum(TmpDividend);
            TmpDivisor_lead_bit_n = GetLeadBitNum(TmpDivisor);

            while (TmpDividend_LeadBitNum >= TmpDivisor_lead_bit_n) {

                TmpQuotient = (TmpDividend_LeadBitNum - TmpDivisor_lead_bit_n);

                TmpMult_bitNum = 0;
                TmpMult_rez = 0;
                while (TmpDivisor != 0) {
                    uint bit_TmpMult = (TmpDivisor & 1u) == 0u ? 0u : 1u;
                    TmpDivisor >>= 1;
                    TmpMult_rez ^= bit_TmpMult << (int)(TmpQuotient + TmpMult_bitNum);
                    TmpMult_bitNum = TmpMult_bitNum + 1;
                }
                TmpDividend = TmpDividend ^ TmpMult_rez;
                TmpDivisor = Poly;
                TmpDividend_LeadBitNum = GetLeadBitNum(TmpDividend);
            }
            //Результат умножения числел есть остаток от деления произведения многочленов на порождающий полином.
            return TmpDividend;
        }
    }
    static class GF_256_Aryth {
        private static uint GF_256_poly;
        private static uint GF_256_prim_memb;
        private static byte[] GF_256_power_a;
        private static byte[] GF_256_log_a;
        static GF_256_Aryth() {
            //По умолчанию порождающий полином поля и примитивный член используются, как правило, такие.
            MakePowTable(285, 2, false);
        }
        public static uint GetCurrPoly() {
            return GF_256_poly;
        }
        public static void MakePowTable(uint New_GF_Byte_poly, uint New_GF_Byte_prim_memb, bool ShowMessageboxes) {
            //Если есть функция умножения, то составить таблицу степеней и логарифмов не составляет никакого труда, 
            //ведь возведение в степень – есть уножение несколько раз
            //Здесь создаётся таблица степеней методом последовательного умножения примитивного
            //члена (как правило выбирают число 2, но здесь это может быть любое число).
            //Затем таблица проверяется на наличие повторяющихся значений. Если нет повторяющихся значений,
            //то выбранный примитивный член и порождающий полином сохраняются вместе с вычислеными таблицами
            //для параметра New_GF_Byte_prim_memb валидны следующие значения:
            //285, 299, 301, 333, 351, 355, 357, 361, 369, 391, 397, 425, 251, 463, 487, 501.
            byte[] tmp_GF_256_power_a = new byte[256]; //Этот массив можно сделать на единицу меньше, так как для поля GF[256] верно, что a^0 = a^255, но чтобы не путаться оставляю 256 элементов массива.
            byte[] tmp_GF_256_log_a = new byte[256];
            GF_256_power_a = new byte[256];
            GF_256_log_a = new byte[256];

            uint tmp_GF_Byte_poly = New_GF_Byte_poly;
            uint tmp_GF_Byte_prim_memb = New_GF_Byte_prim_memb;

            //Пропишем тривиальные вещи как то: a^0=1 и a^1=a, и наоборот
            tmp_GF_256_power_a[0] = 1; 
            tmp_GF_256_log_a[1] = 0;
            tmp_GF_256_power_a[1] = (byte)tmp_GF_Byte_prim_memb;
            tmp_GF_256_log_a[tmp_GF_Byte_prim_memb] = 1;
            //Остальные члены поля
            for (int i = 2; i < 256; i++) {
                tmp_GF_256_power_a[i] = (byte)GF_b2_Aryth.Galois_b2_ext_mult(tmp_GF_Byte_prim_memb, tmp_GF_256_power_a[i - 1], tmp_GF_Byte_poly);
                if(0 != tmp_GF_256_power_a[i]) {//Для значения степени "0" тут проходят 2 значения: 1 и 255. Это надо учесть
                    tmp_GF_256_log_a[tmp_GF_256_power_a[i]] = (byte)i;
                }
            }

            bool Ok = true;
            //Для поля GF[256] верно, что a^0 = a^255. Так что проверка не затрагивает степень 255
            for (int i = 0; i <= 254; i++) {
                for (int j = 0; j <= 254; j++) {
                    if (i != j) {
                        if (tmp_GF_256_power_a[i] == tmp_GF_256_power_a[j]) {
                            Ok = false;
                        }
                    }
                }
            }

            //Копируем в используемые таблицы, если нет повторов в таблице степеней выбранного примитивного члена
            if (Ok) {
                for (int i = 0; i < 256; i++) {
                    GF_256_power_a[i] = tmp_GF_256_power_a[i];
                }
                for (int i = 0; i < 256; i++) {
                    GF_256_log_a[i] = tmp_GF_256_log_a[i];
                }
                GF_256_poly = tmp_GF_Byte_poly;
                GF_256_prim_memb = tmp_GF_Byte_prim_memb;
                if (ShowMessageboxes) { MessageBox.Show("Всё хорошо. При таком сочетании порождающего полинома и примитивного члена в таблице степеней примитивного члена (a^1 .. a^255) нет повторений."); }
            } else {
                if (ShowMessageboxes) { MessageBox.Show("Всё плохо! При таком сочетании порождающего полинома и примитивного члена в таблице степеней примитивного члена есть повторяющиеся значения."); }
            }
        }
        public static byte Pow_a(int Degr) {
            //Возведение примитивного члена в степень. Свойство степени в поле Галуа GF[256] таково, что степень примитивного члена 0 равна степени 255; 1 - 256; 2 - 527 и так далее.
            if (0 <= Degr && Degr < 255) {
                return GF_256_power_a[Degr];
            } else {
                int TmpDegr = Glob.IntMod(Degr);
                TmpDegr %= 255; //Хоть и не существует отрицательных чисел в поле Галуа, здесь под отрицательной степенью подразумевается число обратное.
                if (Degr < 0) {
                    TmpDegr = 255 - TmpDegr;
                }
                return GF_256_power_a[TmpDegr];
            }
        }
        public static byte Log_a(byte Arg) {
            //Логарифм по основаниеию примитивного члена
            if (0 == Arg) {
                throw new Exception("Argument cannot be zero in GF_Byte.Log2(Arg)");
                //return new GF_Byte { val = 0 };
            } else if (1 == Arg) { //Логарифм единицы в GF[256] равен нулю и 255, так как a^0==a^255. Для кодирования Рида-Соломона выбираем 0.
                return 0;
            }else {
                return GF_256_log_a[Arg];
            }
        }
        public static byte Inverse(byte Arg) {
            //Можно обойтись и без этой функции и писать Div(1, Arg)
            if (1 == Arg) { return 1; }
            return GF_256_power_a[255 - GF_256_log_a[Arg]];
        }
        public static byte Add(byte a1, byte a2) {
            //Сложение таково же как и вычитание - побитовое "ИЛИ"
            return (byte)(a1 ^ a2);
        }
        public static byte Sub(byte s1, byte s2) {
            //Сложение таково же как и вычитание - побитовое "ИЛИ"
            return (byte)(s1 ^ s2);
        }
        public static byte Mult(byte m1, byte m2) {
            //Умножение с использованием таблиц степеней и логарифмов. Довольно таки быстрая операция
            if (0 == m1 || 0 == m2) { return 0; }
            return Pow_a(Log_a(m1) + Log_a(m2));
        }
        public static byte Div(byte d1, byte d2) {
            //Деление с использованием таблиц степеней и логарифмов.
            if (0 == d2) { throw new Exception("Division by zero"); }
            if (0 == d1) { return 0; }
            return Pow_a(Log_a(d1) - Log_a(d2));
        }
        public static byte Pow(byte b, int p) {
            //Возведение в степень с помощью таблиц логарифмов и степеней.
            if (0 == p) { return 1; } //Так как нуль в степени нуль равно одному, сначала проверяем на равенство нулю показателя
            if (0 == b) { return 0; }
            byte BaseLog = GF_256_log_a[b];            
            int TmpDegr = Glob.IntMod(p);
            TmpDegr = TmpDegr * BaseLog;
            TmpDegr %= 255;
            if (p < 0) {
                TmpDegr = 255 - TmpDegr;
            }
            return GF_256_power_a[(byte)TmpDegr];
        }
    }
    public class GF_Byte {
        public byte val;
        public static GF_Byte Zero {
            get {
                return new GF_Byte(0);
            }
        }
        public static GF_Byte One {
            get {
                return new GF_Byte(1);
            }
        }
        public GF_Byte() { }
        public GF_Byte(byte _val) {
            val = _val;
        }
        public GF_Byte(GF_Byte _val) {
            val = _val.val;
        }
        public static GF_Byte Pow_a(GF_Byte PowerOf_a) {
            return new GF_Byte(GF_256_Aryth.Pow_a(PowerOf_a.val));
        }
        public GF_Byte Inverse() {
            return new GF_Byte(GF_256_Aryth.Inverse(val));
        }
        public static GF_Byte operator +(GF_Byte t1, GF_Byte t2) {
            return new GF_Byte { val = GF_256_Aryth.Add(t1.val, t2.val) };
        }
        public static GF_Byte operator -(GF_Byte t1, GF_Byte t2) {
            return new GF_Byte { val = GF_256_Aryth.Sub(t1.val, t2.val) };
        }
        public static GF_Byte operator *(GF_Byte m1, GF_Byte m2) {
            return new GF_Byte { val = GF_256_Aryth.Mult(m1.val, m2.val) };
        }
        public static GF_Byte operator /(GF_Byte d1, GF_Byte d2) {
            if (0 == d2.val) { throw new Exception("Division by zero"); }
            if (0 == d1.val) { return new GF_Byte(0); }
            return new GF_Byte(GF_256_Aryth.Div(d1.val, d2.val));
        }
        public static GF_Byte operator /(byte d1, GF_Byte d2) {
            if (0 == d2.val) { throw new Exception("Division by zero"); }
            if (0 == d1) { return new GF_Byte(0); }
            return new GF_Byte(GF_256_Aryth.Div(d1, d2.val));
        }
    }
    public class GF_Poly {
        //Класс для работы с полиномами в поле Галуа GF[256].
        //Объект этого класса представляет собой полином.         
        private GF_Byte[] CoefArr; //Массив, в котором хранятся коэффициенты многочлена в порядке возрастания степени.
                                   //Многочлен вроде этого: 5*x^5 + 17*x^4 + 22*x^2 + 211*x + 7
                                   //будет представлен массивом : {7, 211, 22, 0, 17, 5}. 

        public GF_Byte this[uint MemberNum] {
            get { return GetMemberCoef(MemberNum); }
            set { SetMemberCoef(MemberNum, value); }
        }
        public GF_Byte this[GF_Byte MemberNum] {
            get { return GetMemberCoef(MemberNum.val); }
            set { SetMemberCoef(MemberNum, value); }
        }

        private uint CoefArrLen;//Вообще, можно пользоваться саойством любого массива в C# .Lenght Это поле - для простоты портирования в низкие языки.
        private uint ArrLen {
            get { return CoefArrLen; }
        }
        public uint Len {
            //Возвращает фактическую длину, т. е. количество членов полинома. 
            //В массиве может быть сколько угодно элементов, но если они все
            //равны нулю после какого-то номера, то фактическая длина многочлена будет меньше длины массива.
            //То есть длина многочлена, хранящегося в массиве как 32 A4 77 GF 11 00 00 00 00
            //будет равна 5.
            get {
                for (uint i = CoefArrLen; i >= 1; i--) {
                    if (0 != CoefArr[i - 1].val) {
                        return i;
                    }
                }
                return 0;
            }
        }
        public uint HiDeg {
            //Старшая степень многочлена. Равна количеству членов минус один
            get { if (0 == Len) { return 0; } else { return Len - 1; } }
        }
        public GF_Poly(uint Lenght) {
            CoefArr = Glob.InitializeArray<GF_Byte>(Lenght);
            CoefArrLen = Lenght;
        }
        public GF_Poly(byte[] ByteArr) {
            CoefArrLen = (uint)ByteArr.Length;
            CoefArr = Glob.InitializeArray<GF_Byte>(CoefArrLen);
            if (0 == CoefArrLen) {
                CoefArr = Glob.InitializeArray<GF_Byte>(1);
            }
            for (int i = 0; i < CoefArrLen; i++) {
                CoefArr[i].val = ByteArr[i];
            }
        }
        public GF_Poly(GF_Poly Poly) {
            CoefArr = Glob.InitializeArray<GF_Byte>(Poly.Len);
            CoefArrLen = Poly.Len;
            for (uint i = 0; i < CoefArrLen; i++) {
                CoefArr[i] = Poly.GetMemberCoef(i);
            }
        }
        public GF_Poly(GF_Byte Coef, uint Degree) {
            CoefArr = Glob.InitializeArray<GF_Byte>(Degree + 1);
            CoefArrLen = Degree + 1;
            CoefArr[Degree] = Coef;
        }
        public GF_Byte Eval(GF_Byte x_val) {
            //Полином есть сумма, а если в качестве x задать какое-то число, то его можно вычислить. Чтобы не возводить для каждого
            //члена число в степень, тут используется переменная, которая домножается на число (x_val) каждую итерацию. Фактически получается
            //то же самое, только чуть быстрее
            if (0 == Len) {
                return GF_Byte.Zero;
            }
            GF_Byte x_powered = GF_Byte.One;
            GF_Byte ret = new GF_Byte(0);
            for (uint i = 0; i < Len; i++) {
                ret += x_powered * GetMemberCoef(i);
                x_powered *= x_val;
            }
            return ret;
        }
        private void SetNewCoefArrLen(uint NewLen) {
            if (NewLen == CoefArrLen) { return; }
            GF_Byte[] NewCoefArr = Glob.InitializeArray<GF_Byte>(NewLen);
            uint RezultLen = NewLen < CoefArrLen ? NewLen : CoefArrLen;
            for (uint i = 0; i < RezultLen; i++) {
                NewCoefArr[i].val = CoefArr[i].val;
            }
            CoefArr = NewCoefArr;
            CoefArrLen = NewLen;
        }
        public GF_Poly DiscardHiDeg(uint NumOfLowDeg) {
            if (NumOfLowDeg > Len) {
                return this;
            }
            GF_Poly ret = new GF_Poly(this);
            ret.SetNewCoefArrLen(NumOfLowDeg);
            return ret;
        }
        public byte GetMaxCoef() {
            if (0 == Len) { return 0; }
            byte max = CoefArr[0].val;
            for (uint i = 1; i < Len; i++) {
                if (CoefArr[i].val > max) { max = CoefArr[i].val; }
            }
            return max;
        }
        public void AddToMember(uint MemberNum, GF_Byte Value) {
            if (MemberNum > ArrLen - 1 || 0 == ArrLen) {
                SetNewCoefArrLen(MemberNum + 1);
            }
            CoefArr[MemberNum] += Value;
        }
        public void MultiplyMember(uint MemberNum, GF_Byte Factor) {
            if (MemberNum > Len || 0 == Len) {
                return;
            }
            CoefArr[MemberNum] *= Factor;
        }
        public void SetMemberCoef(uint MemberNum, GF_Byte Value) {
            if (MemberNum > ArrLen - 1) {
                SetNewCoefArrLen(MemberNum + 1);
            }
            CoefArr[MemberNum] = Value;
        }
        public void SetMemberCoef(GF_Byte MemberNum, GF_Byte Value) {
            if (MemberNum.val > ArrLen - 1) {
                SetNewCoefArrLen((uint)MemberNum.val + 1);
            }
            CoefArr[MemberNum.val] = Value;
        }
        public GF_Byte GetMemberCoef(uint MemberNum) {
            if (MemberNum < Len) {
                return CoefArr[MemberNum];
            } else {
                return GF_Byte.Zero;
            }
        }
        public GF_Poly MultiplyByXPower(uint PowOfX) {
            //По сути, добавляет нули в начало полинома
            if (0 == PowOfX) { return this; }
            GF_Poly ret = new GF_Poly(CoefArrLen + PowOfX);
            for (uint i = 0; i < CoefArrLen; i++) {
                ret.CoefArr[i + PowOfX] = CoefArr[i];
            }
            return ret;
        }
        public GF_Poly Scale(GF_Byte Factor) {
            GF_Poly ret = new GF_Poly(this);
            for (uint i = 0; i < Len; i++) {
                ret.MultiplyMember(i, Factor);
            }
            return ret;
        }
        public GF_Poly FindRoots() {
            GF_Poly RootsPoly = new GF_Poly(HiDeg);
            uint NumRoots = 0;
            for (uint i = 0; i < 256; i++) {
                GF_Byte TestedValue = new GF_Byte((byte)i);
                if (0 == Eval(TestedValue).val) {
                    RootsPoly.SetMemberCoef(NumRoots, TestedValue);
                    NumRoots++;
                }
            }
            return RootsPoly;
        }
        public GF_Poly FormalDerivative() {
            GF_Poly ret = new GF_Poly(Len-1);            
            for (uint i = 0; i < ret.ArrLen; i++) {
                if (0 == i % 2) { ret.SetMemberCoef(i, GetMemberCoef(i + 1)); }
            }            
            return ret;
        }
        public static GF_Poly Add(GF_Poly p1, GF_Poly p2) {
            uint RezLen; //Нужно сначала найти длиннейший полином из двух
            if (p1.Len > p2.Len) {
                RezLen = p1.Len;
            } else {
                RezLen = p2.Len;
            }
            for (uint i = 0; i < RezLen; i++) {//Перебираем все члены наидлиннейшего полинома, и прибавляем к каждому соответствующий по степени
                p1.AddToMember(i, p2.GetMemberCoef(i));
            }
            return p1;
        }
        public static GF_Poly Mult(GF_Poly p1, GF_Poly p2) {
            GF_Poly ret;
            //результат перемножения полиномов есть полином со старшей степенью равной сумме старших степеней полиномов - множителей.
            //Старшая степень результата
            uint RezDegree = p1.HiDeg + p2.HiDeg;
            //Количество членов результата. На единицу больше. Ведь для полинома со старшей степенью 0 (константа) требуется один член. Первая степень - бином - это два члена
            ret = new GF_Poly(RezDegree + 1);

            for (uint i = 0; i < p1.Len; i++) {//Перебираем все члены первого полинома. i есть степень члена
                for (uint j = 0; j < p2.Len; j++) {//Перебираем все члены второго полинома. j есть степень члена
                    uint DegreeRez = i + j;//Степень результата перемножения двух членов есть сумма степеней
                    GF_Byte Coef = p1.GetMemberCoef(i) * p2.GetMemberCoef(j);//коэффициент члена есть значение массива. Перемножаем в поле Галуа.
                    //Прибавляем к полиному-результату коэффициент, получившийся при умножении. Конечно же в поле Галуа
                    ret.AddToMember(DegreeRez, Coef);
                }
            }
            return ret;
        }
        public static GF_Poly Div_Remainder(GF_Poly p1, GF_Poly p2) {
            GF_Poly Divident = p1;
            GF_Poly Divisor = p2;
            
            //Если делитель это нуль, то результат ест


            //Если степень делителя больше степени делимого, то остаток - есть делимое
            if (Divisor.HiDeg > Divident.HiDeg) { return Divident; }

            //Поделим полиномы также, как делили числа, представляя их в виде полиномов, при реализации умножения в полях Галуа. 
            //Найдём старшую степень частного (Старшая степень делимого минус старшая степень делителя) и выделим под неё массив
            GF_Poly Quotient = new GF_Poly(Divident.HiDeg - Divisor.HiDeg + 1);
            GF_Poly Tmp_Divident = new GF_Poly(Divident);//Длина массива под делимое не будет увеличиваться – значит можно брать такую же как и у аргумента функции - делимого.

            uint TmpDeg;
            GF_Byte TmpCoef;

            while (Tmp_Divident.Len >= Divisor.Len) {
                //Берём старшую степень делимого и делим на старшую степень делителя (тут просто вычитание степеней и деление коэффициентов)
                TmpDeg = Tmp_Divident.HiDeg - Divisor.HiDeg;
                TmpCoef = Tmp_Divident.GetMemberCoef(Tmp_Divident.HiDeg) / Divisor.GetMemberCoef(Divisor.HiDeg);
                //Сохраняем в частном в нужной позиции (а значит нужную степень)
                Quotient.SetMemberCoef(TmpDeg, TmpCoef);
                //Вычтем из делимого результат умножения
                Tmp_Divident -= (Divisor * new GF_Poly(TmpCoef, TmpDeg));
            }
            return Tmp_Divident;
        }
        public static GF_Poly Div(GF_Poly p1, GF_Poly p2) {
            GF_Poly Divident = p1;
            GF_Poly Divisor = p2;

            //Если степень делителя больше степени делимого, то результат деления есть нуль.
            if (Divisor.HiDeg > Divident.HiDeg) { return new GF_Poly(new byte[] { 0 }); }

            //Поделим полиномы также, как делили числа, представляя их в виде полиномов, при реализации умножения в полях Галуа. 
            //Найдём старшую степень частного (Старшая степень делимого минус старшая степень делителя) и выделим под неё массив
            GF_Poly Quotient = new GF_Poly(Divident.HiDeg - Divisor.HiDeg + 1);
            GF_Poly Tmp_Divident = new GF_Poly(Divident);//Длина массива под делимое не будет увеличиваться – значит можно брать такую же как и у аргумента функции - делимого.

            uint TmpDeg;
            GF_Byte TmpCoef;

            while (Tmp_Divident.Len >= Divisor.Len) {
                //Берём старшую степень делимого и делим на старшую степень делителя 
                //(тут просто вычитание степеней и деление коэффициентов)
                TmpDeg = Tmp_Divident.HiDeg - Divisor.HiDeg;
                TmpCoef = Tmp_Divident.GetMemberCoef(Tmp_Divident.HiDeg) / Divisor.GetMemberCoef(Divisor.HiDeg);
                //Сохраняем в частном в нужной позиции (а значит нужную степень)
                Quotient.SetMemberCoef(TmpDeg, TmpCoef);
                //Вычтем из делимого результат умножения
                Tmp_Divident -= (Divisor * new GF_Poly(TmpCoef, TmpDeg));
            }
            return Quotient;
        }
        public static GF_Poly operator +(GF_Poly s1, GF_Poly s2) {
            return new GF_Poly(Add(s1, s2));
        }
        public static GF_Poly operator -(GF_Poly s1, GF_Poly s2) {
            return new GF_Poly(Add(s1, s2));
        }
        public static GF_Poly operator *(GF_Poly m1, GF_Poly m2) {
            return new GF_Poly(Mult(m1, m2));
        }
        public static GF_Poly operator /(GF_Poly d1, GF_Poly d2) {
            return new GF_Poly(Div(d1, d2));
        }
        public static GF_Poly operator %(GF_Poly d1, GF_Poly d2) {
            return new GF_Poly(Div_Remainder(d1, d2));
        }
        public byte[] GetByteArr() {
            if (0 == Len) {
                return new byte[] { 0 };
            }
            byte[] ret = new byte[Len];
            for (uint i = 0; i < Len; i++) {
                ret[i] = CoefArr[i].val;
            }
            return ret;
        }
    }
    public static class ReedSolomonOps {
        public static GF_Poly GenerateGenerator(uint NumOfErCorrSymbs) {
            //Должно получиться такое произведение (a^1-x)*(a^2-x)*(a^3-x)*...*(a^Num_EC_symbols    -x), где a это примитивный член поля GF[256]. Как правило, выбиарют a=2
            //Корнями этого полинома будут числа обратные a^1; a^2; a^3; ...; a^Num_EC_symbols, что очевидно, так как при этих значениях x один из биномов становится равным нулю 
            //Также при этих значениях будет равным нулю полином, полученный как остаток от деления на генератор
            if (0 == NumOfErCorrSymbs) {
                return new GF_Poly(new byte[] { 0 });
            }
            GF_Poly ret = new GF_Poly(new byte[] { GF_256_Aryth.Pow_a(1), 1 });
            for (int i = 2; i <= NumOfErCorrSymbs; i++) {
                ret *= new GF_Poly(new byte[] { GF_256_Aryth.Pow_a(i), 1 });
            }
            return ret;
        }
        public static GF_Poly EncodeMessage(GF_Poly MessagePoly, GF_Poly GenPoly) {
            if (0 == GenPoly.Len) { return MessagePoly; }
            GF_Poly Rezult = MessagePoly.MultiplyByXPower(GenPoly.Len - 1);
            GF_Poly Remndr = Rezult % GenPoly;
            return Rezult + Remndr;
        }
        public static GF_Poly CalcSyndromes(GF_Poly MessagePoly, uint NumOfErCorrSymbs) {
            GF_Poly Syndromes = new GF_Poly(1);
            for (uint i = 0; i < NumOfErCorrSymbs; i++) {
                Syndromes += new GF_Poly(MessagePoly.Eval(new GF_Byte(GF_256_Aryth.Pow_a((int)i+1))), i);
            }
            return Syndromes;
        }
        public static GF_Poly CalcLocatorPoly(GF_Poly Syndromes, uint NumOfErCorrSymbs) {
            //Алгоритм Берлекэмпа-Мэсси
            GF_Poly Locator;
            GF_Poly Locator_old;
            
            //Присваиваем локатору инициализирующее значение (1*X^0)
            Locator = new GF_Poly(new byte[] { 1 });
            Locator_old = new GF_Poly(Locator);

            uint Synd_Shift = 0;

            for (uint i = 0; i < NumOfErCorrSymbs; i++) {
                uint K = i + Synd_Shift;
                GF_Byte Delta = Syndromes[K];

                for (uint j = 1; j < Locator.Len; j++) {
                    Delta += Locator[j] * Syndromes[K - j];
                }
                Locator_old = Locator_old.MultiplyByXPower(1);//Умножение полинома на икс (эквивалентно сдвигу вправо на 1 байт)
                if (Delta.val != 0) {
                    if (Locator_old.Len > Locator.Len) {
                        GF_Poly Locator_new = Locator_old.Scale(Delta);
                        Locator_old = Locator.Scale(Delta.Inverse());
                        Locator = Locator_new;
                    }
                    //Scale – умножение на константу. Можно было бы
                    //вместо использования Scale 
                    //умножить на полином нулевой степени. Разницы нет, но так короче:
                    Locator += Locator_old.Scale(Delta);
                }
            }
            return Locator;
        }
        public static GF_Poly CalcLocatorPoly(GF_Poly Syndromes, byte[] Erasures, uint NumOfErCorrSymbs) {
            //Алгоритм Берлекэмпа-Мэсси
            GF_Poly Locator;
            GF_Poly Locator_old;

            if (0 == Erasures.Length) {
                Locator = new GF_Poly(new byte[] { 1 });
                Locator_old = new GF_Poly(Locator);
            } else {
                Locator = CalcLocatorPoly(Erasures);
                Locator_old = new GF_Poly(Locator);
            }

            uint Synd_Shift = 0;

            for (uint i = 0; i < NumOfErCorrSymbs-Erasures.Length; i++) {
                uint K = i + Synd_Shift + (uint)Erasures.Length;
                GF_Byte Delta = Syndromes[K];

                for (uint j = 1; j < Locator.Len; j++) {
                    Delta += Locator[j] * Syndromes[K - j];
                }
                Locator_old = Locator_old.MultiplyByXPower(1);//Умножение полинома на икс.
                if (Delta.val != 0) {
                    if (Locator_old.Len > Locator.Len) {
                        GF_Poly Locator_new = Locator_old.Scale(Delta);
                        Locator_old = Locator.Scale(Delta.Inverse());
                        Locator = Locator_new;
                    }
                    Locator += Locator_old.Scale(Delta);
                }
            }
            return Locator;
        }
        public static GF_Poly CalcLocatorPoly(byte[] Erasures) {            
            GF_Poly ret = new GF_Poly(new byte[] { 1 });
            for (uint i = 0; i < Erasures.Length; i++) {
                ret *= new GF_Poly(new byte[] { 1, GF_256_Aryth.Pow_a(Erasures[i])});
            }
            return ret;
        }
        public static GF_Poly CalcLocatorPoly(GF_Poly Erasures) {
            GF_Poly ret = new GF_Poly(new byte[] { 1 });
            for (uint i = 0; i < Erasures.Len; i++) {
                ret *= new GF_Poly(new byte[] { 1, GF_256_Aryth.Pow_a(Erasures.GetMemberCoef(i).val) });
            }
            return ret;
        }
        public static GF_Poly FindErrPos(GF_Poly LocatorPoly) {
            GF_Poly Roots = LocatorPoly.FindRoots();
            GF_Poly ret = new GF_Poly(Roots.Len);
            //Позиция ошибки - есть логарифм числа, обратного корню полинома
            for (uint i = 0; i < Roots.Len; i++) {
                ret.SetMemberCoef(i, new GF_Byte(GF_256_Aryth.Log_a(Roots.GetMemberCoef(i).Inverse().val)));// 
            }
            return ret;
        }
        public static GF_Poly FindMagnitudes(GF_Poly Syndromes, GF_Poly Locator, uint NumOfErCorrSymbs) {

            GF_Poly ErrPoly = new GF_Poly(Syndromes * Locator).DiscardHiDeg(NumOfErCorrSymbs); //Полином ошибок
            GF_Poly ErrPos = FindErrPos(Locator);//Полином позиций ошибок. Может быть вычислен из локатора
            GF_Poly LocatorDer = Locator.FormalDerivative();//Производная локатора

            GF_Poly Magnitudes = new GF_Poly(ErrPos.GetMaxCoef());

            for (uint i = 0; i < ErrPos.Len; i++) {
                GF_Byte Xi = GF_Byte.Pow_a(ErrPos.GetMemberCoef(i)).Inverse(); //"Примитивный член поля в степени, обратной позиции ошибки
                GF_Byte W = new GF_Byte(ErrPoly.Eval(Xi));
                GF_Byte L = new GF_Byte(LocatorDer.Eval(Xi));
                GF_Byte Magnitude = W / L;
                Magnitudes.SetMemberCoef(ErrPos.GetMemberCoef(i).val, Magnitude);                
            }
            return Magnitudes;
        }
        public static GF_Poly FindMagnitudesFromErrPos(GF_Poly Syndromes, GF_Poly ErrPos, uint NumOfErCorrSymbs) {
            //Вычисление локатора из позиций ошибок
            GF_Poly Locator = CalcLocatorPoly(ErrPos);
            //Произведение для вычисления полинома ошибок
            GF_Poly Product = Syndromes * Locator;
            //Полином ошибок. DiscardHiDeg оставляет указаное количество младших степеней
            GF_Poly ErrPoly = Product.DiscardHiDeg(NumOfErCorrSymbs);
            //Производная локатора
            GF_Poly LocatorDer = Locator.FormalDerivative();
            //Здесь будут амплитуды ошибок. Количество членов - это самая большая позиция ошибки
            GF_Poly Magnitudes = new GF_Poly(ErrPos.GetMaxCoef());

            //Перебор каждой заданной позиции ошибки
            for (uint i = 0; i < ErrPos.Len; i++) {
                //число обратное примитивному члену в степени позиции ошибки
                GF_Byte Xi = 1 / GF_Byte.Pow_a(ErrPos[i]);
                //значение полинома ошибок при x = Xi
                GF_Byte W = ErrPoly.Eval(Xi);
                //значение производной локатора при x = Xi
                GF_Byte L = LocatorDer.Eval(Xi);
                //Это как раз и будет найденное значение ошибки,
                //которое надо вычесть из ошибочного символа, чтобы он стал не ошибочным
                GF_Byte Magnitude = W / L;
                //запоминаем найденную амплитуду в текущей позиции ошибки
                Magnitudes[ErrPos[i]] = Magnitude;
            }            
            return Magnitudes;
        }
    }
}
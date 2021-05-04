using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Taylor {
   /// <summary>
   /// Interaction logic for Matrix.xaml
   /// </summary>
   public partial class Matrix : UserControl {
      public Matrix() {
         InitializeComponent();
      }
      public bool IsReadOnly {
         get { return (bool)GetValue(IsReadOnlyProperty); }
         set { SetValue(IsReadOnlyProperty, value); }
      }
      public double[][] Value {
         get { return (double[][])GetValue(ValueProperty); }
         set { SetValue(ValueProperty, value); }
      }


      public static readonly DependencyProperty IsReadOnlyProperty;
      public static readonly DependencyProperty ValueProperty;
      static Matrix() {
         var isReadOnlyMetadata = new FrameworkPropertyMetadata(OnIsReadOnlyChanged);
         var valueMetadata = new FrameworkPropertyMetadata(OnValueChanged);

         IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(Matrix), isReadOnlyMetadata);
         ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double[][]), typeof(Matrix), valueMetadata);
      }

      static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
         var newValue = args.NewValue as double[][];

         if (newValue != null) {
            ((Matrix)d).NumberOfColumns.Text = newValue.Length.ToString();
            ((Matrix)d).NumberOfRows.Text = newValue[0].Length.ToString();
            ((Matrix)d).Refresh(((Matrix)d).IsReadOnly);
         }
      }

      static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
         if (((Matrix)d).Value != null) {
            ((Matrix)d).Refresh((bool)args.NewValue);
         }
      }

      IDictionary<UIElement, Tuple<int, int>> CellToIndex = new Dictionary<UIElement, Tuple<int, int>>();
      void Refresh(bool isReadonly) {
         Fields.Children.Clear();

         Fields.ColumnDefinitions.Clear();
         for (int i = 0; i < Value.Length; i++) {
            Fields.ColumnDefinitions.Add(new ColumnDefinition());
         }

         Fields.RowDefinitions.Clear();
         for (int j = 0; j < Value[0].Length; j++) {
            Fields.RowDefinitions.Add(new RowDefinition());
         }

         CellToIndex.Clear();

         for (int i = 0; i < Value.Length; i++) {
            for (int j = 0; j < Value[i].Length; j++) {
               if (isReadonly) {
                  var v = new TextBlock();
                  v.Text = Value[i][j].ToString();
                  Fields.Children.Add(v);
                  Grid.SetColumn(v, i);
                  Grid.SetRow(v, j);
                  CellToIndex[v] = new Tuple<int, int>(i, j);
               }
               else {
                  var v = new TextBox();
                  v.Text = Value[i][j].ToString();
                  v.PreviewTextInput += V_PreviewTextInput;
                  Fields.Children.Add(v);
                  Grid.SetColumn(v, i);
                  Grid.SetRow(v, j);
                  CellToIndex[v] = new Tuple<int, int>(i, j);
               }
            }
         }
      }

      private void V_PreviewTextInput(object sender, TextCompositionEventArgs e) {
         var cell = e.OriginalSource as TextBox;
         if (cell != null) {
            var text = cell.Text.Insert(cell.CaretIndex, e.Text);
            if (double.TryParse(text, out double value)) {
               if (CellToIndex.TryGetValue(sender as UIElement, out Tuple<int, int> position))
                  Value[position.Item1][position.Item2] = value;
            }
         }
      }
   }
}

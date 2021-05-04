using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Taylor {
   public class Series : INotifyPropertyChanged {
      public double[][] Matrix { get; set; }
      uint _size = 1;
      public uint Size {
         get => _size;
         set {
            _size = value == 0 ? 1 : value;
            Reset(_size);
            NotifyPropertyChanged(nameof(Size));
            NotifyPropertyChanged(nameof(Matrix));
            NotifyPropertyChanged(nameof(Approximation));

         }
      }

      uint _accuracy = 2;
      public uint Accuracy {
         get => _accuracy;
         set {
            _accuracy = value;
            Calculate(_accuracy);
         }
      }
      double[][] _approximation;
      public double[][] Approximation {
         get => _approximation;
         private set {
            _approximation = value;
            NotifyPropertyChanged(nameof(Approximation));
         }
      }

      public Series() {
         Reset(2);
         Matrix[1][0] = Math.PI * -1;
         Matrix[0][1] = Math.PI;
         Calculate(_accuracy);
      }

      public Series(uint size) => Reset(size);

      void Reset(uint size) {
         var newMatrix = new double[size][];
         for (int i = 0; i < size; i++)
            newMatrix[i] = new double[size];

         var newApproximation = new double[size][];
         for (int i = 0; i < size; i++)
            newApproximation[i] = new double[size];

         Matrix = newMatrix;
         Approximation = newApproximation;
      }

      public void Calculate(uint accuracy) {
         var size = (uint)Approximation.Length;

         var result = new double[size][];
         for (int i = 0; i < size; i++)
            result[i] = new double[size];

         var exponents = new Dictionary<uint, double[][]>();
         for (uint a = 0; a <= accuracy; a++) {
            var accumulation = Identity(size);

            if (a != 0) {
               for (int n = 1; n <= a; n++)
                  accumulation = Multiply(accumulation, Matrix);

               var scalar = 1.0 / Factorial(a);
               accumulation = Scale(accumulation, scalar);
            }
            exponents[a] = accumulation;
         }
         foreach (var e in exponents.Values)
            result = Add(result, e);
         Approximation = result;
      }

      double[][] Identity(uint size) {
         var identity = new double[size][];
         for (int i = 0; i < size; i++)
            identity[i] = new double[size];
         for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
               identity[i][j] = i == j ? 1 : 0;
            }
         }
         return identity;
      }

      double[][] Add(double[][] left, double[][] right) {
         var size = left.Length;

         var result = new double[size][];
         for (int i = 0; i < size; i++)
            result[i] = new double[size];

         for (int i = 0; i < Matrix.Length; i++) {
            for (int j = 0; j < Matrix[i].Length; j++) {
               result[i][j] = left[i][j] + right[i][j];
            }
         }
         return result;
      }

      double[][] Multiply(double[][] left, double[][] right) {
         var size = left.Length;

         var result = new double[size][];
         for (int i = 0; i < size; i++)
            result[i] = new double[size];

         var invert_right = new double[size][];
         for (int i = 0; i < size; i++)
            invert_right[i] = new double[size];

         for (int i = 0; i < right.Length; i++) {
            for (int j = 0; j < right[i].Length; j++) {
               invert_right[j][i] = right[i][j];
            }
         }

         for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
               double accumulator = 0;
               for (int n = 0; n < size; n++) {
                  accumulator += left[i][n] * invert_right[j][n];
               }
               result[i][j] = accumulator;
            }
         }

         return result;
      }

      double[][] Scale(double[][] matrix, double scalar) {
         var size = matrix.Length;

         var result = new double[size][];
         for (int i = 0; i < size; i++)
            result[i] = new double[size];

         for (int i = 0; i < matrix.Length; i++)
            for (int j = 0; j < matrix.Length; j++)
               result[i][j] = scalar * matrix[i][j];

         return result;
      }

      int Factorial(uint n) {
         var result = 1;
         for (int i = 0; i <= n; i++) {
            result *= i == 0 ? 1 : i;
         }
         return result;
      }

      public event PropertyChangedEventHandler PropertyChanged;
      protected void NotifyPropertyChanged(string propertyName)
         => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
   }
}

using HierarchyMethodAvaloniaApplication.Models;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace HierarchyMethodAvaloniaApplication.ViewModels
{
    public class CalculatingViewModel : ViewModelBase
    {
        private double[,] _matrix;

        private int n;
        public int N 
        {
            get => n;
            set
            {
                n = value;

                if (Criterios.Any())
                {
                    Criterios.Clear();
                }

                for (int i = 1; i <= n; i++)
                {

                    Criterios.Add(new Criterio() { Id = i });
                }
            }
        }

        private int alternativesCount;
        public int AlternativesCount
        {
            get => alternativesCount;
            set
            {

                if (N < 1)
                {
                    alternativesCount = 0;
                    var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(
                                                   new MessageBoxStandardParams
                                                   {
                                                       ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                                                       ContentTitle = "Ошибка", 
                                                       ContentMessage = "Нужно определить критерии!"
                                                   });
                    messageBoxStandardWindow.Show();
                    return;
                }

                alternativesCount = value;
                if (Alternatives.Any())
                {
                    Alternatives.Clear();
                }

                for (int i = 1; i <= alternativesCount; i++)
                {

                    Alternatives.Add(new Alternative() { Id = i, Criterioes = new ObservableCollection<Criterio>(Criterios.Select(x => x.Clone())) });
                }
            }
        }

        private string _matrixView;
        public string MatrixView 
        { 
            get { return _matrixView; } 
            set => this.RaiseAndSetIfChanged(ref _matrixView, value);
        }


        private ObservableCollection<Criterio> _criterios;
        public ObservableCollection<Criterio> Criterios { get => _criterios; set => _criterios = value; }

        private ObservableCollection<Alternative> _alternatives;
        public ObservableCollection<Alternative> Alternatives { get => _alternatives; set => _alternatives = value; }

        //public DelegateCommand GenerateMatrixCommand { get; set; }

        private double[] _relativeWeigths;

        private Dictionary<string, double[]> _criteriousWeigths;

        private double[,] _alternativesWeigthMatrix;

        public ReactiveCommand<Unit, Unit> GenerateMatrixCommand { get; }

        public CalculatingViewModel()
        {
            Criterios = new ObservableCollection<Criterio>();
            Alternatives = new ObservableCollection<Alternative>();
            _criteriousWeigths = new Dictionary<string, double[]>();
            GenerateMatrixCommand = ReactiveCommand.Create(GenerateMatrix);
        }

        private void GenerateMatrix()
        {
            if (_criteriousWeigths.Keys.SequenceEqual(Criterios.Select(x => x.Name)))
            {
                return;
            }

            CalculateForEveryCriterio();
            MatrixView = "";
            _matrix = new double[N, N];

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    _matrix[i, j] = 0;
                }
            }

            for (int i = 0; i < N; i++)
            {
                int k = Criterios[i].Value;
                for (int j = 0; j < N; j++)
                {
                    if (i == j)
                    {
                        _matrix[i, j] = 1;
                        continue;
                    }
                    _matrix[i, j] = k;
                    _matrix[j, i] = (double)1 / k;
                }
            }
            // Формируем матрицу сравнения
            MatrixView += "Матрица сравнения:" + "\n";
            PrintMatrix(_matrix);

            // Нормировка матрицы -----
            // Сумма столбцов
            var columnsSum = CalculateColumnsSum(_matrix);
            // Сама нормировка
            _matrix = NormalizeMatrix(columnsSum, _matrix);
            // -----
            MatrixView += "Нормализованная матрица:" + "\n";
            PrintMatrix(_matrix);
            MatrixView += "Искомые относительные веса:" + "\n";
            // Среднее значение каждой строки
            _relativeWeigths = CalculateRelativeWeightsOfCriteria(_matrix);
            for (int i = 0; i < N; i++)
            {
                Criterios[i].CriterioWeigth = _relativeWeigths[i];
                MatrixView += $"W_{Criterios[i].Name} = {Criterios[i].CriterioWeigth.ToString()}" + "\n";
            }

            resultMatrix = new double[_alternativesWeigthMatrix.GetLength(0)];
            // Вес альтернатив с точки зрения достижения цели
            MatrixView += "Вес альтернатив с точки зрения достижения цели:" + "\n";
            for (int i = 0; i < _alternativesWeigthMatrix.GetLength(0); i++)
            {
                double rowSum = 0;
                for (int j = 0; j < _relativeWeigths.Length; j++)
                {
                    rowSum += _alternativesWeigthMatrix[i, j] * _relativeWeigths[j];
                }
                resultMatrix[i] = rowSum;
            }
            MatrixView += "\n";
            for (int i = 0; i < resultMatrix.Length; i++)
            {
                MatrixView += $"W_{i} = {resultMatrix[i]}" + "\n";
            }

            MatrixView += "Наилучший вариант: " + Alternatives[Array.IndexOf(resultMatrix, resultMatrix.Max())].Name;
        }

        private double[] resultMatrix;

        private void CalculateForEveryCriterio()
        { 
            foreach (var crit in Criterios)
            {
                var critMatrix = new double[Alternatives.Count, Alternatives.Count];
                for (int i = 0; i < Alternatives.Count; i++)
                {
                    int k = Alternatives[i].Criterioes.Select(x => x).Where(y => y.Name == crit.Name).First().Value;
                    for (int j = 0; j < Alternatives.Count; j++)
                    {
                        if (i == j)
                        {
                            critMatrix[i, j] = 1;
                            continue;
                        }
                        critMatrix[i, j] = k;
                        critMatrix[j, i] = (double)1 / k;
                    }
                }
                var columnsSum = CalculateColumnsSum(critMatrix);
                var normMatrix = NormalizeMatrix(columnsSum, critMatrix);
                var relativeWeigths = CalculateRelativeWeightsOfCriteria(normMatrix);
                _criteriousWeigths.Add(crit.Name, relativeWeigths);
            }

            _alternativesWeigthMatrix = new double[Alternatives.Count, Criterios.Count];
            for (int i = 0; i < Alternatives.Count; i++)
            {
                for (int j = 0; j < Criterios.Count; j++)
                {
                    _alternativesWeigthMatrix[i, j] = _criteriousWeigths[Criterios[j].Name][i];
                }
            }
        }

        private double[] CalculateColumnsSum(double[,] matrix)
        {
            double[] columnsSum = new double[matrix.GetLength(0)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    columnsSum[i] += matrix[j, i];
                }
            }

            return columnsSum;
        }

        private double[,] NormalizeMatrix(double[] relativeWeights, double[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[j, i] /= relativeWeights[i];
                }
            }
            return matrix;
        }

        private double[] CalculateRelativeWeightsOfCriteria(double[,] matrix)
        {
            var relativeWeights = new double[matrix.GetLength(0)];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    relativeWeights[i] += matrix[i, j];
                }
                relativeWeights[i] /= matrix.GetLength(0);
            }

            return relativeWeights;
        }

        private void PrintMatrix(double[,] matrix)
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    MatrixView += matrix[i, j].ToString() + " ";
                }
                MatrixView += "\n";
            }
            MatrixView += "\n";
        }
        private void PrintMatrix(double[] matrix)
        {
            for (int i = 0; i < N; i++)
            {
                MatrixView += matrix[i].ToString() + " ";
            }
            MatrixView += "\n";
        }
    }
}

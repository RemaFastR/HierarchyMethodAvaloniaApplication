using HierarchyMethodAvaloniaApplication.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace HierarchyMethodAvaloniaApplication.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Calculating = new CalculatingViewModel();
        }
        public CalculatingViewModel Calculating{ get; }
    } 
}

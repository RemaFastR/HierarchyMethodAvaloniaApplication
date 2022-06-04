using HierarchyMethodAvaloniaApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HierarchyMethodAvaloniaApplication.Models
{
    public class Alternative : ViewModelBase
    {
        private int _id;
        public int Id { get => _id; set => _id = value; }
        private string? _name;
        public string Name { get => _name; set => _name = value; }
        private ObservableCollection<Criterio>? _criterioes;
        public ObservableCollection<Criterio> Criterioes { get => _criterioes; set => _criterioes = value; }

    }
}

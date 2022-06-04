using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HierarchyMethodAvaloniaApplication.Models
{
    public class Criterio
    {
        private int _id;
        public int Id { get => _id; set => _id = value; }
        private string? _name;
        public string Name { get => _name; set => _name = value; }
        private int _value;
        public int Value { get => _value; set => _value = value; }
        public double CriterioWeigth { get; set; }
        public Criterio Clone()
        {
            return new Criterio() { Id = this.Id, Name = this.Name, Value = this.Value };
        }
    }
}

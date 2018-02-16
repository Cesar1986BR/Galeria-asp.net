using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Galeria.Models
{
    public class ProdutoVM
    {

        public Produto Produto { get; set; }

        public IEnumerable<string> Galeria { get; set; }

    }
}
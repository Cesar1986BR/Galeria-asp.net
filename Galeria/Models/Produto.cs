using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Galeria.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descrição { get; set; }
        public string ImagemNome { get; set; }
        public decimal Valor { get; set; }


   
    }
}
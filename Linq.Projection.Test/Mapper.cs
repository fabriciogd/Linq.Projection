namespace Linq.Projection.Test
{
    using Linq.Projection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class Mapper
    {
        [TestMethod]
        public void Custom_map_properties()
        {
            var pessoas = new List<Pessoa>()
                .AsQueryable()
                .Project<Pessoa>()
                .To<Pessoa>(mapper => mapper.Map(t => t.Id, s => s.Id));
        }
    }

    public class Pessoa
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public int Idade { get; set; }
    }
}


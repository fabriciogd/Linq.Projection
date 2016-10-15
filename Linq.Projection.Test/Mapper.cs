namespace Linq.Projection.Test
{
    using Linq.Projection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class Mapper
    {
        public IList<Pessoa> ObterPessoas()
        {
            IList<Pessoa> pessoas = new List<Pessoa>();

            for (int i = 0; i < 10; i++)
            {
                pessoas.Add(new Pessoa() { Id = i, Nome = "Pessoa" + i, Idade = i });
            }

            return pessoas;
        }

        [TestMethod]
        public void Automatic_map_properties()
        {
            var pessoas = ObterPessoas()
                .AsQueryable()
                .Project<Pessoa>()
                .To<DTODePessoa>();

            Assert.IsInstanceOfType(pessoas, typeof(IQueryable<DTODePessoa>));
        }

        [TestMethod]
        public void Ignore_property()
        {
            var pessoas = ObterPessoas()
                .AsQueryable()
                .Project<Pessoa>()
                .To<DTODePessoa>(mapper => mapper.Ignore(a => a.Nome));

            Assert.IsNull(pessoas.First().Nome);
        }

        [TestMethod]
        public void Map_property()
        {
            var pessoas = ObterPessoas()
                .AsQueryable()
                .Project<Pessoa>()
                .To<DTODePessoa>(mapper => mapper.Map(a => a.Nome, b => b.Nome + "Teste"));

            Assert.AreEqual(pessoas.First().Nome, "Pessoa0Teste");
        }
    }

    public class Pessoa
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public int Idade { get; set; }
    }

    public class DTODePessoa
    {
        public string Nome { get; set; }
    }
}


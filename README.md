# Linq.Projection

## Projeção para IQueryable

Ao escrever consultas IQueryable para obter projeções, nos deparamos com um exercício cansativo. Para resolver este problema, muitas pessoas aderem ao uso do AutoMapper, acreditando que este atinja o mesmo nivel de capacidade de uma escrita manual, poré, o AutoMapper não sabe fazer o uso do IQueryable e só trabalha com dados em memória. A soluçao apresentada permite mapear automaticamente os tipos da sua camada de acesso a dados de uma forma simples e eficiente, através de uma convenção de nomes, ou através de um mecanismo de sobreposição de expressões.

## Exemplos

Mapeamento automático
```
context.Persons.Project().To<PersonDTO>();
```

Mapeamento customizado
```
context.Persons.Project().To<PersonDTO>(mapper => mapper.Map(a => a.Nome, b => b.Nome + "Teste"));
```

Ignorar mapeamento
```
context.Persons.Project().To<PersonDTO>(mapper => mapper.Ignore(a => a.Nome));
```

using DemoMinimalApi.Data;
using DemoMinimalApi.Models;
using Microsoft.EntityFrameworkCore;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

//Configurando os serviços.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Fazendo a conexão com o banco de dados
builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline. (Fluxo do Request)

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/fornecedor", async (
    MinimalContextDb context ) =>
    await context.Fornecedores.ToListAsync())
    .WithName("GetFornecedor")
    .WithTags("Fornecedor");


//Passamos o contexto e o id dentro dos parâmetros da função async
app.MapGet("/fornecedor/{id}", async (
    Guid id, 
    MinimalContextDb context) =>

    await context.Fornecedores.FindAsync(id)
    //Tratamento para null(verificação)
    is Fornecedor fornecedor
        ? Results.Ok(fornecedor)
        : Results.NotFound())
    .Produces<Fornecedor>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetFornecedorPorId")
    .WithTags("Fornecedor");


app.MapPost("/fornecedor", async (
    MinimalContextDb context,
    Fornecedor fornecedor) =>
{
    //Validação feita pela biblioteca MiniValidator
    if (!MiniValidator.TryValidate(fornecedor, out var errors))
        return Results.ValidationProblem(errors);

    context.Fornecedores.Add(fornecedor);
    var result = await context.SaveChangesAsync();

    //Validação que se criar um fornecedor ele já te devolve informando a rota para você acessar(2 métodos para fazer isso)
    return result > 0
        ? Results.CreatedAtRoute("GetFornecedorPorId", new { id = fornecedor.Id }, fornecedor)
        : Results.BadRequest("Houve um problema ao salvar o registro do fornecedor!");
        //? Results.Created($"/fornecedor/{fornecedor.Id}", fornecedor)

}).ProducesValidationProblem()
 .Produces<Fornecedor>(StatusCodes.Status201Created)
 .Produces(StatusCodes.Status400BadRequest)
 .WithName("PostFornecedor")
 .WithTags("Fornecedor");


app.MapPut("fornecedor/{id}", async (
    Guid id,
    MinimalContextDb context,
    Fornecedor fornecedor) =>
{
    //Validando se o fornecedor existe
    var fornecedorBanco = await context.Fornecedores.FindAsync(id);
    if (fornecedorBanco == null) return Results.NotFound();

    if (!MiniValidator.TryValidate(fornecedor, out var errors))
        return Results.ValidationProblem(errors);

    context.Fornecedores.Update(fornecedor);
    var result = await context.SaveChangesAsync();

    return result > 0
        ? Results.NoContent()
        : Results.BadRequest("Houve um problema ao salvar o registro do fornecedor!");
}).ProducesValidationProblem()
.Produces<Fornecedor>(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PutFornecedor")
.WithTags("Fornecedor");

app.Run();
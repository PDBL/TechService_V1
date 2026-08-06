using MySqlConnector;
using TechService.Api.Data;
using TechService.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Serviços usados pelo Swagger/OpenAPI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Uma única factory reutilizada para criar ligações ao MySQL.
builder.Services.AddSingleton<MySqlConnectionFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoint mantido da Versão 0.
app.MapGet("/", () => Results.Ok(new
{
    mensagem = "Olá! Bem-vindo à API TechService - Versão 1",
    versao = "V1",
    estado = "API ligada ao MySQL",
    endpoint_disponivel = "GET /api/clientes"
}))
.WithName("EstadoDaApi")
.WithSummary("Verificar o estado da API")
.Produces(StatusCodes.Status200OK);

// Versão 1: listar clientes ativos da tabela clientes.
app.MapGet("/api/clientes", async (MySqlConnectionFactory factory) =>
{
    const string sql = """
        SELECT
            id_cliente,
            nome,
            email,
            telefone,
            status,
            created_at,
            updated_at,
            deleted_at
        FROM clientes
        WHERE status = 1
        ORDER BY nome;
        """;

    var clientes = new List<Cliente>();

    await using var connection = factory.CreateConnection();
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);
    await using var reader = await command.ExecuteReaderAsync();

    var ordinalIdCliente = reader.GetOrdinal("id_cliente");
    var ordinalNome = reader.GetOrdinal("nome");
    var ordinalEmail = reader.GetOrdinal("email");
    var ordinalTelefone = reader.GetOrdinal("telefone");
    var ordinalStatus = reader.GetOrdinal("status");
    var ordinalCreatedAt = reader.GetOrdinal("created_at");
    var ordinalUpdatedAt = reader.GetOrdinal("updated_at");
    var ordinalDeletedAt = reader.GetOrdinal("deleted_at");

    while (await reader.ReadAsync())
    {
        clientes.Add(new Cliente
        {
            IdCliente = reader.GetInt32(ordinalIdCliente),
            Nome = reader.GetString(ordinalNome),
            Email = reader.GetString(ordinalEmail),
            Telefone = reader.IsDBNull(ordinalTelefone)
                ? null
                : reader.GetString(ordinalTelefone),
            Status = reader.GetInt32(ordinalStatus),
            CreatedAt = reader.GetDateTime(ordinalCreatedAt),
            UpdatedAt = reader.IsDBNull(ordinalUpdatedAt)
                ? null
                : reader.GetDateTime(ordinalUpdatedAt),
            DeletedAt = reader.IsDBNull(ordinalDeletedAt)
                ? null
                : reader.GetDateTime(ordinalDeletedAt)
        });
    }

    return Results.Ok(clientes);
})
.WithName("ListarClientes")
.WithSummary("Listar clientes ativos")
.WithDescription("Devolve os clientes da tabela clientes cujo status é igual a 1.")
.Produces<List<Cliente>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status500InternalServerError);

app.MapGet("/api/clientes/{id:int}", async (
    int id,
    MySqlConnectionFactory factory) =>
{
    const string sql = """
        SELECT
            id_cliente,
            nome,
            email,
            telefone,
            status,
            created_at,
            updated_at,
            deleted_at
        FROM clientes
        WHERE id_cliente = @id
          AND status = 1
          AND deleted_at IS NULL;
        """;

    await using var connection = factory.CreateConnection();
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", id);

    await using var reader = await command.ExecuteReaderAsync();

    var ordinalIdCliente = reader.GetOrdinal("id_cliente");
    var ordinalNome = reader.GetOrdinal("nome");
    var ordinalEmail = reader.GetOrdinal("email");
    var ordinalTelefone = reader.GetOrdinal("telefone");
    var ordinalStatus = reader.GetOrdinal("status");
    var ordinalCreatedAt = reader.GetOrdinal("created_at");
    var ordinalUpdatedAt = reader.GetOrdinal("updated_at");
    var ordinalDeletedAt = reader.GetOrdinal("deleted_at");

    if (!await reader.ReadAsync())
        return Results.NotFound();

    var cliente = new Cliente
    {
        IdCliente = reader.GetInt32(ordinalIdCliente),
        Nome = reader.GetString(ordinalNome),
        Email = reader.GetString(ordinalEmail),
        Telefone = reader.IsDBNull(ordinalTelefone)
            ? null
            : reader.GetString(ordinalTelefone),
        Status = reader.GetInt32(ordinalStatus),
        CreatedAt = reader.GetDateTime(ordinalCreatedAt),
        UpdatedAt = reader.IsDBNull(ordinalUpdatedAt)
            ? null
            : reader.GetDateTime(ordinalUpdatedAt),
        DeletedAt = reader.IsDBNull(ordinalDeletedAt)
            ? null
            : reader.GetDateTime(ordinalDeletedAt)
    };

    return Results.Ok(cliente);
})
.WithName("ObterCliente")
.WithSummary("Obter um cliente pelo ID");

app.MapPost("/api/clientes", async (
    ClienteRequest novoCliente,
    MySqlConnectionFactory factory) =>
{
    const string sql = """
        INSERT INTO clientes
            (nome,email,telefone,status,created_at)
        VALUES
            (@nome,@email,@telefone,1,NOW());

        SELECT LAST_INSERT_ID();
        """;

    await using var connection = factory.CreateConnection();
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);

    command.Parameters.AddWithValue("@nome", novoCliente.Nome);
    command.Parameters.AddWithValue("@email", novoCliente.Email);
    command.Parameters.AddWithValue("@telefone", novoCliente.Telefone);

    var novoId = Convert.ToInt32(await command.ExecuteScalarAsync());

    return Results.Created(
        $"/api/clientes/{novoId}",
        new
        {
            id = novoId,
            mensagem = "Cliente criado com sucesso."
        });
})
.WithName("CriarCliente")
.WithSummary("Criar cliente");

app.MapPut("/api/clientes/{id:int}", async (
    int id,
    ClienteRequest cliente,
    MySqlConnectionFactory factory) =>
{
    const string sql = """
        UPDATE clientes
        SET
            nome=@nome,
            email=@email,
            telefone=@telefone,
            updated_at=NOW()
        WHERE id_cliente=@id;
        """;

    await using var connection = factory.CreateConnection();
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);

    command.Parameters.AddWithValue("@id", id);
    command.Parameters.AddWithValue("@nome", cliente.Nome);
    command.Parameters.AddWithValue("@email", cliente.Email);
    command.Parameters.AddWithValue("@telefone", cliente.Telefone);

    var linhas = await command.ExecuteNonQueryAsync();

    if (linhas == 0)
        return Results.NotFound();

    return Results.Ok(new
    {
        mensagem = "Cliente atualizado com sucesso."
    });
})
.WithName("AtualizarCliente")
.WithSummary("Atualizar cliente");

app.MapDelete("/api/clientes/{id:int}", async (
    int id,
    MySqlConnectionFactory factory) =>
{
    const string sql = """
        UPDATE clientes
        SET
            status=0,
            deleted_at=NOW()
        WHERE id_cliente=@id;
        """;

    await using var connection = factory.CreateConnection();
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);

    command.Parameters.AddWithValue("@id", id);

    var linhas = await command.ExecuteNonQueryAsync();

    if (linhas == 0)
        return Results.NotFound();

    return Results.Ok(new
    {
        mensagem = "Cliente eliminado com sucesso."
    });
})
.WithName("EliminarCliente")
.WithSummary("Eliminar cliente");

app.Run();

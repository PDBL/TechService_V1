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

// ############################
// ###Equipamentos endpoints###
// ############################

app.MapGet("/api/equipamentos", async (MySqlConnectionFactory factory) =>
{
    const string sql = """
        SELECT
            id_equipamento,
            id_cliente,
            tipo,
            marca,
            modelo,
            numero_serie,
            observacoes,
            status,
            created_at,
            updated_at,
            deleted_at
        FROM equipamentos
        WHERE status = 1
        ORDER BY id_equipamento;
        """;

    var equipamentos = new List<Equipamento>();

    await using var connection = factory.CreateConnection();
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);
    await using var reader = await command.ExecuteReaderAsync();

    var ordinalIdEquipamento = reader.GetOrdinal("id_equipamento");
    var ordinalIdCliente = reader.GetOrdinal("id_cliente");
    var ordinalTipo = reader.GetOrdinal("tipo");
    var ordinalMarca = reader.GetOrdinal("marca");
    var ordinalModelo = reader.GetOrdinal("modelo");
    var ordinalNumeroSerie = reader.GetOrdinal("numero_serie");
    var ordinalObservacoes = reader.GetOrdinal("observacoes");
    var ordinalStatus = reader.GetOrdinal("status");
    var ordinalCreatedAt = reader.GetOrdinal("created_at");
    var ordinalUpdatedAt = reader.GetOrdinal("updated_at");
    var ordinalDeletedAt = reader.GetOrdinal("deleted_at");

    while (await reader.ReadAsync())
    {
        equipamentos.Add(new Equipamento
        {
            IdEquipamento = reader.GetInt32(ordinalIdEquipamento),
            IdCliente = reader.GetInt32(ordinalIdCliente),
            Tipo = reader.GetString(ordinalTipo),
            Marca = reader.GetString(ordinalMarca),
            Modelo = reader.GetString(ordinalModelo),
            NumeroSerie = reader.IsDBNull(ordinalNumeroSerie)
                ? null
                : reader.GetString(ordinalNumeroSerie),
            Observacoes = reader.IsDBNull(ordinalObservacoes)
                ? null
                : reader.GetString(ordinalObservacoes),
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

    return Results.Ok(equipamentos);
})
.WithName("ListarEquipamentos")
.WithSummary("Listar equipamentos ativos")
.WithDescription("Devolve os equipamentos cujo status é igual a 1.")
.Produces<List<Equipamento>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status500InternalServerError);

app.MapGet("/api/equipamentos/{id:int}", async (
    int id,
    MySqlConnectionFactory factory) =>
{
    const string sql = """
        SELECT
            id_equipamento,
            id_cliente,
            tipo,
            marca,
            modelo,
            numero_serie,
            observacoes,
            status,
            created_at,
            updated_at,
            deleted_at
        FROM equipamentos
        WHERE id_equipamento = @id
          AND status = 1;
        """;

    await using var connection = factory.CreateConnection();
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", id);

    await using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
    {
        return Results.NotFound();
    }

    var equipamento = new Equipamento
    {
        IdEquipamento = reader.GetInt32(reader.GetOrdinal("id_equipamento")),
        IdCliente = reader.GetInt32(reader.GetOrdinal("id_cliente")),
        Tipo = reader.GetString(reader.GetOrdinal("tipo")),
        Marca = reader.GetString(reader.GetOrdinal("marca")),
        Modelo = reader.GetString(reader.GetOrdinal("modelo")),
        NumeroSerie = reader.IsDBNull(reader.GetOrdinal("numero_serie"))
            ? null
            : reader.GetString(reader.GetOrdinal("numero_serie")),
        Observacoes = reader.IsDBNull(reader.GetOrdinal("observacoes"))
            ? null
            : reader.GetString(reader.GetOrdinal("observacoes")),
        Status = reader.GetInt32(reader.GetOrdinal("status")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
        UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at"))
            ? null
            : reader.GetDateTime(reader.GetOrdinal("updated_at")),
        DeletedAt = reader.IsDBNull(reader.GetOrdinal("deleted_at"))
            ? null
            : reader.GetDateTime(reader.GetOrdinal("deleted_at"))
    };

    return Results.Ok(equipamento);
})
.WithName("ConsultarEquipamento")
.WithSummary("Consultar equipamento por ID")
.Produces<Equipamento>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/equipamentos", async (
    EquipamentoRequest equipamento,
    MySqlConnectionFactory factory) =>
{
    const string sql = """
        INSERT INTO equipamentos
        (
            id_cliente,
            tipo,
            marca,
            modelo,
            numero_serie,
            observacoes
        )
        VALUES
        (
            @id_cliente,
            @tipo,
            @marca,
            @modelo,
            @numero_serie,
            @observacoes
        );

        SELECT LAST_INSERT_ID();
        """;

    await using var connection = factory.CreateConnection();
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);

    command.Parameters.AddWithValue("@id_cliente", equipamento.IdCliente);
    command.Parameters.AddWithValue("@tipo", equipamento.Tipo);
    command.Parameters.AddWithValue("@marca", equipamento.Marca);
    command.Parameters.AddWithValue("@modelo", equipamento.Modelo);
    command.Parameters.AddWithValue(
        "@numero_serie",
        (object?)equipamento.NumeroSerie ?? DBNull.Value
    );
    command.Parameters.AddWithValue(
        "@observacoes",
        (object?)equipamento.Observacoes ?? DBNull.Value
    );

    var id = Convert.ToInt32(await command.ExecuteScalarAsync());

    return Results.Created(
        $"/api/equipamentos/{id}",
        new { idEquipamento = id }
    );
})
.WithName("InserirEquipamento")
.WithSummary("Inserir equipamento")
.Produces(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

app.MapPut("/api/equipamentos/{id:int}", async (
    int id,
    EquipamentoRequest equipamento,
    MySqlConnectionFactory factory) =>
{
    const string sql = """
        UPDATE equipamentos
        SET
            id_cliente = @id_cliente,
            tipo = @tipo,
            marca = @marca,
            modelo = @modelo,
            numero_serie = @numero_serie,
            observacoes = @observacoes
        WHERE id_equipamento = @id
          AND status = 1;
        """;

    await using var connection = factory.CreateConnection();
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);

    command.Parameters.AddWithValue("@id", id);
    command.Parameters.AddWithValue("@id_cliente", equipamento.IdCliente);
    command.Parameters.AddWithValue("@tipo", equipamento.Tipo);
    command.Parameters.AddWithValue("@marca", equipamento.Marca);
    command.Parameters.AddWithValue("@modelo", equipamento.Modelo);
    command.Parameters.AddWithValue(
        "@numero_serie",
        (object?)equipamento.NumeroSerie ?? DBNull.Value
    );
    command.Parameters.AddWithValue(
        "@observacoes",
        (object?)equipamento.Observacoes ?? DBNull.Value
    );

    var linhas = await command.ExecuteNonQueryAsync();

    if (linhas == 0)
    {
        return Results.NotFound();
    }

    return Results.Ok(new
    {
        mensagem = "Equipamento atualizado com sucesso.",
        idEquipamento = id
    });
})
.WithName("AtualizarEquipamento")
.WithSummary("Atualizar equipamento")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapDelete("/api/equipamentos/{id:int}", async (
    int id,
    MySqlConnectionFactory factory) =>
{
    const string sql = """
        UPDATE equipamentos
        SET
            status = 0,
            deleted_at = CURRENT_TIMESTAMP
        WHERE id_equipamento = @id
          AND status = 1;
        """;

    await using var connection = factory.CreateConnection();
    await connection.OpenAsync();

    await using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", id);

    var linhas = await command.ExecuteNonQueryAsync();

    if (linhas == 0)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
})
.WithName("DesativarEquipamento")
.WithSummary("Desativar equipamento")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

app.Run();

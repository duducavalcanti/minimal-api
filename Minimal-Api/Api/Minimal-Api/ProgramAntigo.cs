/* using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Enuns;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.Dominio.ModelViews;
using MinimalAPI.Dominio.Servicos;
using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Infraestrutura.DB;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using MinimalAPI.Infraestrutura.Swagger;
using Microsoft.AspNetCore.Authorization;

# region Builder
var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");

var key = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey não configurado");

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<DBContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
        );
});

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();

builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui."
    });

    // Aqui definimos que apenas endpoints que chamarem RequireAuthorization() terão segurança no Swagger
    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

// Configuração para que os Enums sejam aceitos como string no JSON 
// (ex: "editor", "Editor" ou "EDITOR") em vez de apenas números.
// Também permite que os nomes das propriedades sejam lidos sem diferenciar maiúsculas e minúsculas.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter(
            System.Text.Json.JsonNamingPolicy.CamelCase,
            allowIntegerValues: true
        )
    );
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();
# endregion

# region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
# endregion

# region Administradores
string GerarTokenJwt(Administrador administrador)
{
    var keyBytes = Encoding.UTF8.GetBytes(key);

    if (keyBytes.Length < 32)
    {
        throw new InvalidOperationException("A chave JWT deve ter pelo menos 32 bytes (256 bits)");
    }

    var securityKey = new SymmetricSecurityKey(keyBytes);

    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil.ToUpper())
    };

    var token = new JwtSecurityToken(
        issuer: jwtSettings["Issuer"],
        audience: jwtSettings["Audience"],
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
    {
        var adm = administradorServico.Login(loginDTO);
        if (adm != null)
        {
            string token = GerarTokenJwt(adm);
            return Results.Ok(
                new AdministradorLogado
                {
                    Email = adm.Email,
                    Perfil = adm.Perfil,
                    Token = token
                }
            );
        }
        else
        {
            return Results.Unauthorized();
        }
    }).AllowAnonymous().WithTags("Administradores");

ErrosDeValidacao validaAdministradorDTO(AdministradorDTO administradorDTO)
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(administradorDTO.Email))
    {
        validacao.Mensagens.Add("O email não pode ser vazio.");
    }

    if (string.IsNullOrEmpty(administradorDTO.Senha))
    {
        validacao.Mensagens.Add("A senha não pode ser vazia.");
    }

    if (string.IsNullOrEmpty(administradorDTO.Perfil))
    {
        validacao.Mensagens.Add("O perfil não pode ser vazio.");
    }
    else if (!Enum.TryParse<PerfilAdministrador>(administradorDTO.Perfil?.ToString(), ignoreCase: true, out _))
    {
        validacao.Mensagens.Add($"O perfil '{administradorDTO.Perfil}' não é válido.");
    }

    return validacao;
}

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = validaAdministradorDTO(administradorDTO);

    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil!.ToString()
    };

    administradorServico.CadastrarAdministrador(administrador);

    return Results.Created($"/administradores/{administrador.ID}", new AdministradorModelView
    {
        ID = administrador.ID,
        Email = administrador.Email,
        Perfil = Enum.TryParse<PerfilAdministrador>(administrador.Perfil, ignoreCase: true, out var perfil)
             ? perfil
             : PerfilAdministrador.Editor
    });

}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradorModelView>();

    var administradores = administradorServico.Todos(pagina) ?? new List<Administrador>();

    foreach (var administrador in administradores)
    {
        adms.Add(new AdministradorModelView
        {
            ID = administrador.ID,
            Email = administrador.Email,
            Perfil = Enum.TryParse<PerfilAdministrador>(administrador.Perfil, ignoreCase: true, out var perfil)
             ? perfil
             : PerfilAdministrador.Editor
        });
    }

    return Results.Ok(adms);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscaPorId(id);

    if (administrador == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new AdministradorModelView
    {
        ID = administrador.ID,
        Email = administrador.Email,
        Perfil = Enum.TryParse<PerfilAdministrador>(administrador.Perfil, ignoreCase: true, out var perfil)
             ? perfil
             : PerfilAdministrador.Editor
    });
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).WithTags("Administradores");

app.MapPut("/administradores/{id}", ([FromRoute] int id, [FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscaPorId(id);

    if (administrador == null)
    {
        return Results.NotFound();
    }

    var validacao = validaAdministradorDTO(administradorDTO);

    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    administrador.Email = administradorDTO.Email;
    administrador.Senha = administradorDTO.Senha;
    administrador.Perfil = administradorDTO.Perfil.ToString() ?? PerfilAdministrador.Editor.ToString();

    administradorServico.AtualizarAdministrador(administrador);

    return Results.Ok(new AdministradorModelView
    {
        ID = administrador.ID,
        Email = administrador.Email,
        Perfil = Enum.TryParse<PerfilAdministrador>(administrador.Perfil, ignoreCase: true, out var perfil)
             ? perfil
             : PerfilAdministrador.Editor
    });

}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).WithTags("Administradores");

app.MapDelete("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscaPorId(id);

    if (administrador == null)
    {
        return Results.NotFound();
    }

    administradorServico.ApagarAdministrador(administrador);

    return Results.NoContent();
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).WithTags("Administradores");
#endregion

#region Veiculos
ErrosDeValidacao validaVeiculoDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
    {
        validacao.Mensagens.Add("O nome não pode ser vazio.");
    }

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
    {
        validacao.Mensagens.Add("A marca não pode ficar em branco.");
    }

    if (veiculoDTO.Ano < 1950)
    {
        validacao.Mensagens.Add("Veículo muito antigo, aceito apenas anos superiores a 1950.");
    }

    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var validacao = validaVeiculoDTO(veiculoDTO);

    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };

    veiculoServico.CadastrarVeiculo(veiculo);

    return Results.Created($"/veiculos/{veiculo.ID}", veiculo);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM,EDITOR" }).WithTags("Veículos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM,EDITOR" }).WithTags("Veículos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);

    if (veiculo == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(veiculo);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM,EDITOR" }).WithTags("Veículos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);

    if (veiculo == null)
    {
        return Results.NotFound();
    }

    var validacao = validaVeiculoDTO(veiculoDTO);

    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.AtualizarVeiculo(veiculo);

    return Results.Ok(veiculo);

}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).WithTags("Veículos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);

    if (veiculo == null)
    {
        return Results.NotFound();
    }

    veiculoServico.ApagarVeiculo(veiculo);

    return Results.NoContent();
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).WithTags("Veículos");
#endregion

#region App
app.UseSwagger();

app.UseSwaggerUI();

app.UseAuthentication();

app.UseAuthorization();

app.Run();
#endregion */
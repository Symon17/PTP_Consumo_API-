using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.OutputEncoding = Encoding.UTF8;

string url = "https://api.first.org/data/v1/countries";

using HttpClient client = new HttpClient();

try
{
    Console.WriteLine("A obter dados da API...");

    HttpResponseMessage resposta = await client.GetAsync(url);
    resposta.EnsureSuccessStatusCode();

    string json = await resposta.Content.ReadAsStringAsync();

    RespostaPaises? resultado = JsonSerializer.Deserialize<RespostaPaises>(json);

    if (resultado == null)
    {
        Console.WriteLine("Não foi possível ler os dados.");
        return;
    }

    Console.WriteLine($"Dados recebidos: {resultado.Data.Count} países (total na API: {resultado.Total}).");

    bool continuar = true;

    while (continuar)
    {
        Console.WriteLine();
        Console.WriteLine("========== MENU ==========");
        Console.WriteLine("1 - Listar todos os países");
        Console.WriteLine("2 - Listar países de uma região");
        Console.WriteLine("3 - Pesquisar país pelo nome");
        Console.WriteLine("4 - Procurar país pelo código (ex: MZ)");
        Console.WriteLine("0 - Sair");
        Console.Write("Escolha uma opção: ");

        string? opcao = Console.ReadLine();
        Console.WriteLine();

        switch (opcao)
        {
            case "1":
                foreach (var item in resultado.Data.OrderBy(p => p.Value.Nome))
                {
                    Console.WriteLine($"{item.Key} - {item.Value.Nome} ({item.Value.Regiao})");
                }
                break;

            case "2":
                var regioes = resultado.Data.Values
                    .Select(p => p.Regiao)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();

                Console.WriteLine("Regiões disponíveis: " + string.Join(", ", regioes));
                Console.Write("Escreva a região: ");
                string? regiao = Console.ReadLine();

                var daRegiao = resultado.Data
                    .Where(p => string.Equals(p.Value.Regiao, regiao?.Trim(), StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.Value.Nome)
                    .ToList();

                if (daRegiao.Count == 0)
                {
                    Console.WriteLine("Nenhuma região encontrada com esse nome.");
                }
                else
                {
                    Console.WriteLine();
                    foreach (var item in daRegiao)
                    {
                        Console.WriteLine($"{item.Key} - {item.Value.Nome}");
                    }
                    Console.WriteLine($"Total: {daRegiao.Count} países.");
                }
                break;

            case "3":
                Console.Write("Escreva parte do nome do país: ");
                string? termo = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(termo))
                {
                    Console.WriteLine("Não escreveu nada.");
                    break;
                }

                var encontrados = resultado.Data
                    .Where(p => p.Value.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.Value.Nome)
                    .ToList();

                if (encontrados.Count == 0)
                {
                    Console.WriteLine("Nenhum país encontrado.");
                }
                else
                {
                    foreach (var item in encontrados)
                    {
                        Console.WriteLine($"{item.Key} - {item.Value.Nome} ({item.Value.Regiao})");
                    }
                }
                break;

            case "4":
                Console.Write("Escreva o código do país (2 letras): ");
                string codigo = (Console.ReadLine() ?? "").Trim().ToUpper();

                if (resultado.Data.TryGetValue(codigo, out Pais? pais))
                {
                    Console.WriteLine($"Código: {codigo}");
                    Console.WriteLine($"País:   {pais.Nome}");
                    Console.WriteLine($"Região: {pais.Regiao}");
                }
                else
                {
                    Console.WriteLine("Código não encontrado.");
                }
                break;

            case "0":
                continuar = false;
                Console.WriteLine("Até à próxima!");
                break;

            default:
                Console.WriteLine("Opção inválida. Tente outra vez.");
                break;
        }
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine("Erro de ligação: " + ex.Message);
}
catch (JsonException ex)
{
    Console.WriteLine("Erro ao ler o JSON: " + ex.Message);
}

public class RespostaPaises
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, Pais> Data { get; set; } = new();
}

public class Pais
{
    [JsonPropertyName("country")]
    public string Nome { get; set; } = "";

    [JsonPropertyName("region")]
    public string Regiao { get; set; } = "";
}
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace IbgeConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Por favor, forneça o caminho do arquivo de saída.");
                return;
            }

            var outputPath = args[0];

            var serviceProvider = new ServiceCollection()
                .AddHttpClient()
                .BuildServiceProvider();

            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            var estadosECidades = await GetEstadosECidades(httpClientFactory);

            var json = JsonConvert.SerializeObject(estadosECidades, Newtonsoft.Json.Formatting.Indented);

            await File.WriteAllTextAsync(outputPath, json);

            Console.WriteLine($"Dados salvos em {outputPath}");
        }

        private static async Task<List<EstadoComCidades>> GetEstadosECidades(IHttpClientFactory httpClientFactory)
        {
            var client = httpClientFactory.CreateClient();
            var estadosResponse = await client.GetStringAsync("https://servicodados.ibge.gov.br/api/v1/localidades/estados");
            var estados = JsonConvert.DeserializeObject<List<Estado>>(estadosResponse);

            var resultado = new List<EstadoComCidades>();

            foreach (var estado in estados)
            {
                var cidadesResponse = await client.GetStringAsync($"https://servicodados.ibge.gov.br/api/v1/localidades/estados/{estado.Id}/municipios");
                var cidades = JsonConvert.DeserializeObject<List<Cidade>>(cidadesResponse);

                resultado.Add(new EstadoComCidades
                {
                    Estado = estado.Nome,
                    Sigla = estado.Sigla,
                    Cidades = cidades.ConvertAll(c => c.Nome)
                });
            }

            return resultado;
        }
    }

    public class Estado
    {
        public int Id { get; set; }
        public string Sigla { get; set; }
        public string Nome { get; set; }
    }

    public class Cidade
    {
        public string Nome { get; set; }
    }

    public class EstadoComCidades
    {
        public string Estado { get; set; }
        public string Sigla { get; set; }
        public List<string> Cidades { get; set; }
    }
}
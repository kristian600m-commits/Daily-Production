using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using DailyProduction.Models;

namespace IbasAPI.Services
{
    public class ProductionService
    {
        private readonly string _connectionString;
        private readonly string _tableName = "IBASProduction2022";

        public ProductionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureTableStorage")
                ?? throw new InvalidOperationException("Connection string 'AzureTableStorage' ikke fundet i appsettings.json.");
        }

        public async Task<List<DailyProductionDTO>> GetDailyProductionsAsync()
        {
            var serviceClient = new TableServiceClient(_connectionString);
            await serviceClient.CreateTableIfNotExistsAsync(_tableName);

            var client = new TableClient(_connectionString, _tableName);

            // Tjek om tabellen er tom — hvis ja, populer med CSV-data
            await SeedDataIfEmptyAsync(client);

            var resultList = new List<DailyProductionDTO>();

            await foreach (ProductionEntity entity in client.QueryAsync<ProductionEntity>())
            {
                _ = Enum.TryParse<BikeModel>(entity.PartitionKey, out var bikeModel);
                _ = DateTime.TryParse(entity.RowKey, out var productionDate);

                resultList.Add(new DailyProductionDTO
                {
                    Model = bikeModel,
                    Date = productionDate,
                    ItemsProduced = entity.ItemsProduced
                });
            }

            return resultList;
        }

        private async Task SeedDataIfEmptyAsync(TableClient client)
        {
            // Tjek om der allerede er data
            var existingEntities = client.QueryAsync<ProductionEntity>();
            await using var enumerator = existingEntities.GetAsyncEnumerator();

            if (await enumerator.MoveNextAsync())
            {
                return; // Tabellen har allerede data
            }

            // Sti til din CSV-fil i projektmappen
            string csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "IBASProduction2022.csv");

            if (!File.Exists(csvFilePath))
            {
                return; // CSV-filen blev ikke fundet
            }

            var lines = await File.ReadAllLinesAsync(csvFilePath);

            // Spring over header-linjen (linje 0)
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(';');
                if (parts.Length < 4) parts = line.Split(','); // Håndter kommaseparerede filer

                if (parts.Length >= 4)
                {
                    var entity = new ProductionEntity
                    {
                        PartitionKey = parts[0].Trim(), // F.eks. 1, 2 eller 3
                        RowKey = parts[1].Trim(),       // Dato
                        ProductionTime = DateTimeOffset.TryParse(parts[2].Trim(), out var pt) ? pt : DateTimeOffset.UtcNow,
                        ItemsProduced = int.TryParse(parts[3].Trim(), out var count) ? count : 0
                    };

                    await client.AddEntityAsync(entity);
                }
            }
        }
    }
}
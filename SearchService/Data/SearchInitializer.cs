using Typesense;

namespace SearchService.Data
{
    public static class SearchInitializer
    {
        public static async Task EnsureIndexExists(ITypesenseClient typesenseClient)
		{
			const string collectionName = "questions";
            const int maxAttempts = 10;
            const int delaySeconds = 3;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await typesenseClient.RetrieveCollection(collectionName);
                    Console.WriteLine($"Collection '{collectionName}' already exists.");
                    return;
                }
                catch (TypesenseApiNotFoundException)
                {
                    Console.WriteLine($"Collection '{collectionName}' has not been created yet. It will be created now.");
                    break; // exit retry loop and proceed to schema creation
                }
                catch (TypesenseApiUnprocessableEntityException ex) when (ex.Message.Contains("Not Ready or Lagging", StringComparison.OrdinalIgnoreCase))
                {
                    if (attempt == maxAttempts)
                    {
                        Console.WriteLine($"Typesense is still not ready after {maxAttempts} attempts. Giving up.");
                        throw;
                    }

                    Console.WriteLine($"Typesense not ready (attempt {attempt}/{maxAttempts}): {ex.Message}. Retrying in {delaySeconds} seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            var schema = new Schema(collectionName, new List<Field> { 
                new Field("id", FieldType.String, true),
                new Field("title", FieldType.String, false),
                new Field("content", FieldType.String, false),
                new Field("tags", FieldType.StringArray, false, true),
                new Field("createddate", FieldType.Int64, false),
                new Field("hasacceptedanswer", FieldType.Bool, false),
                new Field("answerscount", FieldType.Int32, false)
            })
            { DefaultSortingField= "createddate"};

            var collection = await typesenseClient.CreateCollection(schema);
            Console.WriteLine($"Collection '{collectionName}' created successfully.");
        }
    }
}

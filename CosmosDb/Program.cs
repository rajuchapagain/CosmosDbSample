
using CosmosDb;
using Microsoft.Azure.Cosmos;

string cosmosEndpointUri = "enter your cosmod db api url";
string cosmosDbKey = "enter your cosmos db api key";
string databaseName = "appdb";
string containerName = "Orders";
string partitionKey = "/category";
await CreateDatabase();
await CreateContainer();
await AddItem("O1", "Laptop", 100);
await AddItem("O2", "Mobile", 200);
await AddItem("O3", "Desktop", 190);
await AddItem("O4", "Battery", 30);

await ReadItem();

async Task CreateDatabase()
{
    CosmosClient cosmosClient;
    cosmosClient = new CosmosClient(cosmosEndpointUri, cosmosDbKey);
    await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
    Console.WriteLine("Database Created");
}
async Task CreateContainer()
{
    CosmosClient cosmosClient;
    cosmosClient = new CosmosClient(cosmosEndpointUri, cosmosDbKey);
    Database database = cosmosClient.GetDatabase(databaseName);

    await database.CreateContainerIfNotExistsAsync(containerName, partitionKey);
}
async Task AddItem(string orderId, string category, int quantity)
{
    CosmosClient cosmosClient;
    cosmosClient = new CosmosClient(cosmosEndpointUri, cosmosDbKey);
    Database database = cosmosClient.GetDatabase(databaseName);
    Container container = database.GetContainer(containerName);

    Orders orders = new Orders
    {
        id = Guid.NewGuid().ToString(),
        category = category,
        orderId = orderId,
        quantity = quantity
    };
    ItemResponse<Orders> response = await container.CreateItemAsync(orders, new PartitionKey(orders.category));

    Console.WriteLine("Added item with order Id {0}", orders.orderId);
    Console.WriteLine("Request unit {0}", response.RequestCharge);
}

async Task ReadItem()
{
    CosmosClient cosmosClient;
    cosmosClient = new CosmosClient(cosmosEndpointUri, cosmosDbKey);
    Database database = cosmosClient.GetDatabase(databaseName);
    Container container = database.GetContainer(containerName);
    string query = "SELECT o.orderId, o.category, o.quantity FROM Orders o";
    FeedIterator<Orders> result = container.GetItemQueryIterator<Orders>(query);

    while (result.HasMoreResults)
    {
        Console.WriteLine("Reading job in progress");
        FeedResponse<Orders> feedResponse = await result.ReadNextAsync();
        foreach (Orders order in feedResponse)
        {
            Console.WriteLine($"Order id {order.orderId} category {order.category} quantity {order.quantity}");
        }
    }
}

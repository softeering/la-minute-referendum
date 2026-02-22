var builder = DistributedApplication.CreateBuilder(args);

// Add Azurite container for Azure Table Storage emulation via Aspire extension
var storage = builder.AddAzureStorage("storage")
	.RunAsEmulator(emulator =>
	{
		emulator.WithEnvironment("AZURITE_ACCOUNTS", "devstoreaccount1:Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUQtSiJGSstq6QblzgdyP89YXLVQ7E7DK45LrZQBQ7zLoDMaoco6Obtau4Toucp3StikrFrsHIYV1d0na8Z8D3II-7i2izsqt69ZNBmUQQfjKDJgw==");
	});

var tableClient = storage.AddTables("AzureTableStorage");

// Add the LaMinuteReferendum project and reference Azure Storage for service discovery
builder.AddProject<Projects.LaMinuteReferendum>("laminutereferendum")
	.WithReference(tableClient);

builder.Build().Run();

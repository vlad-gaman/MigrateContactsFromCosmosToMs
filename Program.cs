var host = Host.CreateDefaultBuilder()
    .ConfigureLogging(builder =>
    {
        builder
            .ClearProviders()
            .AddConsole();
    })
    .ConfigureServices(services =>
    {
        services
            .AddCosmosDatabase()
            .AddContactContainer()
            .AddGroupContainer()
            .AddMsSqlDatabase()
            .AddSingleton<Fixture>()
            .AddSingleton<MigrateDataBackgroundService>()
            .AddSingleton<GenerateDataService>()
            .AddSingleton<OtherStuffService>()
            ;
    })
    .Build();

var action = Actions.MigrateDataFromCosmosInMsSql;
switch (action)
{
    case Actions.GenerateDataInCosmos:
        var ser1 = host.Services.GetRequiredService<GenerateDataService>();
        await ser1.ExecuteAsync();
        break;
    case Actions.MigrateDataFromCosmosInMsSql:
        var sw = new Stopwatch();
        var ser2 = host.Services.GetRequiredService<MigrateDataBackgroundService>();
        sw.Start();
        await ser2.ExecuteAsync();
        sw.Stop();
        Console.WriteLine($"Total time to migrate in milliseconds: {sw.ElapsedMilliseconds}");
        break;
    case Actions.DeleteCertainDataFromCosmos:
        var ser3 = host.Services.GetRequiredService<OtherStuffService>();
        await ser3.DeleteContactsWithVersionLike();
        await ser3.DeleteGroupsWithGroupNameLike();
        await ser3.DeleteDuplicateGroups();
        await ser3.DeleteContactsWithMsisdnLongerThen32();
        break;
    default:
        await host.StartAsync();
        break;
}

enum Actions
{
    GenerateDataInCosmos,
    MigrateDataFromCosmosInMsSql,
    DeleteCertainDataFromCosmos
}
namespace UserManagement.Application.IntegrationTests;


[CollectionDefinition(IntegrationTestCollection.CollectionName)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
    public const string CollectionName = "UserManagement.Application.IntegrationTests";
}
using Blake2Fast;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ImageApiIntegrationTests;

public class BaseIntegrationTest
{
    private readonly WebApplicationFactory<Program> _webHost;
    
    private readonly byte[] _fakePerson1;
    private readonly byte[] _fakePerson2;

    public BaseIntegrationTest()
    {
        using var fileStream = File.OpenRead("./TestFiles/FakePerson1.jpg");
        //read files
        _fakePerson1 = new byte[fileStream.Length];
        fileStream.Read(_fakePerson1);
        FakePerson1Hash = Convert.ToBase64String(Blake2b.ComputeHash(_fakePerson1));
        using var fileStream2 = File.OpenRead("./TestFiles/FakePerson2.jpg");
        _fakePerson2 = new byte[fileStream.Length];
        fileStream.Read(_fakePerson2);
        FakePerson2Hash = Convert.ToBase64String(Blake2b.ComputeHash(_fakePerson1));

        _webHost = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            const string TEMP = "./temp";
            var directoryInfo = new DirectoryInfo(TEMP);
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }
            directoryInfo.Create();
            
            b.ConfigureAppConfiguration(c =>
            {
                c.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ApiOptions:SaveLocation"] = directoryInfo.FullName,
                });
            });
        });
    }

    protected Stream FakePerson1Picture => new MemoryStream(_fakePerson1, writable: false);
    protected Stream FakePerson2Picture => new MemoryStream(_fakePerson2, writable: false);
    protected string FakePerson1Hash { get; }
    protected string FakePerson2Hash { get; }
    protected HttpClient HttpClient => _webHost.CreateDefaultClient();

}
namespace graphtestapp;

using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

class Program
{
    static async Task Main(string[] args)
    {
        var scopes = new[] { "User.Read" };
        var clientSecretCredential = new DefaultAzureCredential();
        var graphClient = new GraphServiceClient(clientSecretCredential);

        // ユーザーのメールアドレスを指定
        var targetUser = "hoge@fuga.com";
        var user = await graphClient.Users[targetUser]
            .GetAsync();

        if (user == null)
        {
            Console.WriteLine($"User {targetUser} not found.");
            return;
        }

        Console.WriteLine($"Groups {user.DisplayName} is in:");

        // ターゲットにしているユーザーが所属しているグループを取得
        var groups = await graphClient.Users[targetUser]
            .MemberOf
            .GetAsync();

        if (groups == null)
        {
            Console.WriteLine($"No groups found for {user.DisplayName}.");
            return;
        }

        await DisplayGroupsAsync(graphClient, groups);
    }

    private static async Task DisplayGroupsAsync(GraphServiceClient graphClient, DirectoryObjectCollectionResponse groups)
    {
        int num = 0;
        var pageIterator = PageIterator<DirectoryObject, DirectoryObjectCollectionResponse>.CreatePageIterator(
            graphClient,
            groups,
            (g) =>
            {
                // ここで g には DirectoryObject が入っているが、普通にアクセスすると DisplayName とかがなぜか見られない。
                // なので、キャストしてやると見れるようになる。
                Console.WriteLine($"{num++}: {g.Id} : {((Group)g).DisplayName}");
                return true;
            }
        );

        await pageIterator.IterateAsync();
    }
}

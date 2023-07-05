namespace Server.Services;

public interface IPlayerService
{
    void DoSomething();
}
public class PlayerService : IPlayerService
{
    public void DoSomething()
    {
        Console.WriteLine("Hey What The Fuck !!!");
    }
}

public class MockPlayerService : IPlayerService
{
    public void DoSomething()
    {
        Console.WriteLine("Fucking Something In Mock ");
    }
}

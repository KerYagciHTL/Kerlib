using Kerlib.Drawing;
using Kerlib.Native;
using Kerlib.Window;
using static Kerlib.Native.Key;

namespace Kerlib;

public class MainWindow : Core.Window
{
    public const string CoinPath = "Assets/coin.png";
    
    private readonly Image _player;
    private readonly RenderStack _coins = new();
    private readonly Random _rand = new();
    private int _score = 0;

    public MainWindow() : base("MiniCoinGame", 800, 600)
    {
        KeysDown += OnKeysDown;
        Tick += OnTick;

        _player = new Image(new Point(375, 275), "Assets/puppy.png", 50, 50);
        Add(_player);

        // initial 5 coins
        for (int i = 0; i < 5; i++)
            SpawnCoin();

        Add(_coins);
    }

    private void OnKeysDown(IReadOnlyCollection<Key> keys)
    {
        if (keys.Contains(A)) _player.Position.X -= 5;
        if (keys.Contains(D)) _player.Position.X += 5;
        if (keys.Contains(W)) _player.Position.Y -= 5;
        if (keys.Contains(S)) _player.Position.Y += 5;

        ClampPlayer();
    }

    private void OnTick()
    {
        CheckCollisions();
        Console.WriteLine($"Score: {_score}");
    }

    private void ClampPlayer()
    {
        _player.Position.X = Math.Clamp(_player.Position.X, 0, Width - _player.Width);
        _player.Position.Y = Math.Clamp(_player.Position.Y, 0, Height - _player.Height);
    }

    private void SpawnCoin()
    {
        int x = _rand.Next(0, Width - 32);
        int y = _rand.Next(0, Height - 32);
        var image = new Image(new Point(x, y), CoinPath, 32, 32);
        
        _coins.Add(image);
        Add(image);
    }

    private void CheckCollisions()
    {
        var collected = new List<Image>();

        foreach (var coin in _coins.OfType<Image>())
        {
            if (IsColliding(_player, coin))
                collected.Add(coin);
        }

        foreach (var coin in collected)
        {
            _coins.Remove(coin);
            Remove(coin);
            _score++;
            SpawnCoin();
        }
    }

    private static bool IsColliding(Image a, Image b)
    {
        return a.Position.X < b.Position.X + b.Width &&
               a.Position.X + a.Width > b.Position.X &&
               a.Position.Y < b.Position.Y + b.Height &&
               a.Position.Y + a.Height > b.Position.Y;
    }
}

namespace HappySnorlax.Utils;

public class RandomThreadSafe
{
    private static readonly Random _random = new();
    
    private object __lockRandom = new();
    
    public T GetThreadSafeRandomValue<T>(Func<Random, T> predicated)
    {
        T result;
        lock (__lockRandom)
        {
            result = predicated(_random);
        }
        return result;
    }
}
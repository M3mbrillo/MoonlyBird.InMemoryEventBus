namespace HappySnorlax.Utils;

public class MemoryKiller
{
    public async Task<byte[][]> BigArrays(int objectSize, int arraySize)
    {
        var largeObjects = new byte[arraySize][];

        for (int i = 0; i < arraySize; i++)
        {
            largeObjects[i] = new byte[objectSize]; // Crear un objeto de 1 MB
            Array.Fill(largeObjects[i], (byte)i); // Llenar con datos
        }

        return largeObjects;
    }
}
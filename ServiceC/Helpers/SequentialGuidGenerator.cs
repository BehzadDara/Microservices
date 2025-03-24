namespace ServiceC.Helpers;

public static class SequentialGuidGenerator
{
    private static readonly long _epoch = new DateTime(2022, 1, 1).Ticks;

    public static Guid Create()
    {
        long timestamp = DateTime.UtcNow.Ticks - _epoch;

        byte[] guidArray = Guid.NewGuid().ToByteArray();

        byte[] timestampBytes = BitConverter.GetBytes(timestamp);
        Array.Copy(timestampBytes, 0, guidArray, 0, 8);

        return new Guid(guidArray);
    }
}

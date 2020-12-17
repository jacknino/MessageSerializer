namespace MessageSerializer
{
    public enum BlobTypes
    {
        None = 0,
        Length,
        Data
    }

    public enum MessageLengthTypes
    {
        None = 0,
        EntireMessage,
        RestOfMessageIncludingLength,
        RestOfMessage
    }

    public enum Endiannesses
    {
        // System means use whatever the system is by default (like x86 processors are little endian)
        System = 0,
        Little,
        Big
    }
}

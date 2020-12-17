namespace MessageSerializer
{
    public class DeserializeResults<T> where T : class, IMessageSerializable
    {
        public DeserializeResults()
        {
            Object = null;
            Status = new DeserializeStatus();
        }

        public T Object { get; set; }
        public DeserializeStatus Status { get; set; }

        public static implicit operator bool(DeserializeResults<T> results)  // implicit digit to byte conversion operator
        {
            return results.Status.Results;
        }
    }
}

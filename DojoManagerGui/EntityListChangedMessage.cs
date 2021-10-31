namespace DojoManagerGui
{
    class EntityListChangedMessage<T> where T : class
    {
        public object Sender { get; private set; }
        public T[] Added { get; private set; }
        public T[] Removed { get; private set; }
        public EntityListChangedMessage(object sender, T[] added, T[] removed)
        {
            Sender = sender;
            Added = added;
            Removed = removed;
        }
    }
}

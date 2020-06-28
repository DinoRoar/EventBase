namespace EventBase.Client
{
    public class CategoryProjection : Projection
    {
        public CategoryProjection()
            : base("Category", BuildStreamName, e => !(e is ProjectedEvent))
        {

        }

        private static string BuildStreamName(StreamEvent e)
        {
            var parts = e.StreamName.Split('-');
            return $"ca-{parts[0]}";
        }
    }
}
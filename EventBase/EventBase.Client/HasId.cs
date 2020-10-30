using System;

namespace EventBase.Client
{
    public class HasId
    {
        public string GetId() => $"{this.GetType().Name}-{Guid.NewGuid()}";
    }
}
 
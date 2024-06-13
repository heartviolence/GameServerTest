using MessagePack;
using System;

namespace ServerShared.Match
{
    [Union(0, typeof(FindUserFilter))]
    [MessagePackObject]
    public abstract class Filter
    {
    }

    [MessagePackObject]
    public class FindUserFilter : Filter
    {
        public Guid UserId { get; set; }
    }
}

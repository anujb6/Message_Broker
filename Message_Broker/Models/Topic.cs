using System;
using System.Collections.Generic;

namespace Message_Broker.Models;

public partial class Topic
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Subscription> Subscriptions { get; } = new List<Subscription>();
}

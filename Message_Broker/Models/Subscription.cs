using System;
using System.Collections.Generic;

namespace Message_Broker.Models;

public partial class Subscription
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int TopicId { get; set; }

    public virtual ICollection<Message> Messages { get; } = new List<Message>();

    public virtual Topic Topic { get; set; } = null!;
}

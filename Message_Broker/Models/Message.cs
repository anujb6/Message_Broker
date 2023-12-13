using System;
using System.Collections.Generic;

namespace Message_Broker.Models;

public partial class Message
{
    public int Id { get; set; }

    public string TopicMessage { get; set; } = null!;

    public int? SubscriptionId { get; set; }

    public DateTime ExpiresAfter { get; set; } = DateTime.UtcNow.AddDays(1);

    public string MessageStatus { get; set; } = "NEW";

    public virtual Subscription? Subscription { get; set; }
}

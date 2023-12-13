using Message_Broker.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var serverVersion = new MySqlServerVersion(new Version(8, 0, 27));

builder.Services.AddDbContext<MessageBrokerContext>(options => options.UseMySql("server=localhost; port=3306; database=message_broker; user=root; password=asdf1234@#;", serverVersion, mysqlOptions =>
{
    mysqlOptions.EnableRetryOnFailure();
}));

var app = builder.Build();

app.UseHttpsRedirection();


// Create Topic
app.MapPost("api/topics", async (MessageBrokerContext context, Topic topic) =>
{
    await context.Topics.AddAsync(topic);
    await context.SaveChangesAsync();

    return Results.Created($"api/topic/{topic.Id}", topic);
});

// Return all topics
app.MapGet("api/topics", async (MessageBrokerContext context) =>
{
    return Results.Ok(await context.Topics.ToListAsync());
});


// Publish message
app.MapPost("api/topics/{topicId}/messages", async (MessageBrokerContext context, int topicId, Message message) =>
{
    bool topics = await context.Topics.AnyAsync(x => x.Id == topicId);

    if (!topics)
        return Results.NotFound("Topic not found");

    var subscriptions = await context.Subscriptions.Where(x => x.TopicId == topicId).ToListAsync();

    if (subscriptions.Count() == 0)
        return Results.NotFound("There are no subscriptions for this topic");

    foreach(var sub in subscriptions)
    {
        Message msg = new()
        {
            TopicMessage = message.TopicMessage,
            SubscriptionId = sub.Id,
            ExpiresAfter = message.ExpiresAfter,
            MessageStatus = message.MessageStatus
        };

        await context.AddAsync(msg);
    }

    await context.SaveChangesAsync();

    return Results.Ok("Message has been Published");

});

//Create Subscription 
app.MapPost("api/topics/{topicId}/subscriptions", async (MessageBrokerContext context, int topicId, Subscription sub) =>
{
    bool topic = await context.Topics.AnyAsync(x => x.Id == topicId);

    if (!topic)
        Results.NotFound("Topic not present to be subscribed");

    sub.TopicId = topicId;

    await context.AddAsync(sub);
    await context.SaveChangesAsync();

    return Results.Created($"api/topics/{topicId}/subscriptions/{sub.Id}", sub);
});


// Get Subscriber Messages
app.MapGet("api/subscriptions/{subId}/messages", async (MessageBrokerContext context, int subId) =>
{
    bool subs = await context.Subscriptions.AnyAsync(x => x.Id == subId);
    if (!subs)
        return Results.NotFound("Subscription not found");

    var messages = await context.Messages.Where(x => x.SubscriptionId ==  subId).ToListAsync();

    if (messages.Count == 0)
        return Results.NotFound($"messages for Subscription Id: {subId} not found");

    foreach(var msg in messages)
    {
        msg.MessageStatus = "REQUESTED";
    }

    await context.SaveChangesAsync();

    return Results.Ok(messages);
});

//message for subscriber

app.MapPost("api/subscriptions/{subId}/messages", async (MessageBrokerContext context, int subId, int[] confs) =>
{
    bool subs = await context.Subscriptions.AnyAsync(x => x.Id == subId);

    if (!subs)
        return Results.NotFound("Subscriptions not found");

    if (confs.Length <= 0)
        return Results.BadRequest();

    int count = 0;

    foreach(int c in confs)
    {
        var msg = context.Messages.FirstOrDefault(x => x.Id == c);

        if(msg != null)
        {
            msg.MessageStatus = "SENT";
            await context.SaveChangesAsync();
            count++;
        }
    }

    return Results.Ok($"Acknowledeged {count}/{confs.Length} messages");


});
app.Run();

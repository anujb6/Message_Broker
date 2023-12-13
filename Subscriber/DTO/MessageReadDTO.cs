using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subscriber.DTO
{
    public class MessageReadDTO
    {
        public int Id { get; set; }
        public string? TopicMessage { get; set; }
        public DateTime ExpieresAfrter { get; set; }
        public string MessageStatus { get; set; }
    }
}

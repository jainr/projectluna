using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities.Luna.AI
{
    public class Publisher
    {
        public Publisher()
        {

        }

        public void Copy(Publisher publisher)
        {
        }


        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        public Guid PublisherId { get; set; }
        
        public string ControlPlaneUrl { get; set; }

        public string Name { get; set; }
    }
}

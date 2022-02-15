using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    class ServiceClient
    {
        public int Id { get; set; }
        
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }
    }
}

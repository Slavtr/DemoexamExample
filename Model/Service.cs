using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    class Service
    {
        public int Id { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// In roubles
        /// </summary>
        public int Cost { get; set; }
        /// <summary>
        /// In seconds
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// In %
        /// </summary>
        public int Discount { get; set; }
        public string MainImagePath { get; set; }
    }
}

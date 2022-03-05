using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserWorking
{
    public class UserAction
    {
        public string ActionId { get; set; }
        public string ElementId { get; set; }
        public string ActionName { get; set; }
        public string ActionDescription { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string ActionTime { get; set; }
        public string ActionRUTime { get; set; }
        public DateTime ActionDate { get; set; }
    }
}

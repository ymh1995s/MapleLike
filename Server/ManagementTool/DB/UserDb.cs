using System;

namespace ManagementTool.DB
{
    public class UserDb
    {
            public string ID { get; set; }
            public string PW { get; set; }
            public string Salt { get; set; }
            public DateTime Created { get; set; }
            public DateTime? LastLogin { get; set; }
        
    }
}

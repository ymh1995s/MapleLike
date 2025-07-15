using Microsoft.EntityFrameworkCore;
using ServerContents.Job;
using ServerContents.Object;
using ServerContents.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.DB
{
    class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();
    }
}

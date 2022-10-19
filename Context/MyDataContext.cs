using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Context.Entities;

namespace Context
{
    public class MyDataContext : DbContext
    {
        public MyDataContext()
        {
            this.Database.Migrate();
        }

        public DbSet<UserMessage> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;Integrated Security=True;Initial Catalog=TCPClientChat"); // строка підключення до БД 
        }
    }
}

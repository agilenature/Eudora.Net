#if OOGA
using Microsoft.EntityFrameworkCore;
using System.IO;


namespace Eudora.Net.Data
{
    internal class MailboxEfContext : DbContext
    {
        ///////////////////////////////////////////////////////////
        #region Fields

        private string _Name = string.Empty;
        private static readonly string RootDbPath = "Data/Mailboxes/";

        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties

        public DbSet<Mailbox> Mailbox { get; set; }
        public DbSet<EmailMessage> Messages { get; set; }
        
        public string Name
        {
            get => _Name; 
            set => _Name = value;
        }

        public string DbPath
        {
            get => Path.Combine(Properties.Settings.Default.DataStoreRoot, RootDbPath, $"{Name}.mbx");
        }

        #endregion Properties
        ///////////////////////////////////////////////////////////


        public MailboxEfContext(string name) : base()
        {
            Name = name;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
            // EF will load the mailbox's messages as needed; not up front or all at once.
            // This reduces memory usage and i/o time
            optionsBuilder.UseLazyLoadingProxies(true);
        }
    }
}

#endif
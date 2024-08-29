using Eudora.Net.Core;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Timers;

namespace Eudora.Net.Data
{
    internal class EudoraStatistics : INotifyPropertyChanged
    {
        internal class EmailStats : INotifyPropertyChanged
        {
            private Guid _Id = Guid.NewGuid();
            [SQLite.PrimaryKey]
            public Guid Id
            {
                get => _Id;
                set => SetField(ref _Id, value, nameof(Id));
            }

            private DateTime _Timestamp = DateTime.Now;
            public DateTime Timestamp
            {
                get => _Timestamp;
                set => SetField(ref _Timestamp, value, nameof(Timestamp));
            }

            private uint _EmailsInCount = 0;
            public uint EmailsInCount 
            { 
                get => _EmailsInCount; 
                set => SetField(ref _EmailsInCount, value, nameof(EmailsInCount)); 
            }

            private uint _AttachmentsInCount = 0;
            public uint AttachmentsInCount
            {
                get => _AttachmentsInCount;
                set => SetField(ref _AttachmentsInCount, value, nameof(AttachmentsInCount));
            }

            private uint _EmailsOutCount = 0;
            public uint EmailsOutCount
            {
                get => _EmailsOutCount;
                set => SetField(ref _EmailsOutCount, value, nameof(EmailsOutCount));
            }

            private uint _AttachmentsOutCount = 0;
            public uint AttachmentsOutCount
            {
                get => _AttachmentsOutCount;
                set => SetField(ref _AttachmentsOutCount, value, nameof(AttachmentsOutCount));
            }

            private uint _ReplyCount = 0;
            public uint ReplyCount
            {
                get => _ReplyCount;
                set => SetField(ref _ReplyCount, value, nameof(ReplyCount));
            }

            private uint _ForwardCount = 0;
            public uint ForwardCount
            {
                get => _ForwardCount;
                set => SetField(ref _ForwardCount, value, nameof(ForwardCount));
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            protected void SetField<TField>(ref TField field, TField value, string propertyName)
            {
                if (EqualityComparer<TField>.Default.Equals(field, value))
                {
                    return;
                }

                field = value;
                OnPropertyChanged(propertyName);
            }
        }

        internal class EudoraStats : INotifyPropertyChanged
        {
            private Guid _Id = Guid.NewGuid();
            [SQLite.PrimaryKey]
            public Guid Id
            {
                get => _Id;
                set => SetField(ref _Id, value, nameof(Id));
            }

            private DateTime _Timestamp = DateTime.Now;
            public DateTime Timestamp
            {
                get => _Timestamp;
                set => SetField(ref _Timestamp, value, nameof(Timestamp));
            }

            private uint _MinutesOverall = 0;
            public uint MinutesOverall
            {
                get => _MinutesOverall;
                set => SetField(ref _MinutesOverall, value, nameof(MinutesOverall));
            }

            private uint _MinutesReading = 0;
            public uint MinutesReading
            {
                get => _MinutesReading;
                set => SetField(ref _MinutesReading, value, nameof(MinutesReading));
            }

            private uint _MinutesWriting = 0;
            public uint MinutesWriting
            {
                get => _MinutesWriting;
                set => SetField(ref _MinutesWriting, value, nameof(MinutesWriting));
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            protected void SetField<TField>(ref TField field, TField value, string propertyName)
            {
                if (EqualityComparer<TField>.Default.Equals(field, value))
                {
                    return;
                }

                field = value;
                OnPropertyChanged(propertyName);
            }
        }


        ///////////////////////////////////////////////////////////
        #region Fields
        /////////////////////////////

        static System.Timers.Timer StatsTimer;

        static bool ReadingEmailActive = false;
        static bool WritingEmailActive = false;

        static DateTime SessionDate = DateTime.Now.Date;

        public event PropertyChangedEventHandler? PropertyChanged;

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public static DatastoreBase<EmailStats> Datastore_EmailStats { get; set; }
        public static DatastoreBase<EudoraStats> Datastore_EudoraStats { get; set; }

        static EmailStats WorkingStats_Email;
        static EudoraStats WorkingStats_Eudora;

        public enum eStat
        {
            EmailIn,
            AttachmentIn,
            EmailOut,
            AttachmentOut,
            Reply,
            Forward,
            MinutesOverall,
            MinutesReading,
            MinutesWriting
        }


        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////


        static EudoraStatistics()
        {
            Datastore_EmailStats = new("Data", "Statistics", "EmailStats");
            Datastore_EudoraStats = new("Data", "Statistics", "EudoraStats");

            StatsTimer = new()
            {
                Interval = 1000 * 60
            };
            StatsTimer.Elapsed += StatsTimer_Elapsed;
        }

        public static void Startup()
        {
            try
            {
                Datastore_EmailStats.Open();
                Datastore_EmailStats.Load();

                Datastore_EudoraStats.Open();
                Datastore_EudoraStats.Load();
            }
            catch(Exception ex)
            {
                Logger.Exception(ex);
            }
           
            NewSession();
            StatsTimer.Enabled = true;
            StatsTimer.Start();
        }

        public static void Shutdown()
        {
            try
            {
                Datastore_EmailStats.Close();
                Datastore_EudoraStats.Close();
            }
            catch(Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        // Get or update the current working entry (row) for each stats table
        static void NewSession()
        {
            SessionDate = DateTime.Now.Date;

            try
            {
                var row = Datastore_EmailStats.Data.Where(r => r.Timestamp.Date == DateTime.Now.Date).FirstOrDefault();
                if(row is null)
                {
                    row = new EmailStats();
                    Datastore_EmailStats.Add(row);
                }
                WorkingStats_Email = row;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            try
            {
                var row = Datastore_EudoraStats.Data.Where(r => r.Timestamp.Date == DateTime.Now.Date).FirstOrDefault();
                if (row is null)
                {
                    row = new EudoraStats();
                    Datastore_EudoraStats.Add(row);
                }
                WorkingStats_Eudora = row;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        ///////////////////////////////////////////////////////////
        #region Stimulus Interface
        /////////////////////////////

        public static void UserIsReadingEmail(bool active)
        {
            ReadingEmailActive = active;
        }

        public static void UserIsWritingEmail(bool active)
        {
            WritingEmailActive = active;
        }

        public static void IncrementCounter(eStat stat, uint increment = 1)
        {
            switch (stat)
            {
                case eStat.EmailIn:
                    WorkingStats_Email.EmailsInCount++;
                    break;
                case eStat.AttachmentIn:
                    WorkingStats_Email.AttachmentsInCount++;
                    break;
                case eStat.EmailOut:
                        WorkingStats_Email.EmailsOutCount++;
                    break;
                case eStat.AttachmentOut:
                    WorkingStats_Email.AttachmentsOutCount++;
                    break;

                case eStat.MinutesOverall:
                    WorkingStats_Eudora.MinutesOverall++;
                    break;
                case eStat.MinutesReading:
                    WorkingStats_Eudora.MinutesReading++;
                    break;
                case eStat.MinutesWriting:
                    WorkingStats_Eudora.MinutesWriting++;
                    break;
            }
        }

        /////////////////////////////
        #endregion Stimulus Interface
        ///////////////////////////////////////////////////////////

        private static void StatsTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // First, we check the current system calendar date. If the date has changed
            // since the session began, a new session begins. This in turn creates new
            // rows for the new date.

            DateTime today = DateTime.Now.Date;
            if (today.Date > SessionDate.Date)
            {
                NewSession();
            }

            IncrementCounter(eStat.MinutesOverall);

            if (ReadingEmailActive)
            {
                IncrementCounter(eStat.MinutesReading);
            }
            if (WritingEmailActive)
            {
                IncrementCounter(eStat.MinutesWriting);
            }
        }
    }
}

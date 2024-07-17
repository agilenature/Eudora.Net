using System.Data;
using System.IO;
using System.Timers;

namespace Eudora.Net.Core
{
    internal static class EudoraStatistics
    {
        ///////////////////////////////////////////////////////////
        #region Fields
        /////////////////////////////

        static DataTable _EmailTable;
        static readonly string EmailTableFilename = @"EmailTable.xml";
        static readonly string EmailTableSchemaFilename = @"EmailTableSchema.xml";
        static readonly string EmailTablePath;
        static readonly string EmailTableSchemaPath;
        static DataRow? WorkingEmailRow;

        static DataTable _EudoraTable;
        static readonly string EudoraTableFilename = @"EudoraTable.xml";
        static readonly string EudoraTableSchemaFilename = @"EudoraTableSchema.xml";
        static readonly string EudoraTablePath;
        static readonly string EudoraTableSchemaPath;
        static DataRow? WorkingEudoraRow;

        static object EmailTableLocker;
        static object EudoraTableLocker;
        
        static readonly string DataRoot;

        static System.Timers.Timer StatsTimer;

        static bool ReadingEmailActive = false;
        static bool WritingEmailActive = false;
        
        static DateTime SessionDate = DateTime.Now.Date;

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////
        
        public enum eRowIndex
        {
            Mail_NewMessageIn = 1,
            Mail_NewAttachmentIn = 2,
            Mail_NewMessageOut = 3,
            Mail_NewAttachmentOut = 4,
            Mail_NewReply = 5,
            Mail_NewForward = 6,
            Minutes_Overall = 1,
            Minutes_Reading = 2,
            Minutes_Writing = 3
        }

        public static DataTable EmailTable
        {
            get => _EmailTable;
        }

        public static DataTable EudoraTable
        {
            get => _EudoraTable;
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////


        static EudoraStatistics()
        {
            DataRoot = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, @"Data\Statistics");
            IoUtil.EnsureFolder(DataRoot);
            
            EmailTablePath = Path.Combine(DataRoot, EmailTableFilename);
            EmailTableSchemaPath = Path.Combine(DataRoot, EmailTableSchemaFilename);

            EudoraTablePath = Path.Combine(DataRoot, EudoraTableFilename);
            EudoraTableSchemaPath = Path.Combine(DataRoot, EudoraTableSchemaFilename);

            _EmailTable = new();
            EmailTableLocker = new object();
            
            _EudoraTable = new();
            EudoraTableLocker = new object();

            StatsTimer = new();
            StatsTimer.Interval = 1000 * 60;
            StatsTimer.Elapsed += StatsTimer_Elapsed;
        }

        public static void Startup()
        {
            LoadEmailStatistics();
            LoadEudoraStatistics();

            NewSession();
            StatsTimer.Enabled = true;
            StatsTimer.Start();
        }

        public static void Shutdown()
        {
            EmailTable.AcceptChanges();
            SaveEmailStatistics();

            EudoraTable.AcceptChanges();
            SaveEudoraStatistics();
        }

        static void NewSession()
        {
            SessionDate = DateTime.Now.Date;

            try
            {
                WorkingEmailRow = GetWorkingRow(EmailTable);
                WorkingEudoraRow = GetWorkingRow(EudoraTable);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
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

        public static void IncrementCounter(eRowIndex rowIndex, uint increment = 1)
        {
            DataRow? row = WorkingEmailRow;
            switch(rowIndex)
            {
                case eRowIndex.Minutes_Overall:
                case eRowIndex.Minutes_Reading:
                case eRowIndex.Minutes_Writing:
                    row = WorkingEudoraRow;
                    break;
            }
            if(row is not null) IncrementUintColumn(row, rowIndex, increment);
        }

        /////////////////////////////
        #endregion Stimulus Interface
        ///////////////////////////////////////////////////////////


        private static void IncrementUintColumn(DataRow row, eRowIndex rowIndex, uint increment = 1)
        {
            try
            {
                int index = (int)Convert.ChangeType(rowIndex, rowIndex.GetTypeCode());

                var column = row[index];
                if(column.GetType() != typeof(uint))
                {
                    throw new ArgumentException($"Column {index} is not of type uint!");
                }
                
                uint t = (uint)row[index];
                t++;
                row[index] = t;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }            
        }

        private static void StatsTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // First, we check the current system calendar date. If the date has changed
            // since the session began, a new session begins. This in turn creates new
            // rows for the new date.

            DateTime today = DateTime.Now.Date;
            if(today.Date > SessionDate.Date)
            {
                NewSession();
            }

            // First, we increment the per-minute counters that are active

            if (WorkingEudoraRow is not null)
            {
                // Total uptime
                IncrementUintColumn(WorkingEudoraRow, eRowIndex.Minutes_Overall);

                if(ReadingEmailActive)
                {
                    IncrementUintColumn(WorkingEudoraRow, eRowIndex.Minutes_Reading);
                }
                if (WritingEmailActive)
                {
                    IncrementUintColumn(WorkingEudoraRow, eRowIndex.Minutes_Writing);
                }
            }
        }

        

        static DataRow? GetWorkingRow(DataTable table)
        {
            try
            {
                DateTime today = DateTime.Now.Date;
                foreach (DataRow r in table.Rows)
                {
                    if (((DateTime)r[0]).Date.Equals(today))
                    {
                        return r;
                    }
                }

                DataRow row = table.NewRow();
                row[0] = today;
                table.Rows.Add(row);
                return row;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return null;
        }

        static void CreateEmailTable()
        {
            try
            {
                EmailTable.TableName = "Email Statistics";

                DataColumn colDate = new()
                {
                    ColumnName = "Timestamp",
                    Caption = "Day",
                    DataType = typeof(DateTime),
                    DefaultValue = DateTime.Now.Date
                };

                DataColumn colEmailIn = new()
                {
                    ColumnName = "EmailsInCount",
                    Caption = "Emails Received",
                    DataType = typeof(UInt32),
                    DefaultValue = UInt32.MinValue
                };

                DataColumn colAttachmemtsIn = new()
                {
                    ColumnName = "AttachmentsInCount",
                    Caption = "Attachments Received",
                    DataType = typeof(UInt32),
                    DefaultValue = UInt32.MinValue
                };

                DataColumn colEmailOut = new()
                {
                    ColumnName = "EmailsOutCount",
                    Caption = "Emails Sent",
                    DataType = typeof(UInt32),
                    DefaultValue = UInt32.MinValue
                };

                DataColumn colAttachmentsOut = new()
                {
                    ColumnName = "AttachmentsOutCount",
                    Caption = "Attachments Sent",
                    DataType = typeof(UInt32),
                    DefaultValue = UInt32.MinValue
                };

                DataColumn colEmailReplies = new()
                {
                    ColumnName = "ReplyCount",
                    Caption = "Replies Sent",
                    DataType = typeof(UInt32),
                    DefaultValue = UInt32.MinValue
                };

                DataColumn colEmailFwd = new()
                {
                    ColumnName = "ForwardCount",
                    Caption = "Forwards Sent",
                    DataType = typeof(UInt32),
                    DefaultValue = UInt32.MinValue
                };

                EmailTable.Columns.Add(colDate);
                EmailTable.Columns.Add(colEmailIn);
                EmailTable.Columns.Add(colAttachmemtsIn);
                EmailTable.Columns.Add(colEmailOut);
                EmailTable.Columns.Add(colAttachmentsOut);
                EmailTable.Columns.Add(colEmailReplies);
                EmailTable.Columns.Add(colEmailFwd);
                EmailTable.PrimaryKey = [colDate];
                EmailTable.AcceptChanges();

                using (var stream1 = File.CreateText(EmailTableSchemaPath))
                {
                    EmailTable.WriteXmlSchema(stream1);
                }

                using (var stream2 = File.CreateText(EmailTablePath))
                {
                    EmailTable.WriteXml(stream2);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        static void LoadEmailStatistics()
        {
            try
            {
                lock (EmailTableLocker)
                {
                    if (!File.Exists(EmailTablePath) || !File.Exists(EmailTableSchemaPath))
                    {
                        CreateEmailTable();
                        return;
                    }

                    EmailTable.ReadXmlSchema(EmailTableSchemaPath);
                    EmailTable.ReadXml(EmailTablePath);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        static void SaveEmailStatistics()
        {
            try
            {
                lock (EmailTableLocker)
                {
                    EmailTable.AcceptChanges();
                    using (var stream = File.CreateText(EmailTablePath))
                    {
                        EmailTable.WriteXml(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        static void CreateEudoraTable()
        {
            try
            {
                EudoraTable.TableName = "Eudora Statistics";

                DataColumn colDate = new()
                {
                    ColumnName = "Timestamp",
                    Caption = "Date",
                    DataType = typeof(DateTime),
                    DefaultValue = DateTime.Now.Date
                };

                DataColumn colMinutesOverall = new()
                {
                    ColumnName = "MinutesOverall",
                    Caption = "Uptime",
                    DataType = typeof(UInt32),
                    DefaultValue = UInt32.MinValue
                };

                DataColumn colMinutesReading = new()
                {
                    ColumnName = "MinutesReading",
                    Caption = "Minutes Reading Email",
                    DataType = typeof(UInt32),
                    DefaultValue = UInt32.MinValue
                };

                DataColumn colMinutesWriting = new()
                {
                    ColumnName = "MinutesWriting",
                    Caption = "Minutes Authoring Email",
                    DataType = typeof(UInt32),
                    DefaultValue = UInt32.MinValue
                };

                EudoraTable.Columns.Add(colDate);
                EudoraTable.Columns.Add(colMinutesOverall);
                EudoraTable.Columns.Add(colMinutesReading);
                EudoraTable.Columns.Add(colMinutesWriting);
                EudoraTable.PrimaryKey = [colDate];
                EudoraTable.AcceptChanges();

                using(var stream1 = File.CreateText(EudoraTableSchemaPath))
                {
                    EudoraTable.WriteXmlSchema(stream1);
                }

                using (var stream2 = File.CreateText(EudoraTablePath))
                {
                    EudoraTable.WriteXml(stream2);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        static void LoadEudoraStatistics()
        {
            try
            {
                lock (EudoraTableLocker)
                {
                    if(!File.Exists(EudoraTablePath) || !File.Exists(EudoraTableSchemaPath))
                    {
                        CreateEudoraTable();
                        return;
                    }

                    EudoraTable.ReadXmlSchema(EudoraTableSchemaPath);
                    EudoraTable.ReadXml(EudoraTablePath);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        static void SaveEudoraStatistics()
        {
            try
            {
                lock(EudoraTableLocker)
                {
                    EudoraTable.AcceptChanges();
                    using (var stream = File.CreateText(EudoraTablePath))
                    {
                        EudoraTable.WriteXml(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

    }
}

﻿using System.Windows;
using System.Data;
using Eudora.Net.Core;
using Eudora.Net.Data;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_EudoraStatisticsView.xaml
    /// </summary>
    public partial class uc_EudoraStatisticsView : ChildWindowBase
    {
        ///////////////////////////////////////////////////////////
        #region Dependency Properties
        /////////////////////////////

        // Query / Report parameters
        private static readonly DependencyProperty StatsPeriodProperty =
            DependencyProperty.Register("StatsPeriod", typeof(eStatPeriod), typeof(uc_EudoraStatisticsView), 
                new PropertyMetadata(eStatPeriod.Day, OnStatPeriodChanged));

        public static readonly DependencyProperty StatsBeginProperty =
            DependencyProperty.Register("StatsBegin", typeof(DateTime), typeof(uc_EudoraStatisticsView),
                new PropertyMetadata(DateTime.Now.Date));

        public static readonly DependencyProperty StatsEndProperty =
            DependencyProperty.Register("StatsEnd", typeof(DateTime), typeof(uc_EudoraStatisticsView),
                new PropertyMetadata(DateTime.Now.Date));

        private eStatPeriod StatsPeriod
        {
            get { return (eStatPeriod)GetValue(StatsPeriodProperty); }
            set { SetValue(StatsPeriodProperty, value); }
        }

        private DateTime StatsBegin
        {
            get { return (DateTime)GetValue(StatsBeginProperty); }
            set { SetValue(StatsBeginProperty, value); }
        }

        private DateTime StatsEnd
        {
            get { return (DateTime)GetValue(StatsEndProperty); }
            set { SetValue(StatsEndProperty, value); }
        }


        // Email Table
        public static readonly DependencyProperty EmailsInProperty =
            DependencyProperty.Register("EmailsIn", typeof(UInt32), typeof(uc_EudoraStatisticsView), new PropertyMetadata(UInt32.MinValue));

        public static readonly DependencyProperty AttachmentsInProperty =
            DependencyProperty.Register("AttachmentsIn", typeof(UInt32), typeof(uc_EudoraStatisticsView), new PropertyMetadata(UInt32.MinValue));

        public static readonly DependencyProperty EmailsOutProperty =
            DependencyProperty.Register("EmailsOut", typeof(UInt32), typeof(uc_EudoraStatisticsView), new PropertyMetadata(UInt32.MinValue));

        public static readonly DependencyProperty AttachmentsOutProperty =
            DependencyProperty.Register("AttachmentsOut", typeof(UInt32), typeof(uc_EudoraStatisticsView), new PropertyMetadata(UInt32.MinValue));

        public static readonly DependencyProperty RepliesProperty =
            DependencyProperty.Register("Replies", typeof(UInt32), typeof(uc_EudoraStatisticsView), new PropertyMetadata(UInt32.MinValue));

        public static readonly DependencyProperty ForwardsProperty =
            DependencyProperty.Register("Forwards", typeof(UInt32), typeof(uc_EudoraStatisticsView), new PropertyMetadata(UInt32.MinValue));


        public UInt32 EmailsIn
        {
            get { return (UInt32)GetValue(EmailsInProperty); }
            set { SetValue(EmailsInProperty, value); }
        }

        public UInt32 AttachmentsIn
        {
            get { return (UInt32)GetValue(AttachmentsInProperty); }
            set { SetValue(AttachmentsInProperty, value); }
        }

        public UInt32 EmailsOut
        {
            get { return (UInt32)GetValue(EmailsOutProperty); }
            set { SetValue(EmailsOutProperty, value); }
        }

        public UInt32 AttachmentsOut
        {
            get { return (UInt32)GetValue(AttachmentsOutProperty); }
            set { SetValue(AttachmentsOutProperty, value); }
        }

        public UInt32 Replies
        {
            get { return (UInt32)GetValue(RepliesProperty); }
            set { SetValue(RepliesProperty, value); }
        }

        public UInt32 Forwards
        {
            get { return (UInt32)GetValue(ForwardsProperty); }
            set { SetValue(ForwardsProperty, value); }
        }


        // Eudora Table
        public static readonly DependencyProperty MinutesUsageProperty =
            DependencyProperty.Register("MinutesUsage", typeof(UInt32), typeof(uc_EudoraStatisticsView), new PropertyMetadata(UInt32.MinValue));

        public static readonly DependencyProperty MinutesReadingProperty =
            DependencyProperty.Register("MinutesReading", typeof(UInt32), typeof(uc_EudoraStatisticsView), new PropertyMetadata(UInt32.MinValue));

        public static readonly DependencyProperty MinutesWritingProperty =
            DependencyProperty.Register("MinutesWriting", typeof(UInt32), typeof(uc_EudoraStatisticsView), new PropertyMetadata(UInt32.MinValue));

        public UInt32 MinutesUsage
        {
            get { return (UInt32)GetValue(MinutesUsageProperty); }
            set { SetValue(MinutesUsageProperty, value); }
        }

        public UInt32 MinutesReading
        {
            get { return (UInt32)GetValue(MinutesReadingProperty); }
            set { SetValue(MinutesReadingProperty, value); }
        }

        public UInt32 MinutesWriting
        {
            get { return (UInt32)GetValue(MinutesWritingProperty); }
            set { SetValue(MinutesWritingProperty, value); }
        }

        /////////////////////////////
        #endregion Dependency Properties
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        private enum eStatPeriod
        {
            Day,
            Week,
            Month,
            Year
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public uc_EudoraStatisticsView()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += Uc_EudoraStatisticsView_Loaded;
        }

        private void Uc_EudoraStatisticsView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            cb_Period.ItemsSource = Enum.GetValues(typeof(eStatPeriod));
            cb_Period.SelectedIndex = 0;
            dg_Email.DataContext = EudoraStatistics.Datastore_EmailStats.Data;
            dg_Eudora.DataContext = EudoraStatistics.Datastore_EudoraStats.Data;
        }

        private static void OnStatPeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is uc_EudoraStatisticsView me)
            {
                me.RefreshQuery();
            }
        }

        private void WipeStats()
        {
            EmailsIn = 0;
            AttachmentsIn = 0;
            EmailsOut = 0;
            AttachmentsOut = 0;
            Replies = 0;
            Forwards = 0;
            MinutesUsage = 0;
            MinutesReading = 0;
            MinutesWriting = 0;
        }

        private void RefreshQuery()
        {
            // This part is clean and clear

            WipeStats();

            StatsEnd = DateTime.Now.Date;
            StatsBegin = StatsEnd; // This is just a temporary assignment, not a metaphysical premise

            try
            {

                switch (StatsPeriod)
                {
                    case eStatPeriod.Day:
                        StatsBegin = StatsEnd.AddDays(-1).Date;
                        break;
                    case eStatPeriod.Week:
                        StatsBegin = StatsEnd.AddDays(-7).Date;
                        break;
                    case eStatPeriod.Month:
                        StatsBegin = StatsEnd.AddMonths(-1).Date;
                        break;
                    case eStatPeriod.Year:
                        StatsBegin = StatsEnd.AddYears(-1).Date;
                        break;
                }

                var emailRows = EudoraStatistics.Datastore_EmailStats.Data.Where(i =>
                i.Timestamp.Date >= StatsBegin && i.Timestamp.Date <= StatsEnd).ToList();

                var usageRows = EudoraStatistics.Datastore_EudoraStats.Data.Where(i =>
                i.Timestamp.Date >= StatsBegin && i.Timestamp.Date <= StatsEnd).ToList();

                foreach (var emailRow in emailRows)
                {
                    EmailsIn += emailRow.EmailsInCount;
                    AttachmentsIn += emailRow.AttachmentsInCount;
                    EmailsOut += emailRow.EmailsOutCount;
                    AttachmentsOut += emailRow.AttachmentsOutCount;
                    Replies += emailRow.ReplyCount;
                    Forwards += emailRow.ForwardCount;
                }

                foreach (var usageRow in usageRows)
                {
                    MinutesUsage += usageRow.MinutesOverall;
                    MinutesReading += usageRow.MinutesReading;
                    MinutesWriting += usageRow.MinutesWriting;
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }
    }
}

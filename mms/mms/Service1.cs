using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;



namespace mms
{
    public partial class Service1 : ServiceBase
    {
        string strLogPath = ConfigurationSettings.AppSettings["LogPath"];
        string strLog = "";
        double dbDailyAddDate = Convert.ToDouble(ConfigurationSettings.AppSettings["RSSSSubscribers"].ToString());
        Timer _timer = new Timer();
        public string strQry = "";
        private int i;

        string[] str1 = new string[12] { "ஜனவரி", "பிப்ரவரி", "மார்ச்", "ஏப்ரல்", "மே", "ஜூன்", "ஜூலை", "ஆகஸ்ட்", "செப்டம்பர்", "அக்டோபர்", "நவம்பர்", "டிசம்பர்" };
        private int j;

        public char Message { get; private set; }

        public Service1()
        {
            InitializeComponent();
            _timer.Interval = Convert.ToDouble(ConfigurationSettings.AppSettings["TimerInterval"].ToString());

            //enabling the timer
            _timer.Enabled = true;

            //handle Elapsed event
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
           

        }



        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            try
            {
                string TimerDuration = ConfigurationSettings.AppSettings["TimerDuration"].ToString();
                string strCurrnetTime = System.DateTime.Now.Hour.ToString() + ":" + System.DateTime.Now.Minute.ToString();


                if (TimerDuration.Split(',').Contains(strCurrnetTime))
                {
                    strLog = DateTime.Now.ToString("hh:mm:ss tt") + "Hittttttted Time taken service " + strCurrnetTime;
                    if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_DailtPageWise_LOG.txt"))
                        using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_DailtPageWise_LOG.txt"))
                            objWritter.WriteLine(strLog);
                    else
                        using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_DailtPageWise_LOG.txt"))
                            objWritter.WriteLine(strLog);
                    HMSendSMSnumber();
                    KMSendSMSnumber();
                    RSSSSubscribers();
                    RSSSSubscribers5days();
                    Last13Days();
                    Dailybasisreceipt();
                    Rss1times();
                    Rss3times();
                    Rss2times();
                    HinduAgencies();
                    MonthFirstKamadhenu();
                }
                else
                {
                    strLog = DateTime.Now.ToString("hh:mm:ss tt") + " Time not Hitting service " + strCurrnetTime;
                    if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_DailtPageWise_LOG.txt"))
                        using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_DailtPageWise_LOG.txt"))
                            objWritter.WriteLine(strLog);
                    else
                        using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_DailtPageWise_LOG.txt"))
                            objWritter.WriteLine(strLog);
                }
            }
            catch (Exception ex1)
            {

            }
            finally
            {
                _timer.Start();
            }
        }

        
        //// Hindu Tamil News agents Every month last 5 days continuously

        private void HMSendSMSnumber()
        {
            int sendsms = Convert.ToInt32(ConfigurationSettings.AppSettings["dateAddDaily"]);

            var firstDayOfMonth = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var SendSMSnumber = lastDayOfMonth.AddDays(sendsms);
            if (SendSMSnumber.Date <= DateTime.Now.Date)
            {
                strQry = "select  Agent_Id,RATpay,(select Per_Mobile from m_agent x where x.agent_id= y.agent_id and Per_Mobile!=' ' ) as Per_Mobile from Print_Order_Bills y where Status='BA'and month(convert(date,Bill_Date))=month(getdate()) and year(convert(date,Bill_Date))=year(getdate()) and RATpay>0 and Pub_Id='TM' order by Bill_Date";

                DataTable data = new DataTable();
                data = ReturndatatableSQL(strQry);
                try
                {
                    SMSUnicode.RouteSMSUnicode objuni = new SMSUnicode.RouteSMSUnicode();

                    for (i = 0; i < data.Rows.Count; i++)
                    {
                        string strAmount = "0";
                        if (data.Rows[i]["RATpay"].ToString() != "")
                            strAmount = Math.Round(Convert.ToDouble(data.Rows[i]["RATpay"].ToString())).ToString();

                        string strMobileno = "0";
                        if (data.Rows[i]["Per_Mobile"].ToString() != "")
                            strMobileno = Math.Round(Convert.ToDouble(data.Rows[i]["Per_Mobile"].ToString())).ToString();

                        string strMsg = "அன்புடையீர் ! தங்களின் 'இந்து தமிழ் திசை' " + str1[DateTime.Now.Date.Month - 1] + " " + DateTime.Now.Date.Year.ToString() + " மாத பில் பாக்கி தொகை ₹." + strAmount + "- ஐ உடனடியாக செலுத்தி கணக்கினை நேர்செய்து வட்டித்தொகை பிடித்தத்தை தவிர்த்துக் கொள்ளுமாறு கேட்டுகொள்கிறோம் –நன்றி";
                        string strMsgmobile = objuni.ConvertToUnicode(strMsg);
                        bool resSMSgateway = false;
                        resSMSgateway = SendSMSGateway(strMobileno, strMsgmobile);
                        if (resSMSgateway == true)
                        {
                            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HMSendSMSnumber_Logs.txt"))
                                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HMSendSMSnumber_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent id" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno + " Mesaages send successfully ");
                            else
                                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HMSendSMSnumber_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent id" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages send successfully ");
                        }
                        else
                        {
                            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HMSendSMSnumber_Logs.txt"))
                                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HMSendSMSnumber_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent id" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                            else
                                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HMSendSMSnumber_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent id" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                        }
                    }
                }

              catch (Exception ex2) { }

            }
        }

        //Kamadanu every month last 5 day bill continusilly

        private void KMSendSMSnumber()
        {
            int sendsms = Convert.ToInt32(ConfigurationSettings.AppSettings["kam"]);

            var firstDayOfMonth = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var SendSMSnumber = lastDayOfMonth.AddDays(sendsms);
            if (SendSMSnumber.Date <= DateTime.Now.Date)
            {
                strQry = "select  Agent_Id,RATpay,(select Per_Mobile from m_agent x where x.agent_id= y.agent_id and Per_Mobile!=' ' ) as Per_Mobile from Print_Order_Bills y where Status='BA'and month(convert(date,Bill_Date))=month(getdate()) and year(convert(date,Bill_Date))=year(getdate()) and RATpay>0 and Pub_Id='KM' order by Bill_Date";

                DataTable data = new DataTable();
                data = ReturndatatableSQL(strQry);
                try
                {
                    SMSUnicode.RouteSMSUnicode objuni = new SMSUnicode.RouteSMSUnicode();

                    for (i = 0; i < data.Rows.Count; i++)
                    {
                        string strAmount = "0";
                        if (data.Rows[i]["RATpay"].ToString() != "")
                            strAmount = Math.Round(Convert.ToDouble(data.Rows[i]["RATpay"].ToString())).ToString();

                        string strMobileno = "0";
                        if (data.Rows[i]["Per_Mobile"].ToString() != "")
                            strMobileno = Math.Round(Convert.ToDouble(data.Rows[i]["Per_Mobile"].ToString())).ToString();
                        string strMsg = "அன்புடையீர் ! தங்களின் 'காமதேனு' " + str1[DateTime.Now.Date.Month - 1] + " " + DateTime.Now.Date.Year.ToString() + " மாத பில் பாக்கி தொகை ₹." + strAmount + "- ஐ உடனடியாக செலுத்தி கணக்கினை நேர்செய்து வட்டித்தொகை பிடித்தத்தை தவிர்த்துக் கொள்ளுமாறு கேட்டுகொள்கிறோம் –நன்றி";
                        string strMsgmobile = objuni.ConvertToUnicode(strMsg);
                        bool resSMSgateway = false;
                        resSMSgateway = SendSMSGateway(strMobileno, strMsgmobile);
                        if (resSMSgateway == true)
                        {
                            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_KMSendSMSnumber_Logs.txt"))
                                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_KMSendSMSnumber_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Name" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno + " Mesaages send successfully ");
                            else
                                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_KMSendSMSnumber_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Name" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages send successfully ");
                        }
                        else
                        {
                            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_KMSendSMSnumber_Logs.txt"))
                                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_KMSendSMSnumber_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Name" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                            else
                                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_KMSendSMSnumber_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Name" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                        }
                    }
                }
                catch (Exception ex2) { }

            }
        }

        //// RSSS Subscribers -- about expire 5 dys before
        private void RSSSSubscribers5days()
        {
            strQry = "select Subscribercode,format(cast(Temp_Stop_Effective_To as date), 'dd/MM/yyyy') as Temp_Stop_Effective_To,   convert(date, Area_Agent_Effective_To) as Area_Agent_Effective_To,(select top 1 x.MobileNo  from m_rsss x where x.Subscribercode=y.Subscribercode  ) as MobileNo from rsss_trans y    ";
            strQry += " where convert(date, GETDATE()+5) = convert(date, Area_Agent_Effective_To) and convert(date, GETDATE()+5)= convert(date, Temp_Stop_Effective_To ) and  Edition_id!='SM' and  Edition_id!='cb' and  Edition_id!='TI' ";
            DataTable data = new DataTable();
            data = ReturndatatableSQL(strQry);
            try
            {
                SMSUnicode.RouteSMSUnicode objuni = new SMSUnicode.RouteSMSUnicode();

                for (i = 0; i < data.Rows.Count; i++)
                {
                    string strMobileno = "0";
                    if (data.Rows[i]["mobile"].ToString() != "")
                        strMobileno = Math.Round(Convert.ToDouble(data.Rows[i]["mobile"].ToString())).ToString();

                    string strMsg = "அன்புடையீர் ! இந்து தமிழ் திசையை சிறப்பு சந்தா மூலம் புதுப்பிக்க 5 நாட்களே உள்ளன. புதுப்பித்தவர்களுக்கு மிக்க நன்றி... புதுப்பிக்காதவர்கள் இணைந்திருக்க  https://bit.ly/2uxHogk";
                    string strMsgmobile = objuni.ConvertToUnicode(strMsg);
                    bool resSMSgateway = false;
                    resSMSgateway = SendSMSGateway(strMobileno, strMsgmobile);
                    if (resSMSgateway == true)
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribers5days_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribers5days_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Subscribercode" + data.Rows[i]["Subscribercode"].ToString() + " " + "  To Mobile Number: " + strMobileno + " Mesaages send successfully ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribers5days_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Subscribercode" + data.Rows[i]["Subscribercode"].ToString() + " " + "  To Mobile Number: " + strMobileno + "  Mesaages send successfully ");
                    }
                    else
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribers5days_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribers5days_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Subscribercode" + data.Rows[i]["Subscribercode"].ToString() + " " + " To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribers5days_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Subscribercode" + data.Rows[i]["Subscribercode"].ToString() + " " + " To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                    }
                }

            }
            catch (Exception ex2) { }

        }

        //// RSSS Subscribers -- about expire 3 dys before
        private void RSSSSubscribers()
        {
            strQry = "select Subscribercode,format(cast(Temp_Stop_Effective_To as date), 'dd/MM/yyyy') as Temp_Stop_Effective_To,   convert(date, Area_Agent_Effective_To) as Area_Agent_Effective_To,(select top 1 x.MobileNo  from m_rsss x where x.Subscribercode=y.Subscribercode  ) as MobileNo from rsss_trans y    ";
            strQry += " where convert(date, GETDATE()+3) = convert(date, Area_Agent_Effective_To) and convert(date, GETDATE()+3)= convert(date, Temp_Stop_Effective_To ) and  Edition_id!='SM' and  Edition_id!='CB' and  Edition_id!='TU' ";
            DataTable data = new DataTable();
            data = ReturndatatableSQL(strQry);
            try
            {
                SMSUnicode.RouteSMSUnicode objuni = new SMSUnicode.RouteSMSUnicode();

                for (i = 0; i < data.Rows.Count; i++)
                {
                    string strMobileno = "0";
                    if (data.Rows[i]["MobileNo"].ToString() != "")
                        strMobileno = Math.Round(Convert.ToDouble(data.Rows[i]["MobileNo"].ToString())).ToString();

                    string strMsg = "அன்புடையீர் ! இந்து தமிழ் திசையை சிறப்பு சந்தா மூலம் புதுப்பிக்க 3 நாட்களே உள்ளன. புதுப்பித்தவர்களுக்கு மிக்க நன்றி... புதுப்பிக்காதவர்கள் இணைந்திருக்க  https://bit.ly/2uxHogk";
                    string strMsgmobile = objuni.ConvertToUnicode(strMsg);
                    bool resSMSgateway = false;
                    resSMSgateway = SendSMSGateway(strMobileno, strMsgmobile);
                    if (resSMSgateway == true)
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribers_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribers_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Subscribercode" + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + " Mesaages send successfully ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribers_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Subscribercode" + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages send successfully ");
                    }
                    else
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribersFail_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribersFail_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Subscribercode" + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_RSSSSubscribersFail_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Subscribercode" + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                    }
                }
            }

            catch (Exception ex2) { }

        }

        //// RSSS Subscribers -- about expire 13 dys before
       private void Last13Days()
        {
            int sendsms = Convert.ToInt32(ConfigurationSettings.AppSettings["last13days"]);
            var firstDayOfMonth = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, sendsms);
             
            if (firstDayOfMonth.Date == DateTime.Now.Date)

            {
                strQry = "select  Agent_Id,RATpay,(select Per_Mobile from m_agent x where x.agent_id= y.agent_id ) as Per_Mobile from Print_Order_Bills y where Status='BA' and  month(convert(date,Bill_Date))=month(getdate()) and RATpay>0 order by Bill_Date";

                DataTable data = new DataTable();
                data = ReturndatatableSQL(strQry);

                try
                {
                    SMSUnicode.RouteSMSUnicode objuni = new SMSUnicode.RouteSMSUnicode();
                    for (i = 0; i < data.Rows.Count; i++)
                    {

                        string strAmount = "0";
                        string strMobileno = "0";
                        if (data.Rows[i]["Per_Mobile"].ToString() != "")
                            strMobileno = Math.Round(Convert.ToDouble(data.Rows[i]["Per_Mobile"].ToString())).ToString();
                        if (data.Rows[i]["RATpay"].ToString() != "")
                            strAmount = Math.Round(Convert.ToDouble(data.Rows[i]["RATpay"].ToString())).ToString();
                        string strMsg = "அன்புடையீர் ! 'இந்து தமிழ் திசை' " + str1[DateTime.Now.Date.Month - 1] + " " + DateTime.Now.Date.Year.ToString() + " மாத பில் பாக்கி தொகை ₹." + strAmount + "/-ஐ ." + DateTime.Now.Date.Month + "." + DateTime.Now.Date.Year + " பிற்பகல் 5 மணிக்குள் செலுத்தி  ஊக்கத் தொகையை தக்கவைத்துக் கொள்ளும்படி அன்புடன் கேட்டுகொள்கிறோம்–நன்றி";

                        string strMsgmobile = objuni.ConvertToUnicode(strMsg);

                        SendSMSGateway(strMobileno.ToString(), strMsgmobile);

                        bool resSMSgateway = false;
                        resSMSgateway = SendSMSGateway(strMobileno.ToString(), strMsgmobile);
                        if (resSMSgateway == true)
                        {
                            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Last13Days_log.txt"))
                                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Last13Days_log.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno.ToString() + " Mesaages send successfully ");
                            else
                                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Last13Days_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno.ToString() + "  Mesaages send successfully ");
                        }
                        else
                        {
                            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Last13DaysFail_Logs.txt"))
                                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Last13DaysFail_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno.ToString() + "  Mesaages sending Faild ");
                            else
                                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Last13DaysFail_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["Agent_Id"].ToString() + "  To Mobile Number: " + strMobileno.ToString() + "  Mesaages sending Faild ");
                        }
                    }
                }

                catch (Exception ex) { }
            }
        }

        private void Dailybasisreceipt()
        {

            strQry = "select  agent_id,sum(Amount) as Amount,(select Per_Mobile from m_agent x where x.agent_id= y.agent_id ) as Per_Mobile from print_order_bills_payment y where Status = 'A' and type not in('SECURITY ADVANCE,SECURITY DEPOSIT') and Amount > 0 and convert(date, Receipt_Date)= convert(date, getdate()) group by Receipt_No,agent_id";
            DataTable data = new DataTable();
            data = ReturndatatableSQL(strQry);
            try
            {
                SMSUnicode.RouteSMSUnicode objuni = new SMSUnicode.RouteSMSUnicode();

                for (i = 0; i <= data.Rows.Count; i++)
                {

                    string strMobileno = "0";
                    if (data.Rows[i]["Per_Mobile"].ToString() != "")
                        strMobileno = Math.Round(Convert.ToDouble(data.Rows[i]["Per_Mobile"].ToString())).ToString();
                    string strAmount = "0";
                    if (data.Rows[i]["Amount"].ToString() != "")
                        strAmount = Math.Round(Convert.ToDouble(data.Rows[i]["Amount"].ToString())).ToString();
                    string strMsg = "அன்புடையீர் ! 'இந்து தமிழ் திசை' " + str1[DateTime.Now.Date.Month - 1] + " " + DateTime.Now.Date.Year.ToString() + "  மாத பில் தொகையில் ₹." + strAmount + " /- கிடைக்கப்பெற்றது ஏற்கனவே பணம் செலுத்தினால் இக்குறுஞ்செய்தியை  பொருட்படுத்த வேண்டாம் –நன்றி";

                    string strMsgmobile = objuni.ConvertToUnicode(strMsg);
                    bool resSMSgateway = false;
                    resSMSgateway = SendSMSGateway(strMobileno, strMsgmobile);
                    if (resSMSgateway == true)
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Dailybasisreceipt_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Dailybasisreceipt_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["agent_id"].ToString() + "  To Mobile Number: " + strMobileno + " Mesaages send successfully ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Dailybasisreceipt_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["agent_id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages send successfully ");
                    }
                    else
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_DailybasisreceiptFail_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_DailybasisreceiptFail_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["agent_id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_DailybasisreceiptFail_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["agent_id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                    }
                }
            }

            catch (Exception ex2) { }

        }

        //Hindu Agencies Subscribers -- about  "Every month 1st day once the bill is raised"

        private void HinduAgencies()
        {

            var firstDayOfMonth = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);

            if (firstDayOfMonth.Date == DateTime.Now.Date)

            {
                strQry = "select  Agent_Id,RATpay,(select Per_Mobile from m_agent x where x.agent_id= y.agent_id and Per_Mobile!=' ' ) as Per_Mobile from Print_Order_Bills y where Status='BA'and month(convert(date,Bill_Date))=month(getdate()) and year(convert(date,Bill_Date))=year(getdate()) and RATpay>0 and Pub_Id='TM' order by Bill_Date";
                DataTable data = new DataTable();
                data = ReturndatatableSQL(strQry);
                try
                {
                    SMSUnicode.RouteSMSUnicode objuni = new SMSUnicode.RouteSMSUnicode();
                    for (i = 0; i < data.Rows.Count; i++)
                    {

                        string strAmount = "0";
                        string strMobileno = "0";
                        if ((data.Rows[i]["Per_Mobile"].ToString() != "") && (data.Rows[i]["Per_Mobile"].ToString() != "0") && (data.Rows[i]["Per_Mobile"].ToString().Length == 10))

                            strMobileno = Math.Round(Convert.ToDouble(data.Rows[i]["Per_Mobile"].ToString())).ToString();


                        if (data.Rows[i]["RATpay"].ToString() != "")
                            strAmount = Math.Round(Convert.ToDouble(data.Rows[i]["RATpay"].ToString())).ToString();
                        string strMsg = "அன்புடையீர்! தங்களின் 'இந்து தமிழ் திசை' " + str1[DateTime.Now.Date.Month -2] + " " + DateTime.Now.Date.Year.ToString() + " மாத பில் தொகை ₹." + strAmount + "/-ஐ உரிய தேதிக்குள் செலுத்துமாறு கேட்டுகொள்கிறோம்–நன்றி";

                        string strMsgmobile = objuni.ConvertToUnicode(strMsg);

                        SendSMSGateway(strMobileno.ToString(), strMsgmobile);

                        bool resSMSgateway = false;
                        resSMSgateway = SendSMSGateway(strMobileno.ToString(), strMsgmobile);
                        if (resSMSgateway == true)
                        {
                            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HinduAgencies_Logs.txt"))
                                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HinduAgencies_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " "+"Agent Id:"+" "+ data.Rows[i]["Per_Mobile"].ToString() + "  To Mobile Number: " + strMobileno + " Mesaages send successfully ");
                            else
                                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HinduAgencies_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id:" + " " + data.Rows[i]["Per_Mobile"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages send successfully ");
                        }
                        else
                        {
                            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HinduAgenciesFail_Logs.txt"))
                                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HinduAgenciesFail_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " "+"Agent Id:" + " " + data.Rows[i]["Per_Mobile"].ToString()  + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                            else
                                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_HinduAgenciesFail_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id:" + " " + data.Rows[i]["Per_Mobile"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                        }
                    }
                }

              catch (Exception ex) { }

         }
        }


        //Kamadhenu Agencies "Every month 1st dayonce the bill is raised"

        private void MonthFirstKamadhenu()
        {

            var firstDayOfMonth = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);

            if (firstDayOfMonth.Date == DateTime.Now.Date)

            {
                strQry = "select  Agent_Id,RATpay,(select Per_Mobile from m_agent x where x.agent_id= y.agent_id and Per_Mobile!=' ' ) as Per_Mobile from Print_Order_Bills y where Status='BA'and month(convert(date,Bill_Date))=month(getdate()) and year(convert(date,Bill_Date))=year(getdate()) and RATpay>0 and Pub_Id='KM' order by Bill_Date";

                DataTable data = new DataTable();
                data = ReturndatatableSQL(strQry);
                try
                {
                    SMSUnicode.RouteSMSUnicode objuni = new SMSUnicode.RouteSMSUnicode();

                    for (i = 0; i < data.Rows.Count; i++)
                    {
                        string strAmount = "0";
                        string strMobileno = "0";
                        if (data.Rows[i]["RATpay"].ToString() != "")
                            strAmount = Math.Round(Convert.ToDouble(data.Rows[i]["RATpay"].ToString())).ToString();
                        if (data.Rows[i]["Per_Mobile"].ToString() != "")
                            strMobileno = Math.Round(Convert.ToDouble(data.Rows[i]["Per_Mobile"].ToString())).ToString();

                        string strMsg = "அன்புடையீர்! தங்களின்  'காமதேனு'" + str1[DateTime.Now.Date.Month - 2] + " " + DateTime.Now.Date.Year.ToString() + " மாத பில் தொகை ₹." + strAmount + " /-ஐ உரிய தேதிக்குள் செலுத்துமாறு கேட்டுகொள்கிறோம்–நன்றி";
                        string strMsgmobile = objuni.ConvertToUnicode(strMsg);
                        bool resSMSgateway = false;
                        resSMSgateway = SendSMSGateway(strMobileno, strMsgmobile);
                        if (resSMSgateway == true)
                        {
                            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_MonthFirstKamadhenu_Logs.txt"))
                                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_MonthFirstKamadhenu_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id:" + " " + data.Rows[i]["agent_id"].ToString() + "  To Mobile Number: " + strMobileno + " Mesaages send successfully ");
                            else
                                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_MonthFirstKamadhenu_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id:" + " " + data.Rows[i]["agent_id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages send successfully ");
                        }
                        else
                        {
                            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_MonthFirstKamadhenuFail_Logs.txt"))
                                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_MonthFirstKamadhenuFail_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id:" + " " + data.Rows[i]["agent_id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                            else
                                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_MonthFirstKamadhenuFail_Logs.txt"))
                                    objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id:" + " " + data.Rows[i]["agent_id"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                        }
                    }
                }

                catch (Exception ex1) { }
           }
        }

        private void Rss3times()
        {


            strQry = "select Subscribercode,format(cast(Temp_Stop_Effective_To as date), 'dd/MM/yyyy') as Temp_Stop_Effective_To,  Area_Agent_Effective_To,(select top 1 x.MobileNo  from m_rsss x where x.Subscribercode=y.Subscribercode  ) as MobileNo from rsss_trans y  ";
            strQry += "where convert(date, GETDATE()+45) = convert(date, Area_Agent_Effective_To) and convert(date, GETDATE()+45)= convert(date, Temp_Stop_Effective_To) and Edition_id!='SM' and  Edition_id!='CB' and  Edition_id!='TU' ";
            DataTable data = new DataTable();
            data = ReturndatatableSQL(strQry);
            try
            {
                SMSUnicode.RouteSMSUnicode objuni = new SMSUnicode.RouteSMSUnicode();

                for (i = 0; i < data.Rows.Count; i++)
                {
                    string strMobileno = "0";
                    if (data.Rows[i]["mobile"].ToString() != "")
                        strMobileno = Math.Round(Convert.ToDouble(data.Rows[i]["mobile"].ToString())).ToString();

                    string strMsg = "அன்புடையீர்! இந்து தமிழ் திசையின் தங்களது சந்தாகாலம் " + data.Rows[i]["Temp_Stop_Effective_To"].ToString() + " தேதியுடன் நிறைவடைகிறது.சிறப்பு சந்தாவை செலுத்தி இணைந்திருக்க  https://bit.ly/2uxHogk";
                    string strMsgmobile = objuni.ConvertToUnicode(strMsg);
                    bool resSMSgateway = false;
                    resSMSgateway = SendSMSGateway(strMobileno, strMsgmobile);
                    if (resSMSgateway == true)
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss3times_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss3times_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + " Mesaages send successfully ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss3times_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages send successfully ");
                    }
                    else
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss3times_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss3timesFail_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss3timesFail_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                    }
                }

            }
            catch (Exception ex2) { }
         }

        private void Rss2times()
        {
            strQry = "select Subscribercode,format(cast(Temp_Stop_Effective_To as date), 'dd/MM/yyyy') as Temp_Stop_Effective_To,  Area_Agent_Effective_To,(select top 1 x.MobileNo  from m_rsss x where x.Subscribercode=y.Subscribercode and MobileNo!=' ')  as MobileNo from rsss_trans y  ";
            strQry += "where convert(date, GETDATE()+30) = convert(date, Area_Agent_Effective_To) and convert(date, GETDATE()+30)= convert(date, Temp_Stop_Effective_To) and Edition_id!='SM' and  Edition_id!='CB' and  Edition_id!='TU' ";
            DataTable data = new DataTable();
            data = ReturndatatableSQL(strQry);
            try
            {
                SMSUnicode.RouteSMSUnicode objuni = new SMSUnicode.RouteSMSUnicode();

                for (i = 0; i < data.Rows.Count; i++)
                {
                    string strMobileno = "0";
                    if (data.Rows[i]["mobile"].ToString() != "")
                        strMobileno = Math.Round(Convert.ToDouble(data.Rows[i]["mobile"].ToString())).ToString();

                    string strMsg = "அன்புடையீர்! இந்து தமிழ் திசையின் தங்களது சந்தாகாலம் " + data.Rows[i]["Temp_Stop_Effective_To"].ToString() + " தேதியுடன் நிறைவடைகிறது.சிறப்பு சந்தாவை செலுத்தி இணைந்திருக்க  https://bit.ly/2uxHogk";
                    string strMsgmobile = objuni.ConvertToUnicode(strMsg);
                    bool resSMSgateway = false;
                    resSMSgateway = SendSMSGateway(strMobileno, strMsgmobile);
                    if (resSMSgateway == true)
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss2times_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss2times_Logs.txt"))
                                objWritter.WriteLine("Message :" + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + " " + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + " Mesaages send successfully ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss2times_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + " " + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages send successfully ");
                    }
                    else
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss2timesFail_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss2timesFail_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + " " + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss2timesFail_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + " " + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                    }
                }

            }
            catch (Exception ex2) { }
         }

        private void Rss1times()
        {
            strQry = "select Subscribercode,format(cast(Temp_Stop_Effective_To as date), 'dd/MM/yyyy') as Temp_Stop_Effective_To,  Area_Agent_Effective_To,(select top 1 x.MobileNo  from m_rsss x where x.Subscribercode=y.Subscribercode  ) as MobileNo from rsss_trans y  ";
            strQry += "where convert(date, GETDATE()+15) = convert(date, Area_Agent_Effective_To) and convert(date, GETDATE()+15)= convert(date, Temp_Stop_Effective_To) and Edition_id!='SM' and  Edition_id!='CB' and  Edition_id!='TU' ";
            DataTable data = new DataTable();
            data = ReturndatatableSQL(strQry);
            try
            {
                SMSUnicode.RouteSMSUnicode objuni = new SMSUnicode.RouteSMSUnicode();

                for (i = 0; i < data.Rows.Count; i++)
                {
                    string strMobileno = "0";
                    if (data.Rows[i]["mobile"].ToString() != "")
                        strMobileno = Math.Round(Convert.ToDouble(data.Rows[i]["mobile"].ToString())).ToString();

                    string strMsg = "அன்புடையீர்! இந்து தமிழ் திசையின் தங்களது சந்தாகாலம் " + data.Rows[i]["Temp_Stop_Effective_To"].ToString() + " தேதியுடன் நிறைவடைகிறது.சிறப்பு சந்தாவை செலுத்தி இணைந்திருக்க  https://bit.ly/2uxHogk";
                    string strMsgmobile = objuni.ConvertToUnicode(strMsg);
                    bool resSMSgateway = false;
                    resSMSgateway = SendSMSGateway(strMobileno, strMsgmobile);
                    if (resSMSgateway == true)
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss1times_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss1times_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + " " + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + " Mesaages send successfully ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss1times_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + " " + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages send successfully ");
                    }
                    else
                    {
                        if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss1timesFail_Logs.txt"))
                            using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss1timesFail_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + " " + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                        else
                            using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_Rss1timesFail_Logs.txt"))
                                objWritter.WriteLine("Message : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + "Agent Id" + " " + data.Rows[i]["Subscribercode"].ToString() + "  To Mobile Number: " + strMobileno + "  Mesaages sending Faild ");
                    }
                }

            }
            catch (Exception ex2) { }
        }


        private DataTable ReturndatatableSQL(string strQry)
        {
            DataTable dt = new DataTable();
            try
            {
                string _strConstr = ConfigurationSettings.AppSettings["ConStrSql"].ToString();

                try
                {
                    using (SqlConnection conn = new SqlConnection(_strConstr))
                    {
                        try
                        {
                            conn.Open();
                            SqlDataAdapter da = new SqlDataAdapter(strQry, conn);
                            da.Fill(dt); conn.Close();
                        }
                        catch (SqlException ex)
                        { if (conn.State == ConnectionState.Open) conn.Close(); }
                    }
                }
                catch (Exception ex) { }
            }
            catch (Exception ex1) { }
            return dt;
        }

        protected override void OnStart(string[] args)
        {

            _timer.Start();
            strLogPath = ConfigurationSettings.AppSettings["LogPath"].ToString();

            if (!Directory.Exists(strLogPath))
                Directory.CreateDirectory(strLogPath);

            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "CirculationSMS.txt"))
                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_CirculationSMS_LOG.txt"))
                    objWritter.WriteLine(DateTime.Now.ToString("hh:mm:ss tt") + " _CirculationSMS_LOG  Service Started ....");
            else
                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_CirculationSMS_LOG.txt"))
                    objWritter.WriteLine(DateTime.Now.ToString("hh:mm:ss tt") + " _CirculationSMS_LOG  Service Started ....");
        }


        protected override void OnStop()
        {
            _timer.Stop();

            if (!Directory.Exists(strLogPath))
                Directory.CreateDirectory(strLogPath);

            if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "CirculationSMS.txt"))
                using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_CirculationSMS_LOG.txt"))
                    objWritter.WriteLine(DateTime.Now.ToString("hh:mm:ss tt") + " _CirculationSMS_LOG  Service Stoped ....");
            else
                using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_CirculationSMS_LOG.txt"))
                    objWritter.WriteLine(DateTime.Now.ToString("hh:mm:ss tt") + "_CirculationSMS_LOG  Service Stoped ....");
        }
        public bool SendSMSGateway(string strToNumber, string strMsg)
        {
            bool blnRes = false;
            try
            {
                string url = sendSMS(strToNumber, strMsg.Replace("#", "%23"));

                var request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = "POST";
                var response = (HttpWebResponse)request.GetResponse();

                try
                {
                    strLog = DateTime.Now.ToString("hh:mm:ss tt") + " SMS Mob no:" + strToNumber + " ,Resp :" + response.StatusDescription.ToString() + ", " + response.StatusCode.ToString();
                    if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_SMS_RES_LOG.txt"))
                        using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_SMS_RES_LOG.txt"))
                            objWritter.WriteLine(strLog);
                    else
                        using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_SMS_RES_LOG.txt"))
                            objWritter.WriteLine(strLog);
                }
                catch { }

                response.Close();
                blnRes = true;
            }
            catch (Exception ex)
            {
                if (!File.Exists(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_SMSsendDailyMail_Logs.txt"))
                    using (StreamWriter objWritter = File.CreateText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_SMSsendDailyMail_Logs.txt"))
                        objWritter.WriteLine("Error : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + ex.Message + "  To Mobile Number: " + strToNumber + " Message: " + strMsg + "  SendSMSGateway()");
                else
                    using (StreamWriter objWritter = File.AppendText(strLogPath + DateTime.Now.ToString("dd-MMM-yyyy") + "_SMSsendDailyMail_Logs.txt"))
                        objWritter.WriteLine("Error : " + DateTime.Now.ToString("hh:mm:ss tt") + " " + ex.Message + "  To Mobile Number: " + strToNumber + " Message: " + strMsg + "  SendSMSGateway()");
            }
            return blnRes;
        }


        public string sendSMS(string strToNumber, string strMsg)
        {
            string url = null;
            try
            {
                switch (System.Configuration.ConfigurationSettings.AppSettings["SMS"].ToString())
                {
                    case "ACL":
                        string SMSUrl = System.Configuration.ConfigurationSettings.AppSettings["SMSUrl"].ToString();
                        string SMSUserIDval = System.Configuration.ConfigurationSettings.AppSettings["SMSUserIDVal"].ToString();
                        string SMSPwd = System.Configuration.ConfigurationSettings.AppSettings["SMSPwd"].ToString();
                        string SMSPwdval = System.Configuration.ConfigurationSettings.AppSettings["SMSPwdVal"].ToString();
                        string SMSTo = System.Configuration.ConfigurationSettings.AppSettings["SMSTo"].ToString();
                        string SMSToVal = System.Configuration.ConfigurationSettings.AppSettings["SMSToVal"].ToString();
                        string SMSMessage = System.Configuration.ConfigurationSettings.AppSettings["SMSMessage"].ToString();
                        string SMSSenderID = System.Configuration.ConfigurationSettings.AppSettings["SMSSenderID"].ToString();
                        string SMSSenderIDVal = System.Configuration.ConfigurationSettings.AppSettings["SMSSenderIDVal"].ToString();
                        url = SMSUrl + SMSUserIDval + "&" + SMSPwd + SMSPwdval + "&" + SMSTo + SMSToVal + strToNumber + "&" + SMSSenderID + SMSSenderIDVal + "&" + SMSMessage + strMsg;

                        break;
                    case "ROUTE":
                        strMsg = strMsg.Trim().Replace(" ", "%20");
                        string RUrl = System.Configuration.ConfigurationSettings.AppSettings["RUrl"].ToString();
                        string RUserName = System.Configuration.ConfigurationSettings.AppSettings["RUserName"].ToString();
                        string RUserNameVal = System.Configuration.ConfigurationSettings.AppSettings["RUserNameVal"].ToString();
                        string RPwd = System.Configuration.ConfigurationSettings.AppSettings["RPwd"].ToString();
                        string RPwdVal = System.Configuration.ConfigurationSettings.AppSettings["RPwdVal"].ToString();
                        string RType = System.Configuration.ConfigurationSettings.AppSettings["RType"].ToString();
                        string RTypeVal = System.Configuration.ConfigurationSettings.AppSettings["RTypeVal"].ToString();
                        string RDlr = System.Configuration.ConfigurationSettings.AppSettings["RDlr"].ToString();
                        string RDlrVal = System.Configuration.ConfigurationSettings.AppSettings["RDlrVal"].ToString();
                        string RDestination = System.Configuration.ConfigurationSettings.AppSettings["RDestination"].ToString();
                        string RSource = System.Configuration.ConfigurationSettings.AppSettings["RSource"].ToString();
                        string RSourceVal = System.Configuration.ConfigurationSettings.AppSettings["RSourceVal"].ToString();
                        string RMessage = System.Configuration.ConfigurationSettings.AppSettings["RMessage"].ToString();
                        url = RUrl + RUserName + "=" + RUserNameVal + "&" + RPwd + "=" + RPwdVal + "&";
                        url += RType + "=" + RTypeVal + "&" + RDlr + "=" + RDlrVal + "&";
                        url += RDestination + "=" + strToNumber + "&";
                        url += RSource + "=" + RSourceVal + "&" + RMessage + "=" + strMsg;
                        break;
                    default:
                        break;
                }
            }
            catch { }
            finally
            {
                TimeSpan ts = DateTime.Now.TimeOfDay;
                string strTimeFrom = System.Configuration.ConfigurationSettings.AppSettings["SmsTimeFrom"].ToString();
                string strTimeTo = System.Configuration.ConfigurationSettings.AppSettings["SmsTimeTo"].ToString();
                int sysTs = ts.Hours;
                int intWebConfigFrom = Convert.ToInt32(strTimeFrom); int intWebConfigTo = Convert.ToInt32(strTimeTo);
                if ((sysTs >= intWebConfigFrom) && (sysTs <= intWebConfigTo))
                {

                }
                else
                {
                    //strMsg = strMsg.Replace("+", " ").Replace("%20", " ");
                    //strQry = "";
                    //strQry = "insert into cls_send_sms values('" + strToNumber + "','" + strMsg + "','Y')";
                    //ExecuteOleDbNonQueryForDirectText(strQry);
                    //InsertUpdate(strQry);
                    //url = "";
                }
            }

            return url;
        }
    }
}

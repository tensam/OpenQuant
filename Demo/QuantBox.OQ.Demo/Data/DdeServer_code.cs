﻿using System;
using System.Collections;
using System.Drawing;
using System.Timers;

using OpenQuant.API;
using OpenQuant.API.Indicators;

using NDde.Server;
using NDde.Advanced;
using System.Windows.Forms;

namespace QuantBox.OQ.Demo.Data
{
    public class DdeServer_code : Strategy
    {
        private const string DDE_SERVER = "OpenQuant";

        private const string TOPIC_TRADE_PRICE = "Trade.Price";
        private const string TOPIC_TRADE_SIZE = "Trade.Size";

        private const string TOPIC_QUOTE_BID = "Quote.Bid";
        private const string TOPIC_QUOTE_BID_SIZE = "Quote.BidSize";

        private const string TOPIC_QUOTE_ASK = "Quote.Ask";
        private const string TOPIC_QUOTE_ASK_SIZE = "Quote.AskSize";

        private const string TOPIC_TIME = "Time";

        static TestServer server = new TestServer(DDE_SERVER);
        static int Counter = 0;

        public override void OnStrategyStart()
        {
            ++Counter;
            if (Counter == this.Instruments.Count)
            {
                try
                {
                    server.Register();
                    Console.WriteLine("注册DDE服务:" + server.Service);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            AddTimer(Clock.Now.AddSeconds(3));
        }

        public override void OnStrategyStop()
        {
            server.Unregister();
        }

        public override void OnTrade(Trade trade)
        {
            server.SetData(TOPIC_TRADE_PRICE,
                Instrument.ToString(),
                1,
                trade.Price
                );

            server.SetData(TOPIC_TRADE_SIZE,
                Instrument.ToString(),
                1,
                trade.Size
                );
        }

        public override void OnQuote(Quote quote)
        {
            server.SetData(TOPIC_QUOTE_BID,
                Instrument.ToString(),
                1,
                quote.Bid
                );

            server.SetData(TOPIC_QUOTE_BID_SIZE,
                Instrument.ToString(),
                1,
                quote.BidSize
                );

            server.SetData(TOPIC_QUOTE_ASK,
                Instrument.ToString(),
                1,
                quote.Ask
                );

            server.SetData(TOPIC_QUOTE_ASK_SIZE,
                Instrument.ToString(),
                1,
                quote.AskSize
                );
        }

        public override void OnTimer(DateTime datetime, object data)
        {
            server.SetData(TOPIC_TIME,
                "Now",
                1,
                Clock.Now
                );

            AddTimer(Clock.Now.AddSeconds(2));
        }

        #region DDE服务
        private sealed class TestServer : DdeServer
        {
            private System.Timers.Timer _Timer = new System.Timers.Timer();
            private string _Command = "";
            private IDictionary _Data = new Hashtable();

            public TestServer(string service)
                : base(service)
            {
                _Timer.Elapsed += this.OnTimerElapsed;
                _Timer.Interval = 1000;
                _Timer.SynchronizingObject = this.Context;
            }

            public double Interval
            {
                get { return _Timer.Interval; }
            }

            public string Command
            {
                get { return _Command; }
            }

            public object GetData(string topic, string item, int format)
            {
                string key = topic + ":" + item + ":" + format.ToString();
                return _Data[key];
            }

            //public void SetData(string topic, string item, int format, byte[] data)
            //{
            //    string key = topic + ":" + item + ":" + format.ToString();
            //    _Data[key] = data;

            //    // 如果数据变化少，变化慢，就用这句，效率高
            //    //Advise(topic, item);
            //}

            public void SetData(string topic, string item, int format, object data)
            {
                string key = topic + ":" + item + ":" + format.ToString();
                // 最后不加0也不出错，但示例中又是加了0
                _Data[key] = data;

                // 如果数据变化少，变化慢，就用这句，效率高
                //Advise(topic, item);
            }

            private void OnTimerElapsed(object sender, ElapsedEventArgs args)
            {
                // Advise all topic name and item name pairs.

                // 如果数据变化很多，用这句，定时更新
                Advise("*", "*");
            }

            public override void Register()
            {
                base.Register();
                _Timer.Start();
            }

            public override void Unregister()
            {
                _Timer.Stop();
                base.Unregister();
            }

            protected override bool OnStartAdvise(DdeConversation conversation, string item, int format)
            {
                //Console.WriteLine("OnStartAdvise:".PadRight(16)
                //    + " Service='" + conversation.Service + "'"
                //    + " Topic='" + conversation.Topic + "'"
                //    + " Handle=" + conversation.Handle.ToString()
                //    + " Item='" + item + "'"
                //    + " Format=" + format.ToString());

                // Initiate the advisory loop only if the format is CF_TEXT.
                // 表示只对文本进行热连接的循环
                return format == 1;
            }

            protected override ExecuteResult OnExecute(DdeConversation conversation, string command)
            {
                base.OnExecute(conversation, command);
                _Command = command;
                switch (command)
                {
                    case "#NotProcessed":
                        {
                            return ExecuteResult.NotProcessed;
                        }
                    case "#PauseConversation":
                        {
                            if ((string)conversation.Tag == command)
                            {
                                conversation.Tag = null;
                                return ExecuteResult.Processed;
                            }
                            conversation.Tag = command;
                            if (!_Timer.Enabled) _Timer.Start();
                            return ExecuteResult.PauseConversation;
                        }
                    case "#Processed":
                        {
                            return ExecuteResult.Processed;
                        }
                    case "#TooBusy":
                        {
                            return ExecuteResult.TooBusy;
                        }
                }
                return ExecuteResult.Processed;
            }

            protected override PokeResult OnPoke(DdeConversation conversation, string item, byte[] data, int format)
            {
                base.OnPoke(conversation, item, data, format);
                string key = conversation.Topic + ":" + item + ":" + format.ToString();
                _Data[key] = data;
                switch (item)
                {
                    case "#NotProcessed":
                        {
                            return PokeResult.NotProcessed;
                        }
                    case "#PauseConversation":
                        {
                            if ((string)conversation.Tag == item)
                            {
                                conversation.Tag = null;
                                return PokeResult.Processed;
                            }
                            conversation.Tag = item;
                            if (!_Timer.Enabled) _Timer.Start();
                            return PokeResult.PauseConversation;
                        }
                    case "#Processed":
                        {
                            return PokeResult.Processed;
                        }
                    case "#TooBusy":
                        {
                            return PokeResult.TooBusy;
                        }
                }
                return PokeResult.Processed;
            }

            protected override RequestResult OnRequest(DdeConversation conversation, string item, int format)
            {
                base.OnRequest(conversation, item, format);
                string key = conversation.Topic + ":" + item + ":" + format.ToString();
                if (_Data.Contains(key))
                {
                    return new RequestResult(System.Text.Encoding.ASCII.GetBytes(_Data[key].ToString()));
                }
                return RequestResult.NotProcessed;
            }

            protected override byte[] OnAdvise(string topic, string item, int format)
            {
                base.OnAdvise(topic, item, format);
                string key = topic + ":" + item + ":" + format.ToString();
                if (_Data.Contains(key))
                {
                    return System.Text.Encoding.ASCII.GetBytes(_Data[key].ToString());
                }
                return null;
            }
        }
        #endregion
    }
}


﻿using QuantBox.OQ.Extensions.OrderText;
using SmartQuant.Execution;
using SmartQuant.FIX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantBox.OQ.Extensions.Combiner
{
    public class GenericOrderItem
    {
        public TextRequest TextRequest;

        public bool IsPendingCancel;

        public virtual void Init(TextRequest t)
        {
            TextRequest = t;
        }

        public virtual void Add(SingleOrder order, TextRequest t)
        {
        }

        public virtual IEnumerable<MultiOrderLeg> GetLegs()
        {
            return null;
        }

        public virtual MultiOrderLeg GetLeg(Side side, string instrument)
        {
            return null;
        }

        public virtual bool IsCreated()
        {
            return false;
        }

        public virtual bool IsDone()
        {
            foreach (var order in GetLegs())
            {
                if (order.Order.IsDone)
                    return true;
            }
            return false;
        }
    }
}
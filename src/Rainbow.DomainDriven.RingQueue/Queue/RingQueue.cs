using System;
using System.Collections.Generic;
using System.Linq;
using Rainbow.DomainDriven.RingQueue.Utilities;

namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public class RingQueue<TMessage> : IRingQueue<TMessage>
    {

        #region 内部类成员

        private struct RingBufferFields
        {
            public object[] Data;
            public int Size;
            public int IndexMask;

        }
        #endregion

        private RingBufferFields _value;
        private readonly ISequencer _sequencer;


        public RingQueue(ISequencer sequencer)
        {
            this._value.Size = sequencer.BufferSize;
            
            if (this._value.Size < 1)
            {
                throw new ArgumentException("bufferSize must not be less than 1");
            }
            if (RingUtil.CeilingNextPowerOfTwo(this._value.Size) != this._value.Size)
                throw new ArgumentException("bufferSize must be a power of 2");

            this._value.IndexMask = sequencer.BufferSize - 1;
            this._sequencer = sequencer;
            Fill();
        }

        #region 内部方法

        private void Fill()
        {
            this._value.Data = new object[this._value.Size];
            for (int i = 0; i < this._value.Size; i++)
            {
                this._value.Data[i] = new WrapMessage<TMessage>();
            }
        }


        #endregion

        public WrapMessage<TMessage> this[long sequence] => (WrapMessage<TMessage>)_value.Data[(sequence & _value.IndexMask)];

        public int Size => this._value.Size;

        public long Next()
        {
            return this._sequencer.Next();
        }

        public void Publish(long sequence)
        {
            this._sequencer.Publish(sequence);
        }

        public long Next(int n)
        {
            return this._sequencer.Next(n);
        }

        public void Publish(long lo, long hi)
        {
            this._sequencer.Publish(lo, hi);
        }

        public void AddGatingSequences(params ISequence[] gatingSequences)
        {
            this._sequencer.AddGatingSequences(gatingSequences);
        }

        public ISequenceBarrier NewBarrier(params ISequence[] sequencesToTrack)
        {
            return this._sequencer.NewBarrier(sequencesToTrack);
        }

    }
}
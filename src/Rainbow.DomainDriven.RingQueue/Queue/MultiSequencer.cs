using System;
using System.Collections.Generic;
using System.Threading;
using Rainbow.DomainDriven.RingQueue.Utilities;

namespace Rainbow.DomainDriven.RingQueue.Queue
{
    public class MultiSequencer : ISequencer
    {

        //队列使用情况标记，每过一轮+1
        private readonly int[] _availableBuffer;
        //队列最大可存储值下标
        private readonly int _indexMask;
        //位移数量值，如1位移1位，2位移2位，4位移3位，8位移4位以此类推
        private readonly int _indexShift;
        //队列大小
        private readonly int _size;
        //消费者最小消费的序列缓存值
        private Sequence _sequenceCache = new Sequence();
        //当前生产者生产的序列值
        private Sequence _sequence = new Sequence();
        //序列闸门，用来限制可进行生产的序列
        private List<ISequence> _gatingSequences;


        public MultiSequencer(int size)
        {
            this._availableBuffer = new int[size];
            this._size = size;
            this._indexMask = size - 1;
            this._indexShift = RingUtil.Log2(size);
            this._gatingSequences = new List<ISequence>();
            InitialiseAvailableBuffer();
        }

        #region 私有方法


        private void InitialiseAvailableBuffer()
        {
            for (int i = _availableBuffer.Length - 1; i != 0; i--)
            {
                SetAvailableBufferValue(i, -1);
            }

            SetAvailableBufferValue(0, -1);

        }


        private int CalculateIndex(long sequence)
        {
            return ((int)sequence) & _indexMask;
        }

        private int CalculateAvailabilityFlag(long sequence)
        {
            return (int)((ulong)sequence >> _indexShift);
        }

        private void SetAvailableBufferValue(int index, int flag)
        {
            _availableBuffer[index] = flag;
        }

        private void SetAvailable(long sequence)
        {
            SetAvailableBufferValue(CalculateIndex(sequence), CalculateAvailabilityFlag(sequence));
        }

        private bool IsAvailable(long sequence)
        {
            int index = CalculateIndex(sequence);
            int flag = CalculateAvailabilityFlag(sequence);
            return Volatile.Read(ref _availableBuffer[index]) == flag;
        }
        #endregion

        public long Current => _sequence.Value;

        public int BufferSize => _size;


        public long Next()
        {
            return Next(1);
        }

        public long Next(int n)
        {

            if (n < 1)
            {
                throw new ArgumentException("n must be > 0");
            }

            long current;
            long next;

            var spinWait = default(SpinWait);
            do
            {
                current = _sequence.Value;
                next = current + n;

                long offsetSequence = next - _size;
                long cachedGatingSequence = _sequenceCache.Value;

                //获取队列长度与当前处理值比较，如果没有超过，为可用
                if (offsetSequence > cachedGatingSequence || cachedGatingSequence > current)
                {
                    //询问消费者已经处理的最小的序列是多少，并进行设置
                    long gatingSequence = RingUtil.GetMinimum(this._gatingSequences, current);

                    if (offsetSequence > gatingSequence)
                    {
                        spinWait.SpinOnce();
                        continue;
                    }

                    _sequenceCache.SetValue(gatingSequence);
                }
                else if (_sequence.CompareAndSet(current, next))
                {
                    break;
                }
            }
            while (true);

            return next;
        }

        public void Publish(long sequence)
        {
            SetAvailable(sequence);
        }

        public void Publish(long lo, long hi)
        {
            for (long l = lo; l <= hi; l++)
            {
                SetAvailable(l);
            }
        }

        public long GetUseSequence(long lo, long hi)
        {
            for (long sequence = lo; sequence <= hi; sequence++)
            {
                if (!IsAvailable(sequence))
                {
                    return sequence - 1;
                }
            }

            return hi;
        }

        public void AddGatingSequences(params ISequence[] gatingSequences)
        {
            _gatingSequences.AddRange(gatingSequences);
        }

        public ISequenceBarrier NewBarrier(params ISequence[] sequencesToTrack)
        {
            return new SequenceBarrier(this, _sequence, sequencesToTrack);
        }
    }
}
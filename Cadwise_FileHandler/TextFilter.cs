using System.Collections.Generic;
namespace Cadwise_FileHandler
{
    public class FilterableBuffer
	{
		public FilterableBuffer()
		{
			m_bufferSize = 0;
			m_currentWordLength = 0;
			m_buffer = new char[]{};
			m_currentWordSymbols = new Queue<char>{};
		}
		public bool EndOfBuffer
        {
            get { return (m_bufferSize > Buffer.Length); }
        }
		public void RefreshBuffer()
        {
            Buffer = new char[Buffer.Length];
            m_bufferSize = 0;
        }
		public void EnqueueSymbol(char symb)
		{
			m_currentWordSymbols.Enqueue(symb);
		}
		public void Write(char symb)
		{
			m_bufferSize++;
            if(!EndOfBuffer)
                m_buffer[m_bufferSize - 1] = symb;
		}
		public void WriteCurrentWord()
		{
			while(m_currentWordSymbols.Count>0)
				Write(m_currentWordSymbols.Dequeue());
		}
		public char[] Buffer { get { return m_buffer;} set {m_buffer = value;}}
		public Queue<char> CurrentWordSymbols {get {return m_currentWordSymbols;} set {m_currentWordSymbols = value;}}
		protected Queue<char> m_currentWordSymbols = new Queue<char> {};
		protected int m_currentWordLength;
		protected char[] m_buffer;
		protected int m_bufferSize;
        public int Size { get { return m_bufferSize; } }
    }
    public class TextFilter: FilterableBuffer
    {
		public TextFilter()
		{
			m_bufferSize = 0;
			m_buffer = new char[]{};
			m_minWordLength = 0;
		}
        public TextFilter(int length, bool removingPunctuation, int bufferSize)
        {
			m_bufferSize = bufferSize;
            m_buffer = new char[bufferSize];
            m_minWordLength = length;
            m_removingPunctuation = removingPunctuation;
        }
        public TextFilter(int length, bool removing, int bufferSize,HashSet<char> punctuation):this(length,removing,bufferSize)
        { 
            foreach (char symb in punctuation)
                m_punctuation.Add(symb);
        }
        public void FilterBuffer(char[] original)
        {
            RefreshBuffer();
            bool emptyLine = true;
            for (int i = 0; i < original.Length; i++)
            {
                char symb = original[i];
                if (m_splitters.Contains(symb) || m_punctuation.Contains(symb))
                {
                    if (m_currentWordLength >= m_minWordLength)
                    {
                        if (!(m_removingPunctuation && m_punctuation.Contains(symb)))
                            EnqueueSymbol(symb);
                        WriteCurrentWord();
                        emptyLine = false; 
                    }
                    else
                    {
                        if ((symb == '\n' || symb == '\r' || m_splitters.Contains(symb)&&m_currentWordLength==0) && !emptyLine)
                        {
                            Write(symb);
                        }
                    }
					m_currentWordSymbols = new Queue<char>{};
					m_currentWordLength = 0;
                }
                else
                {
                    EnqueueSymbol(symb);
					m_currentWordLength++;
                }
                if (i == original.Length - 1)
                {
                    if (m_currentWordLength >= m_minWordLength)
                    {
                        WriteCurrentWord();
                    }
                }
            }
        }
        private HashSet<char> m_punctuation = new HashSet<char> { ',', '.', '!', '?', '—', '(', ')', '[', ']', '{', '}', '«', '»' };
        private HashSet<char> m_splitters = new HashSet<char> { ' ', '\t', '\n', '\r' };
        private int m_minWordLength;
        private bool m_removingPunctuation;
    }
}

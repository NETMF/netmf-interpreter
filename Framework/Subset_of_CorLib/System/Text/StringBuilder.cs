namespace System.Text
{   
    /// <summary>
    /// A Micro Framework port of the Full Framework StringBuilder. Contributed by Julius Friedman.
    /// </summary>
    public sealed class StringBuilder
    {
        #region Fields

        int m_MaxCapacity;
        char[] m_ChunkChars;
        int m_ChunkLength;
        StringBuilder m_ChunkPrevious;
        int m_ChunkOffset;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the maximum capacity of this instance. 
        /// </summary>
        public int MaxCapacity
        {
            get
            {
                return this.m_MaxCapacity;
            }
        }

        /// <summary>
        /// Gets or sets a character from the underlying buffer
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public char this[int index]
        {
            get
            {
                StringBuilder chunkPrevious = this;
                while (true)
                {
                    int num = index - chunkPrevious.m_ChunkOffset;
                    if (num >= 0)
                    {
                        if (num >= chunkPrevious.m_ChunkLength)
                        {
                            throw new IndexOutOfRangeException();
                        }
                        return chunkPrevious.m_ChunkChars[num];
                    }
                    chunkPrevious = chunkPrevious.m_ChunkPrevious;
                    if (chunkPrevious == null)
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
            }
            set
            {
                int num;
                StringBuilder chunkPrevious = this;
            Label_0002:
                num = index - chunkPrevious.m_ChunkOffset;
                if (num >= 0)
                {
                    if (num >= chunkPrevious.m_ChunkLength)
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }
                    chunkPrevious.m_ChunkChars[num] = value;
                }
                else
                {
                    chunkPrevious = chunkPrevious.m_ChunkPrevious;
                    if (chunkPrevious == null)
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }
                    goto Label_0002;
                }
            }
        }
 
        /// <summary>
        /// Gets or sets the maximum number of characters that can be contained in the memory allocated by the current instance. 
        /// </summary>
        public int Capacity
        {
            get
            {
                return (this.m_ChunkChars.Length + this.m_ChunkOffset);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value > this.MaxCapacity)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value < this.Length)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.Capacity != value)
                {
                    int num = value - this.m_ChunkOffset;
                    char[] destinationArray = new char[num];
                    Array.Copy(this.m_ChunkChars, destinationArray, this.m_ChunkLength);
                    this.m_ChunkChars = destinationArray;
                }
            }
        }

        /// <summary>
        /// Gets or sets the length of the current StringBuilder object. 
        /// </summary>
        public int Length
        {
            get
            {
                return (this.m_ChunkOffset + this.m_ChunkLength);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value > this.MaxCapacity)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                int capacity = this.Capacity;
                if ((value == 0) && (this.m_ChunkPrevious == null))
                {
                    this.m_ChunkLength = 0;
                    this.m_ChunkOffset = 0;
                }
                else
                {
                    int repeatCount = value - this.Length;
                    if (repeatCount > 0)
                    {
                        this.Append('\0', repeatCount);
                    }
                    else
                    {
                        StringBuilder builder = this.FindChunkForIndex(value);
                        if (builder != this)
                        {
                            int num3 = capacity - builder.m_ChunkOffset;
                            char[] destinationArray = new char[num3];
                            Array.Copy(builder.m_ChunkChars, destinationArray, builder.m_ChunkLength);
                            this.m_ChunkChars = destinationArray;
                            this.m_ChunkPrevious = builder.m_ChunkPrevious;
                            this.m_ChunkOffset = builder.m_ChunkOffset;
                        }
                        this.m_ChunkLength = value - builder.m_ChunkOffset;
                    }
                }

            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the StringBuilder class from the specified substring and capacity. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="capacity"></param>
        public unsafe StringBuilder(string value, int startIndex, int length, int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if (value == null)
            {
                value = string.Empty;
            }
            if (startIndex > (value.Length - length))
            {
                throw new ArgumentOutOfRangeException("length");
            }
            this.m_MaxCapacity = 0x7fffffff;
            if (capacity == 0)
            {
                capacity = 0x10;
            }
            if (capacity < length)
            {
                capacity = length;
            }
            //Allocate the chunk of capactity
            this.m_ChunkChars = new char[capacity];
            //Set the length of the chunk
            this.m_ChunkLength = length;
            //Copy the value to the chunkChars
            {                
                value.ToCharArray().CopyTo(this.m_ChunkChars, 0);
            }
        }

        private StringBuilder(int size, int maxCapacity, StringBuilder previousBlock)
        {
            this.m_ChunkChars = new char[size];
            this.m_MaxCapacity = maxCapacity;
            this.m_ChunkPrevious = previousBlock;
            if (previousBlock != null)
            {
                this.m_ChunkOffset = previousBlock.m_ChunkOffset + previousBlock.m_ChunkLength;
            }
        }

        /// <summary>
        /// Initializes a new instance of the StringBuilder class using the specified string and capacity. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="capacity"></param>
        public StringBuilder(string value, int capacity)
            : this(value, 0, (value != null) ? value.Length : 0, capacity) { }

        /// <summary>
        /// Initializes a new instance of the StringBuilder class that starts with a specified capacity and can grow to a specified maximum. 
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="maxCapacity"></param>
        public StringBuilder(int capacity, int maxCapacity)
        {
            if (capacity > maxCapacity)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }
            if (maxCapacity < 1)
            {
                throw new ArgumentOutOfRangeException("maxCapacity");
            }
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }
            if (capacity == 0)
            {
                capacity = Math.Min(0x10, maxCapacity);
            }
            this.m_MaxCapacity = maxCapacity;
            this.m_ChunkChars = new char[capacity];
        }

        private StringBuilder(StringBuilder from)
        {
            this.m_ChunkLength = from.m_ChunkLength;
            this.m_ChunkOffset = from.m_ChunkOffset;
            this.m_ChunkChars = from.m_ChunkChars;
            this.m_ChunkPrevious = from.m_ChunkPrevious;
            this.m_MaxCapacity = from.m_MaxCapacity;
        }

        /// <summary>
        /// Initializes a new instance of the StringBuilder class using the specified capacity. 
        /// </summary>
        /// <param name="capacity"></param>
        public StringBuilder(int capacity)
            : this(string.Empty, capacity) { }

        /// <summary>
        /// Initializes a new instance of the StringBuilder class using the specified string. 
        /// </summary>
        /// <param name="value"></param>
        public StringBuilder(string value)
            : this(value, 0x10) { }

        /// <summary>
        /// Initializes a new instance of the StringBuilder class. 
        /// </summary>
        public StringBuilder()
            : this(0x10) { }

        #endregion

        #region Methods

        /// <summary>
        /// Removes all characters from the current StringBuilder instance. 
        /// </summary>
        /// <returns></returns>
        public StringBuilder Clear()
        {
            this.Length = 0;
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified Boolean value to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(bool value)
        {
            return this.Append(value.ToString());
        }

        /// <summary>
        /// Appends the string representation of a specified 8-bit unsigned integer to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(byte value)
        {
            return this.Append(value.ToString());
        }

        /// <summary>
        /// Appends the string representation of a specified Unicode character to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(char value)
        {
            if (this.m_ChunkLength < this.m_ChunkChars.Length)
            {
                this.m_ChunkChars[this.m_ChunkLength++] = value;
            }
            else
            {
                this.Append(value, 1);
            }
            return this;
        }

        /*
         * netMF is missing the Decimal Type
        public void Append(decimal value)
        {
            this.Append(value.ToString());
        }
        */

        /// <summary>
        /// Appends the string representation of a specified double-precision floating-point number to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(double value)
        {
            return this.Append(value.ToString());
        }

        /// <summary>
        /// Appends the string representation of a specified 16-bit signed integer to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(short value)
        {
            return this.Append(value.ToString());
        }

        /// <summary>
        /// Appends the string representation of the Unicode characters in a specified array to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(char[] value)
        {
            if ((value != null) && (value.Length > 0))
            {
                this.Append(value, value.Length);
            }
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified 32-bit signed integer to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(int value)
        {
            return this.Append(value.ToString());
        }

        /// <summary>
        /// Appends the string representation of a specified 64-bit unsigned integer to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(long value)
        {
            return this.Append(value.ToString());
        }

        /// <summary>
        /// Appends the string representation of a specified object to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(object value)
        {
            if (value == null) return this;
            return this.Append(value.ToString());
        }

        public StringBuilder Append(string value)
        {
            if (value != null && value != string.Empty)
            {
                char[] chunkChars = this.m_ChunkChars;
                int chunkLength = this.m_ChunkLength;
                int length = value.Length;
                int num3 = chunkLength + length;
                if (num3 < chunkChars.Length)
                {
                    if (length <= 2)
                    {
                        if (length > 0) chunkChars[chunkLength] = value[0];
                        if (length > 1) chunkChars[chunkLength + 1] = value[1];
                    }
                    else
                    {                        
                        char[] tmp = value.ToCharArray();
                        System.Array.Copy(tmp, 0, chunkChars, chunkLength, length);                       
                    }
                    this.m_ChunkLength = num3;
                }
                else
                {
                    this.AppendHelper(ref value);
                }
            }
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified 8-bit signed integer to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(sbyte value)
        {
            return this.Append(value.ToString());
        }

        /// <summary>
        /// Appends the string representation of a specified double-precision floating-point number to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(float value)
        {
            return this.Append(value.ToString());
        }

        /// <summary>
        /// Appends the string representation of a specified 16-bit unsigned integer to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(ushort value)
        {
            return this.Append(value.ToString());
        }

        /// <summary>
        /// Appends the string representation of a specified 32-bit unsigned integer to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(uint value)
        {
            return this.Append(value.ToString());
        }

        /// <summary>
        /// Appends the string representation of a specified 64-bit unsigned integer to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder Append(ulong value)
        {
            return this.Append(value.ToString());
        }

        /// <summary>
        /// Appends a copy of a specified substring to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public StringBuilder Append(string value, int startIndex, int count)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (value == null)
            {
                if ((startIndex != 0) || (count != 0))
                {
                    throw new ArgumentNullException("value");
                }
                return this;
            }
            if (count != 0)
            {
                if (startIndex > (value.Length - count))
                {
                    throw new ArgumentOutOfRangeException("startIndex");
                }
                this.Append(value.Substring(startIndex, count));
            }
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified subarray of Unicode characters to this instance
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <param name="charCount"></param>
        /// <returns></returns>
        public StringBuilder Append(char[] value, int startIndex, int charCount)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (value == null)
            {
                if ((startIndex != 0) || (charCount != 0))
                {
                    throw new ArgumentNullException("value");
                }
                return this;
            }
            if (charCount > (value.Length - startIndex))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (charCount != 0)
            {
                for(int i = startIndex; i < startIndex + charCount; ++i)
                {
                    this.Append(value[i], 1);
                }
            }
            return this;
        }
        
        /// <summary>
        /// Appends a specified number of copies of the string representation of a Unicode character to this instance. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public StringBuilder Append(char value, int repeatCount)
        {
            if (repeatCount < 0)
            {
                throw new ArgumentOutOfRangeException("repeatCount");
            }
            if (repeatCount != 0)
            {
                int chunkLength = this.m_ChunkLength;
                while (repeatCount > 0)
                {
                    if (chunkLength < this.m_ChunkChars.Length)
                    {
                        this.m_ChunkChars[chunkLength++] = value;
                        repeatCount--;
                    }
                    else
                    {
                        this.m_ChunkLength = chunkLength;
                        this.ExpandByABlock(repeatCount);
                        chunkLength = 0;
                    }
                }
                this.m_ChunkLength = chunkLength;
            }
            return this;
        }

        /// <summary>
        /// Removes the specified range of characters from this instance. 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public StringBuilder Remove(int startIndex, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if (length > (this.Length - startIndex))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((this.Length == length) && (startIndex == 0))
            {
                this.Length = 0;
                return this;
            }
            if (length > 0)
            {
                StringBuilder builder;
                int num;
                this.Remove(startIndex, length, out builder, out num);
            }
            return this;
        }

        /// <summary>
        /// Converts the value of this instance to a String. (Overrides Object.ToString().)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            char[] result = new char[this.Length];
            StringBuilder chunkPrevious = this;
            do
            {
                if (chunkPrevious.m_ChunkLength > 0)
                {
                    char[] chunkChars = chunkPrevious.m_ChunkChars;
                    int chunkOffset = chunkPrevious.m_ChunkOffset;
                    int chunkLength = chunkPrevious.m_ChunkLength;
                    System.Array.Copy(chunkChars, 0, result, chunkOffset, chunkLength);
                }
                chunkPrevious = chunkPrevious.m_ChunkPrevious;
            }
            while (chunkPrevious != null);
            return new string(result);
        }

        /// <summary>
        /// Converts the value of a substring of this instance to a String. 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string ToString(int startIndex, int length)
        {
            int num = this.Length;
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if (startIndex > num)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (startIndex > (num - length))
            {
                throw new ArgumentOutOfRangeException("length");
            }
            StringBuilder chunkPrevious = this;
            int num2 = startIndex + length;
            char[] result = new char[this.Length];
            int num3 = length;
            while (num3 > 0)
            {
                int chunkLength = num2 - chunkPrevious.m_ChunkOffset;
                if (chunkLength >= 0)
                {
                    if (chunkLength > chunkPrevious.m_ChunkLength)
                    {
                        chunkLength = chunkPrevious.m_ChunkLength;
                    }
                    int num5 = num3;
                    int charCount = num5;
                    int index = chunkLength - num5;
                    if (index < 0)
                    {
                        charCount += index;
                        index = 0;
                    }
                    num3 -= charCount;
                    if (charCount > 0)
                    {
                        char[] chunkChars = chunkPrevious.m_ChunkChars;
                        if ((((charCount + num3)) > length) || ((charCount + index) > chunkChars.Length))
                        {
                            throw new ArgumentOutOfRangeException("chunkCount");
                        }                       
                        System.Array.Copy(chunkChars, index, result, 0, charCount);
                    }
                }
                chunkPrevious = chunkPrevious.m_ChunkPrevious;
            }
            return new string(result);
        }

        /// <summary>
        /// Inserts one or more copies of a specified string into this instance at the specified character position. 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public StringBuilder Insert(int index, string value, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            int length = this.Length;
            if (index > length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (((value != null) && (value.Length != 0)) && (count != 0))
            {
                StringBuilder builder;
                int num3;
                long num2 = value.Length * count;
                if (num2 > (this.MaxCapacity - this.Length))
                {
                    throw new OutOfMemoryException();
                }
                this.MakeRoom(index, (int)num2, out builder, out num3, false);
                char[] chars = value.ToCharArray();
                int charLength = chars.Length;
                while (count > 0)
                {                    
                    int cindex = 0;
                    this.ReplaceInPlaceAtChunk(ref builder, ref num3, chars, ref cindex, charLength);
                    --count;
                }
            }
            return this;
        }

        /// <summary>
        /// Inserts the string representation of a specified subarray of Unicode characters into this instance at the specified character position. 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <param name="charCount"></param>
        /// <returns></returns>
        public StringBuilder Insert(int index, char[] value, int startIndex, int charCount)
        {
            int length = this.Length;
            if (index > length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (value == null)
            {
                if ((startIndex != 0) || (charCount != 0))
                {
                    throw new ArgumentNullException("index");
                }
                return this;
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (startIndex > (value.Length - charCount))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if (charCount > 0)
            {                
                this.Insert(index, new string(value, startIndex, charCount), 1);
            }
            return this;
        }

        /// <summary>
        /// Replaces, within a substring of this instance, all occurrences of a specified character with another specified character. 
        /// </summary>
        /// <param name="oldChar"></param>
        /// <param name="newChar"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public StringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            int num3;
            int length = this.Length;
            if (startIndex > length)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if ((count < 0) || (startIndex > (length - count)))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            int num2 = startIndex + count;
            StringBuilder chunkPrevious = this;
        Label_0048:
            num3 = num2 - chunkPrevious.m_ChunkOffset;
            int num4 = startIndex - chunkPrevious.m_ChunkOffset;
            if (num3 >= 0)
            {
                int index = Math.Max(num4, 0);
                int num6 = Math.Min(chunkPrevious.m_ChunkLength, num3);
                while (index < num6)
                {
                    if (chunkPrevious.m_ChunkChars[index] == oldChar)
                    {
                        chunkPrevious.m_ChunkChars[index] = newChar;
                    }
                    index++;
                }
            }
            if (num4 < 0)
            {
                chunkPrevious = chunkPrevious.m_ChunkPrevious;
                goto Label_0048;
            }
            return this;
        }

        /// <summary>
        /// Replaces all occurrences of a specified character in this instance with another specified character. 
        /// </summary>
        /// <param name="oldChar"></param>
        /// <param name="newChar"></param>
        /// <returns></returns>
        public StringBuilder Replace(char oldChar, char newChar)
        {
            return this.Replace(oldChar, newChar, 0, this.Length);
        }

        /// <summary>
        /// Replaces, within a substring of this instance, all occurrences of a specified string with another specified string. 
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public StringBuilder Replace(string oldValue, string newValue, int startIndex, int count)
        {
            int length = this.Length;
            if (startIndex > length)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if ((count < 0) || (startIndex > (length - count)))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (oldValue == null)
            {
                throw new ArgumentNullException("oldValue");
            }
            if (oldValue.Length == 0)
            {
                throw new ArgumentException("oldValue");
            }
            if (newValue == null)
            {
                newValue = string.Empty;
            }
            int newLength = newValue.Length;
            int oldLength = oldValue.Length;
            int[] sourceArray = null;
            int replacementsCount = 0;
            StringBuilder chunk = this.FindChunkForIndex(startIndex);
            int indexInChunk = startIndex - chunk.m_ChunkOffset;
            //While there is a replacement remaining
            while (count > 0)
            {                
                //If the old value if found in the chunk at the index
                if (this.StartsWith(chunk, indexInChunk, count, oldValue))
                {
                    //If we need to allocate for a match then do so
                    if (sourceArray == null)
                    {
                        sourceArray = new int[5];
                    }
                    else if (replacementsCount >= sourceArray.Length)
                    {
                        //We have more matches than allocated for resize the buffer
                        int[] destinationArray = new int[((sourceArray.Length * 3) / 2) + 4];
                        Array.Copy(sourceArray, destinationArray, sourceArray.Length);
                        sourceArray = destinationArray;
                    }
                    //Save the index in the next avilable replacement slot
                    sourceArray[replacementsCount] = indexInChunk;
                    ++replacementsCount;
                    //Move the index pointer
                    indexInChunk += oldLength;
                    //Decrement the count
                    count -= oldLength;
                }
                else
                {

                    //A match at the index was not found
                    //Move the pointer
                    ++indexInChunk;
                    //Decrement the count
                    --count;
                }
                //If we are past the chunk boundry or the no replacements remaining
                if ((indexInChunk >= chunk.m_ChunkLength) || (count == 0))
                {
                    //Determine the index
                    int index = indexInChunk + chunk.m_ChunkOffset;
                    //Replace the remaining characters
                    this.ReplaceAllInChunk(sourceArray, replacementsCount, chunk, oldLength, newValue);
                    //Move the index
                    index += (newLength - oldLength) * replacementsCount;
                    //Resert the replacements count
                    replacementsCount = 0;
                    //Find the next chunk and continue
                    chunk = this.FindChunkForIndex(index);
                    //Move the index reletive to inside the chunk
                    indexInChunk = index - chunk.m_ChunkOffset;
                }
            }
            return this;
        }

        /// <summary>
        /// Replaces all occurrences of a specified string in this instance with another specified string. 
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public StringBuilder Replace(string oldValue, string newValue)
        {
            return this.Replace(oldValue, newValue, 0, this.Length);
        }

        /// <summary>
        /// Append the provided line along with a new line.
        /// </summary>
        /// <param name="str"></param>
        public StringBuilder AppendLine(string str)
        {
            this.Append(str);
            return this.AppendLine();
        }

        /// <summary>
        /// Appends a NewLine onto the String
        /// </summary>
        public StringBuilder AppendLine()
        {
            return this.Append("\r\n");
        }

        #endregion

        #region Internals

        internal int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }
            if (this.Capacity < capacity)
            {
                this.Capacity = capacity;
            }
            return this.Capacity;
        }

        internal bool StartsWith(StringBuilder chunk, int indexInChunk, int count, string value)
        {
            for (int i = 0, e = value.Length; i < e; ++i)
            {
                if (count == 0)
                {
                    return false;
                }
                if (indexInChunk >= chunk.m_ChunkLength)
                {
                    chunk = this.Next(chunk);
                    if (chunk == null) return false;
                    indexInChunk = 0;
                }
                if (value[i] != chunk.m_ChunkChars[indexInChunk])
                {
                    return false;
                }
                ++indexInChunk;
                --count;
            }
            return true;
        }

        internal void ReplaceAllInChunk(int[] replacements, int replacementsCount, StringBuilder sourceChunk, int removeCount, string value)
        {
            //If there is a replacement to occur
            if (replacementsCount > 0)
            {
                //Determine the cmount of characters to remove
                int count = (value.Length - removeCount) * replacementsCount;
                //Scope the working chunk
                StringBuilder chunk = sourceChunk;
                //Determine the index of the first replacement
                int indexInChunk = replacements[0];
                //If there is a character being added make room
                if (count > 0)
                {
                    this.MakeRoom(chunk.m_ChunkOffset + indexInChunk, count, out chunk, out indexInChunk, true);
                }
                //Start at the first replacement
                int index = 0;
                int replacementIndex = 0;
                char[] chars = value.ToCharArray();
            ReplaceValue:
                //Replace the value                 
                this.ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, chars, ref replacementIndex, value.Length);
                if (replacementIndex == value.Length)
                {
                    replacementIndex = 0;
                }

                //Determine the next replacement 
                int valueIndex = replacements[index] + removeCount;
                //Move the pointer of the working replacement
                ++index;
                //If we are not past the replacement boundry
                if (index < replacementsCount)
                {
                    //Determine the next replacement
                    int nextIndex = replacements[index];
                    //If there is a character remaining to be replaced
                    if (count != 0)
                    {
                        //Replace it
                        this.ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, sourceChunk.m_ChunkChars, ref valueIndex, nextIndex - valueIndex);
                    }//Move the pointer
                    else indexInChunk += nextIndex - valueIndex;
                    goto ReplaceValue;//Finish replacing
                }
                //We are are done and there is charcters to be removed they are at the end
                if (count < 0)
                {
                    //Remove them by negating the count to make it positive the chars removed are from (chunk.m_ChunkOffset + indexInChunk) to -count
                    this.Remove(chunk.m_ChunkOffset + indexInChunk, -count, out chunk, out indexInChunk);
                }
            }
        }

        internal StringBuilder Next(StringBuilder chunk)
        {
            if (chunk == this)
            {
                return null;
            }
            return this.FindChunkForIndex(chunk.m_ChunkOffset + chunk.m_ChunkLength);
        }

        private void ReplaceInPlaceAtChunk(ref StringBuilder chunk, ref int indexInChunk, char[] value, ref int valueIndex, int count)
        {
            if (count == 0)
            {
                return;
            }
            while (true)
            {
                //int num = chunk.m_ChunkLength - indexInChunk;
                int length = Math.Min(chunk.m_ChunkLength - indexInChunk, count);
                //ThreadSafeCopy(value, ref valueIndex, chunk.m_ChunkChars, ref indexInChunk, num2);
                System.Array.Copy(value, valueIndex, chunk.m_ChunkChars, indexInChunk, length);
                indexInChunk += length;
                if (indexInChunk >= chunk.m_ChunkLength)
                {
                    chunk = this.Next(chunk);
                    indexInChunk = 0;
                }
                count -= length;
                valueIndex += length;
                if (count == 0) return;
            }
        }

        internal void MakeRoom(int index, int count, out StringBuilder chunk, out int indexInChunk, bool doneMoveFollowingChars)
        {
            if ((count + this.Length) > this.m_MaxCapacity) throw new ArgumentOutOfRangeException("requiredLength");
            chunk = this;
            while (chunk.m_ChunkOffset > index)
            {
                chunk.m_ChunkOffset += count;
                chunk = chunk.m_ChunkPrevious;
            }
            indexInChunk = index - chunk.m_ChunkOffset;
            if ((!doneMoveFollowingChars && (chunk.m_ChunkLength <= 0x20)) && ((chunk.m_ChunkChars.Length - chunk.m_ChunkLength) >= count))
            {
                int chunkLength = chunk.m_ChunkLength;
                while (chunkLength > indexInChunk)
                {
                    chunkLength--;
                    chunk.m_ChunkChars[chunkLength + count] = chunk.m_ChunkChars[chunkLength];
                }
                chunk.m_ChunkLength += count;
            }
            else
            {
                StringBuilder builder = new StringBuilder(Math.Max(count, 0x10), chunk.m_MaxCapacity, chunk.m_ChunkPrevious);
                builder.m_ChunkLength = count;
                int length = Math.Min(count, indexInChunk);
                if (length > 0)
                {
                    System.Array.Copy(chunk.m_ChunkChars, 0, builder.m_ChunkChars, 0, length);
                    int nextLength = indexInChunk - length;
                    if (nextLength >= 0)
                    {
                        System.Array.Copy(chunk.m_ChunkChars, length, chunk.m_ChunkChars, 0, nextLength);
                        indexInChunk = nextLength;
                    }
                }
                chunk.m_ChunkPrevious = builder;
                chunk.m_ChunkOffset += count;
                if (length < count)
                {
                    chunk = builder;
                    indexInChunk = length;
                }
            }
        }

        internal StringBuilder FindChunkForIndex(int index)
        {
            StringBuilder chunkPrevious = this;
            while (chunkPrevious.m_ChunkOffset > index) chunkPrevious = chunkPrevious.m_ChunkPrevious;
            return chunkPrevious;
        }

        internal void AppendHelper(ref string value)
        {            
            if (value == null || value == string.Empty) return;
            this.Append(value.ToCharArray(), value.Length);
        }

        internal void ExpandByABlock(int minBlockCharCount)
        {
            if ((minBlockCharCount + this.Length) > this.m_MaxCapacity) throw new ArgumentOutOfRangeException("requiredLength");
            int num = Math.Max(minBlockCharCount, Math.Min(this.Length, 0x1f40));
            this.m_ChunkPrevious = new StringBuilder(this);
            this.m_ChunkOffset += this.m_ChunkLength;
            this.m_ChunkLength = 0;
            //If Allocated does not match required storage
            if ((this.m_ChunkOffset + num) < num)
            {                
                this.m_ChunkChars = null;
                throw new OutOfMemoryException();
            }
            this.m_ChunkChars = new char[num];
        }

        internal void Remove(int startIndex, int count, out StringBuilder chunk, out int indexInChunk)
        {
            int num = startIndex + count;
            chunk = this;
            StringBuilder builder = null;
            int sourceIndex = 0;
            while (true)
            {
                if ((num - chunk.m_ChunkOffset) >= 0)
                {
                    if (builder == null)
                    {
                        builder = chunk;
                        sourceIndex = num - builder.m_ChunkOffset;
                    }
                    if ((startIndex - chunk.m_ChunkOffset) >= 0)
                    {
                        indexInChunk = startIndex - chunk.m_ChunkOffset;
                        int destinationIndex = indexInChunk;
                        int num4 = builder.m_ChunkLength - sourceIndex;
                        if (builder != chunk)
                        {
                            destinationIndex = 0;
                            chunk.m_ChunkLength = indexInChunk;
                            builder.m_ChunkPrevious = chunk;
                            builder.m_ChunkOffset = chunk.m_ChunkOffset + chunk.m_ChunkLength;
                            if (indexInChunk == 0)
                            {
                                builder.m_ChunkPrevious = chunk.m_ChunkPrevious;
                                chunk = builder;
                            }
                        }
                        builder.m_ChunkLength -= sourceIndex - destinationIndex;
                        if (destinationIndex != sourceIndex)
                        {
                            //ThreadSafeCopy(builder.m_ChunkChars, ref sourceIndex, builder.m_ChunkChars, ref destinationIndex, num4);
                            System.Array.Copy(builder.m_ChunkChars, sourceIndex, builder.m_ChunkChars, destinationIndex, num4);
                        }
                        return;
                    }
                }
                else chunk.m_ChunkOffset -= count;
                chunk = chunk.m_ChunkPrevious;
            }
        }

        internal void Append(char[] value, int valueCount)
        {
            int num = valueCount + this.m_ChunkLength;
            if (num <= this.m_ChunkChars.Length)
            {
                //ThreadSafeCopy(value, this.m_ChunkChars, this.m_ChunkLength, valueCount);
                System.Array.Copy(value, 0, this.m_ChunkChars, this.m_ChunkLength, valueCount);
                this.m_ChunkLength = num;
            }
            else
            {
                int count = this.m_ChunkChars.Length - this.m_ChunkLength;
                if (count > 0)
                {
                    //ThreadSafeCopy(value, this.m_ChunkChars, this.m_ChunkLength, count);
                    System.Array.Copy(value, 0, this.m_ChunkChars, this.m_ChunkLength, count);
                    this.m_ChunkLength = this.m_ChunkChars.Length;
                }
                int minBlockCharCount = valueCount - count;
                this.ExpandByABlock(minBlockCharCount);
                //ThreadSafeCopy(value + count, this.m_ChunkChars, 0, minBlockCharCount);
                System.Array.Copy(value, count, this.m_ChunkChars, 0, minBlockCharCount);
                this.m_ChunkLength = minBlockCharCount;
            }
            return;
        }

        #endregion        
    }
}


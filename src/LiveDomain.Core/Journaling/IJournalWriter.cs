﻿using System;
namespace LiveDomain.Core
{
	interface IJournalWriter : IDisposable
    {
        long Length { get; }
        void Write(JournalEntry item);
    	void Close();
    }
}
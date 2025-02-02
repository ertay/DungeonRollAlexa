﻿using System;
/// <summary>
/// A thread safe, true random nubmer generator.
/// Grabbed from: https://stackoverflow.com/a/1262619/3646421
/// </summary>
public class ThreadSafeRandom
{
    [ThreadStatic] private static Random Local;

    public static Random ThisThreadsRandom
    {
        get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Environment.CurrentManagedThreadId))); }
    }
}


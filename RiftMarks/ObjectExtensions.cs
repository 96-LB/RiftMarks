﻿using System;

namespace RiftMarks;


internal static class ObjectExtensions {
    public static void Pipe<T>(this T value, Action<T> func) {
        func(value);
    }

    public static R Pipe<T, R>(this T value, Func<T, R> func) {
        return func(value);
    }

    public static ref T Pipe<T>(this T value, ref T var) {
        var = value;
        return ref var;
    }
}

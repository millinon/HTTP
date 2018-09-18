using System;

namespace HTTP
{
    public class Option<T>
    {
        public readonly bool HasValue;

        public readonly T Value;

        public Option(T Value)
        {
            this.Value = Value;
            this.HasValue = true;
        }

        public Option()
        {
            this.Value = default(T);
            this.HasValue = false;
        }

        public static Option<T> Some(T Value) => new Option<T>(Value);
        public static Option<T> None() => new Option<T>();

        public static implicit operator Option<T>(T Value)
        {
            return new Option<T>(Value);
        }

        public static explicit operator T(Option<T> Option)
        {
            if (Option.HasValue) return Option.Value;
            else throw new InvalidOperationException("Attempted to read empty value");
        }
    }
}

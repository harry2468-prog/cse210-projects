using System;

public sealed class Fraction : IEquatable<Fraction>, IComparable<Fraction>
{
    public int Numerator { get; }
    public int Denominator { get; }

    // Constructors
    public Fraction() : this(1, 1) { }

    public Fraction(int numerator) : this(numerator, 1) { }

    public Fraction(int numerator, int denominator)
    {
        if (denominator == 0)
            throw new DivideByZeroException("Denominator cannot be zero.");

        // Normalize sign so denominator is always positive
        if (denominator < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        // Reduce to simplest terms
        int g = Gcd(Math.Abs(numerator), denominator);
        Numerator = numerator / g;
        Denominator = denominator / g;
    }

    // String forms
    public override string ToString()
    {
        return Numerator + "/" + Denominator;
    }

    public string ToMixedString()
    {
        if (Denominator == 1) return Numerator.ToString();
        int whole = Numerator / Denominator;
        int rem = Math.Abs(Numerator % Denominator);
        if (whole != 0 && rem != 0) return whole + " " + rem + "/" + Denominator;
        if (whole == 0) return (Numerator < 0 ? "-" : "") + rem + "/" + Denominator;
        return whole.ToString();
    }

    // Numeric value
    public double ToDouble()
    {
        return (double)Numerator / (double)Denominator;
    }

    // Operators
    public static Fraction operator +(Fraction a, Fraction b)
    {
        return new Fraction(
            a.Numerator * b.Denominator + b.Numerator * a.Denominator,
            a.Denominator * b.Denominator
        );
    }

    public static Fraction operator -(Fraction a, Fraction b)
    {
        return new Fraction(
            a.Numerator * b.Denominator - b.Numerator * a.Denominator,
            a.Denominator * b.Denominator
        );
    }

    public static Fraction operator *(Fraction a, Fraction b)
    {
        return new Fraction(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
    }

    public static Fraction operator /(Fraction a, Fraction b)
    {
        if (b.Numerator == 0)
            throw new DivideByZeroException("Cannot divide by zero fraction.");
        return new Fraction(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    }

    // Equality and comparison
    public bool Equals(Fraction other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Numerator == other.Numerator && Denominator == other.Denominator;
    }

    public override bool Equals(object obj)
    {
        return obj is Fraction f && Equals(f);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Numerator * 397) ^ Denominator;
        }
    }

    public int CompareTo(Fraction other)
    {
        if (other == null) return 1;
        // Compare without converting to double to avoid precision issues
        long left = (long)Numerator * other.Denominator;
        long right = (long)other.Numerator * Denominator;
        return left.CompareTo(right);
    }

    // Helpers
    private static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            int t = a % b;
            a = b;
            b = t;
        }
        return a == 0 ? 1 : a;
    }

    // Convenience creators
    public static Fraction FromDouble(double value, int maxDenominator = 1000000)
    {
        // Continued fraction approximation
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Value must be a finite number.");

        int sign = value < 0 ? -1 : 1;
        value = Math.Abs(value);

        int a0 = (int)Math.Floor(value);
        double frac = value - a0;
        if (frac == 0) return new Fraction(sign * a0, 1);

        long numPrev = 1, num = a0;
        long denPrev = 0, den = 1;

        while (true)
        {
            if (frac == 0) break;
            double inv = 1.0 / frac;
            int ai = (int)Math.Floor(inv);
            long numNext = ai * num + numPrev;
            long denNext = ai * den + denPrev;

            if (denNext > maxDenominator) break;

            numPrev = num;
            denPrev = den;
            num = numNext;
            den = denNext;

            frac = inv - ai;
        }

        return new Fraction((int)(sign * num), (int)den);
    }
}
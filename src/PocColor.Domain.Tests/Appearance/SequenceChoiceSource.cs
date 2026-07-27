using System;
using System.Collections.Generic;
using PocColor.Domain.Appearance.Port;

namespace PocColor.Domain.Tests.Appearance;

internal sealed class SequenceChoiceSource : IChoiceSource
{
    private readonly Queue<int> _values;

    public SequenceChoiceSource(params int[] values)
    {
        _values = new Queue<int>(values ?? Array.Empty<int>());
    }

    public int CallCount { get; private set; }

    public int NextIndex(int exclusiveUpperBound)
    {
        CallCount++;
        if (exclusiveUpperBound <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exclusiveUpperBound));
        }

        if (_values.Count == 0)
        {
            return 0;
        }

        int value = _values.Dequeue();
        return ((value % exclusiveUpperBound) + exclusiveUpperBound) % exclusiveUpperBound;
    }
}

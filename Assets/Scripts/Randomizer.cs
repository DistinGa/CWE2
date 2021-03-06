﻿using System;

public class Randomizer
{
    Random _rnd;
    int _seed;

    public Randomizer()
    {
        _seed = new Random().Next();
        _rnd = new Random(_seed);
    }

    public Randomizer(int seed)
    {
        _rnd = new Random(seed);
        _seed = seed;
    }

    public int GetRnd()
    {
        return _rnd.Next();
    }

    public int GetRndRange(int min, int max)
    {
        return _rnd.Next(min, max);
    }

    public int GetRndMax(int max)
    {
        return _rnd.Next(0, max);
    }
}

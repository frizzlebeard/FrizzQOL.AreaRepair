#nullable disable
using System;
using System.Collections.Generic;

public static class RepairRules
{
    public static bool IsInRange(float distance, float radius)
    {
        return radius > 0f && distance <= radius;
    }

    public static bool ShouldRepair(bool isDamaged, float distance, float radius)
    {
        return isDamaged && IsInRange(distance, radius);
    }

    public static bool ShouldAreaRepair(bool hoveredRepairAccepted)
    {
        return hoveredRepairAccepted;
    }

    public static bool ShouldRepairNearby(float distance, float radius, bool wardAllows)
    {
        return wardAllows && IsInRange(distance, radius);
    }
}

public sealed class RepairIndex<T> where T : class
{
    private readonly float _cell;
    private readonly Dictionary<T, Cell> _cellOf = new Dictionary<T, Cell>();
    private readonly Dictionary<T, Pos> _pos = new Dictionary<T, Pos>();
    private readonly Dictionary<Cell, List<T>> _cells = new Dictionary<Cell, List<T>>();

    public RepairIndex(float cellSize)
    {
        if (cellSize <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(cellSize));
        }

        _cell = cellSize;
    }

    public int Count => _cellOf.Count;

    public int LastVisited { get; private set; }

    public void Add(T item, float x, float y, float z)
    {
        if (item == null)
        {
            return;
        }

        Remove(item);
        Cell cell = CellOf(x, y, z);
        _cellOf[item] = cell;
        _pos[item] = new Pos(x, y, z);
        if (!_cells.TryGetValue(cell, out List<T> list))
        {
            list = new List<T>();
            _cells[cell] = list;
        }

        list.Add(item);
    }

    public void Remove(T item)
    {
        if (item == null)
        {
            return;
        }

        Cell cell;
        if (!_cellOf.TryGetValue(item, out cell))
        {
            return;
        }

        _cellOf.Remove(item);
        _pos.Remove(item);
        if (!_cells.TryGetValue(cell, out List<T> list))
        {
            return;
        }

        list.Remove(item);
        if (list.Count == 0)
        {
            _cells.Remove(cell);
        }
    }

    public void Collect(float x, float y, float z, float radius, List<T> results)
    {
        if (results == null)
        {
            throw new ArgumentNullException(nameof(results));
        }

        results.Clear();
        LastVisited = 0;
        if (radius <= 0f || _cellOf.Count == 0)
        {
            return;
        }

        double minX = Math.Floor((x - radius) / _cell);
        double maxX = Math.Floor((x + radius) / _cell);
        double minY = Math.Floor((y - radius) / _cell);
        double maxY = Math.Floor((y + radius) / _cell);
        double minZ = Math.Floor((z - radius) / _cell);
        double maxZ = Math.Floor((z + radius) / _cell);
        double spanX = maxX - minX + 1.0;
        double spanY = maxY - minY + 1.0;
        double spanZ = maxZ - minZ + 1.0;
        double cellCount = spanX * spanY * spanZ;
        if (cellCount > _cellOf.Count
            || spanX > 32.0
            || spanY > 32.0
            || spanZ > 32.0
            || minX < int.MinValue
            || maxX > int.MaxValue
            || minY < int.MinValue
            || maxY > int.MaxValue
            || minZ < int.MinValue
            || maxZ > int.MaxValue)
        {
            ScanAll(x, y, z, radius * radius, results);
            return;
        }

        float r2 = radius * radius;
        int fromX = (int)minX;
        int toX = (int)maxX;
        int fromY = (int)minY;
        int toY = (int)maxY;
        int fromZ = (int)minZ;
        int toZ = (int)maxZ;
        for (int cx = fromX; cx <= toX; cx++)
        {
            for (int cy = fromY; cy <= toY; cy++)
            {
                for (int cz = fromZ; cz <= toZ; cz++)
                {
                    if (!_cells.TryGetValue(new Cell(cx, cy, cz), out List<T> list))
                    {
                        continue;
                    }

                    Visit(list, x, y, z, r2, results);
                }
            }
        }
    }

    private void ScanAll(float x, float y, float z, float r2, List<T> results)
    {
        foreach (KeyValuePair<T, Pos> entry in _pos)
        {
            LastVisited++;
            Pos pos = entry.Value;
            float dx = pos.X - x;
            float dy = pos.Y - y;
            float dz = pos.Z - z;
            if (dx * dx + dy * dy + dz * dz <= r2)
            {
                results.Add(entry.Key);
            }
        }
    }

    private void Visit(List<T> list, float x, float y, float z, float r2, List<T> results)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T item = list[i];
            LastVisited++;
            Pos pos;
            if (!_pos.TryGetValue(item, out pos))
            {
                continue;
            }

            float dx = pos.X - x;
            float dy = pos.Y - y;
            float dz = pos.Z - z;
            if (dx * dx + dy * dy + dz * dz <= r2)
            {
                results.Add(item);
            }
        }
    }

    private Cell CellOf(float x, float y, float z)
    {
        return new Cell(Floor(x), Floor(y), Floor(z));
    }

    private int Floor(float value)
    {
        return (int)Math.Floor(value / _cell);
    }

    private readonly struct Pos
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Pos(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    private readonly struct Cell : IEquatable<Cell>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public Cell(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(Cell other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is Cell other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X;
                hash = (hash * 397) ^ Y;
                hash = (hash * 397) ^ Z;
                return hash;
            }
        }
    }
}

using Xunit;

public class RepairRulesTests
{
    [Fact]
    public void Damaged_inside_radius_repairs()
    {
        Assert.True(RepairRules.IsInRange(3f, 5f));
        Assert.True(RepairRules.ShouldRepair(true, 3f, 5f));
        Assert.True(RepairRules.ShouldRepair(true, 5f, 5f));
    }

    [Fact]
    public void Damaged_outside_radius_skips()
    {
        Assert.False(RepairRules.IsInRange(5.1f, 5f));
        Assert.False(RepairRules.ShouldRepair(true, 5.1f, 5f));
    }

    [Fact]
    public void Full_health_inside_radius_skips()
    {
        Assert.False(RepairRules.ShouldRepair(false, 1f, 5f));
    }

    [Fact]
    public void Zero_or_negative_radius_skips()
    {
        Assert.False(RepairRules.IsInRange(0f, 0f));
        Assert.False(RepairRules.ShouldRepair(true, 0f, 0f));
        Assert.False(RepairRules.ShouldRepair(true, 0f, -1f));
    }

    [Fact]
    public void Rejected_hover_repair_does_not_area_repair()
    {
        Assert.False(RepairRules.ShouldAreaRepair(false));
    }

    [Fact]
    public void Accepted_hover_repair_allows_area_repair()
    {
        Assert.True(RepairRules.ShouldAreaRepair(true));
    }

    [Fact]
    public void Ward_denies_nearby_repair()
    {
        Assert.False(RepairRules.ShouldRepairNearby(1f, 5f, false));
        Assert.True(RepairRules.ShouldRepairNearby(1f, 5f, true));
        Assert.False(RepairRules.ShouldRepairNearby(6f, 5f, true));
        Assert.False(RepairRules.ShouldRepairNearby(0f, 0f, true));
    }

    [Fact]
    public void Radius_query_skips_pieces_outside_nearby_cells()
    {
        var index = new RepairIndex<Marker>(10f);
        var near = new Marker("near");
        index.Add(near, 0f, 0f, 0f);
        for (int i = 0; i < 5000; i++)
        {
            index.Add(new Marker("far"), 10000f, 0f, i);
        }

        var found = new System.Collections.Generic.List<Marker>();
        index.Collect(0f, 0f, 0f, 5f, found);

        Assert.Equal(5001, index.Count);
        Assert.Equal(1, index.LastVisited);
        Assert.Single(found);
        Assert.Same(near, found[0]);
    }

    [Fact]
    public void Radius_query_includes_boundary_and_drops_removed_pieces()
    {
        var index = new RepairIndex<Marker>(10f);
        var edge = new Marker("edge");
        var west = new Marker("west");
        var outside = new Marker("outside");
        var above = new Marker("above");
        index.Add(edge, 10f, 0f, 0f);
        index.Add(west, -10f, 0f, 0f);
        index.Add(outside, 10.1f, 0f, 0f);
        index.Add(above, 0f, 100f, 0f);

        var found = new System.Collections.Generic.List<Marker>();
        index.Collect(0f, 0f, 0f, 10f, found);
        Assert.Equal(2, found.Count);
        Assert.Contains(edge, found);
        Assert.Contains(west, found);

        index.Remove(edge);
        index.Remove(west);
        index.Collect(0f, 0f, 0f, 10f, found);
        Assert.Empty(found);
    }

    [Fact]
    public void Moving_a_piece_updates_its_cell()
    {
        var index = new RepairIndex<Marker>(10f);
        var piece = new Marker("cart");
        index.Add(piece, 0f, 0f, 0f);
        index.Add(piece, 40f, 0f, 0f);

        var found = new System.Collections.Generic.List<Marker>();
        index.Collect(0f, 0f, 0f, 5f, found);
        Assert.Empty(found);

        index.Collect(40f, 0f, 0f, 5f, found);
        Assert.Single(found);
        Assert.Same(piece, found[0]);
    }

    private sealed class Marker
    {
        public Marker(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
